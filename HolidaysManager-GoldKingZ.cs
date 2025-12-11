using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using HolidaysManager_GoldKingZ.Config;
using CounterStrikeSharp.API.Modules.Commands;
using System.Text.RegularExpressions;
using System.Text;
using Microsoft.Extensions.Localization;
using CounterStrikeSharp.API.Modules.Cvars;
using MySqlConnector;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Timers;
using System.Diagnostics;
using System.IO;
using CounterStrikeSharp.API.ValveConstants.Protobuf;
using System.Xml.Linq;
using CounterStrikeSharp.API.Core.Translations;
using System.Globalization;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;


namespace HolidaysManager_GoldKingZ;

public class MainPlugin : BasePlugin
{
    public override string ModuleName => "Make The World Celebrate Every Holiday :)";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "Gold KingZ";
    public override string ModuleDescription => "https://github.com/oqyh";
    public static MainPlugin Instance { get; set; } = new();
    public Globals g_Main = new();
    public readonly Game_Listeners Game_Listeners = new();
    public readonly Game_UserMessages Game_UserMessages = new();
    public readonly Game_Hook Game_Hook = new();
    
    public override void Load(bool hotReload)
    {
        Instance = this;
        Configs.Load(ModuleDirectory);

        _ = Task.Run(Helper.DownloadMissingFilesAsync);
        Helper.RemoveRegisterCommandsAndHooks();
        Helper.RegisterCommandsAndHooks(true);

        if (hotReload)
        {
            Helper.RemoveAllPlayersSnowFall();
            Helper.RemoveAllProps();
            Helper.RemoveRegisterCommandsAndHooks();
            Helper.ClearVariables(true);

            Configs.Load(ModuleDirectory);            
            _ = Task.Run(Helper.DownloadMissingFilesAsync);

            Helper.RegisterCommandsAndHooks();
            Helper.ReloadPlayersGlobals();
            Helper.ChangeSkyBox(null!, true);
            Helper.StartTimer();
            _ = Helper.LoadMapData();

            if (Configs.Instance.MySql_Enable > 0)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await MySqlDataManager.CreateTableIfNotExistsAsync();
                    }
                    catch (Exception ex)
                    {
                        Helper.DebugMessage($"hotReload Error: {ex.Message}");
                    }
                });
            }
        }
    }



    public void OnServerPrecacheResources(ResourceManifest manifest)
    {
        try
        {
            manifest.AddResource("soundevents/game_sounds_player.vsndevts");
            manifest.AddResource(Configs.Instance.SkyBox_Path);
            manifest.AddResource(Configs.Instance.SnowFall_Path);
            manifest.AddResource(Configs.Instance.SnowBall_Model);
            manifest.AddResource(Configs.Instance.SnowBall_Model_ag2);
            manifest.AddResource(Configs.Instance.SnowBall_Trail_Particle);
            manifest.AddResource(Configs.Instance.SnowBall_Hit_Particle);

            var mapname = Server.MapName;
            if (!string.IsNullOrEmpty(mapname))
            {
                var GetModelPaths = MapData.GetModelPathsForMap(mapname);
                foreach (var ModelPaths in GetModelPaths)
                {
                    if(string.IsNullOrEmpty(ModelPaths))continue;

                    manifest.AddResource(ModelPaths);
                }
            }
            
            if(Configs.Instance.AutoPrecacheResources)
            {
                foreach (var Events in VpkParser.GetEventsFromVpk())
                {
                    var folders = Configs.Instance.AutoPrecacheResources_Folders;

                    if (folders.Count == 0 || folders.Any(f => Events.StartsWith(f)))
                    {
                        manifest.AddResource(Events);
                        Helper.DebugMessage("Auto ResourceManifest : " + Events);
                    }
                }
            }

            string filePath = Path.Combine(ModuleDirectory, "config/ServerPrecacheResources.txt");
            string[] lines = File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                if (line.TrimStart().StartsWith("//")) continue;
                manifest.AddResource(line);
                Helper.DebugMessage("Manual ResourceManifest : " + line);
            }
        }
        catch (Exception ex)
        {
            Helper.DebugMessage($"OnServerPrecacheResources Error: {ex.Message}");
        }
    }


    public void OnMapStart(string Map)
    {
        Helper.RemoveRegisterCommandsAndHooks();
        Helper.RegisterCommandsAndHooks();
        Helper.StartTimer();
    }

    public void OnEntityCreated(CEntityInstance entity)
    {
        if (entity == null || !entity.IsValid ) return;

        Server.NextFrame(() =>
        {
            Helper.ChangeSkyBox(entity);
        });
        
    }

    public void OnEntitySpawned(CEntityInstance entity)
    {
        if (entity == null || !entity.IsValid) return;

        if (entity.DesignerName.Contains("decoy_projectile") || entity.DesignerName.Contains("instanced_scripted_scene"))
        {
            Helper.MuteRadioDecoy(entity);
            Server.NextFrame(()=>
            {
                Helper.ChangeDecoyToSnow(entity);
            });
        }
    }

    public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        if (@event == null) return HookResult.Continue;
        Helper.StartTimer();

        _ = Helper.LoadMapData();

        return HookResult.Continue;
    }

    public void OnTick()
    {
        if(g_Main.Server_EditMode_Player == null || !g_Main.Server_EditMode_Player.IsValid)
        {
            Helper.ClearPropHighlight();
            return;
        }
        if (g_Main.Server_EditMode_Player.Buttons.HasFlag(PlayerButtons.Scoreboard))
        {
            if(g_Main.Server_EditMode_Player.IsOnGround())
            {
                g_Main.Server_EditMode_Player.PlayerUnFreeze();
            }

            Helper.CloseEditMode();

            _ = Helper.SaveMapData();
            return;
        }

        StringBuilder builder = new StringBuilder();
        string localized = Localizer["PrintToCenterToPlayer.PropsEditMode"];
        builder.AppendFormat(localized);
        var centerhtml = builder.ToString();
        g_Main.Server_EditMode_Player.PrintToCenterHtml(centerhtml);

        if(g_Main.Server_Prop_Attached != null && g_Main.Server_Prop_Attached.IsValid)
        {
            Helper.HandleAdjustmentInput(g_Main.Server_EditMode_Player);
            Helper.UpdatePropPosition(g_Main.Server_EditMode_Player);
        }
        else
        {
            Helper.CheckPropAndColor(g_Main.Server_EditMode_Player);
        }

        if(g_Main.Server_EditMode_Player.Buttons == 0)
        {
            if(g_Main.Server_EditMode_Player.IsOnGround())
            {
                g_Main.Server_EditMode_Player.PlayerUnFreeze();
            }
        }
    }

    public void OnCheckTransmit(CCheckTransmitInfoList infoList)
    {
        foreach ((CCheckTransmitInfo info, CCSPlayerController? receiver) in infoList)
        {
            if (!receiver.IsValid()) continue;

            
            foreach (var Player_Data in g_Main.Player_Data.Values)
            {
                if(Player_Data == null)continue;

                var player = Player_Data.Player;
                if(!player.IsValid())continue;

                var particle = Player_Data.Snow_Fall;
                if (particle == null || !particle.IsValid)continue;

                bool ShowRemove = false;
                if (Configs.Instance.SnowFall == 1)
                {
                    if(player.Slot != receiver.Slot)
                    {
                        ShowRemove = true;
                    }
                }
                else if (Configs.Instance.SnowFall > 1)
                {
                    if(player.Slot != receiver.Slot)
                    {
                        ShowRemove = true;
                    }

                    if(g_Main.Player_Data.TryGetValue(receiver.Slot, out var handle) && (handle.Toggle_Snow == 2 || handle.Toggle_Snow == -2))
                    {
                        ShowRemove = true;
                    }
                }

                if(ShowRemove)
                {
                    info.TransmitEntities.Remove(particle);
                }
            }

            foreach (var DroppedProps_Data in g_Main.DroppedProps_Data)
            {
                if(DroppedProps_Data == null)continue;

                var props = DroppedProps_Data.PropEntity;
                if (props == null || !props.IsValid)continue;
                
                bool ShowRemove = false;
                if (Configs.Instance.Props > 1)
                {
                    if(g_Main.Player_Data.TryGetValue(receiver.Slot, out var handle) && (handle.Toggle_Props == 2 || handle.Toggle_Props == -2))
                    {
                        ShowRemove = true;
                    }
                }

                if(ShowRemove)
                {
                    info.TransmitEntities.Remove(props);
                }
            }
        }
    }


    public HookResult OnEventPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        if (@event == null) return HookResult.Continue;
        
        var player = @event.Userid;
        if (!player.IsValid(true)) return HookResult.Continue;
        
        _ = HandlePlayerConnectionsAsync(player);
        
        return HookResult.Continue;
    }
    public async Task HandlePlayerConnectionsAsync(CCSPlayerController Getplayer)
    {
        if (!Getplayer.IsValid(true)) return;

        try
        {
            var player = Getplayer;
            if (!player.IsValid(true)) return;

            if (Configs.Instance.AutoSetPlayerLanguage)
            {
                var playerip = player.IpAddress?.Split(':')[0] ?? "";
                var countryCode = Helper.GetGeoIsoCodeInfoAsync(playerip);

                Server.NextFrame(() =>
                {
                    if (player.IsValid())
                    {
                        Helper.SetPlayerLanguage(player, countryCode);
                        Helper.ApplySnowFallToPlayer(player);
                    }
                });
            }

            await Helper.LoadPlayerData(player);
        }
        catch (Exception ex)
        {
            Helper.DebugMessage($"HandlePlayerConnectionsAsync error: {ex.Message}");
        }
    }

    public HookResult OnEventPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        if(@event == null)return HookResult.Continue;

        Helper.StartTimer();

        var player = @event.Userid;
        if (!player.IsValid(true)) return HookResult.Continue;





        return HookResult.Continue;
    }

    public HookResult OnEventGrenadeThrown(EventGrenadeThrown @event, GameEventInfo info)
    {
        if (@event == null || Configs.Instance.SnowBall_Message_Announcement == 0 || !@event.Weapon.Contains("decoy")) return HookResult.Continue;

        var player = @event.Userid;
        if (!player.IsValid(true)) return HookResult.Continue;

        bool teammatesAreEnemies = ConVar.Find("mp_teammates_are_enemies")!.GetPrimitiveValue<bool>();
        if (player.IsBot && (Configs.Instance.SnowBall_Message_Announcement == 1 || Configs.Instance.SnowBall_Message_Announcement == 3 || Configs.Instance.SnowBall_Message_Announcement == 5 || Configs.Instance.SnowBall_Message_Announcement == 6)) return HookResult.Continue;

        Server.NextFrame(() =>
        {
            if (!player.IsValid(true)) return;

            var playerTeam = player.TeamNum;
            string lastPlaceName = player.PlayerPawn?.Value?.LastPlaceName ?? "";
            
            foreach (var target in Helper.GetPlayersController(true, false, false))
            {
                if (!target.IsValid()) continue;
                
                bool sameTeam = playerTeam == target.TeamNum;
                bool shouldShow = Configs.Instance.SnowBall_Message_Announcement switch
                {
                    1 => !teammatesAreEnemies && !player.IsBot && sameTeam,
                    2 => !teammatesAreEnemies && sameTeam,
                    3 => !teammatesAreEnemies && !player.IsBot,
                    4 => !teammatesAreEnemies,
                    5 => teammatesAreEnemies && target == player,
                    6 => teammatesAreEnemies && !player.IsBot,
                    7 => teammatesAreEnemies,
                    _ => false
                };
                
                if (shouldShow)
                {
                    var message_formate = Localizer["PrintToChat.SnowBall.Throw"].ToString();
                    var formattedMessage = message_formate.Replace("{team_color}", player.TeamNum.ToTeamColor());
                    Helper.AdvancedPlayerPrintToChat(target, null!, formattedMessage, player.PlayerName ?? "", lastPlaceName ?? "");
                }
                    
            }
        });

        return HookResult.Continue;
    }

    public HookResult OnEventGrenadeBounce(EventGrenadeBounce @event, GameEventInfo info)
    {
        if (@event == null) return HookResult.Continue;

        var player = @event.Userid;
        if (!player.IsValid(true)) return HookResult.Continue;

        var attacker = player.CheckPlayerController();
        if (!attacker.IsValid(true)) return HookResult.Continue;

        var decoy = attacker.GetDecoyFromThrower();
        if(decoy == null || !decoy.IsValid)return HookResult.Continue;

        decoy.Remove();
        Helper.CreateSnowEffect(decoy);
        Helper.CreateSoundEffectOnWorld(decoy);

        return HookResult.Continue;
    }

    public HookResult OnEntityOutput_OnStartTouch(CEntityIOOutput output, string name, CEntityInstance activator, CEntityInstance caller, CVariant value, float delay)
    {
        if (activator == null || !activator.IsValid || activator.DesignerName != "player") return HookResult.Continue;

        var pawn = activator.As<CCSPlayerPawn>();
        if (pawn == null || !pawn.IsValid) return HookResult.Continue;

        var OriginalController = pawn.OriginalController?.Get();
        if (OriginalController == null || !OriginalController.IsValid(true)) return HookResult.Continue;

        var player = OriginalController.CheckPlayerController();
        if (!player.IsValid(true)) return HookResult.Continue;
        if (!g_Main.Player_Data.TryGetValue(player.Slot, out var playerData)) return HookResult.Continue;
        
        var decoys = player.GetPlayerDecoyAmmo();

        var Max_Server_Decoy = ConVar.Find("ammo_grenade_limit_total")?.GetPrimitiveValue<int>();
        if(decoys >= Max_Server_Decoy)
        {
            Helper.AdvancedPlayerPrintToChat(player, null!, Localizer["PrintToChatToPlayer.SnowBall.Refill.Max"], Max_Server_Decoy);
            return HookResult.Continue;
        }
        
        if(decoys >= Configs.Instance.Props_Mode2And3_Max)
        {
            Helper.AdvancedPlayerPrintToChat(player, null!, Localizer["PrintToChatToPlayer.SnowBall.Refill.Max"], Configs.Instance.Props_Mode2And3_Max);
            return HookResult.Continue;
        }

        if (Configs.Instance.Immunity_From_Cooldown_Props_Mode2And3_Refill.HasValidPermissionData() && !Helper.IsPlayerInGroupPermission(player, Configs.Instance.Immunity_From_Cooldown_Props_Mode2And3_Refill))
        {
            if (DateTime.Now < playerData.Refill_CoolDown)
            {
                var remainingCooldown = (int)(playerData.Refill_CoolDown - DateTime.Now).TotalSeconds;
                Helper.AdvancedPlayerPrintToChat(player, null!, Localizer["PrintToChatToPlayer.SnowBall.Refill.CoolDown"], remainingCooldown);
                return HookResult.Continue;
            }
        }
        
        var PlayerPawn = player.PlayerPawn;
        if (PlayerPawn == null || !PlayerPawn.IsValid) return HookResult.Continue;

        var PlayerPawnValue = PlayerPawn.Value;
        if (PlayerPawnValue == null || !PlayerPawnValue.IsValid) return HookResult.Continue;

        var WeaponServices = PlayerPawnValue.WeaponServices;
        if (WeaponServices == null) return HookResult.Continue;

        var Ammo = WeaponServices.Ammo;
        if (Ammo == null) return HookResult.Continue;
        
        bool IsDecoy = Ammo[17] > 0;
        if (!IsDecoy)
        {
            player.GiveNamedItem(CsItem.DecoyGrenade);
        }else
        {
            Ammo[17] += 1;
            Utilities.SetStateChanged(PlayerPawnValue, "CBasePlayerPawn", "m_pWeaponServices");
        }

        Helper.AdvancedPlayerPrintToChat(player, null!, Localizer["PrintToChatToPlayer.SnowBall.Refill"]);
        playerData.Refill_CoolDown = DateTime.Now.AddSeconds(Configs.Instance.Props_Mode2And3_Refill_CoolDown);

        return HookResult.Continue;
    }

    public HookResult OnEventPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        if (@event == null) return HookResult.Continue;
        Helper.StartTimer();

        var victim = @event.Userid;
        if (!victim.IsValid(true)) return HookResult.Continue;
        Helper.CheckPlayerInGlobals(victim);

        Helper.RemoveSnowFallToPlayer(victim);

        return HookResult.Continue;
    }

    public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        if (@event == null) return HookResult.Continue;

        var player = @event.Userid;
        if (!player.IsValid()) return HookResult.Continue;

        Helper.RemoveSnowFallToPlayer(player);

        if (Configs.Instance.MySql_Enable == 1 || Configs.Instance.Cookies_Enable == 1)
        {
            _ = HandlePlayerDisconnectAsync(player);
        }

        return HookResult.Continue;
    }

    public async Task HandlePlayerDisconnectAsync(CCSPlayerController player)
    {
        try
        {
            if (!player.IsValid()) return;
            await Helper.SavePlayerDataOnDisconnect(player);
        }
        catch (Exception ex)
        {
            Helper.DebugMessage($"HandlePlayerDisconnectAsync error: {ex.Message}");
        }
    }


    public HookResult OnPlayerSay(CCSPlayerController? player, CommandInfo info)
    {
        if (!player.IsValid()) return HookResult.Continue;

        var eventmessage = info.ArgString;
        eventmessage = eventmessage.TrimStart('"');
        eventmessage = eventmessage.TrimEnd('"');
        if (string.IsNullOrWhiteSpace(eventmessage)) return HookResult.Continue;

        string message = eventmessage.Trim();

        Game_UserMessages.HookPlayerChat_UserMessages(null, player, message);

        return HookResult.Continue;
    }
    public HookResult OnPlayerSay_Team(CCSPlayerController? player, CommandInfo info)
    {
        if (!player.IsValid()) return HookResult.Continue;

        var eventmessage = info.ArgString;
        eventmessage = eventmessage.TrimStart('"');
        eventmessage = eventmessage.TrimEnd('"');
        if (string.IsNullOrWhiteSpace(eventmessage)) return HookResult.Continue;

        string message = eventmessage.Trim();

        Game_UserMessages.HookPlayerChat_UserMessages(null, player, message);

        return HookResult.Continue;
    }
    public HookResult OnUserMessage_OnSayText2(CounterStrikeSharp.API.Modules.UserMessages.UserMessage um)
    {
        var entityindex = um.ReadInt("entityindex");
        var player = Utilities.GetPlayerFromIndex(entityindex);
        if (!player.IsValid()) return HookResult.Continue;

        var eventmessage_Bytes = um.ReadBytes("param2");
        var eventmessage = Encoding.UTF8.GetString(eventmessage_Bytes);
        if (string.IsNullOrWhiteSpace(eventmessage)) return HookResult.Continue;

        string message = eventmessage.Trim();
        Game_UserMessages.HookPlayerChat_UserMessages(um, player, message);

        return HookResult.Continue;
    }

    public void OnMapEnd()
    {
        try
        {
            if (Configs.Instance.MySql_Enable > 0 || Configs.Instance.Cookies_Enable > 0)
            {
                Helper.SavePlayersValues();
            }

            Helper.ClearVariables();

            if (g_Main.OnTakeDamage_Hooked)
            {
                VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Unhook(Game_Hook.OnTakeDamage, HookMode.Pre);
                g_Main.OnTakeDamage_Hooked = false;
            }
        }
        catch (Exception ex)
        {
            Helper.DebugMessage($"OnMapEnd Error: {ex.Message}", true);
        }
    }

    public override void Unload(bool hotReload)
    {
        try
        {
            Helper.RemoveAllPlayersSnowFall();
            Helper.RemoveAllProps();
            Helper.RemoveRegisterCommandsAndHooks();
            Helper.ClearVariables(true);

        }
        catch (Exception ex)
        {
            Helper.DebugMessage($"Unload Error: {ex.Message}", true);
        }

        if (hotReload)
        {
            try
            {
                Helper.RemoveAllPlayersSnowFall();
                Helper.RemoveAllProps();
                Helper.RemoveRegisterCommandsAndHooks();
                Helper.ClearVariables(true);
            }
            catch (Exception ex)
            {
                Helper.DebugMessage($"Unload hotReload Error: {ex.Message}", true);
            }
        }
    }

    [ConsoleCommand("gkz_download", "Force Download Addons")]
    [ConsoleCommand("gkz_forcedownload", "Force Download Addons")]
    [RequiresPermissions("@css/root")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void ForceUpdate(CCSPlayerController? player, CommandInfo commandInfo)
    {
        string command = commandInfo.GetArg(1);
        if(string.IsNullOrEmpty(command))
        {
            commandInfo.ReplyToCommand("Missing <WorkShopID>");
            return;
        }

        AddTimer(1.0f, () =>
        {
            Server.ExecuteCommand("mm_remove_addon " + command);
            AddTimer(0.5f, () =>
            {
                Server.ExecuteCommand("mm_add_addon " + command);
                AddTimer(0.10f, () =>
                {
                    Server.ExecuteCommand("mm_download_addon " + command);
                }, TimerFlags.STOP_ON_MAPCHANGE);
            }, TimerFlags.STOP_ON_MAPCHANGE);
        }, TimerFlags.STOP_ON_MAPCHANGE);
    }

}