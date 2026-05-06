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
    internal static new ManualLogSource Logger { get; private set; } = null!;

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        gameObject.hideFlags = HideFlags.DontSaveInEditor;

        foreach (var m in typeof(TMP_FontAsset).GetMethods(
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        if (m.Name.Contains("TryAdd"))
            Debug.Log($"{m.Name}({string.Join(", ", Array.ConvertAll(m.GetParameters(), p => p.ParameterType.Name + " " + p.Name))})");

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
        var pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        // 先找目标字体获取 pointSize
        TMP_FontAsset targetFont = null;
        foreach (var fa in Resources.FindObjectsOfTypeAll<TMP_FontAsset>())
            if (fa.name == "fs-tahoma-8px-v2 SDF")
            {
                targetFont = fa;
                Plugin.Logger.LogInfo($"Target font pointSize: {fa.faceInfo.pointSize}, lineHeight: {fa.faceInfo.lineHeight}");
                break;
            }

        var fallbackAsset = FontLoader.CreateFontAssetFromFile(
            Path.Combine(pluginDir, "font.otf"),
            samplingPointSize: targetFont != null ? (int)targetFont.faceInfo.pointSize : 16
        );

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