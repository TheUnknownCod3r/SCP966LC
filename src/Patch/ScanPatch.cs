using HarmonyLib;
using SCP966.SCP966Manager;
using UnityEngine;
using UnityEngine.InputSystem;

[HarmonyPatch(typeof(HUDManager))]
internal class ScanPatch
{
    [HarmonyPatch("PingScan_performed")]
    [HarmonyPostfix]
    private static void PostFix(InputAction.CallbackContext context)
    {
        Scp966Manager.Instance.StartScanOnAllInstances();
    }
}