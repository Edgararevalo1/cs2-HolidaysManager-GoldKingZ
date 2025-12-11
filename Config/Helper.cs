using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using HolidaysManager_GoldKingZ.Config;
using CounterStrikeSharp.API.Modules.Memory;
using System.Drawing;
using System.Security.Cryptography;
using System.Globalization;
using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Exceptions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Entities.Constants;


namespace HolidaysManager_GoldKingZ;

public class Helper
{
    public static void DebugMessage(string message, bool important = false)
    {
        if (!Configs.Instance.EnableDebug && !important) return;

        Console.ForegroundColor = ConsoleColor.Magenta;
        string output = $"[HolidaysManager]: {message}";
        Console.WriteLine(output);

        Console.ResetColor();
    }

    public static void CreateResource(string jsonFilePath)
    {
        string[] forcedHeaders =
        {
            "////// vvvvvv Add Paths For Precache Resources Down vvvvvvvvvv //////",
            "// soundevents/goldkingz_sounds.vsndevts",
            "// particles/goldkingz/snowing.vpcf",
            "// --------------vvvvvvvvv--------------------",
        };

        string[] optionalHeaders =
        {
            "scripts/weapons.vdata",
            "soundevents/game_sounds_player.vsndevts",
            "models/goldkingz/snowball/snowball.vmdl",
            "models/goldkingz/snowball/ag2/snowball_ag2.vmdl",
            "panorama/images/icons/equipment/decoy.svg",
            "particles/goldkingz/snow_impact/snow_impact.vpcf",
            "particles/goldkingz/weapons/snowball_trail/snowball_trail.vpcf",
            "particles/goldkingz/weapons/snowball_impact/snowball_impact.vpcf"
        };
            

        if (!File.Exists(jsonFilePath))
        {
            File.WriteAllLines(jsonFilePath, forcedHeaders.Concat(optionalHeaders));
            return;
        }

        var lines = File.ReadAllLines(jsonFilePath).ToList();
        bool needsRewrite = false;

        for (int i = 0; i < forcedHeaders.Length; i++)
        {
            if (lines.Count <= i || lines[i] != forcedHeaders[i])
            {
                needsRewrite = true;
                break;
            }
        }

        if (needsRewrite)
        {
            for (int i = 0; i < Math.Min(forcedHeaders.Length, lines.Count); i++)
            {
                if (forcedHeaders.Contains(lines[i]))
                {
                    lines.RemoveAt(i);
                    i--;
                }
            }

            lines.InsertRange(0, forcedHeaders);
        }
        File.WriteAllLines(jsonFilePath, lines);
    }

    public static void RegisterCssCommands(string[]? commands, string description, CommandInfo.CommandCallback callback)
    {
        if (commands == null || commands.Length == 0) return;

        foreach (var cmd in commands)
        {
            if (string.IsNullOrEmpty(cmd)) continue;
            MainPlugin.Instance.AddCommand(cmd, description, callback);
        }
    }


    public static void RemoveCssCommands(string[]? commands, CommandInfo.CommandCallback callback)
    {
        if (commands == null || commands.Length == 0) return;

        foreach (var cmd in commands)
        {
            if (string.IsNullOrEmpty(cmd)) continue;
            MainPlugin.Instance.RemoveCommand(cmd, callback);
        }
    }

    public static void AdvancedPlayerPrintToChat(CCSPlayerController player, CounterStrikeSharp.API.Modules.Commands.CommandInfo commandInfo, string message, params object[] args)
    {
        if (string.IsNullOrEmpty(message)) return;

        for (int i = 0; i < args.Length; i++)
        {
            message = message.Replace($"{{{i}}}", args[i]?.ToString() ?? "");
        }

        if (Regex.IsMatch(message, "{nextline}", RegexOptions.IgnoreCase))
        {
            string[] parts = Regex.Split(message, "{nextline}", RegexOptions.IgnoreCase);
            foreach (string part in parts)
            {
                string trimmedPart = part.Trim();
                trimmedPart = trimmedPart.ReplaceColorTags();
                if (!string.IsNullOrEmpty(trimmedPart))
                {
                    if (commandInfo != null && commandInfo.CallingContext == CounterStrikeSharp.API.Modules.Commands.CommandCallingContext.Console)
                    {
                        player.PrintToConsole(" " + trimmedPart);
                    }
                    else
                    {
                        player.PrintToChat(" " + trimmedPart);
                    }
                }
            }
        }
        else
        {
            message = message.ReplaceColorTags();
            if (commandInfo != null && commandInfo.CallingContext == CounterStrikeSharp.API.Modules.Commands.CommandCallingContext.Console)
            {
                player.PrintToConsole(message);
            }
            else
            {
                player.PrintToChat(message);
            }
        }
    }
    public static void AdvancedPlayerPrintToConsole(CCSPlayerController player, string message, params object[] args)
    {
        if (string.IsNullOrEmpty(message)) return;

        for (int i = 0; i < args.Length; i++)
        {
            message = message.Replace($"{{{i}}}", args[i].ToString());
        }
        if (Regex.IsMatch(message, "{nextline}", RegexOptions.IgnoreCase))
        {
            string[] parts = Regex.Split(message, "{nextline}", RegexOptions.IgnoreCase);
            foreach (string part in parts)
            {
                string trimmedPart = part.Trim();
                trimmedPart = trimmedPart.ReplaceColorTags();
                if (!string.IsNullOrEmpty(trimmedPart))
                {
                    player.PrintToConsole(" " + trimmedPart);
                }
            }
        }
        else
        {
            message = message.ReplaceColorTags();
            player.PrintToConsole(message);
        }
    }
    public static void AdvancedServerPrintToChatAll(string message, params object[] args)
    {
        if (string.IsNullOrEmpty(message)) return;

        for (int i = 0; i < args.Length; i++)
        {
            message = message.Replace($"{{{i}}}", args[i].ToString());
        }
        if (Regex.IsMatch(message, "{nextline}", RegexOptions.IgnoreCase))
        {
            string[] parts = Regex.Split(message, "{nextline}", RegexOptions.IgnoreCase);
            foreach (string part in parts)
            {
                string trimmedPart = part.Trim();
                trimmedPart = trimmedPart.ReplaceColorTags();
                if (!string.IsNullOrEmpty(trimmedPart))
                {
                    Server.PrintToChatAll(" " + trimmedPart);
                }
            }
        }
        else
        {
            message = message.ReplaceColorTags();
            Server.PrintToChatAll(message);
        }
    }
    
    public static bool IsPlayerInGroupPermission(CCSPlayerController player, string groups)
    {
        if (string.IsNullOrEmpty(groups) || player == null || !player.IsValid)
            return false;

        return groups.Split('|')
            .Select(segment => segment.Trim())
            .Any(trimmedSegment => Permission_CheckPermissionSegment(player, trimmedSegment));
    }

    private static bool Permission_CheckPermissionSegment(CCSPlayerController player, string segment)
    {
        if (string.IsNullOrEmpty(segment)) return false;

        int colonIndex = segment.IndexOf(':');
        if (colonIndex == -1 || colonIndex == 0) return false;

        string prefix = segment.Substring(0, colonIndex).Trim().ToLower();
        string values = segment.Substring(colonIndex + 1).Trim();

        return prefix switch
        {
            "steamid" or "steamids" or "steam" or "steams" => Permission_CheckSteamIds(player, values),
            "flag" or "flags" => Permission_CheckFlags(player, values),
            "group" or "groups" => Permission_CheckGroups(player, values),
            _ => false
        };
    }

    private static bool Permission_CheckSteamIds(CCSPlayerController player, string steamIds)
    {
        if (string.IsNullOrEmpty(steamIds)) return false;

        steamIds = steamIds.Replace("[", "").Replace("]", "");

        var (steam2, steam3, steam32, steam64) = player.SteamID.GetPlayerSteamID();
        var steam3NoBrackets = steam3.Trim('[', ']');

        return steamIds
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(id => id.Trim())
            .Any(trimmedId =>
                string.Equals(trimmedId, steam2, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(trimmedId, steam3NoBrackets, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(trimmedId, steam32, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(trimmedId, steam64, StringComparison.OrdinalIgnoreCase)
            );
    }

    private static bool Permission_CheckFlags(CCSPlayerController player, string flags)
    {
        if (player == null || !player.IsValid ||
            player.Connected != PlayerConnectedState.PlayerConnected ||
            player.IsBot || player.IsHLTV)
            return false;

        if (string.IsNullOrEmpty(flags))
            return false;

        var playerData = AdminManager.GetPlayerAdminData(player);
        if (playerData == null)
            return false;

        var requiredFlags = flags
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(f => f.Trim())
            .ToList();

        if (playerData._flags != null &&
            requiredFlags.Any(reqFlag =>
                playerData._flags.Contains(reqFlag, StringComparer.OrdinalIgnoreCase)))
            return true;

        var allFlags = playerData.GetAllFlags();
        return allFlags != null &&
            requiredFlags.Any(reqFlag =>
                allFlags.Contains(reqFlag, StringComparer.OrdinalIgnoreCase));
    }

    private static bool Permission_CheckGroups(CCSPlayerController player, string groups)
    {
        if (player == null || !player.IsValid ||
            player.Connected != PlayerConnectedState.PlayerConnected ||
            player.IsBot || player.IsHLTV)
            return false;

        if (string.IsNullOrEmpty(groups))
            return false;

        var playerData = AdminManager.GetPlayerAdminData(player);
        if (playerData == null || playerData.Groups == null || !playerData.Groups.Any())
            return false;

        return groups
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(g => g.Trim())
            .Any(reqGroup => playerData.Groups.Contains(reqGroup, StringComparer.OrdinalIgnoreCase));
    }

    public static List<CCSPlayerController> GetPlayersController(bool IncludeBots = false, bool IncludeHLTV = false, bool IncludeNone = true, bool IncludeSPEC = true, bool IncludeCT = true, bool IncludeT = true)
    {
        return Utilities
            .FindAllEntitiesByDesignerName<CCSPlayerController>("cs_player_controller")
            .Where(p =>
                p != null &&
                p.IsValid &&
                p.Connected == PlayerConnectedState.PlayerConnected &&
                (IncludeBots || !p.IsBot) &&
                (IncludeHLTV || !p.IsHLTV) &&
                ((IncludeCT && p.TeamNum == (byte)CsTeam.CounterTerrorist) ||
                (IncludeT && p.TeamNum == (byte)CsTeam.Terrorist) ||
                (IncludeNone && p.TeamNum == (byte)CsTeam.None) ||
                (IncludeSPEC && p.TeamNum == (byte)CsTeam.Spectator)))
            .ToList();
    }
    public static int GetPlayersCount(bool IncludeBots = false, bool IncludeHLTV = false, bool IncludeSPEC = true, bool IncludeCT = true, bool IncludeT = true)
    {
        return Utilities.GetPlayers().Count(p =>
            p != null &&
            p.IsValid &&
            p.Connected == PlayerConnectedState.PlayerConnected &&
            (IncludeBots || !p.IsBot) &&
            (IncludeHLTV || !p.IsHLTV) &&
            ((IncludeCT && p.TeamNum == (byte)CsTeam.CounterTerrorist) ||
            (IncludeT && p.TeamNum == (byte)CsTeam.Terrorist) ||
            (IncludeSPEC && p.TeamNum == (byte)CsTeam.Spectator))
        );
    }

    public static void ClearVariables(bool clear_data = false)
    {
        var g_Main = MainPlugin.Instance.g_Main;

        g_Main.Clear(clear_data);
    }


    public static void ChangeSkyBox(CEntityInstance entity = null!, bool reload = false)
    {
        if(string.IsNullOrEmpty(Configs.Instance.SkyBox_Path))return;

        if(reload)
        {
            Server.NextFrame(() =>
            {
                ChangeSkyBox_Start();
            });
            return;
        }
        if(entity == null || !entity.IsValid)return;

        if(!Configs.Instance.SkyBox_Path.IsMounted())
        {
            DebugMessage($"SkyBox Will Not Change Because [{Configs.Instance.SkyBox_Path}] Is Not Mounted Into MultiAddonManager.cfg", true);
            return;
        }

        var g_Main = MainPlugin.Instance.g_Main;

        if (entity.DesignerName == "env_cubemap_fog")
        {
            entity.Remove();
        }

        if (entity.DesignerName == "sky_camera")
        {
            entity.Remove();
        }
        
        if (entity.DesignerName == "env_sky")
        {
            if(entity.Entity?.Name == "Custom_SkyBox")return;

            entity.Remove();

            Server.NextFrame(() =>
            {
                ChangeSkyBox_Start();
            });
        }
    }

    public unsafe static void ChangeSkyBox_Start()
    {
        try
        {
            if(!Configs.Instance.SkyBox_Path.IsMounted())
            {
                DebugMessage($"SkyBox Will Not Change Because [{Configs.Instance.SkyBox_Path}] Is Not Mounted Into MultiAddonManager.cfg", true);
                return;
            }

            var g_Main = MainPlugin.Instance.g_Main;
            if(g_Main.Server_SkyBox != null && g_Main.Server_SkyBox.IsValid)return;

            var Env_Sky = Utilities.CreateEntityByName<CEnvSky>("env_sky");
            if(Env_Sky == null)return;
            
            var materialPtr = MainPlugin.Instance.Game_Hook.FindMaterialByPath(Configs.Instance.SkyBox_Path);
            if (materialPtr == IntPtr.Zero)return;
        
            Unsafe.Write((void*)Env_Sky.SkyMaterial.Handle, materialPtr);
            Unsafe.Write((void*)Env_Sky.SkyMaterialLightingOnly.Handle, materialPtr);  
            
            Env_Sky.TintColor = Configs.Instance.SkyBox_Color.ToColor();
            if(Env_Sky.Entity != null)Env_Sky.Entity.Name = "Custom_SkyBox";
            Env_Sky.BrightnessScale = Configs.Instance.SkyBox_Brightness;
            Env_Sky.DispatchSpawn();
            var rotation = Configs.Instance.SkyBox_Rotation_XYZ.ToQAngle();
            Env_Sky.Teleport(null, rotation);
            g_Main.Server_SkyBox = Env_Sky;
        }
        catch (Exception ex)
        {
            DebugMessage($"ChangeSkyBox_Start Error {ex.Message}");
        }
    }
    public static void StartEditMode(CCSPlayerController player)
    {
        if(!player.IsValid() || MainPlugin.Instance.g_Main.Server_EditMode)return;

        MainPlugin.Instance.g_Main.Server_EditMode = true;
        MainPlugin.Instance.g_Main.Server_EditMode_Player = player;
        MainPlugin.Instance.RegisterListener<Listeners.OnTick>(MainPlugin.Instance.OnTick);

        Server.ExecuteCommand("mp_warmup_end");
        Server.ExecuteCommand("mp_warmup_pausetimer 1");
        Server.ExecuteCommand("mp_warmup_start");

        AdvancedPlayerPrintToChat(player, null!, MainPlugin.Instance.Localizer["PrintToChatToPlayer.PropsEditMode.Started"]);
        AdvancedPlayerPrintToConsole(player, MainPlugin.Instance.Localizer["PrintToConsoleToPlayer.PropsEditMode.Info"]);

        MainPlugin.Instance.AddTimer(2.0f, ()=>
        {
            if(MainPlugin.Instance.g_Main.Server_EditMode_Player != null && MainPlugin.Instance.g_Main.Server_EditMode_Player.IsValid)
            {
                AdvancedServerPrintToChatAll(MainPlugin.Instance.Localizer["PrintToChatToAll.PropsEditMode.Started"], MainPlugin.Instance.g_Main.Server_EditMode_Player.PlayerName);
            }
        }, TimerFlags.STOP_ON_MAPCHANGE);
        
    }

    public static void CloseEditMode()
    {
        MainPlugin.Instance.RemoveListener<Listeners.OnTick>(MainPlugin.Instance.OnTick);

        Server.ExecuteCommand("mp_warmup_pausetimer 0");
        Server.ExecuteCommand("mp_warmup_end");

        MainPlugin.Instance.g_Main.Server_EditMode_Player = null!;
        MainPlugin.Instance.g_Main.Server_EditMode = false;
        
    }
    public static void SpawnPropFrontPlayer(string modelpath, CCSPlayerController player, int prop_type)
    {
        var g_Main = MainPlugin.Instance.g_Main;
        if(string.IsNullOrEmpty(modelpath) || player == null || !player.IsValid)return;

        if(g_Main.Server_Prop_Attached != null && g_Main.Server_Prop_Attached.IsValid)
        {
            g_Main.Server_Prop_Attached.Remove();
        }


        var PlayerPawn = player.PlayerPawn;
        if(PlayerPawn == null || !PlayerPawn.IsValid)return;

        var pawn = PlayerPawn.Value;
        if(pawn == null || !pawn.IsValid)return;

        var (position, angle) = player.GetFrontPosition();
        if(position == null || angle == null)return;

        var SpawnProp = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic");
        if (SpawnProp == null)return;
        
        SpawnProp.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags &= ~(1u << 2);
        if(prop_type == 0 || prop_type == 2)
        {
            SpawnProp.AcceptInput("DisableCollision", SpawnProp, SpawnProp);
        }else if(prop_type == 1 || prop_type == 3)
        {
            SpawnProp.MoveType = MoveType_t.MOVETYPE_VPHYSICS;
            SpawnProp.Collision.SolidType = SolidType_t.SOLID_VPHYSICS;
            SpawnProp.AcceptInput("EnableCollision", SpawnProp, SpawnProp);
        }
        
        SpawnProp.SetModel(modelpath);
        SpawnProp.DispatchSpawn();
        var rnd = new Random();
        int id = rnd.Next(1, 100000);
        if(SpawnProp.Entity != null)SpawnProp.Entity.Name = $"Xmass_Prop_{prop_type}_{id}";
        SpawnProp.Teleport(position, angle, null);

        g_Main.Server_Prop_Attached = SpawnProp;
        g_Main.ForwardBack = 0.0f;
        g_Main.RightLeft = 0.0f;
        g_Main.UpDown = 0.0f;
        g_Main.RotationOffset = new QAngle(0.0f, 0.0f, 0.0f);

        UpdatePropPosition(player);

        string modelName = modelpath.ExtractModelNameFromPath();
        AdvancedPlayerPrintToConsole(player, MainPlugin.Instance.Localizer["PrintToConsoleToPlayer.PropsEditMode.Spawned"], modelName ?? "");
        
    }

    private static void AttachHighlightedProp(CCSPlayerController player)
    {
        var g_Main = MainPlugin.Instance.g_Main;
        if(g_Main.Server_Highlighted_Prop == null || !g_Main.Server_Highlighted_Prop.IsValid)return;
        
        SetPropColor(g_Main.Server_Highlighted_Prop, 255, 255, 255);
        g_Main.Server_Highlighted_Prop.AcceptInput("DisableCollision", g_Main.Server_Highlighted_Prop, g_Main.Server_Highlighted_Prop);
        
        for(int i = 0; i < g_Main.DroppedProps_Data.Count; i++)
        {
            if(g_Main.DroppedProps_Data[i].PropEntity == g_Main.Server_Highlighted_Prop)
            {
                g_Main.DroppedProps_Data.RemoveAt(i);
                break;
            }
        }
        
        var (position, angle) = player.GetFrontPosition();
        
        if(position != null && angle != null)
        {
            g_Main.Server_Prop_Attached = g_Main.Server_Highlighted_Prop;
            g_Main.Server_Highlighted_Prop = null!;
            
            g_Main.RightLeft = 0.0f;
            g_Main.ForwardBack = 0.0f;
            g_Main.UpDown = 0.0f;
            g_Main.RotationOffset = new QAngle(0, 0, 0);
            
            g_Main.Server_Prop_Attached.Teleport(position, angle, null);
            
        }
        else
        {
            ClearPropHighlight();
        }
    }

    private static void RemoveHighlightedProp(CCSPlayerController player)
    {
        var g_Main = MainPlugin.Instance.g_Main;
        if(g_Main.Server_Highlighted_Prop == null || !g_Main.Server_Highlighted_Prop.IsValid) return;

        for(int i = 0; i < g_Main.DroppedProps_Data.Count; i++)
        {
            if(g_Main.DroppedProps_Data[i].PropEntity == g_Main.Server_Highlighted_Prop)
            {
                g_Main.DroppedProps_Data.RemoveAt(i);
                break;
            }
        }

        var prop = g_Main.Server_Highlighted_Prop;
        if(prop == null || !prop.IsValid) return;

        var prop_path = prop.CBodyComponent?.SceneNode?.GetSkeletonInstance().ModelState.ModelName ?? "";
        if(string.IsNullOrEmpty(prop_path))return;

        prop.Remove();

        g_Main.Server_Highlighted_Prop = null!;
        g_Main.RightLeft = 0.0f;
        g_Main.ForwardBack = 0.0f;
        g_Main.UpDown = 0.0f;
        g_Main.RotationOffset = new QAngle(0, 0, 0);
        
        string modelName = prop_path.ExtractModelNameFromPath();
        AdvancedPlayerPrintToChat(player, null!, MainPlugin.Instance.Localizer["PrintToChatToPlayer.PropsEditMode.Removed"], modelName ?? "");
    }

    public static void DropAttachedProp(CCSPlayerController player)
    {
        if(player == null || !player.IsValid) return;

        var g_Main = MainPlugin.Instance.g_Main;
        if(g_Main.Server_Prop_Attached == null || !g_Main.Server_Prop_Attached.IsValid)return;

        var prop = g_Main.Server_Prop_Attached;
        if(prop == null || !prop.IsValid) return;

        var prop_path = prop.CBodyComponent?.SceneNode?.GetSkeletonInstance().ModelState.ModelName ?? "";
        if(string.IsNullOrEmpty(prop_path))return;

        int prop_type = prop.Entity?.Name.ExtractModeFromName() ?? 0;
        var position = prop.AbsOrigin;
        var rotation = prop.AbsRotation;
        
        if(position == null || rotation == null) return;

        Globals.DroppedPropInfo droppedProp = new Globals.DroppedPropInfo
        {
            ModelPath = prop_path,
            ModelType = prop_type,
            Position = new Vector(position.X, position.Y, position.Z),
            Rotation = new QAngle(rotation.X, rotation.Y, rotation.Z),
            PropEntity = prop
        };

        g_Main.DroppedProps_Data.Add(droppedProp);

        prop.Teleport(position, rotation, new Vector(0, 0, 0));

        if(prop_type == 0 || prop_type == 2)
        {
            prop.AcceptInput("DisableCollision", prop, prop);
            if(prop_type == 2)
            {
                CreateTriggerMultipleOnProp(prop);
            }
        }else if(prop_type == 1 || prop_type == 3)
        {
            prop.MoveType = MoveType_t.MOVETYPE_VPHYSICS;
            prop.Collision.SolidType = SolidType_t.SOLID_VPHYSICS;
            prop.AcceptInput("EnableCollision", prop, prop);

            if(prop_type == 3)
            {
                CreateTriggerMultipleOnProp(prop);
            }
        }


        g_Main.Server_Prop_Attached = null!;
        g_Main.RightLeft = 0.0f;
        g_Main.ForwardBack = 0.0f;
        g_Main.UpDown = 0.0f;
        g_Main.RotationOffset = new QAngle(0, 0, 0);
        
        string modelName = prop_path.ExtractModelNameFromPath();
        AdvancedPlayerPrintToChat(player, null!, MainPlugin.Instance.Localizer["PrintToChatToPlayer.PropsEditMode.Dropped"], modelName ?? "");
    }

    public static void SpawnPropsFromData()
    {
        var g_Main = MainPlugin.Instance.g_Main;

        foreach(var getprops in g_Main.DroppedProps_Data)
        {
            if(getprops == null)continue;
            
            var prop = getprops.PropEntity;
            if(prop != null && prop.IsValid)
            {
                prop.Remove();
            }

            var prop_path = getprops.ModelPath;
            var prop_type = getprops.ModelType;
            var prop_postion = getprops.Position;
            var prop_rotation = getprops.Rotation;

            if(string.IsNullOrEmpty(prop_path) || prop_postion == null || prop_rotation == null)continue;

            var SpawnProp = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic");
            if (SpawnProp == null)return;

            SpawnProp.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags &= ~(1u << 2);

            if(prop_type == 1 || prop_type == 3)
            {
                SpawnProp.MoveType = MoveType_t.MOVETYPE_VPHYSICS;
                SpawnProp.Collision.SolidType = SolidType_t.SOLID_VPHYSICS;
            }
            SpawnProp.SetModel(prop_path);
            SpawnProp.DispatchSpawn();

            if(prop_type == 0 || prop_type == 2)
            {
                SpawnProp.AcceptInput("DisableCollision", SpawnProp, SpawnProp);
                if(prop_type == 2)
                {
                    CreateTriggerMultipleOnProp(SpawnProp);
                }
            }
            else if(prop_type == 1 || prop_type == 3)
            {
                SpawnProp.AcceptInput("EnableCollision", SpawnProp, SpawnProp);
                if(prop_type == 3)
                {
                    CreateTriggerMultipleOnProp(SpawnProp);
                }
            }
            
            
            var rnd = new Random();
            int id = rnd.Next(1, 100000);
            if(SpawnProp.Entity != null)SpawnProp.Entity.Name = $"Xmass_Prop_{prop_type}_{id}";
            SpawnProp.Teleport(prop_postion, prop_rotation, null);

            getprops.PropEntity = SpawnProp;
        }
    }

    private static void CreateTriggerMultipleOnProp(CBaseModelEntity prop)
    {
        if(prop == null || !prop.IsValid)return;

        int prop_type = prop.Entity?.Name.ExtractModeFromName() ?? 0;
        var trigger = Utilities.CreateEntityByName<CTriggerMultiple>("trigger_multiple");
        if (trigger == null)return;

        trigger.Spawnflags = 1;
        trigger.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags &= ~(uint)(1 << 2);
        trigger.Collision.SolidType = SolidType_t.SOLID_VPHYSICS;
        trigger.Collision.SolidFlags = 0;
        trigger.Collision.CollisionGroup = 14;
        trigger.SetModel(prop.CBodyComponent!.SceneNode!.GetSkeletonInstance().ModelState.ModelName);
        trigger.DispatchSpawn();

        var rnd = new Random();
        int id = rnd.Next(1, 100000);
        if(trigger.Entity != null)trigger.Entity.Name = $"Xmass_TriggerProp_{prop_type}_{id}";

        trigger.Teleport(prop.AbsOrigin, prop.AbsRotation);
        trigger.AcceptInput("SetParent", prop, trigger, "!activator");
        trigger.AcceptInput("Enable");
         
    }

    public static void UpdatePropPosition(CCSPlayerController player)
    {
        var g_Main = MainPlugin.Instance.g_Main;
        if(player == null || !player.IsValid || g_Main.Server_Prop_Attached == null || !g_Main.Server_Prop_Attached.IsValid)return;

        var PlayerPawn = player.PlayerPawn;
        if(PlayerPawn == null || !PlayerPawn.IsValid)return;

        var pawn = PlayerPawn.Value;
        if(pawn == null || !pawn.IsValid)return;

        var V_angle = pawn.V_angle;
        var AbsOrigin = pawn.AbsOrigin;
        var ViewOffset = pawn.ViewOffset;
        if(V_angle == null || AbsOrigin == null || ViewOffset == null)return;


        Vector vecForward = new();
        Vector vecUp = new();
        Vector vecRight = new();


        NativeAPI.AngleVectors(V_angle.Handle, vecForward.Handle, vecRight.Handle, vecUp.Handle);
        Vector vecEyePos = new Vector(AbsOrigin.X + ViewOffset.X,AbsOrigin.Y + ViewOffset.Y,AbsOrigin.Z + ViewOffset.Z);

        var desiredPosition = vecEyePos + vecForward * (50.0f + g_Main.ForwardBack);
        desiredPosition += vecRight * g_Main.RightLeft;
        desiredPosition += vecUp * g_Main.UpDown;

        QAngle desiredRotation = new QAngle(V_angle.X + g_Main.RotationOffset.X, V_angle.Y + g_Main.RotationOffset.Y, V_angle.Z + g_Main.RotationOffset.Z);
        
        g_Main.Server_Prop_Attached.Teleport(desiredPosition, desiredRotation, null);
    }

    public static void CheckPropAndColor(CCSPlayerController player)
    {
        var g_Main = MainPlugin.Instance.g_Main;

        if(player == null || !player.IsValid) return;
        
        var buttons = player.Buttons;
        
        if (buttons.HasFlag(PlayerButtons.Inspect) && buttons.HasFlag(PlayerButtons.Attack))
        {            
            if(g_Main.Server_Highlighted_Prop != null && g_Main.Server_Highlighted_Prop.IsValid)
            {
                AttachHighlightedProp(player);
                return;
            }
            
            if(player.IsOnGround())
            {
                player.PlayerUnFreeze();
            }
        }
        else if (buttons.HasFlag(PlayerButtons.Inspect) && buttons.HasFlag(PlayerButtons.Attack2))
        {
            if(g_Main.Server_Highlighted_Prop != null && g_Main.Server_Highlighted_Prop.IsValid)
            {
                RemoveHighlightedProp(player);
                return;
            }

            if(player.IsOnGround())
            {
                player.PlayerUnFreeze();
            }
        }
        else if (buttons.HasFlag(PlayerButtons.Inspect))
        {
            HighlightClosestProp(player);
            
            if(player.IsOnGround())
            {
                player.PlayerUnFreeze();
            }
        }
        else if (buttons.HasFlag(PlayerButtons.Attack))
        {
            ClearPropHighlight();
            
            if(player.IsOnGround())
            {
                player.PlayerUnFreeze();
            }
        }
        else
        {
            ClearPropHighlight();
        }
    }

    public static void HandleAdjustmentInput(CCSPlayerController player)
    {
        var g_Main = MainPlugin.Instance.g_Main;
        var buttons = player.Buttons;
        
        if (buttons.HasFlag(PlayerButtons.Attack) && !buttons.HasFlag(PlayerButtons.Inspect))
        {
            if(g_Main.Server_Prop_Attached != null && g_Main.Server_Prop_Attached.IsValid)
            {
                DropAttachedProp(player);
            }
            
            if(player.IsOnGround())
            {
                player.PlayerUnFreeze();
            }
            return;
        }

        ProcessAdjustmentButtons(player, buttons);
    }

    private static void ProcessAdjustmentButtons(CCSPlayerController player, PlayerButtons buttons)
    {
        var g_Main = MainPlugin.Instance.g_Main;
        if (buttons.HasFlag(PlayerButtons.Speed))
        {
            if (buttons.HasFlag(PlayerButtons.Forward))
            {
                if(player.IsOnGround())
                {
                    player.PlayerFreeze();
                }
                g_Main.ForwardBack += Globals_Static.MoveSpeed;

            }
            else if (buttons.HasFlag(PlayerButtons.Back))
            {
                if(player.IsOnGround())
                {
                    player.PlayerFreeze();
                }

                g_Main.ForwardBack -= Globals_Static.MoveSpeed;

            }
            else if (buttons.HasFlag(PlayerButtons.Moveleft))
            {
                if(player.IsOnGround())
                {
                    player.PlayerFreeze();
                }

                g_Main.RightLeft -= Globals_Static.MoveSpeed;

            }
            else if (buttons.HasFlag(PlayerButtons.Moveright))
            {
                if(player.IsOnGround())
                {
                    player.PlayerFreeze();
                }

                g_Main.RightLeft += Globals_Static.MoveSpeed;

            }
            else if (buttons.HasFlag(PlayerButtons.Reload))
            {
                if(player.IsOnGround())
                {
                    player.PlayerFreeze();
                }

                g_Main.RotationOffset.Y += Globals_Static.RotateSpeed;
                if (g_Main.RotationOffset.Y >= 360) g_Main.RotationOffset.Y -= 360;
                if (g_Main.RotationOffset.Y < 0) g_Main.RotationOffset.Y += 360;
                
            }
        }
        else if (buttons.HasFlag(PlayerButtons.Duck))
        {
            if (buttons.HasFlag(PlayerButtons.Forward))
            {
                if(player.IsOnGround())
                {
                    player.PlayerFreeze();
                }

                g_Main.UpDown += Globals_Static.MoveSpeed;

            }
            else if (buttons.HasFlag(PlayerButtons.Back))
            {
                if(player.IsOnGround())
                {
                    player.PlayerFreeze();
                }
                
                g_Main.UpDown -= Globals_Static.MoveSpeed;

            }
            else if (buttons.HasFlag(PlayerButtons.Reload))
            {
                if(player.IsOnGround())
                {
                    player.PlayerFreeze();
                }

                g_Main.RotationOffset.Y += Globals_Static.RotateSpeed;
                if (g_Main.RotationOffset.Y >= 360) g_Main.RotationOffset.Y -= 360;
                if (g_Main.RotationOffset.Y < 0) g_Main.RotationOffset.Y += 360;
                
            }
        }
    }

    private static void HighlightClosestProp(CCSPlayerController player)
    {
        var g_Main = MainPlugin.Instance.g_Main;
        if(player == null || !player.IsValid) return;
        
        var PlayerPawn = player.PlayerPawn;
        if(PlayerPawn == null || !PlayerPawn.IsValid) return;
        
        var pawn = PlayerPawn.Value;
        if(pawn == null || !pawn.IsValid) return;
        
        var playerPos = pawn.AbsOrigin;
        if(playerPos == null) return;
        
        CDynamicProp closestProp = null!;
        float closestDistance = float.MaxValue;
        
        foreach(var droppedProp in g_Main.DroppedProps_Data)
        {
            if(droppedProp.PropEntity == null || !droppedProp.PropEntity.IsValid)
                continue;
                
            var prop = droppedProp.PropEntity as CDynamicProp;
            if(prop == null || !prop.IsValid)
                continue;
                
            var propPos = prop.AbsOrigin;
            if(propPos == null)
                continue;
                
            float dx = playerPos.X - propPos.X;
            float dy = playerPos.Y - propPos.Y;
            float dz = playerPos.Z - propPos.Z;
            float distance = (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
            
            if(distance < Globals_Static.HighlightDistance && distance < closestDistance)
            {
                closestDistance = distance;
                closestProp = prop;
            }
        }
        
        if(g_Main.Server_Highlighted_Prop != null && g_Main.Server_Highlighted_Prop.IsValid)
        {
            if(closestProp != null && closestProp != g_Main.Server_Highlighted_Prop)
            {
                SetPropColor(g_Main.Server_Highlighted_Prop, 255, 255, 255);
            }
        }
        
        if(closestProp != null && closestProp.IsValid)
        {
            g_Main.Server_Highlighted_Prop = closestProp;

            SetPropColor(closestProp, 255, 0, 0);
        }
        else if(g_Main.Server_Highlighted_Prop != null && g_Main.Server_Highlighted_Prop.IsValid)
        {
            ClearPropHighlight();
        }
    }

    public static void ClearPropHighlight()
    {
        SetPropColor(MainPlugin.Instance.g_Main.Server_Highlighted_Prop, 255, 255, 255);
        MainPlugin.Instance.g_Main.Server_Highlighted_Prop = null!;
    }
    
    
    public static void SetPropColor(CDynamicProp prop, int r, int g, int b)
    {
        if(prop == null || !prop.IsValid) return;
        
        prop.Render = Color.FromArgb(255, r, g, b);
        Utilities.SetStateChanged(prop, "CBaseModelEntity", "m_clrRender");
    }
    

    public static void RemoveSnowFallToPlayer(CCSPlayerController player)
    {
        var g_Main = MainPlugin.Instance.g_Main;
        if (!player.IsValid(true) || !g_Main.Player_Data.TryGetValue(player.Slot, out var handle)) return;

        if(handle.Snow_Fall != null  && handle.Snow_Fall.IsValid)
        {
            handle.Snow_Fall.Remove();
        }
    }
    public static void ApplySnowFallToPlayer(CCSPlayerController player)
    {
        var g_Main = MainPlugin.Instance.g_Main;
        if (!player.IsValid() || !g_Main.Player_Data.TryGetValue(player.Slot, out var handle) || handle.Snow_Fall != null  && handle.Snow_Fall.IsValid) return;

        var GetPlayerPawn = player.GetPlayerPawnLocated();
        if (GetPlayerPawn == null || !GetPlayerPawn.IsValid) return;

        var SpawnSnowFall = Utilities.CreateEntityByName<CParticleSystem>("info_particle_system");
        if (SpawnSnowFall == null) return;
        SpawnSnowFall.StartActive = true;
        SpawnSnowFall.EffectName = Configs.Instance.SnowFall_Path;
        SpawnSnowFall.DispatchSpawn();
        
        var postion = new Vector
        {
            X = GetPlayerPawn.AbsOrigin!.X,
            Y = GetPlayerPawn.AbsOrigin.Y,
            Z = GetPlayerPawn.AbsOrigin.Z + Configs.Instance.SnowFall_Z_Height
        };

        var angle = new QAngle
        {
            X = GetPlayerPawn.EyeAngles!.X,
            Y = GetPlayerPawn.EyeAngles.Y,
            Z = GetPlayerPawn.EyeAngles.Z
        };

        SpawnSnowFall.Teleport(postion, angle);
        SpawnSnowFall.AcceptInput("SetParent", GetPlayerPawn, null, "!activator");
        handle.Snow_Fall = SpawnSnowFall;
        handle.PlayerPawnLocated = GetPlayerPawn;
    }




    public static async Task DownloadMissingFilesAsync()
    {
        try
        {
            await DownloadMissingFiles();
            await Server.NextFrameAsync(() => CustomHooks.StartHook());
        }
        catch (Exception ex)
        {
            DebugMessage($"DownloadMissingFiles failed: {ex.Message}");
        }
    }

    public static async Task DownloadMissingFiles()
    {
        try
        {
            string gamedata = "gamedata/gamedata.json";
            string gamedata_GithubUrl = "https://raw.githubusercontent.com/oqyh/cs2-Private-Plugins/main/Resources/gamedata.json";
            await DownloadFromGitHub(gamedata, gamedata_GithubUrl, Configs.Instance.AutoUpdateSignatures);

            string geoFileName = Path.GetFullPath(Path.Combine(MainPlugin.Instance.ModuleDirectory, "..", "..", "shared/GoldKingZ/GeoLocation/GeoLite2-City.mmdb"));
            string geoUpdateUrl = "https://raw.githubusercontent.com/oqyh/cs2-Connect-Disconnect-Sound-GoldKingZ/main/Resources/update.txt";
            await DownloadFromGitHub(geoFileName, geoUpdateUrl, Configs.Instance.AutoUpdateGeoLocation);

        }
        catch (Exception ex)
        {
            DebugMessage($"DownloadMissingFiles Error: {ex.Message}");
        }
    }

    public static async Task DownloadFromGitHub(string filePath, string githubUrl, bool AutoUpdate = false)
    {
        try
        {
            string fullPath = Path.Combine(MainPlugin.Instance.ModuleDirectory, filePath);

            string? dir = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", $"CS2-Plugin-XmasSnow");
            client.Timeout = TimeSpan.FromSeconds(50);

            string actualDownloadUrl = githubUrl;

            if (githubUrl.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
            {
                actualDownloadUrl = await client.GetStringAsync(githubUrl);
                actualDownloadUrl = actualDownloadUrl.Trim();
            }

            byte[] remoteBytes = await client.GetByteArrayAsync(actualDownloadUrl);

            bool needDownload = !File.Exists(fullPath);

            if (!needDownload && AutoUpdate)
            {
                using var sha256 = SHA256.Create();
                string Hash(byte[] b) => BitConverter.ToString(sha256.ComputeHash(b)).Replace("-", "").ToLowerInvariant();

                byte[] localBytes = await File.ReadAllBytesAsync(fullPath);
                needDownload = Hash(localBytes) != Hash(remoteBytes);
            }

            if (needDownload)
            {
                await File.WriteAllBytesAsync(fullPath, remoteBytes);
            }
        }
        catch (Exception ex)
        {
            DebugMessage($"DownloadFromGitHub Error: {ex.Message}");
        }
    }
    public static void RemoveAllPlayersSnowFall()
    {
        var g_Main = MainPlugin.Instance.g_Main;

        foreach (var Player_Data in g_Main.Player_Data.Values)
        {
            var Particles = Player_Data.Snow_Fall;
            if(Particles == null || !Particles.IsValid)continue;

            Particles.Remove();
        }
    }

    public static void RemoveAllProps()
    {
        var g_Main = MainPlugin.Instance.g_Main;

        foreach (var Player_Data in g_Main.DroppedProps_Data)
        {
            var props = Player_Data.PropEntity;
            if(props == null || !props.IsValid)continue;

            props.Remove();
        }
    }

    public static void ReloadPlayersGlobals()
    {
        foreach (var players in GetPlayersController(false, false, false))
        {
            if (!players.IsValid()) continue;

            _ = MainPlugin.Instance.HandlePlayerConnectionsAsync(players);
        }
    }
    public static void MuteCommands(CounterStrikeSharp.API.Modules.UserMessages.UserMessage? um, int Config, bool Fully = false)
    {
        if (um == null) return;
        if (!Fully && Config == 2 || Fully && Config > 0)
        {
            um.Recipients.Clear();
        }
    }

    public static void RegisterCommandsAndHooks(bool Late_Hook = false)
    {
        Server.ExecuteCommand("sv_hibernate_when_empty false");

        MainPlugin.Instance.RegisterListener<Listeners.OnServerPrecacheResources>(MainPlugin.Instance.OnServerPrecacheResources);
        MainPlugin.Instance.RegisterListener<Listeners.OnMapStart>(MainPlugin.Instance.OnMapStart);
        MainPlugin.Instance.RegisterListener<Listeners.OnEntityCreated>(MainPlugin.Instance.OnEntityCreated);
        MainPlugin.Instance.RegisterListener<Listeners.OnEntitySpawned>(MainPlugin.Instance.OnEntitySpawned);
        MainPlugin.Instance.RegisterListener<Listeners.OnMapEnd>(MainPlugin.Instance.OnMapEnd);
        MainPlugin.Instance.RegisterListener<Listeners.CheckTransmit>(MainPlugin.Instance.OnCheckTransmit);

        MainPlugin.Instance.RegisterEventHandler<EventRoundStart>(MainPlugin.Instance.OnRoundStart);
        MainPlugin.Instance.RegisterEventHandler<EventPlayerConnectFull>(MainPlugin.Instance.OnEventPlayerConnectFull);
        MainPlugin.Instance.RegisterEventHandler<EventPlayerDisconnect>(MainPlugin.Instance.OnPlayerDisconnect, HookMode.Pre);
        MainPlugin.Instance.RegisterEventHandler<EventPlayerSpawn>(MainPlugin.Instance.OnEventPlayerSpawn);
        MainPlugin.Instance.RegisterEventHandler<EventPlayerDeath>(MainPlugin.Instance.OnEventPlayerDeath);
        MainPlugin.Instance.RegisterEventHandler<EventGrenadeBounce>(MainPlugin.Instance.OnEventGrenadeBounce, HookMode.Pre);
        MainPlugin.Instance.RegisterEventHandler<EventGrenadeThrown>(MainPlugin.Instance.OnEventGrenadeThrown);
        if(!string.IsNullOrEmpty(Configs.Instance.SnowBall_Model) || !string.IsNullOrEmpty(Configs.Instance.SnowBall_Model_ag2) || Configs.Instance.SnowBall_Message_Announcement > 0)
        {
            MainPlugin.Instance.HookUserMessage(208, MainPlugin.Instance.Game_UserMessages.MuteSounds_UserMessages, HookMode.Pre);
            MainPlugin.Instance.HookUserMessage(322, MainPlugin.Instance.Game_UserMessages.Ignore_RadioText_UserMessages, HookMode.Pre);
        }
        MainPlugin.Instance.HookEntityOutput("trigger_multiple", "OnStartTouch", MainPlugin.Instance.OnEntityOutput_OnStartTouch, HookMode.Pre);

        MainPlugin.Instance.AddCommandListener("say", MainPlugin.Instance.OnPlayerSay, HookMode.Post);
        MainPlugin.Instance.AddCommandListener("say_team", MainPlugin.Instance.OnPlayerSay_Team, HookMode.Post);
        MainPlugin.Instance.HookUserMessage(118, MainPlugin.Instance.OnUserMessage_OnSayText2, HookMode.Pre);

        RegisterCssCommands(Configs.Instance.Reload_HolidaysManager.Reload_HolidaysManager_CommandsInGame.ConvertCommands(), "Commands To Reload XmasSnow Plugin", MainPlugin.Instance.Game_UserMessages.CommandsAction_ReloadPlugin);
        RegisterCssCommands(Configs.Instance.SnowFall_CommandsInGame.ConvertCommands(), "Commands To Toggle SnowFall", MainPlugin.Instance.Game_UserMessages.CommandsAction_Toggle_SnowFall);
        RegisterCssCommands(Configs.Instance.Props_CommandsInGame.ConvertCommands(), "Commands To Toggle Props", MainPlugin.Instance.Game_UserMessages.CommandsAction_Toggle_Props);
        RegisterCssCommands(Configs.Instance.Props_Edit_Mode_CommandsInGame.ConvertCommands(), "Commands To Edit Props", MainPlugin.Instance.Game_UserMessages.CommandsAction_Props_Edit);

        MainPlugin.Instance.AddCommandListener("!s", MainPlugin.Instance.Game_Listeners.DropProps_Listener, HookMode.Pre);

        CustomHooks.StartHook();
        StartTimer();

        if (Late_Hook) return;

        if (!MainPlugin.Instance.g_Main.OnTakeDamage_Hooked)
        {
            VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Hook(MainPlugin.Instance.Game_Hook.OnTakeDamage, HookMode.Pre);
            MainPlugin.Instance.g_Main.OnTakeDamage_Hooked = true;
        }
    }
    
    public static void RemoveRegisterCommandsAndHooks()
    {
        MainPlugin.Instance.RemoveListener<Listeners.OnServerPrecacheResources>(MainPlugin.Instance.OnServerPrecacheResources);
        MainPlugin.Instance.RemoveListener<Listeners.OnMapStart>(MainPlugin.Instance.OnMapStart);
        MainPlugin.Instance.RemoveListener<Listeners.OnTick>(MainPlugin.Instance.OnTick);
        MainPlugin.Instance.RemoveListener<Listeners.OnEntityCreated>(MainPlugin.Instance.OnEntityCreated);
        MainPlugin.Instance.RemoveListener<Listeners.OnEntitySpawned>(MainPlugin.Instance.OnEntitySpawned);
        MainPlugin.Instance.RemoveListener<Listeners.OnMapEnd>(MainPlugin.Instance.OnMapEnd);
        MainPlugin.Instance.RemoveListener<Listeners.CheckTransmit>(MainPlugin.Instance.OnCheckTransmit);

        MainPlugin.Instance.DeregisterEventHandler<EventRoundStart>(MainPlugin.Instance.OnRoundStart);
        MainPlugin.Instance.DeregisterEventHandler<EventPlayerConnectFull>(MainPlugin.Instance.OnEventPlayerConnectFull);
        MainPlugin.Instance.DeregisterEventHandler<EventPlayerDisconnect>(MainPlugin.Instance.OnPlayerDisconnect, HookMode.Pre);
        MainPlugin.Instance.DeregisterEventHandler<EventPlayerSpawn>(MainPlugin.Instance.OnEventPlayerSpawn);
        MainPlugin.Instance.DeregisterEventHandler<EventPlayerDeath>(MainPlugin.Instance.OnEventPlayerDeath);
        MainPlugin.Instance.DeregisterEventHandler<EventGrenadeBounce>(MainPlugin.Instance.OnEventGrenadeBounce, HookMode.Pre);
        MainPlugin.Instance.DeregisterEventHandler<EventGrenadeThrown>(MainPlugin.Instance.OnEventGrenadeThrown);
        MainPlugin.Instance.UnhookUserMessage(208, MainPlugin.Instance.Game_UserMessages.MuteSounds_UserMessages, HookMode.Pre);
        MainPlugin.Instance.UnhookUserMessage(322, MainPlugin.Instance.Game_UserMessages.Ignore_RadioText_UserMessages, HookMode.Pre);
        MainPlugin.Instance.UnhookEntityOutput("trigger_multiple", "OnStartTouch", MainPlugin.Instance.OnEntityOutput_OnStartTouch, HookMode.Pre);

        MainPlugin.Instance.RemoveCommandListener("say", MainPlugin.Instance.OnPlayerSay, HookMode.Post);
        MainPlugin.Instance.RemoveCommandListener("say_team", MainPlugin.Instance.OnPlayerSay_Team, HookMode.Post);
        MainPlugin.Instance.UnhookUserMessage(118, MainPlugin.Instance.OnUserMessage_OnSayText2, HookMode.Pre);

        RemoveCssCommands(Configs.Instance.Reload_HolidaysManager.Reload_HolidaysManager_CommandsInGame.ConvertCommands(), MainPlugin.Instance.Game_UserMessages.CommandsAction_ReloadPlugin);
        RemoveCssCommands(Configs.Instance.SnowFall_CommandsInGame.ConvertCommands(), MainPlugin.Instance.Game_UserMessages.CommandsAction_Toggle_SnowFall);
        RemoveCssCommands(Configs.Instance.Props_CommandsInGame.ConvertCommands(), MainPlugin.Instance.Game_UserMessages.CommandsAction_Toggle_Props);
        RemoveCssCommands(Configs.Instance.Props_Edit_Mode_CommandsInGame.ConvertCommands(), MainPlugin.Instance.Game_UserMessages.CommandsAction_Props_Edit);

        MainPlugin.Instance.RemoveCommandListener("!s", MainPlugin.Instance.Game_Listeners.DropProps_Listener, HookMode.Pre);

        CustomHooks.UnHook();

        if (MainPlugin.Instance.g_Main.OnTakeDamage_Hooked)
        {
            VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Unhook(MainPlugin.Instance.Game_Hook.OnTakeDamage, HookMode.Pre);
            MainPlugin.Instance.g_Main.OnTakeDamage_Hooked = false;
        }
    }

    public static string GetGeoIsoCodeInfoAsync(string ipAddress)
    {
        if (!Configs.Instance.AutoSetPlayerLanguage || ipAddress == "127.0.0.1" || ipAddress.Contains("192.168."))
            return "";

        try
        {
            using var reader = new DatabaseReader(Path.Combine(MainPlugin.Instance.ModuleDirectory, "GeoLocation/GeoLite2-City.mmdb"));

            var response = reader.City(ipAddress);

            return response.Country.IsoCode ?? "";
        }
        catch (Exception ex)
        {
            DebugMessage($"GetGeoIsoCodeInfoAsync Error {ex.Message}");
        }
        return "";
    }
    public static void SetPlayerLanguage(CCSPlayerController? player, string isoCode)
    {
        if (!Configs.Instance.AutoSetPlayerLanguage || !player.IsValid()) return;
        if (string.IsNullOrWhiteSpace(isoCode)) return;

        try
        {
            var cultureInfo = CultureInfo
            .GetCultures(CultureTypes.SpecificCultures)
            .FirstOrDefault(c =>
            {
                try
                {
                    var region = new RegionInfo(c.LCID);
                    return region.TwoLetterISORegionName.Equals(isoCode, StringComparison.OrdinalIgnoreCase);
                }
                catch
                {
                    return false;
                }
            });

            if (cultureInfo == null)
            {
                cultureInfo = CultureInfo.CurrentCulture;
            }

            var steamId = (SteamID)player.SteamID;
            PlayerLanguageManager.Instance.SetLanguage(steamId, cultureInfo);
        }
        catch (Exception ex)
        {
            DebugMessage($"SetPlayerLanguage Error: {ex.Message}");
        }
    }

    public static void MuteRadioDecoy(CEntityInstance entity)
    {
        var g_Main = MainPlugin.Instance.g_Main;
        if(string.IsNullOrEmpty(Configs.Instance.SnowBall_Model_ag2) || entity == null || !entity.IsValid) return;
        
        if (entity.DesignerName.Contains("decoy_projectile") )
        {
            var getplayer = entity.GetCCSPlayerControllerFromCEntityInstance();
            if (getplayer == null || !getplayer.IsValid || !g_Main.Player_Data.TryGetValue(getplayer.Slot, out var handle)) return;
            
            handle.Decoy_Throwed = true;
            MainPlugin.Instance.AddTimer(0.1f, ()=>
            {
                if (getplayer == null || !getplayer.IsValid || !g_Main.Player_Data.TryGetValue(getplayer.Slot, out var handle)) return;

                handle.Decoy_Throwed = false;
            }, TimerFlags.STOP_ON_MAPCHANGE);
        }
        
        if (entity.DesignerName.Contains("instanced_scripted_scene") )
        {
            if(g_Main.Player_Data.Any(kvp => kvp.Value.Decoy_Throwed))
            {
                entity.Remove();
            }
        }

    }


    public static void ChangeDecoyToSnow(CEntityInstance entity)
    {
        if(string.IsNullOrEmpty(Configs.Instance.SnowBall_Model_ag2) || entity == null || !entity.IsValid) return;

        var decoy = entity.As<CBaseGrenade>();
        if (decoy == null || !decoy.IsValid) return;

        var getplayer = decoy.GetCCSPlayerControllerFromCBaseGrenade();
        if (getplayer == null || !getplayer.IsValid) return;

        if(entity.DesignerName.Contains("decoy_projectile"))
        {
            decoy.SetModel(Configs.Instance.SnowBall_Model_ag2);
            CreateSnowTrail(decoy);
        }
        
    }
    public static void CreateSnowTrail(CBaseGrenade decoy)
    {
        if(string.IsNullOrEmpty(Configs.Instance.SnowBall_Trail_Particle) || decoy == null || !decoy.IsValid) return;

        var particle = Utilities.CreateEntityByName<CParticleSystem>("info_particle_system")!;
        if(particle == null ) return;
        
        particle.Tint = Configs.Instance.SnowBall_Trail_Particle_Color.ToColor();
        particle.StartActive = true;
        particle.EffectName = Configs.Instance.SnowBall_Trail_Particle;
        particle.DispatchSpawn();

        particle.Teleport(decoy.AbsOrigin);
        particle.AcceptInput("SetParent", decoy, null, "!activator");
    }

    public static void CreateSnowEffect(CBaseEntity decoy)
    {
        if(string.IsNullOrEmpty(Configs.Instance.SnowBall_Hit_Particle) || decoy == null || !decoy.IsValid) return;

        var particle = Utilities.CreateEntityByName<CParticleSystem>("info_particle_system")!;
        if(particle == null ) return;

        particle.Tint = Configs.Instance.SnowBall_Hit_Particle_Color.ToColor();
        particle.StartActive = true;
        particle.EffectName = Configs.Instance.SnowBall_Hit_Particle;
        particle.DispatchSpawn();
        particle.Teleport(decoy.AbsOrigin);

        MainPlugin.Instance.AddTimer(5.0f, ()=>
        {
            if(particle == null || !particle.IsValid) return;

            particle.Remove();
        }, TimerFlags.STOP_ON_MAPCHANGE);
    }

    public static void CreateSoundEffectOnVictim(CCSPlayerController player)
    {
        if(string.IsNullOrEmpty(Configs.Instance.SnowBall_Hit_Sound) || !player.IsValid(true))return;

        var filter = new RecipientFilter();
        filter.AddAllPlayers();
        player.EmitSound(Configs.Instance.SnowBall_Hit_Sound, filter);

    }

    public static void CreateSoundEffectOnWorld(CBaseEntity decoy)
    {
        if(string.IsNullOrEmpty(Configs.Instance.SnowBall_Hit_Sound) || decoy == null || !decoy.IsValid) return;

        var worldEntity = Utilities.GetEntityFromIndex<CBaseEntity>(0);
        if (worldEntity == null || !worldEntity.IsValid || !worldEntity.DesignerName.Contains("world"))return;
        worldEntity.Teleport(decoy.AbsOrigin);

        var filter = new RecipientFilter();
        filter.AddAllPlayers();
        worldEntity.EmitSound(Configs.Instance.SnowBall_Hit_Sound, filter);
    }


    public static async Task LoadMapData()
    {
        try
        {
            await MapData.StartLoadMapData();
        }
        catch (Exception ex)
        {
            DebugMessage($"LoadMapData Error: {ex.Message}");
        }
    }

    public static async Task SaveMapData()
    {
        try
        {
            await MapData.StartSaveMapData();
        }
        catch (Exception ex)
        {
            DebugMessage($"SaveMapData Error: {ex.Message}");
        }
    }

    public static void StartTimer()
    {
        if (MainPlugin.Instance.g_Main.TimerChecker == null)
        {
            MainPlugin.Instance.g_Main.TimerChecker = MainPlugin.Instance.AddTimer(2.0f, () => Timer_Checker_Callback(), TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);
        }
    }

    public static void Timer_Checker_Callback()
    {
        var g_Main = MainPlugin.Instance.g_Main;
        foreach(var Player_Data in g_Main.Player_Data.Values)
        {
            if(Player_Data == null)continue;

            var player = Player_Data.Player;
            if(!player.IsValid())continue;

            var GetPlayerPawnLocated = player.GetPlayerPawnLocated();
            if(GetPlayerPawnLocated == null || !GetPlayerPawnLocated.IsValid)continue;

            if(Player_Data.PlayerPawnLocated != GetPlayerPawnLocated || Player_Data.Snow_Fall == null || !Player_Data.Snow_Fall.IsValid)
            {
                RemoveSnowFallToPlayer(player);
                ApplySnowFallToPlayer(player);
            }
        }
    }

    public static void CheckPlayerInGlobals(CCSPlayerController player)
    {
        var g_Main = MainPlugin.Instance.g_Main;
        if(!player.IsValid() || g_Main.Player_Data.ContainsKey(player.Slot))return;

        var initialData = new Globals.PlayerDataClass(
            player,
            null!,
            player.SteamID,
            Configs.Instance.SnowFall == 2 ? 1 : Configs.Instance.SnowFall == 3 ? 2 : 0,
            Configs.Instance.Props == 2 ? 1 : Configs.Instance.Props == 3 ? 2 : 0,
            false,
            null!,
            DateTime.MinValue,
            DateTime.Now
        );
        g_Main.Player_Data.TryAdd(player.Slot, initialData);
    }

    

    public static async Task LoadPlayerData(CCSPlayerController player)
    {
        try
        {
            var g_Main = MainPlugin.Instance.g_Main;
            if (!player.IsValid() || g_Main.Player_Data.ContainsKey(player.Slot)) return;

            var steamId = player.SteamID;

            await Server.NextFrameAsync(() =>
            {
                if (!player.IsValid()) return;

                CheckPlayerInGlobals(player);
            });

            if (Configs.Instance.Cookies_Enable > 0)
            {
                try
                {
                    await Server.NextFrameAsync(async () =>
                    {
                        if (!player.IsValid()) return;

                        var cookieData = await Cookies.RetrievePersonDataById(steamId);
                        if (cookieData.PlayerSteamID != 0)
                        {
                            UpdatePlayerData(player, cookieData);
                        }
                    });
                }
                catch (Exception ex)
                {
                    DebugMessage($"LoadPlayerData Update Cookies Error: {ex.Message}");
                }
            }


            if (Configs.Instance.MySql_Enable > 0)
            {
                try
                {
                    var mysqlData = await MySqlDataManager.RetrievePersonDataByIdAsync(steamId);

                    await Server.NextFrameAsync(() =>
                    {
                        if (!player.IsValid()) return;

                        if (mysqlData.PlayerSteamID != 0)
                        {
                            UpdatePlayerData(player, mysqlData);
                        }
                    });
                }
                catch (Exception ex)
                {
                    DebugMessage($"LoadPlayerData Update MySql Error: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            DebugMessage($"LoadPlayerData Error: {ex.Message}");
        }
    }

    private static void UpdatePlayerData(CCSPlayerController player, Globals_Static.PersonData data)
    {
        if (!player.IsValid()) return;

        var g_Main = MainPlugin.Instance.g_Main;
        if (!g_Main.Player_Data.TryGetValue(player.Slot, out var handle)) return;
        
        if (data.Toggle_Snow < 0 || data.Toggle_Props < 0)
        {
            if (data.Toggle_Snow < 0)
            {
                handle.Toggle_Snow = data.Toggle_Snow;
            }

            if (data.Toggle_Props < 0)
            {
                handle.Toggle_Props = data.Toggle_Props;
            }
        }
    }

    public static async Task SavePlayerDataOnDisconnect(CCSPlayerController player)
    {
        try
        {
            if (!player.IsValid()) return;

            var g_Main = MainPlugin.Instance.g_Main;
            var steamId = player.SteamID;

            if (g_Main.Player_Data.TryGetValue(player.Slot, out var alldata))
            {
                if (alldata == null) return;

                if (alldata.Toggle_Snow < 0 || alldata.Toggle_Props < 0)
                {
                    var player_SteamID = alldata.SteamId;

                    var player_Toggle_Snow = alldata.Toggle_Snow;
                    var player_Toggle_Props = alldata.Toggle_Props;

                    if (Configs.Instance.Cookies_Enable == 1)
                    {
                        try
                        {
                            await Server.NextFrameAsync(async () =>
                            {
                                await Cookies.SaveToJsonFile(
                                player_SteamID,
                                player_Toggle_Snow,
                                player_Toggle_Props,
                                DateTime.Now
                                );

                            });
                        }
                        catch (Exception ex)
                        {
                            DebugMessage($"SavePlayerDataOnDisconnect Save Cookies Error: {ex.Message}");
                        }
                    }

                    if (Configs.Instance.MySql_Enable == 1)
                    {
                        try
                        {
                            await Server.NextFrameAsync(async () =>
                            {
                                await MySqlDataManager.SaveToMySqlAsync(new Globals_Static.PersonData
                                {
                                    PlayerSteamID = player_SteamID,
                                    Toggle_Snow = player_Toggle_Snow,
                                    Toggle_Props = player_Toggle_Props,
                                    DateAndTime = DateTime.Now
                                });

                            });
                        }
                        catch (Exception ex)
                        {
                            DebugMessage($"SavePlayerDataOnDisconnect Save MySql Error: {ex.Message}");
                        }
                    }
                }

                g_Main.Player_Data.Remove(player.Slot);
            }
        }
        catch (Exception ex)
        {
            DebugMessage($"SavePlayerDataOnDisconnect Error: {ex.Message}");
        }
    }

    public static void SavePlayersValues()
    {
        var g_Main = MainPlugin.Instance.g_Main;
        foreach (var alldata in g_Main.Player_Data.Values)
        {
            if (alldata == null)
            {
                g_Main.Player_Data.Clear();
                return;
            }

            if (alldata.Toggle_Snow < 0 || alldata.Toggle_Props < 0)
            {
                var player_SteamID = alldata.SteamId;

                var player_Toggle_Snow = alldata.Toggle_Snow;
                var player_Toggle_Props = alldata.Toggle_Props;
                
                if (Configs.Instance.Cookies_Enable == 2)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await Cookies.SaveToJsonFile(
                            player_SteamID,
                            player_Toggle_Snow,
                            player_Toggle_Props,
                            DateTime.Now
                            );
                        }
                        catch (Exception ex)
                        {
                            DebugMessage($"SavePlayersValues Save Cookies Error: {ex.Message}");
                        }
                    });
                }


                if (Configs.Instance.MySql_Enable == 2)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await MySqlDataManager.SaveToMySqlAsync(new Globals_Static.PersonData
                            {
                                PlayerSteamID = player_SteamID,
                                Toggle_Snow = player_Toggle_Snow,
                                Toggle_Props = player_Toggle_Props,
                                DateAndTime = DateTime.Now
                            });
                        }
                        catch (Exception ex)
                        {
                            DebugMessage($"SavePlayersValues Save MySql Error: {ex.Message}");
                        }
                    });
                }
            }
        }

        g_Main.Player_Data.Clear();

        if (Configs.Instance.Cookies_Enable > 0)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await Cookies.RemoveOldEntries();
                }
                catch (Exception ex)
                {
                    DebugMessage($"SavePlayersValues Remove Cookies Error: {ex.Message}");
                }
            });
        }

        if (Configs.Instance.MySql_Enable > 0)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await MySqlDataManager.DeleteOldPlayersAsync();
                }
                catch (Exception ex)
                {
                    DebugMessage($"SavePlayersValues Remove MySql Error: {ex.Message}");
                }
            });
        }
    }
    
}