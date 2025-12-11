using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using HolidaysManager_GoldKingZ.Config;
using Newtonsoft.Json.Linq;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Timers;
using System.Runtime.InteropServices;

namespace HolidaysManager_GoldKingZ;

public static class CustomHooks
{
    public static CustomGameData? CustomFunctions { get; private set; }
    private static bool _isHooked = false;

    internal static void StartHook()
    {
        if (_isHooked) return;
        
        CustomFunctions = new CustomGameData();
        _isHooked = true;
        Helper.DebugMessage("Hooks Started Successfully");
    }

    internal static void UnHook()
    {
        if (!_isHooked || CustomFunctions == null) return;

        //if (CustomFunctions.TESTFunc_2 != null)
        ///{
        //    CustomFunctions.TESTFunc_2.Unhook(CSoundOpGameSystem_StartSoundEventFunc_2_PreHook, HookMode.Pre);
        //}

        CustomFunctions = null;
        _isHooked = false;
        Helper.DebugMessage("Hooks Removed Successfully");
    }
}

public class Game_Hook
{
    public HookResult OnTakeDamage(DynamicHook hook)
    {
        try
        {
            var g_Main = MainPlugin.Instance.g_Main;
            var ent = hook.GetParam<CEntityInstance>(0);
            if (ent == null || !ent.IsValid || ent.DesignerName != "player") return HookResult.Continue;

            var damageinfo = hook.GetParam<CTakeDamageInfo>(1);
            if (damageinfo == null) return HookResult.Continue;

            if (damageinfo.Attacker.Value == null) return HookResult.Continue;
            var attackerPawn = damageinfo.Attacker.Value.As<CBasePlayerPawn>();
            if (attackerPawn == null || !attackerPawn.IsValid) return HookResult.Continue;
            
            var GetAttacker = attackerPawn.Controller.Value;
            if (GetAttacker == null || !GetAttacker.IsValid) return HookResult.Continue;

            var pawn = ent.As<CCSPlayerPawn>();
            if (pawn == null || !pawn.IsValid) return HookResult.Continue;

            var attacker = Utilities.GetPlayerFromIndex((int)GetAttacker.Index);
            if (!attacker.IsValid(true)) return HookResult.Continue;
            
            var victim = pawn.OriginalController?.Get();
            if (victim == null || !victim.IsValid(true)) return HookResult.Continue;

            var decoy = damageinfo.Inflictor.Value;
            if (decoy == null || !decoy.IsValid) return HookResult.Continue;

            if(decoy.DesignerName == "decoy_projectile")
            {
                decoy.Remove();
                damageinfo.Damage = Configs.Instance.SnowBall_Damage;
                damageinfo.DamageFlags = TakeDamageFlags_t.DFLAG_IGNORE_ARMOR;
                Helper.CreateSnowEffect(decoy);
                Helper.CreateSoundEffectOnVictim(victim);
                return HookResult.Changed;
            }

            return HookResult.Continue;
        }
        catch (Exception ex)
        {
            Helper.DebugMessage($"[Message] OnTakeDamage Error : {ex.Message}");
            Helper.DebugMessage($"[StackTrace] OnTakeDamage Error: {ex.StackTrace}");
            return HookResult.Continue;
        }
    }

    delegate IntPtr FindOrCreateMaterialFromResourceDelegate(IntPtr pMaterialSystem, IntPtr pOut, string materialName);
    public unsafe IntPtr FindMaterialByPath(string material)
    {
        try
        {
            if (CustomHooks.CustomFunctions == null || string.IsNullOrEmpty(material)) return IntPtr.Zero;
            
            string original = material;
            if (material.EndsWith("_c")) material = material.Substring(0, material.Length - 2);
            
            IntPtr pIMaterialSystem2 = NativeAPI.GetValveInterface(0, "VMaterialSystem2_001");
            if (pIMaterialSystem2 == IntPtr.Zero) return IntPtr.Zero;
            
            int offset = CustomHooks.CustomFunctions.GetOffset("IMaterialSystem_FindOrCreateMaterialFromResource");
            if (offset <= 0) return IntPtr.Zero;
            
            IntPtr vtablePtr = Marshal.ReadIntPtr(pIMaterialSystem2);
            if (vtablePtr == IntPtr.Zero) return IntPtr.Zero;
            
            IntPtr functionPtr = Marshal.ReadIntPtr(vtablePtr + (offset * IntPtr.Size));
            if (functionPtr == IntPtr.Zero) return IntPtr.Zero;
            
            var func = Marshal.GetDelegateForFunctionPointer<FindOrCreateMaterialFromResourceDelegate>(functionPtr);
            IntPtr outMaterial = IntPtr.Zero;
            IntPtr pOutMaterial = (IntPtr)(&outMaterial);
            IntPtr materialptr3 = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) 
                ? func.Invoke(pIMaterialSystem2, pOutMaterial, material)
                : func.Invoke(pOutMaterial, IntPtr.Zero, material);
            
            return materialptr3 != IntPtr.Zero ? *(IntPtr*)materialptr3 : IntPtr.Zero;
        }
        catch { return IntPtr.Zero; }
    }
}