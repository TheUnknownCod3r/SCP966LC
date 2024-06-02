using System;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

namespace SCP966.Ai;
public class Scp966 : EnemyAI
{
    //TODO Make him look staight at the player! He looks at the body
    //TODO make him start moving again
    public SkinnedMeshRenderer mesh;
    public Transform lookAt;
    public Transform defaultLookAt;

    
    [NonSerialized]
    private NetworkVariable<NetworkBehaviourReference> _playerNetVar = new();
    public PlayerControllerB Scp966TargetPlayer
    {
        get
        {
            return (PlayerControllerB)_playerNetVar.Value;
        }
        set 
        {
            if (value == null)
            {
                _playerNetVar.Value = null;
            }
            else
            {
                _playerNetVar.Value = new NetworkBehaviourReference(value);
            }
        }
    }

    [NonSerialized] public bool isVisibleForLocalPlayer;
    private PlayerControllerB localPlayer;

    public override void Start()
    {
        base.Start();
        localPlayer = RoundManager.Instance.playersManager.localPlayerController;
        StartSearch(transform.position);
    }

    private void LateUpdate()
    {
        CheckIfLocalPlayerHasNightVision();
        if (Scp966TargetPlayer != null)
        {
            lookAt.position = Scp966TargetPlayer.transform.position;
        }
        else
        {
            lookAt.position = defaultLookAt.position;
        }
    }

    public override void DoAIInterval()
    {
        base.DoAIInterval();
        switch(currentBehaviourStateIndex) {
            case (int)State.Searching:
                if (CheckLineOfSightForPlayer()){
                    StopSearch(currentSearch);
                    Scp966TargetPlayer = CheckLineOfSightForPlayer();
                    SwitchToBehaviourClientRpc((int)State.Tiering);
                }
                break;

            case (int)State.Tiering:
                agent.stoppingDistance = 3f;
                SetDestinationToPosition(Scp966TargetPlayer.transform.position);
                if (Vector3.Distance(transform.position, Scp966TargetPlayer.transform.position) <agent.stoppingDistance)
                {
                    creatureAnimator.SetBool("Walking",false);
                    if (Scp966TargetPlayer.carryWeight <= 100f)
                    {
                        Scp966TargetPlayer.carryWeight = 100f;
                        
                    }
                }
                else
                {
                    creatureAnimator.SetBool("Walking",true);
                }
                creatureAnimator.SetBool("Walking",false);
                if (Scp966TargetPlayer.carryWeight <= 1.5f)
                {
                    Scp966TargetPlayer.carryWeight = 1.5f;
                        
                }
                //TODO ERROR IS HERE
                if (Scp966TargetPlayer.sprintMeter <= 0.1f)
                {
                    SwitchToBehaviourClientRpc((int)State.Chasing);
                }
                break;
            case (int)State.Chasing:
                // We don't care about doing anything here
                break;
                    
            default:

                break;
        }
    }

    /// <summary>
    /// Check if night vision is enabled on local client
    /// </summary>
    private void CheckIfLocalPlayerHasNightVision()
    {
        
        GameObject foundNightVision = GameObject.Find("nightVision(Clone)");
        if (foundNightVision != null)
        {
            Transform childTransform = foundNightVision.transform.Find("Battery");
            if (childTransform!=null)
            {
                GameObject gameObjectBattery = childTransform.gameObject;
                if (gameObjectBattery.activeSelf == true)
                {
                    isVisibleForLocalPlayer = true;
                    mesh.enabled = true;
                    return;
                }
                else
                {
                    isVisibleForLocalPlayer = false;
                    mesh.enabled = false;
                    return;
                }
                
            }
        }
        isVisibleForLocalPlayer = false;
        mesh.enabled = false;
    }
    public override void HitEnemy(int force = 1, PlayerControllerB? playerWhoHit = null, bool playHitSFX = false, int hitID = -1) {
        base.HitEnemy(force, playerWhoHit, playHitSFX, hitID);
        if(isEnemyDead){
            return;
        }
        enemyHP -= force;
        if (IsOwner) {
            if (enemyHP <= 0 && !isEnemyDead) {
                // Our death sound will be played through creatureVoice when KillEnemy() is called.
                // KillEnemy() will also attempt to call creatureAnimator.SetTrigger("KillEnemy"),
                // so we don't need to call a death animation ourselves.
                
                //StopCoroutine(SwingAttack());
                // We need to stop our search coroutine, because the game does not do that by default.
                //StopCoroutine(searchCoroutine);
                KillEnemyOnOwnerClient();
            }
        }
    }


}