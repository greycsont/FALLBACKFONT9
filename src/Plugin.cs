using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using HarmonyLib;
using System.Reflection;
using System;
using TMPro;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

namespace FALLBACKFONT9;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static List<TMP_FontAsset> fonts = new();
    internal static new ManualLogSource Logger { get; private set; } = null!;

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        gameObject.hideFlags = HideFlags.DontSaveInEditor;

        var pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var fontExtensions = new[] { "*.otf", "*.ttf" };
        var fontFiles = fontExtensions.SelectMany(ext => Directory.GetFiles(pluginDir, "font" + ext))
                                      .OrderBy(f => Path.GetFileName(f));
        foreach (var file in fontFiles)
        {
            var fontAsset = FontLoader.CreateFontAssetFromFile(file);
            if (fontAsset != null)
            {
                fonts.Add(fontAsset);
                Logger.LogInfo($"Loaded font: {Path.GetFileName(file)}");
            }
        }

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
        if (fonts.Count == 0) return;

        foreach (var fontAsset in Resources.FindObjectsOfTypeAll<TMP_FontAsset>())
        {
            fontAsset.fallbackFontAssetTable ??= new List<TMP_FontAsset>();
            if (!fontAsset.fallbackFontAssetTable.Contains(fonts[0]))
            {
                foreach (var f in fonts)
                    fontAsset.fallbackFontAssetTable.Add(f);
                Plugin.Logger.LogInfo($"Added fallback to {fontAsset.name}");
            }
        }
    }
}