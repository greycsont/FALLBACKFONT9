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

namespace Fffffff;

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
                                      .OrderBy(f => {
                                          var stem = Path.GetFileNameWithoutExtension(f);
                                          return int.TryParse(stem["font".Length..], out int n) ? n : int.MaxValue;
                                      });
        foreach (var file in fontFiles)
        {
            var fontAsset = FNT.FontLoader.CreateFontAssetFromFile(file);
            if (fontAsset != null)
            {
                fonts.Add(fontAsset);
                Logger.LogInfo($"Loaded font: {Path.GetFileName(file)}");
            }
        }

        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AddFallbackFont();
    }

    private void AddFallbackFont()
    {
        if (fonts.Count == 0) return;

        foreach (var fontAsset in Resources.FindObjectsOfTypeAll<TMP_FontAsset>().Except(fonts))
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