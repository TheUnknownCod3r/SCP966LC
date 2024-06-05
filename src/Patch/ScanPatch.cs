using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;

[HarmonyPatch(typeof(HUDManager))]
internal class ScanPatch
{
    [HarmonyPatch("PingScan_performed")]
    [HarmonyPostfix]
    private static void PostFix(InputAction.CallbackContext context)
    {
        // Log to verify postfix execution
        Debug.Log("Postfix method called");

        // Ensure the context is valid before proceeding
        if (context.performed)
        {
            Debug.Log("Context performed");

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
}