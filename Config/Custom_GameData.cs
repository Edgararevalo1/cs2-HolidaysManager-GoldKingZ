using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;

namespace HolidaysManager_GoldKingZ;

public class CustomGameData
{
    public static Dictionary<string, Dictionary<OSPlatform, string>> _customSignatures = new();
    public static Dictionary<string, Dictionary<OSPlatform, int>> _customOffsets = new();
    
    //public readonly MemoryFunctionWithReturn<IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr> TESTFunc_2;

    public CustomGameData()
    {
        LoadCustomGameDataFromJson();
        //TESTFunc_2 = new(GetSignature("TESTFunc_2"));
        
        //TESTFunc_2.Hook(CustomHooks.TESTFunc_2_PreHook, HookMode.Pre);
    }

    public void LoadCustomGameDataFromJson()
    {
        string jsonFilePath = Path.Combine(MainPlugin.Instance.ModuleDirectory, "gamedata/gamedata.json");
        if (!File.Exists(jsonFilePath))
        {
            Helper.DebugMessage($"JSON file does not exist at path: {jsonFilePath}. Returning without loading custom game data.", true);
            return;
        }
        
        try
        {
            var jsonData = File.ReadAllText(jsonFilePath);
            var jsonObject = JObject.Parse(jsonData);

            foreach (var item in jsonObject.Properties())
            {
                string key = item.Name;
                
                var signatures = item.Value["signatures"];
                if (signatures != null)
                {
                    var platformData = new Dictionary<OSPlatform, string>();
                    
                    if (signatures["windows"] != null)
                    {
                        platformData[OSPlatform.Windows] = signatures["windows"]!.ToString();
                    }
                    if (signatures["linux"] != null)
                    {
                        platformData[OSPlatform.Linux] = signatures["linux"]!.ToString();
                    }
                    
                    _customSignatures[key] = platformData;
                }
                
                var offsets = item.Value["offsets"];
                if (offsets != null)
                {
                    var platformData = new Dictionary<OSPlatform, int>();
                    
                    if (offsets["windows"] != null && int.TryParse(offsets["windows"]!.ToString(), out int windowsOffset))
                    {
                        platformData[OSPlatform.Windows] = windowsOffset;
                    }
                    if (offsets["linux"] != null && int.TryParse(offsets["linux"]!.ToString(), out int linuxOffset))
                    {
                        platformData[OSPlatform.Linux] = linuxOffset;
                    }
                    
                    _customOffsets[key] = platformData;
                }
            }
        }
        catch (Exception ex)
        {
            Helper.DebugMessage($"Error loading custom game data: {ex.Message}", true);
        }
    }

    public string GetSignature(string key)
    {
        if (!_customSignatures.TryGetValue(key, out var customSignature))
        {
            throw new Exception($"Invalid signature key {key}");
        }

        OSPlatform platform = GetCurrentPlatform();

        return customSignature.TryGetValue(platform, out var signatureData)
            ? signatureData
            : throw new Exception($"Missing signature data for {key} on {platform}");
    }

    public int GetOffset(string key)
    {
        if (!_customOffsets.TryGetValue(key, out var customOffset))
        {
            throw new Exception($"Invalid offset key {key}");
        }

        OSPlatform platform = GetCurrentPlatform();

        return customOffset.TryGetValue(platform, out var offsetData)
            ? offsetData
            : throw new Exception($"Missing offset data for {key} on {platform}");
    }

    private OSPlatform GetCurrentPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return OSPlatform.Linux;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return OSPlatform.Windows;
        }
        else
        {
            throw new Exception("Unsupported platform");
        }
    }
}