using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using System.Collections.Concurrent;
using HolidaysManager_GoldKingZ.Config;

namespace HolidaysManager_GoldKingZ;

public static class MapData
{
    private static readonly SemaphoreSlim _fileLock = new SemaphoreSlim(1, 1);
    private static readonly ConcurrentDictionary<string, List<Globals.DroppedPropInfo>> _cachedMapData = new();

    public static async Task StartSaveMapData()
    {
        var currentMap = Server.MapName;
        if (string.IsNullOrEmpty(currentMap))
        {
            Helper.DebugMessage("Cannot Save Map Data Current Map Is Empty", true);
            return;
        }

        var propsData = MainPlugin.Instance.g_Main.DroppedProps_Data;
        if (propsData == null)
        {
            Helper.DebugMessage("Cannot Save Map Data Is Empty", true);
            return;
        }

        await _fileLock.WaitAsync();
        try
        {
            var mapPath = GetMapFilePath(currentMap);
            await RemoveExistingProps(mapPath);
            if (propsData.Count > 0)
            {
                await SaveNewProps(mapPath, propsData);
                Helper.DebugMessage($"Saved {propsData.Count} Props For Map: {currentMap}");
            }
            else
            {

                await DeleteMapFile(mapPath);
                Helper.DebugMessage($"No Props To Save, Deleted Map File: {currentMap}");
            }
            _cachedMapData[currentMap] = propsData.ToList();
        }
        catch (Exception ex)
        {
            Helper.DebugMessage($"SaveMapData Error: {ex.Message}", true);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    private static async Task SaveNewProps(string mapPath, List<Globals.DroppedPropInfo> propsData)
    {
        var tempPath = mapPath + ".tmp";
        try
        {
            var directory = Path.GetDirectoryName(mapPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var propsDict = new Dictionary<string, JObject>();
            for (int i = 0; i < propsData.Count; i++)
            {
                var prop = propsData[i];
                if (prop.Position == null || prop.Rotation == null)
                    continue;

                var propKey = (i + 1).ToString();
                
                var propObj = new JObject
                {
                    ["ModelPath"] = prop.ModelPath,
                    ["ModelType"] = prop.ModelType,
                    ["Position"] = $"{prop.Position.X:0.00} {prop.Position.Y:0.00} {prop.Position.Z:0.00}",
                    ["Rotation"] = $"{prop.Rotation.X:0.00} {prop.Rotation.Y:0.00} {prop.Rotation.Z:0.00}"
                };
                
                propsDict[propKey] = propObj;
            }

            var jsonData = JsonConvert.SerializeObject(propsDict, Formatting.Indented);
            await File.WriteAllTextAsync(tempPath, jsonData);
            File.Move(tempPath, mapPath, true);
        }
        catch
        {
            try
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
            catch
            {
            }
            
            throw;
        }
    }

    public static async Task StartLoadMapData()
    {
        var currentMap = Server.MapName;
        if (string.IsNullOrEmpty(currentMap))
        {
            Helper.DebugMessage("Cannot Load Map Data Current Map Is Empty", true);
            return;
        }

        if (string.IsNullOrEmpty(currentMap))
            return;
        
        await Server.NextFrameAsync(() =>
        {
            try
            {
                ClearCurrentProps();
                
                var mapPath = GetMapFilePath(currentMap);
                if (!File.Exists(mapPath))
                    return;
                
                var json = File.ReadAllText(mapPath);
                var propsDict = JsonConvert.DeserializeObject<Dictionary<string, JObject>>(json);
                
                if (propsDict == null)
                    return;
                
                var g_Main = MainPlugin.Instance.g_Main;
                
                foreach (var kvp in propsDict)
                {
                    var propObj = kvp.Value;
                    var modelPath = propObj["ModelPath"]?.ToString();
                    var modelTypeToken = propObj["ModelType"];
                    var positionStr = propObj["Position"]?.ToString();
                    var rotationStr = propObj["Rotation"]?.ToString();
                    
                    if (string.IsNullOrEmpty(modelPath) || 
                        string.IsNullOrEmpty(positionStr) || 
                        string.IsNullOrEmpty(rotationStr))
                        continue;
                    
                    var posParts = positionStr.Split(' ');
                    if (posParts.Length != 3) continue;
                    
                    var position = new Vector(
                        float.Parse(posParts[0]),
                        float.Parse(posParts[1]),
                        float.Parse(posParts[2])
                    );
                    
                    var rotParts = rotationStr.Split(' ');
                    if (rotParts.Length != 3) continue;
                    
                    var rotation = new QAngle(
                        float.Parse(rotParts[0]),
                        float.Parse(rotParts[1]),
                        float.Parse(rotParts[2])
                    );

                    int modelType = 0;
                    if (modelTypeToken != null)
                    {
                        if (modelTypeToken.Type == JTokenType.Integer)
                        {
                            modelType = modelTypeToken.Value<int>();
                        }
                        else if (modelTypeToken.Type == JTokenType.String)
                        {
                            int.TryParse(modelTypeToken.Value<string>(), out modelType);
                        }
                    }

                    
                    g_Main.DroppedProps_Data.Add(new Globals.DroppedPropInfo
                    {
                        ModelPath = modelPath,
                        ModelType = modelType,
                        Position = position,
                        Rotation = rotation,
                        PropEntity = null
                    });
                }

                Helper.SpawnPropsFromData();
            }
            catch (Exception ex)
            {
                Helper.DebugMessage($"StartLoadMapData Error: {ex.Message}", true);
            }
        });
    }

    private static Task RemoveExistingProps(string mapPath)
    {
        if (File.Exists(mapPath))
        {
            try
            {
                File.Delete(mapPath);
                Helper.DebugMessage($"Removed Existing Map File: {Path.GetFileName(mapPath)}");
            }
            catch (Exception ex)
            {
                Helper.DebugMessage($"RemoveExistingProps Error : {ex.Message}", true);
            }
        }
        return Task.CompletedTask;
    }

    private static Task DeleteMapFile(string mapPath)
    {
        if (File.Exists(mapPath))
        {
            try
            {
                File.Delete(mapPath);
            }
            catch (Exception ex)
            {
                Helper.DebugMessage($"Error DeleteMapFile : {ex.Message}", true);
            }
        }
        return Task.CompletedTask;
    }

    private static void ClearCurrentProps()
    {
        var g_Main = MainPlugin.Instance?.g_Main;
        if (g_Main == null) return;
        
        if (g_Main.DroppedProps_Data == null) return;
        
        foreach (var prop in g_Main.DroppedProps_Data)
        {
            if (prop.PropEntity != null && prop.PropEntity.IsValid)
            {
                prop.PropEntity.Remove();
            }
        }
        
        g_Main.DroppedProps_Data.Clear();
    }

    public static HashSet<string> GetModelPathsForMap(string mapName)
    {
        var modelPaths = new HashSet<string>();
        
        var mapPath = GetMapFilePath(mapName);
        if (!File.Exists(mapPath))
            return modelPaths;
        
        try
        {
            var json = File.ReadAllText(mapPath);
            var propsDict = JsonConvert.DeserializeObject<Dictionary<string, JObject>>(json);
            
            if (propsDict == null)
                return modelPaths;
            
            foreach (var kvp in propsDict)
            {
                var modelPath = kvp.Value["ModelPath"]?.ToString();
                if (!string.IsNullOrEmpty(modelPath))
                {
                    modelPaths.Add(modelPath);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetModelPathsForMap Error {mapName}: {ex.Message}");
        }
        
        return modelPaths;
    }
    private static string GetMapFilePath(string mapName)
    {
        return Path.Combine(MainPlugin.Instance.ModuleDirectory, "map_data", $"{mapName}.json");
    }
}