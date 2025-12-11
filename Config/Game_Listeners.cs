using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using HolidaysManager_GoldKingZ.Config;
using System.Text.RegularExpressions;

namespace HolidaysManager_GoldKingZ;

public class Game_Listeners
{
    public HookResult DropProps_Listener(CCSPlayerController? player, CommandInfo info)
    {
        var g_Main = MainPlugin.Instance.g_Main;

        if (!player.IsValid() || g_Main.Server_EditMode_Player == null || !g_Main.Server_EditMode_Player.IsValid || g_Main.Server_EditMode_Player != player) 
            return HookResult.Continue;

        string commands = info.ArgString;
        if(string.IsNullOrEmpty(commands))
        {
            Helper.AdvancedPlayerPrintToConsole(player, MainPlugin.Instance.Localizer["PrintToConsoleToPlayer.PropsEditMode.Empty"]);
            return HookResult.Handled;
        }

        int prop_type = 0;
        string modelPath = commands;
        string[] parts = commands.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        if (parts.Length > 0 && int.TryParse(parts[0], out int parsedInt))
        {
            prop_type = parsedInt;
            if (parts.Length > 1)
            {
                modelPath = string.Join(" ", parts.Skip(1));
            }
            else
            {
                Helper.AdvancedPlayerPrintToConsole(player, MainPlugin.Instance.Localizer["PrintToConsoleToPlayer.PropsEditMode.Empty"]);
                return HookResult.Handled;
            }
        }
        
        string modelName = modelPath.ExtractModelNameFromPath();
        if (string.IsNullOrEmpty(modelName))
        {
            Helper.AdvancedPlayerPrintToConsole(player, MainPlugin.Instance.Localizer["PrintToConsoleToPlayer.PropsEditMode.Wrong"]);
            return HookResult.Handled;
        }

        Helper.SpawnPropFrontPlayer(modelPath, player, prop_type);

        return HookResult.Handled;
    }
}