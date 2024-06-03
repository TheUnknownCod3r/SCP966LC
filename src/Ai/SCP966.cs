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
    public Transform AttackArea;

    [Header("Damage Attacks")] 
    public int firstAttack;
    public int secondAttack;
    public int thirdAttack;
    private int currentDamage;

    public float attackCooldown;
    private float attackCooldownBeheader;
    private float? previousNightVisionValue = null;
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
        attackCooldownBeheader = attackCooldown;
    }

    private void LateUpdate()
    {
        CheckIfLocalPlayerHasNightVision();
        if (Scp966TargetPlayer != null)
        {
            lookAt.position = Scp966TargetPlayer.playerEye.position;
        }
        else
        {
            lookAt.position = defaultLookAt.position;
        }
        attackCooldown -= Time.deltaTime;
    }

    public override void DoAIInterval()
    {
        base.DoAIInterval();
        switch(currentBehaviourStateIndex) {
            case (int)State.Searching:
                DoAnimationClientRpc("Walking",true);
                agent.autoBraking = true;
                agent.speed = 5f;
                if (CheckLineOfSightForPlayer(45f,60, 3)){
                    StopSearch(currentSearch);
                    Scp966TargetPlayer = CheckLineOfSightForPlayer();
                    SwitchToBehaviourClientRpc((int)State.Tiering);
                }
                break;
            case (int)State.Tiering:
                agent.stoppingDistance = 8f;
                agent.speed = 5f;
                agent.autoBraking = true;
                if (!Scp966TargetPlayer.isInsideFactory || 
                    Scp966TargetPlayer.isPlayerDead || 
                    Vector3.Distance(
                        transform.position, 
                        Scp966TargetPlayer.transform.position) > 25)
                {
                    StartSearch(transform.position);
                    SwitchToBehaviourClientRpc((int)State.Searching);
                    Scp966TargetPlayer = null;
                    break;
                }
                SetDestinationToPosition(Scp966TargetPlayer.transform.position);
                if (Vector3.Distance(transform.position, Scp966TargetPlayer.transform.position) <agent.stoppingDistance)
                {
                    DoAnimationClientRpc("Walking",false);
                }
                else
                {
                    DoAnimationClientRpc("Walking",true);
                }
                if (Scp966TargetPlayer.carryWeight <= 1.5f)
                {
                    Scp966TargetPlayer.carryWeight = 1.5f;
                        
                }
                if (Scp966TargetPlayer.sprintMeter <= 0.3f)
                {
                    SwitchToBehaviourClientRpc((int)State.Chasing);
                }
                break;
            case (int)State.Chasing:
                agent.stoppingDistance = 0f;
                agent.speed = 7f;
                agent.autoBraking = false;
                if (!Scp966TargetPlayer.isInsideFactory || 
                    Scp966TargetPlayer.isPlayerDead || 
                    Vector3.Distance(
                        transform.position, 
                        Scp966TargetPlayer.transform.position) > 25)
                {
                    StartSearch(transform.position);
                    SwitchToBehaviourClientRpc((int)State.Searching);
                    Scp966TargetPlayer = null;
                    break;
                }
                SetDestinationToPosition(Scp966TargetPlayer.transform.position);
                DoAnimationClientRpc("Walking",true);

                //If reached player and attack cooldown is 0, then :
                if (
                    Vector3.Distance(
                        transform.position, 
                        Scp966TargetPlayer.transform.position
                        ) < agent.stoppingDistance && 
                    attackCooldown==0
                    )
                {
                    creatureAnimator.SetTrigger("Attack");
                    switch (creatureAnimator.GetInteger("AttackNumber"))
                    {
                        case 0:
                            currentDamage = firstAttack;
                            creatureAnimator.SetInteger("AttackNumber", 1);
                            break;
                        case 1:
                            currentDamage = secondAttack;
                            creatureAnimator.SetInteger("AttackNumber", 2);
                            break;
                        case 2:
                            currentDamage = thirdAttack; ;
                            creatureAnimator.SetInteger("AttackNumber", 0);
                            break;
                        default:
                            MonsterLogger("We are outside of possible attack array!", true);
                            break;
                    }
                }
                break;
                    
            default:
                MonsterLogger("Behaviour State was changed to an non real one", true);
                break;
        }
    }
    
    
    private GameObject foundNightVision;
    private Transform batteryTransform;
    private Transform barTransform;



    private void EnableMesh()
    {
        isVisibleForLocalPlayer = true;
        if (mesh == null)
        {
            mesh = GetComponent<SkinnedMeshRenderer>();
        }
        mesh.enabled = true;
    }

    private void DisableMesh()
    {
        isVisibleForLocalPlayer = false;
        if (mesh == null)
        {
            mesh = GetComponent<SkinnedMeshRenderer>();
        }
        mesh.enabled = false;
    }
    /// <summary>
    /// Check if night vision is enabled on local client
    /// </summary>
    private void CheckIfLocalPlayerHasNightVision()
    {
        if (foundNightVision == null)
        {
            foundNightVision = GameObject.Find("nightVision(Clone)");
            if (foundNightVision != null)
            {
                batteryTransform = foundNightVision.transform.Find("Battery");
                if (batteryTransform != null)
                {
                    barTransform = batteryTransform.Find("Bar");
                }
            }
        }

        if (foundNightVision != null && batteryTransform != null && barTransform != null)
        {
            GameObject gameObjectBattery = batteryTransform.gameObject;
            if (gameObjectBattery.activeSelf)
            {
                float barScaleX = barTransform.localScale.x;

                if (previousNightVisionValue.HasValue)
                {
                    if (barScaleX < previousNightVisionValue.Value)
                    {
                        EnableMesh();
                    }
                    else
                    {
                        DisableMesh();
                        
                    }
                    previousNightVisionValue = barScaleX;
                }
                else
                {
                    MonsterLogger("There was no value for previousNightVisionValue");
                    previousNightVisionValue = barScaleX;
                }
            }
            else
            {
                DisableMesh();
            }
        }
        else
        {
            DisableMesh();
        }
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
    
    [ClientRpc]
    public void SwingAttackHitClientRpc() {
        int playerLayer = 1 << 3; // This can be found from the game's Asset Ripper output in Unity
        Collider[] hitColliders = Physics.OverlapBox(AttackArea.position, AttackArea.localScale, Quaternion.identity, playerLayer);
        if(hitColliders.Length > 0){
            foreach (var player in hitColliders){
                PlayerControllerB playerControllerB = MeetsStandardPlayerCollisionConditions(player);
                if (playerControllerB != null)
                {
                    playerControllerB.DamagePlayer(40);
                }
            }
        }

        attackCooldown = attackCooldownBeheader;
    }
    
    [ClientRpc]
    public void DoAnimationClientRpc(string animationName) {
        creatureAnimator.SetTrigger(animationName);
    }
    [ClientRpc]
    public void DoAnimationClientRpc(string animationName, bool value) {
        creatureAnimator.SetBool(animationName,value);
    }

    private void MonsterLogger(String x, bool pleaseReport = false)
    {
        Debug.Log($"[SCP966SleepKiller][SCP966 - AI CLASS][{(pleaseReport? "This Error should be reported with the context of the error":"NO NEED REPORT")}] :: " + x);
    }


}