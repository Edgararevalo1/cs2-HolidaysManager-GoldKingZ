using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using System.Text;
using HolidaysManager_GoldKingZ.Config;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Core.Translations;
using Microsoft.VisualBasic;
using System.Text.RegularExpressions;
using CounterStrikeSharp.API.Modules.UserMessages;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Timers;

namespace HolidaysManager_GoldKingZ;

public class Game_UserMessages
{
    public HookResult MuteSounds_UserMessages(CounterStrikeSharp.API.Modules.UserMessages.UserMessage um)
    {
        var entityIndex = um.ReadInt("source_entity_index");

        var entity = Utilities.GetEntityFromIndex<CBaseEntity>(entityIndex);
        if (entity == null || !entity.IsValid) return HookResult.Continue;

        if (entity.DesignerName.Contains("decoy_projectile"))
        {
            return HookResult.Stop;
        }


        return HookResult.Continue;
    }
    public HookResult Ignore_RadioText_UserMessages(CounterStrikeSharp.API.Modules.UserMessages.UserMessage um)
    {
        for (int i = 0; i < um.GetRepeatedFieldCount("params"); i++)
        {
            var message = um.ReadString("params", i);

            if (message.Contains("#SFUI_TitlesTXT_Decoy_in_the_hole"))
            {
                um.Recipients.Clear();
            }
            
        }
        return HookResult.Continue;
    }
    public HookResult HookPlayerChat_UserMessages(CounterStrikeSharp.API.Modules.UserMessages.UserMessage? um, CCSPlayerController? player, string message)
    {
        if (!player.IsValid()) return HookResult.Continue;

        var g_Main = MainPlugin.Instance.g_Main;
        Helper.CheckPlayerInGlobals(player);

        if (!g_Main.Player_Data.TryGetValue(player.Slot, out var playerData)) return HookResult.Continue;

        bool onetime = (DateTime.Now - playerData.EventPlayerChat).TotalSeconds > 0.4;
        if (onetime)
        {
            playerData.EventPlayerChat = DateTime.Now;
        }

        if(g_Main.Server_EditMode_Player != null && g_Main.Server_EditMode_Player.IsValid && g_Main.Server_EditMode_Player == player && onetime)
        {
            if(message.Equals("!i") || message.Equals("!info"))
            {
                Helper.AdvancedPlayerPrintToConsole(player, MainPlugin.Instance.Localizer["PrintToConsoleToPlayer.PropsEditMode.Info"]);
                Helper.AdvancedPlayerPrintToChat(player, null!, MainPlugin.Instance.Localizer["PrintToChatToPlayer.PropsEditMode.Info"]);
            }

            if(message.Equals("!d") || message.Equals("!data"))
            {
                player.PrintToConsole("===================== Models Paths Data =====================");

                Helper.AdvancedPlayerPrintToChat(player, null!, MainPlugin.Instance.Localizer["PrintToChatToPlayer.PropsEditMode.Data"]);
                foreach (var Events in VpkParser.GetEventsFromVpk())
                {
                    if(Events.Count() == 0 || Events == null)
                    {
                        player.PrintToConsole("No Data Found!");
                        return HookResult.Continue;
                    }

                    if (Events.StartsWith("models"))
                    {
                        player.PrintToConsole(Events);
                    }

                }
                
                player.PrintToConsole("============================================================");
            }
        }

        if (Configs.Instance.Reload_HolidaysManager.Reload_HolidaysManager_CommandsInGame.ConvertCommands(true)?.Any(c => message.Equals(c.Trim(), StringComparison.OrdinalIgnoreCase)) == true)
        {
            if (Configs.Instance.Reload_HolidaysManager.Reload_HolidaysManager_Flags.HasValidPermissionData() && !Helper.IsPlayerInGroupPermission(player, Configs.Instance.Reload_HolidaysManager.Reload_HolidaysManager_Flags))
            {
                if (onetime) Helper.AdvancedPlayerPrintToChat(player, null!, MainPlugin.Instance.Localizer["PrintToChatToPlayer.ReloadPlugin.Not.Allowed"]);
            }
            else
            {
                if (onetime)
                {
                    Helper.RemoveAllPlayersSnowFall();
                    Helper.RemoveAllProps();
                    Helper.RemoveRegisterCommandsAndHooks();
                    Helper.ClearVariables(true);

                    Configs.Load(MainPlugin.Instance.ModuleDirectory);            
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
                            if (Configs.Instance.MySql_Enable > 0)
                            {
                                await MySqlDataManager.CreateTableIfNotExistsAsync();
                            }
                        });
                    }
                    
                    Helper.AdvancedPlayerPrintToChat(player, null!, MainPlugin.Instance.Localizer["PrintToChatToPlayer.ReloadPlugin.Successfully"]);
                }
                Helper.MuteCommands(um, Configs.Instance.Reload_HolidaysManager.Reload_HolidaysManager_Hide);
            }
            Helper.MuteCommands(um, Configs.Instance.Reload_HolidaysManager.Reload_HolidaysManager_Hide, true);
        }

        if (Configs.Instance.SnowFall > 1)
        {
            if (Configs.Instance.SnowFall_CommandsInGame.ConvertCommands(true)?.Any(c => message.Equals(c.Trim(), StringComparison.OrdinalIgnoreCase)) == true)
            {
                if (Configs.Instance.SnowFall_Flags.HasValidPermissionData() && !Helper.IsPlayerInGroupPermission(player, Configs.Instance.SnowFall_Flags))
                {
                    if (onetime) Helper.AdvancedPlayerPrintToChat(player, null!, MainPlugin.Instance.Localizer["PrintToChatToPlayer.Toggle.SnowFall.Not.Allowed"]);
                }
                else
                {
                    if (onetime)
                    {
                        playerData.Toggle_Snow = playerData.Toggle_Snow.ToggleOnOff();
                        if (playerData.Toggle_Snow == -1)
                        {
                            Helper.AdvancedPlayerPrintToChat(player, null!, MainPlugin.Instance.Localizer["PrintToChatToPlayer.Toggle.SnowFall.Enabled"]);
                        }
                        else if (playerData.Toggle_Snow == -2)
                        {
                            Helper.AdvancedPlayerPrintToChat(player, null!, MainPlugin.Instance.Localizer["PrintToChatToPlayer.Toggle.SnowFall.Disabled"]);
                        }
                    }
                    Helper.MuteCommands(um, Configs.Instance.SnowFall_Hide);
                }
                Helper.MuteCommands(um, Configs.Instance.SnowFall_Hide, true);
            }
        }
        

        if(Configs.Instance.Props > 0)
        {
            if (Configs.Instance.Props_Edit_Mode_CommandsInGame.ConvertCommands(true)?.Any(c => message.Equals(c.Trim(), StringComparison.OrdinalIgnoreCase)) == true)
            {
                if (Configs.Instance.Props_Edit_Mode_Flags.HasValidPermissionData() && !Helper.IsPlayerInGroupPermission(player, Configs.Instance.Props_Edit_Mode_Flags))
                {
                    if(onetime)Helper.AdvancedPlayerPrintToChat(player, null!, MainPlugin.Instance.Localizer["PrintToChatToPlayer.PropsEditMode.Not.Allowed"]);
                }
                else
                {
                    if(MainPlugin.Instance.g_Main.Server_EditMode_Player != null && MainPlugin.Instance.g_Main.Server_EditMode_Player.IsValid && player != MainPlugin.Instance.g_Main.Server_EditMode_Player)
                    {
                        if(onetime)Helper.AdvancedPlayerPrintToChat(player, null!, MainPlugin.Instance.Localizer["PrintToChatToPlayer.PropsEditMode.Is.Started"], MainPlugin.Instance.g_Main.Server_EditMode_Player.PlayerName);
                    }else
                    {
                        if(onetime)
                        {
                            Helper.StartEditMode(player);
                        }
                        Helper.MuteCommands(um, Configs.Instance.Props_Edit_Mode_Hide);
                    }
                }
                Helper.MuteCommands(um, Configs.Instance.Props_Edit_Mode_Hide, true);
            }
            else if (Configs.Instance.Props > 1 && Configs.Instance.Props_CommandsInGame.ConvertCommands(true)?.Any(c => message.Equals(c.Trim(), StringComparison.OrdinalIgnoreCase)) == true)
            {
                if (Configs.Instance.Props_Flags.HasValidPermissionData() && !Helper.IsPlayerInGroupPermission(player, Configs.Instance.Props_Flags))
                {
                    if (onetime) Helper.AdvancedPlayerPrintToChat(player, null!, MainPlugin.Instance.Localizer["PrintToChatToPlayer.Toggle.Props.Not.Allowed"]);
                }
                else
                {
                    if (onetime)
                    {
                        playerData.Toggle_Props = playerData.Toggle_Props.ToggleOnOff();
                        if (playerData.Toggle_Props == -1)
                        {
                            Helper.AdvancedPlayerPrintToChat(player, null!, MainPlugin.Instance.Localizer["PrintToChatToPlayer.Toggle.Props.Enabled"]);
                        }
                        else if (playerData.Toggle_Props == -2)
                        {
                            Helper.AdvancedPlayerPrintToChat(player, null!, MainPlugin.Instance.Localizer["PrintToChatToPlayer.Toggle.Props.Disabled"]);
                        }
                    }
                    Helper.MuteCommands(um, Configs.Instance.Props_Hide);
                }
                Helper.MuteCommands(um, Configs.Instance.Props_Hide, true);
            }
        }

        return HookResult.Continue;
    }














    #region Commands Hook

    public void CommandsAction_ReloadPlugin(CCSPlayerController? player, CommandInfo info)
    {
        if (!player.IsValid()) return;

        Helper.CheckPlayerInGlobals(player);

        if (!MainPlugin.Instance.g_Main.Player_Data.TryGetValue(player.Slot, out var playerData)) return;
        if ((DateTime.Now - playerData.EventPlayerChat).TotalSeconds <= 0.4) return;

        if (Configs.Instance.Reload_HolidaysManager.Reload_HolidaysManager_Flags.HasValidPermissionData() && !Helper.IsPlayerInGroupPermission(player, Configs.Instance.Reload_HolidaysManager.Reload_HolidaysManager_Flags))
        {
            Helper.AdvancedPlayerPrintToChat(player, info, MainPlugin.Instance.Localizer["PrintToChatToPlayer.ReloadPlugin.Not.Allowed"]);
        }
        else
        {
            Helper.RemoveAllPlayersSnowFall();
            Helper.RemoveAllProps();
            Helper.RemoveRegisterCommandsAndHooks();
            Helper.ClearVariables(true);

            Configs.Load(MainPlugin.Instance.ModuleDirectory);            
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
                    if (Configs.Instance.MySql_Enable > 0)
                    {
                        await MySqlDataManager.CreateTableIfNotExistsAsync();
                    }
                });
            }
            Helper.AdvancedPlayerPrintToChat(player, info, MainPlugin.Instance.Localizer["PrintToChatToPlayer.ReloadPlugin.Successfully"]);
        }
    }

    public void CommandsAction_Toggle_SnowFall(CCSPlayerController? player, CommandInfo info)
    {
        if (Configs.Instance.SnowFall < 2 || !player.IsValid()) return;

        Helper.CheckPlayerInGlobals(player);

        if (!MainPlugin.Instance.g_Main.Player_Data.TryGetValue(player.Slot, out var playerData)) return;
        if ((DateTime.Now - playerData.EventPlayerChat).TotalSeconds <= 0.4) return;

        if (Configs.Instance.SnowFall_Flags.HasValidPermissionData() && !Helper.IsPlayerInGroupPermission(player, Configs.Instance.SnowFall_Flags))
        {
            Helper.AdvancedPlayerPrintToChat(player, info, MainPlugin.Instance.Localizer["PrintToChatToPlayer.Toggle.SnowFall.Not.Allowed"]);
        }
        else
        {
            playerData.Toggle_Snow = playerData.Toggle_Snow.ToggleOnOff();
            if (playerData.Toggle_Snow == -1)
            {
                Helper.AdvancedPlayerPrintToChat(player, info, MainPlugin.Instance.Localizer["PrintToChatToPlayer.Toggle.SnowFall.Enabled"]);
            }
            else if (playerData.Toggle_Snow == -2)
            {
                Helper.AdvancedPlayerPrintToChat(player, info, MainPlugin.Instance.Localizer["PrintToChatToPlayer.Toggle.SnowFall.Disabled"]);
            }
        }
    }

    public void CommandsAction_Toggle_Props(CCSPlayerController? player, CommandInfo info)
    {
        if (Configs.Instance.Props < 2 || !player.IsValid()) return;

        Helper.CheckPlayerInGlobals(player);

        if (!MainPlugin.Instance.g_Main.Player_Data.TryGetValue(player.Slot, out var playerData)) return;
        if ((DateTime.Now - playerData.EventPlayerChat).TotalSeconds <= 0.4) return;

        if (Configs.Instance.Props_Flags.HasValidPermissionData() && !Helper.IsPlayerInGroupPermission(player, Configs.Instance.Props_Flags))
        {
            Helper.AdvancedPlayerPrintToChat(player, info, MainPlugin.Instance.Localizer["PrintToChatToPlayer.Toggle.Props.Not.Allowed"]);
        }
        else
        {
            playerData.Toggle_Props = playerData.Toggle_Props.ToggleOnOff();
            if (playerData.Toggle_Props == -1)
            {
                Helper.AdvancedPlayerPrintToChat(player, info, MainPlugin.Instance.Localizer["PrintToChatToPlayer.Toggle.Props.Enabled"]);
            }
            else if (playerData.Toggle_Props == -2)
            {
                Helper.AdvancedPlayerPrintToChat(player, info, MainPlugin.Instance.Localizer["PrintToChatToPlayer.Toggle.Props.Disabled"]);
            }
        }

    }

    public void CommandsAction_Props_Edit(CCSPlayerController? player, CommandInfo info)
    {
        if (Configs.Instance.Props < 1 || !player.IsValid()) return;

        Helper.CheckPlayerInGlobals(player);

        if (!MainPlugin.Instance.g_Main.Player_Data.TryGetValue(player.Slot, out var playerData)) return;
        if ((DateTime.Now - playerData.EventPlayerChat).TotalSeconds <= 0.4) return;

        if (Configs.Instance.Props_Edit_Mode_Flags.HasValidPermissionData() && !Helper.IsPlayerInGroupPermission(player, Configs.Instance.Props_Edit_Mode_Flags))
        {
            Helper.AdvancedPlayerPrintToChat(player, info, MainPlugin.Instance.Localizer["PrintToChatToPlayer.PropsEditMode.Not.Allowed"]);
        }
        else
        {
            if(MainPlugin.Instance.g_Main.Server_EditMode_Player != null && MainPlugin.Instance.g_Main.Server_EditMode_Player.IsValid && player != MainPlugin.Instance.g_Main.Server_EditMode_Player)
            {
                Helper.AdvancedPlayerPrintToChat(player, info, MainPlugin.Instance.Localizer["PrintToChatToPlayer.PropsEditMode.Is.Started"], MainPlugin.Instance.g_Main.Server_EditMode_Player.PlayerName);
            }else
            {
                Helper.StartEditMode(player);
            }
            
        }

    }

    #endregion Commands Hook
}