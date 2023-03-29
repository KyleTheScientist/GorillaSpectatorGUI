using GorillaNetworking;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpectatorGUI.Patches
{
    [HarmonyPatch(typeof(GorillaScoreBoard))]
    [HarmonyPatch("RedrawPlayerLines", MethodType.Normal)]
    internal class RedrawPlayerLines
    {
        private static void Postfix(GorillaScoreBoard __instance)
        {
            if (SpectatorGUI.Instance)
            {
                SpectatorGUI.Instance.RefreshPlayerList(__instance.lines);
            }
        }
    }

    [HarmonyPatch(typeof(PhotonNetworkController))]
    [HarmonyPatch("ProcessState", MethodType.Normal)]
    internal class ProcessState
    {
        private static void Postfix(PhotonNetworkController __instance, PhotonNetworkController.ConnectionEvent connectionEvent)
        {
            if (connectionEvent == PhotonNetworkController.ConnectionEvent.OnDisconnected)
            {
                SpectatorGUI.Instance.RefreshPlayerList(null);
            }
            //SpectatorGUI.Instance.debugText.text = connectionEvent.ToString();
        }
    }
}
