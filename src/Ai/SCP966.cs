using System;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

namespace SCP966.Ai;
public class Scp966 : EnemyAI
{
    public SkinnedMeshRenderer mesh;
    public Transform lookAt;
    public Transform defaultLookAt;


    [NonSerialized] public bool isVisibleForLocalPlayer;
    private PlayerControllerB localPlayer;

    public override void Start()
    {
        base.Start();
        localPlayer = RoundManager.Instance.playersManager.localPlayerController;
    }

    public override void DoAIInterval()
    {
        base.DoAIInterval();
        CheckIfLocalPlayerHasNightVisionClientRpc();
        
    }

    /// <summary>
    /// Check if night vision is enabled on local client
    /// I DON'T KNOW IF THIS WORKS
    /// </summary>
    [ClientRpc]
    private void CheckIfLocalPlayerHasNightVisionClientRpc()
    {
        if (localPlayer.nightVision.enabled == true )
        {
            isVisibleForLocalPlayer = true;
            mesh.enabled = true;
        }
        else
        {
            isVisibleForLocalPlayer = false;
            mesh.enabled = false;
        }
    }


}