using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using HarmonyLib;
using System.Reflection;
using System;
using TMPro;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace FALLBACKFONT9;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static TMP_FontAsset? fallbackAsset;
    internal static new ManualLogSource Logger { get; private set; } = null!;

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        gameObject.hideFlags = HideFlags.DontSaveInEditor;
        
        var pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        fallbackAsset = FontLoader.CreateFontAssetFromFile(
            Path.Combine(pluginDir, "font.otf")
        );

        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void GetFontEngineInfoFromAssembly()
    {
        var type = typeof(TMP_FontAsset);

        Plugin.Logger.LogInfo("=== Fields ===");
        foreach (var f in type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
            Plugin.Logger.LogInfo(f.Name + " : " + f.FieldType);

        Plugin.Logger.LogInfo("=== Methods ===");
        foreach (var m in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
            Plugin.Logger.LogInfo(m.Name);
            
        Plugin.Logger.LogInfo("=== FontEngine Static Methods ===");
        var fontEngineType = Assembly.Load("UnityEngine.TextCoreFontEngineModule")
            .GetType("UnityEngine.TextCore.LowLevel.FontEngine");
        foreach (var m in fontEngineType.GetMethods(
                     BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public))
            Plugin.Logger.LogInfo(m.Name + "(" + string.Join(", ", 
                System.Array.ConvertAll(m.GetParameters(), p => p.ParameterType.Name)) + ")");
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AddFallbackFont();
    }

    private void AddFallbackFont()
    {
        if (fallbackAsset == null) return;

        Plugin.Logger.LogInfo($"Fallback pointSize: {fallbackAsset.faceInfo.pointSize}, lineHeight: {fallbackAsset.faceInfo.lineHeight}");

        foreach (var fontAsset in Resources.FindObjectsOfTypeAll<TMP_FontAsset>())
        {
            fontAsset.fallbackFontAssetTable ??= new List<TMP_FontAsset>();
            if (!fontAsset.fallbackFontAssetTable.Contains(fallbackAsset))
            {
                fontAsset.fallbackFontAssetTable.Add(fallbackAsset);
                Plugin.Logger.LogInfo($"Added fallback to {fontAsset.name}");
            }
        }
    }
}