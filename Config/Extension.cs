using CounterStrikeSharp.API.Core;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Timers;
using System.Globalization;
using Newtonsoft.Json.Converters;
using System.Drawing;
using CounterStrikeSharp.API.Modules.UserMessages;
using HolidaysManager_GoldKingZ.Config;
using System.Security.Cryptography;
using CounterStrikeSharp.API.Modules.Cvars;

namespace HolidaysManager_GoldKingZ;
public static class Extension
{

    public static bool IsValid([NotNullWhen(true)] this CCSPlayerController? player, bool IncludeBots = false, bool IncludeHLTV = false)
    {
        if (player == null || !player.IsValid)
            return false;

        if (!IncludeBots && player.IsBot)
            return false;

        if (!IncludeHLTV && player.IsHLTV)
            return false;

        return true;
    }

    public static bool IsPlayerAlive(this CCSPlayerController? player)
    {
        if (player == null || !player.IsValid ||
        player.Pawn == null || !player.Pawn.IsValid ||
        player.Pawn.Value == null || !player.Pawn.Value.IsValid ||
        player.PlayerPawn == null || !player.PlayerPawn.IsValid ||
        player.PlayerPawn.Value == null || !player.PlayerPawn.Value.IsValid) return false;

        if (player.PlayerPawn.Value.LifeState == (byte)LifeState_t.LIFE_ALIVE || player.Pawn.Value.LifeState == (byte)LifeState_t.LIFE_ALIVE)
        {
            return true;
        }

        return false;
    }

    public static bool IsOnGround(this CCSPlayerController? player)
    {
        if (player == null || !player.IsValid ||
        player.Pawn == null || !player.Pawn.IsValid ||
        player.Pawn.Value == null || !player.Pawn.Value.IsValid ||
        player.PlayerPawn == null || !player.PlayerPawn.IsValid ||
        player.PlayerPawn.Value == null || !player.PlayerPawn.Value.IsValid) return false;

        return ((PlayerFlags)player?.PlayerPawn.Value?.Flags! & PlayerFlags.FL_ONGROUND) == PlayerFlags.FL_ONGROUND;
    }

    
    public static CCSPlayerController? CheckPlayerController(this CCSPlayerController player)
    {
        if (player == null || !player.IsValid) return null;

        var Pawn = player.Pawn;
        if (Pawn == null || !Pawn.IsValid) return null;

        var PawnValue = Pawn.Value;
        if (PawnValue == null || !PawnValue.IsValid) return null;

        var getplayer = Utilities.GetPlayers().FirstOrDefault(p => p.IsValid(true) && p.PlayerPawn?.Value?.Index == PawnValue.Index);
        if (getplayer == null || !getplayer.IsValid) return null;

        return getplayer;
    }

    public static CCSPlayerController? GetCCSPlayerControllerFromCEntityInstance(this CEntityInstance entity, bool nowdebug = false)
    {
        if (entity == null || !entity.IsValid)return null;

        var baseEntity = entity.As<CBaseEntity>();
        if (baseEntity == null || !baseEntity.IsValid)return null;

        var OwnerEntity = baseEntity.OwnerEntity;
        if (OwnerEntity == null || !OwnerEntity.IsValid)return null;

        var OwnerValue = OwnerEntity.Value;
        if (OwnerValue == null || !OwnerValue.IsValid)return null;

        var PlayerPawn = OwnerValue.As<CCSPlayerPawn>();
        if (PlayerPawn == null || !PlayerPawn.IsValid)return null;

        var OriginalController = PlayerPawn.OriginalController?.Get();
        if (OriginalController == null || !OriginalController.IsValid)return null;

        return OriginalController;
    }

    public static CCSPlayerPawn? GetPlayerPawnLocated(this CCSPlayerController player)
    {
        if (player == null || !player.IsValid) return null;

        var Pawn = player.Pawn;
        if (Pawn == null || !Pawn.IsValid) return null;

        var PawnValue = Pawn.Value;
        if (PawnValue == null || !PawnValue.IsValid) return null;


        if (PawnValue.LifeState == (byte)LifeState_t.LIFE_DEAD)
        {
            var ObserverServices = PawnValue.ObserverServices;
            if (ObserverServices == null) return null;

            var observerServicesPawn = ObserverServices.Pawn?.Value;
            if (observerServicesPawn != null && observerServicesPawn.IsValid)
            {
                return observerServicesPawn.As<CCSPlayerPawn>();
            }
        }

        return PawnValue.As<CCSPlayerPawn>();
    }
    
    public static CDecoyProjectile GetDecoyFromThrower(this CCSPlayerController player)
    {
        if(player == null || !player.IsValid)
        {
            return null!;
        }

        var PlayerPawn = player.PlayerPawn;
        if(PlayerPawn == null || !PlayerPawn.IsValid)
        {
            return null!;
        }

        var PlayerPawnValue = PlayerPawn.Value;
        if(PlayerPawnValue == null || !PlayerPawnValue.IsValid)
        {
            return null!;
        }

        foreach(var decoy in Utilities.FindAllEntitiesByDesignerName<CDecoyProjectile>("decoy_projectile"))
        {
            if(decoy == null || !decoy.IsValid)
            {
                return null!;
            }

            var attacker = decoy.Thrower;
            if(attacker == null || !attacker.IsValid)
            {
                return null!;
            }

            var attackerValue = attacker.Value;
            if(attackerValue == null || !attackerValue.IsValid)
            {
                return null!;
            }

            if(attackerValue == PlayerPawnValue)
            {
                return decoy;
            }
        }

        return null!;
    }

    public static void PlayerFreeze(this CCSPlayerController? player)
    {

        if (player == null || !player.IsValid || !player.IsPlayerAlive()) return;

        var PlayerPawn = player.PlayerPawn;
        if (PlayerPawn == null || !PlayerPawn.IsValid) return;
        
        var PlayerPawnValue = PlayerPawn.Value;
        if (PlayerPawnValue == null || !PlayerPawnValue.IsValid) return;

        PlayerPawnValue.MoveType = MoveType_t.MOVETYPE_NONE;
        Schema.SetSchemaValue(PlayerPawnValue.Handle, "CBaseEntity", "m_nActualMoveType", 0);
        Utilities.SetStateChanged(PlayerPawnValue, "CBaseEntity", "m_MoveType");
    }

    public static void PlayerUnFreeze(this CCSPlayerController? player)
    {
        if (player == null || !player.IsValid || !player.IsPlayerAlive()) return;

        var PlayerPawn = player.PlayerPawn;
        if (PlayerPawn == null || !PlayerPawn.IsValid) return;
        
        var PlayerPawnValue = PlayerPawn.Value;
        if (PlayerPawnValue == null || !PlayerPawnValue.IsValid) return;

        PlayerPawnValue.MoveType = MoveType_t.MOVETYPE_WALK;
        Schema.SetSchemaValue(PlayerPawnValue.Handle, "CBaseEntity", "m_nActualMoveType", 2);
        Utilities.SetStateChanged(PlayerPawnValue, "CBaseEntity", "m_MoveType");
    }

    public static int ToggleOnOff(this int value)
    {
        return value switch
        {
            1 => -2,
            2 => -1,
            -1 => -2,
            -2 => -1,
            _ => value
        };
    }

    public static string ToTeamColor(this byte team)
    {
        return team switch
        {
            1 => "{LightPurple}",
            2 => "{Orange}",
            3 => "{LightBlue}",
            0 => "{White}",
            _ => "{White}"
        };
    }

    public static bool HasValidPermissionData(this string? groups)
    {
        if (string.IsNullOrWhiteSpace(groups)) return false;

        var segments = groups.Split('|', StringSplitOptions.RemoveEmptyEntries);
        foreach (var seg in segments)
        {
            var trimmed = seg.Trim();
            if (string.IsNullOrEmpty(trimmed))
                continue;

            int colonIndex = trimmed.IndexOf(':');
            if (colonIndex == -1 || colonIndex == 0)
                continue;

            string values = trimmed.Substring(colonIndex + 1).Trim();
            if (!string.IsNullOrEmpty(values))
                return true;
        }

        return false;
    }
    
    private const ulong Steam64Offset = 76561197960265728UL;
    public static (string steam2, string steam3, string steam32, string steam64) GetPlayerSteamID(this ulong steamId64)
    {
        uint id32 = (uint)(steamId64 - Steam64Offset);
        var steam32 = id32.ToString();
        uint y = id32 & 1;
        uint z = id32 >> 1;
        var steam2 = $"STEAM_0:{y}:{z}";
        var steam3 = $"[U:1:{steam32}]";
        var steam64 = steamId64.ToString();
        return (steam2, steam3, steam32, steam64);
    }

    public static string[]? ConvertCommands(this string input, bool EventPlayerChat = false)
    {
        var parts = input.Split('|', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Split(':', 2))
            .ToDictionary(
                p => p[0].Trim(),
                p => p.Length > 1
                    ? p[1].Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(c => c.Trim())
                        .Where(c => !string.IsNullOrEmpty(c))
                    : Enumerable.Empty<string>()
            );

        if (!parts.Values.Any(v => v.Any())) return null;

        if (!EventPlayerChat)
        {
            return parts.FirstOrDefault().Value?.Select(c =>
            {
                if (c.StartsWith("!"))
                {
                    var cmd = c.TrimStart('!');
                    return cmd.StartsWith("css_") ? cmd : "css_" + cmd;
                }
                return c;
            }).Distinct().ToArray();
        }

        var first = parts.FirstOrDefault().Value?
            .Select(c =>
            {
                var cmd = c.TrimStart('!');
                if (cmd.StartsWith("css_"))
                    cmd = cmd.Substring(4);
                return "!" + cmd;
            }) ?? Enumerable.Empty<string>();

        var rest = parts.Skip(1).SelectMany(p => p.Value);
        var result = first.Concat(rest).Distinct().ToArray();

        return result.Length == 0 ? null : result;
    }
    
    public static (Vector Position, QAngle Angle) GetFrontPosition(this CCSPlayerController player)
    {
        if(player == null || !player.IsValid)
            return (null!, null!);

        var PlayerPawn = player.PlayerPawn;
        if(PlayerPawn == null || !PlayerPawn.IsValid)
            return (null!, null!);
        
        var pawn = PlayerPawn.Value;
        if(pawn == null || !pawn.IsValid)
            return (null!, null!);
        
        var V_angle = pawn.V_angle;
        var AbsOrigin = pawn.AbsOrigin;
        var ViewOffset = pawn.ViewOffset;
        
        if(V_angle == null || AbsOrigin == null || ViewOffset == null)
            return (null!, null!);
        
        Vector vecForward = new();
        Vector vecUp = new();
        Vector vecRight = new();

        NativeAPI.AngleVectors(V_angle.Handle, vecForward.Handle, vecRight.Handle, vecUp.Handle);
        Vector vecEyePos = new Vector(AbsOrigin.X + ViewOffset.X, AbsOrigin.Y + ViewOffset.Y, AbsOrigin.Z + ViewOffset.Z);

        var SpawnPos = vecEyePos + vecForward * 50.0f;
        return (SpawnPos, V_angle);
    }

    public static CCSPlayerController? GetCCSPlayerControllerFromCBaseGrenade(this CBaseGrenade? CBaseGrenade)
    {
        if(CBaseGrenade == null || !CBaseGrenade.IsValid)
        {
            return null!;
        }

        var Thrower = CBaseGrenade.Thrower;
        if(Thrower == null || !Thrower.IsValid)
        {
            return null!;
        }

        var ThrowerValue = Thrower.Value;
        if(ThrowerValue == null || !ThrowerValue.IsValid)
        {
            return null!;
        }

        var PlayerPawn = ThrowerValue.As<CCSPlayerPawn>();
        if(PlayerPawn == null || !PlayerPawn.IsValid)
        {
            return null!;
        }

        var Controller = PlayerPawn.Controller;
        if(Controller == null || !Controller.IsValid)
        {
            return null!;
        }

        var ControllerValue = Controller.Value;
        if(ControllerValue == null || !ControllerValue.IsValid)
        {
            return null!;
        }
        
        var player = ControllerValue.As<CCSPlayerController>();
        if(player == null || !player.IsValid)
        {
            return null!;
        }

        return player;
    }

    public static Color ToColor(this string colorString)
    {
        if (string.IsNullOrWhiteSpace(colorString))
        {
            Helper.DebugMessage("Color string cannot be empty or whitespace.");
            return Color.Transparent;
        }

        var components = colorString
            .Replace(',', ' ')
            .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (components.Length != 3 && components.Length != 4)
        {
            Helper.DebugMessage("Invalid color format. Expected 3 or 4 components.");
            return Color.Transparent;
        }

        if (!int.TryParse(components[0], out int r) || r < 0 || r > 255)
        {
            Helper.DebugMessage("Invalid Red component value. Must be 0-255.");
            return Color.Transparent;
        }
        if (!int.TryParse(components[1], out int g) || g < 0 || g > 255)
        {
            Helper.DebugMessage("Invalid Green component value. Must be 0-255.");
            return Color.Transparent;
        }
        if (!int.TryParse(components[2], out int b) || b < 0 || b > 255)
        {
            Helper.DebugMessage("Invalid Blue component value. Must be 0-255.");
            return Color.Transparent;
        }

        byte a = 255;
        if (components.Length == 4)
        {
            string alphaComponent = components[3];
            if (alphaComponent.Contains('.'))
            {
                if (!double.TryParse(alphaComponent, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double alphaDouble))
                {
                    Helper.DebugMessage("Invalid alpha format. Must be 0-255 or 0.0-1.0.");
                    return Color.Transparent;
                }
                if (alphaDouble < 0 || alphaDouble > 1)
                {
                    Helper.DebugMessage("Alpha value must be between 0.0 and 1.0 when using decimal format.");
                    return Color.Transparent;
                }
                a = (byte)(alphaDouble * 255);
            }
            else
            {
                if (!int.TryParse(alphaComponent, out int alphaInt) || alphaInt < 0 || alphaInt > 255)
                {
                    Helper.DebugMessage("Invalid alpha value. Must be 0-255 or 0.0-1.0.");
                    return Color.Transparent;
                }
                a = (byte)alphaInt;
            }
        }
        return Color.FromArgb(a, r, g, b);
    }

    public static string ExtractModelNameFromPath(this string modelpath, bool nameOnly = true)
    {
        if (string.IsNullOrWhiteSpace(modelpath))
            return string.Empty;

        var file = modelpath.Replace("\\", "/").Split('/').Last();

        if (file.EndsWith(".vmdl", StringComparison.OrdinalIgnoreCase))
        {
            if (nameOnly)
                return file.Substring(0, file.Length - 5);

            return file;
        }

        if (file.EndsWith(".vmdl_c", StringComparison.OrdinalIgnoreCase))
        {
            if (nameOnly)
                return file.Substring(0, file.Length - 7);

            return file;
        }

        return string.Empty;
    }
    public static int ExtractModeFromName(this string propName)
    {
        if (string.IsNullOrEmpty(propName))return 0;
            
        string[] parts = propName.Split('_');

        if (parts.Length >= 3 && parts.Length <= 4)
        {
            if (int.TryParse(parts[2], out int mode))
            {
                return mode;
            }
        }

        return 0;
    }

    public static int GetPlayerDecoyAmmo(this CCSPlayerController player)
    {
        if (!player.IsValid(true)) return 0;

        var PlayerPawn = player.PlayerPawn;
        if (PlayerPawn == null || !PlayerPawn.IsValid) return 0;

        var PlayerPawnValue = PlayerPawn.Value;
        if (PlayerPawnValue == null || !PlayerPawnValue.IsValid) return 0;

        var WeaponServices = PlayerPawnValue.WeaponServices;
        if (WeaponServices == null) return 0;

        var Ammo = WeaponServices.Ammo;
        if (Ammo == null) return 0;
        
        bool IsDecoy = Ammo[17] > 0;
        if (!IsDecoy) return 0;

        return Ammo[17];
    }

    public static QAngle ToQAngle(this string s) => 
    Regex.Matches(s ?? "", @"X:\s*(-?\d+(?:\.\d+)?)|Y:\s*(-?\d+(?:\.\d+)?)|Z:\s*(-?\d+(?:\.\d+)?)", RegexOptions.IgnoreCase)
    .Cast<Match>().Aggregate(new QAngle(0, 100, 0), (angle, m) => 
        new QAngle(
            !string.IsNullOrEmpty(m.Groups[1].Value) ? float.Parse(m.Groups[1].Value) : angle.X,
            !string.IsNullOrEmpty(m.Groups[2].Value) ? float.Parse(m.Groups[2].Value) : angle.Y,
            !string.IsNullOrEmpty(m.Groups[3].Value) ? float.Parse(m.Groups[3].Value) : angle.Z
        )
    );

    public static bool IsMounted(this string path) => 
        !string.IsNullOrEmpty(path) && 
        VpkParser.GetEventsFromVpk().Any(e => e.Contains(path, StringComparison.OrdinalIgnoreCase));
}