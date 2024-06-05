using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;

[HarmonyPatch(typeof(HUDManager))]
internal class ScanPatch
{
    [HarmonyPatch("PingScan_performed")]
    [HarmonyPostfix]
    private static void PostFix(HUDManager __instance, InputAction.CallbackContext context)
    {
        /*// Log to verify postfix execution
        Debug.Log("Postfix method called");

        // Use reflection to access private members
        var canPlayerScanMethod = AccessTools.Method(typeof(HUDManager), "CanPlayerScan");
        var playerPingingScanField = AccessTools.Field(typeof(HUDManager), "playerPingingScan");

        bool canPlayerScan = (bool)canPlayerScanMethod.Invoke(__instance, null);
        float playerPingingScan = (float)playerPingingScanField.GetValue(__instance);

        // Ensure the context is valid before proceeding
        if ((UnityEngine.Object)GameNetworkManager.Instance.localPlayerController == (UnityEngine.Object)null 
            || !context.performed 
            || !canPlayerScan 
            || (double)playerPingingScan > -1.0)
        {
            return;
        }

        Debug.Log("Context performed and conditions met");
        */

        // Find all instances of SCP966.Ai.Scp966
        SCP966.Ai.Scp966[] allScp966Instances = GameObject.FindObjectsOfType<SCP966.Ai.Scp966>();

        Debug.Log($"Number of SCP966 instances found: {allScp966Instances.Length}");

        // Start the scan coroutine for each instance
        foreach (var scp966 in allScp966Instances)
        {
            Debug.Log($"Starting scan coroutine for: {scp966.gameObject.name}");
            scp966.StartScanCoroutine();
        }
    }
}