using BepInEx;
using System;
using UnityEngine;
using Utilla;

namespace SpectatorGUI
{
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        private SpectatorGUI spectatorGUI;
        private bool initialized = false;
        private bool _enabled;

        void Start()
        {
            Utilla.Events.GameInitialized += OnGameInitialized;
        }

        void OnEnable()
        {
            HarmonyPatches.ApplyHarmonyPatches();
            _enabled = true;
            if (initialized)
            {
                spectatorGUI = new GameObject().AddComponent<SpectatorGUI>();
                spectatorGUI.Initialize();
            }
        }

        void OnDisable()
        {
            HarmonyPatches.RemoveHarmonyPatches();
            _enabled = false;
            if(spectatorGUI)
                Destroy(spectatorGUI.gameObject);
        }

        void OnGameInitialized(object sender, EventArgs e)
        {
            initialized = true;
            if (_enabled)
            {
                spectatorGUI = new GameObject().AddComponent<SpectatorGUI>();
                spectatorGUI.Initialize();
            }
        }
    }
}
