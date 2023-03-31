using BepInEx;
using Bepinject;
using ComputerModExample;
using System;
using UnityEngine;

namespace SpectatorGUI
{
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        private SpectatorGUI spectatorGUI;
        private bool initialized = false;
        private bool _enabled;

        void Awake()
        {
            Zenjector.Install<MainInstaller>().OnProject();
        }

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
