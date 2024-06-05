using System;
using System.Collections;
using System.Security.Cryptography;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

namespace SCP966.Ai;
public class Scp966 : EnemyAI
{
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
    [Header("Audio")] 
    public AudioClip[] echoes;
    public AudioClip[] idles;
    private Coroutine passiveSound;
    private float waitTimer;

    private bool justStopedMoving = true;

    private bool weightModifiedLocal = false;
    private bool gettingScanned = false;

    private Coroutine killCoroutine;
    //Making sure that TargetPlayerVariable is synchronised
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
        if (IsHost)
        {
            passiveSound = StartCoroutine(IdlePlay());
        }
    }
    /// <summary>
    /// No access to UpdateMethod, so we will use the LateUpdate for a similar result
    /// </summary>
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
    /// <summary>
    /// Called by base class, decides current behaviour of AI
    /// </summary>
    public override void DoAIInterval()
    {
        base.DoAIInterval();
        switch(currentBehaviourStateIndex) {
            case (int)State.Searching:
                DoAnimationClientRpc("Walking",true);
                agent.autoBraking = true;
                agent.speed = 5f;
                agent.stoppingDistance = 1f;
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
                    agent.ResetPath();
                    StartSearch(transform.position);
                    SwitchToBehaviourClientRpc((int)State.Searching);
                    SetWeightPlayerClientRpc(Scp966TargetPlayer.playerClientId, 0f);
                    //Scp966TargetPlayer.carryWeight -= Mathf.Clamp(0.5f - 1f, 0.0f, 10f);
                    Scp966TargetPlayer = null;
                    break;
                }
                SetDestinationToPosition(Scp966TargetPlayer.transform.position);
                if (Vector3.Distance(transform.position, Scp966TargetPlayer.transform.position) <agent.stoppingDistance)
                {
                    
                    DoAnimationClientRpc("Walking",false);
                    if (justStopedMoving)
                    {
                        switch (RandomNumberGenerator.GetInt32(3))
                        {
                            case 0:
                                //NOTHING
                                MonsterLogger("WE ARE doing nothing");
                                break;
                            case 1:
                                //Growl
                                MonsterLogger("WE ARE SCREAMING");
                                DoAnimationClientRpc("Scream");
                                PlayScreamClientRpc(RandomNumberGenerator.GetInt32(echoes.Length));
                                break;
                            case 2:
                                //IDLE AN
                                MonsterLogger("Staring");
                                DoAnimationClientRpc("Stare");
                                break;
                            
                        }
                    }
                    justStopedMoving = false;

                }
                else
                {
                    justStopedMoving = true;
                    DoAnimationClientRpc("Walking",true);
                }

                if (!Scp966TargetPlayer.isPlayerDead)
                {
                    SetWeightPlayerClientRpc(Scp966TargetPlayer.playerClientId,1f);
                
                    CheckIfTargetHasLowStamClientRpc(Scp966TargetPlayer.playerClientId);
                }

                
                /*if (Scp966TargetPlayer.sprintMeter <= 0.3f)
                {
                    SwitchToBehaviourClientRpc((int)State.Chasing);
                }*/
                break;
            case (int)State.Chasing:
                agent.stoppingDistance = 0f;
                agent.speed = 5f;
                agent.acceleration = 12f;
                agent.autoBraking = false;
                DoAnimationClientRpc("Walking",true);
                if (!Scp966TargetPlayer.isInsideFactory || 
                    Scp966TargetPlayer.isPlayerDead || 
                    Vector3.Distance(
                        transform.position, 
                        Scp966TargetPlayer.transform.position) > 25)
                {
                    agent.ResetPath();
                    StartSearch(transform.position);
                    SwitchToBehaviourClientRpc((int)State.Searching);
                    SetWeightPlayerClientRpc(Scp966TargetPlayer.playerClientId,0f);
                        
                    Scp966TargetPlayer = null;
                    break;
                }
                SetDestinationToPosition(Scp966TargetPlayer.transform.position);
                

                //If reached player and attack cooldown is 0, then :
                if (
                    Vector3.Distance(
                        transform.position, 
                        Scp966TargetPlayer.transform.position
                        ) < 1f && 
                    attackCooldown<=0.3f
                    )
                {
                    DoAnimationClientRpc("Attack");
                    switch (creatureAnimator.GetInteger("AttackNumber"))
                    {
                        case 0:
                            SetCurrentDamageClientRpc(firstAttack,1);
                            break;
                        case 1:
                            
                            SetCurrentDamageClientRpc(secondAttack,2);
                            break;
                        case 2:
                            SetCurrentDamageClientRpc(thirdAttack,0);

                            break;
                        default:
                            MonsterLogger("We are outside of possible attack array!", true);
                            break;
                    }
                }
                if (!Scp966TargetPlayer.isPlayerDead)
                {
                    SetWeightPlayerClientRpc(Scp966TargetPlayer.playerClientId,1f);
                
                    CheckIfTargetHasLowStamClientRpc(Scp966TargetPlayer.playerClientId);
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


    /// <summary>
    /// Enable the mesh of SCP966
    /// </summary>
    private void EnableMesh()
    {
        isVisibleForLocalPlayer = true;
        if (mesh == null)
        {
            mesh = GetComponent<SkinnedMeshRenderer>();
        }
        mesh.enabled = true;
    }
    /// <summary>
    /// Disable the mesh of SCP966
    /// </summary>
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
        if (gettingScanned)
            return;
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

    public void StartScanCoroutine()
    {
        gettingScanned = true;
        StartCoroutine(ScanCoroutine());
    }

    IEnumerator ScanCoroutine()
    {
        EnableMesh();
        yield return new WaitForSeconds(0.1f);
        DisableMesh();
        gettingScanned = false;
    }
    /// <summary>
    /// When he gets hit
    /// </summary>
    /// <param name="force"></param>
    /// <param name="playerWhoHit"></param>
    /// <param name="playHitSFX"></param>
    /// <param name="hitID"></param>
    public override void HitEnemy(int force = 1, PlayerControllerB? playerWhoHit = null, bool playHitSFX = false, int hitID = -1) {
        base.HitEnemy(force, playerWhoHit, playHitSFX, hitID);
        SendIfTargetHasLowStamServerRpc();
        
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
                if (searchCoroutine != null)
                {
                    StopCoroutine(searchCoroutine);
                }
                KillEnemyOnOwnerClient();
                StopCoroutine(passiveSound);
                SetWeightPlayerClientRpc(Scp966TargetPlayer.playerClientId, 0f);
            }
        }
    }

    /// <summary>
    /// This little cute function will play an idle sound ever certain amount of time
    /// </summary>
    /// <returns></returns>
    IEnumerator IdlePlay()
    {
        while (!isEnemyDead)
        {
            yield return new WaitForSeconds(waitTimer);
            PlayOnShotIdleClientRpc(RandomNumberGenerator.GetInt32(idles.Length));
            waitTimer = 5 + RandomNumberGenerator.GetInt32(20);
        }
    }

    /// <summary>
    /// Play the idles on all playes
    /// </summary>
    /// <param name="x">The array number of the  played Idle</param>
    [ClientRpc]
    public void PlayOnShotIdleClientRpc(int x)
    {
        creatureVoice.PlayOneShot(idles[x]);
    }
    /// <summary>
    /// Called by animation and resolve damage on player.
    /// </summary>
    [ClientRpc]
    public void SwingAttackHitClientRpc() {
        int playerLayer = 1 << 3; // This can be found from the game's Asset Ripper output in Unity
        Collider[] hitColliders = Physics.OverlapBox(AttackArea.position, AttackArea.localScale, Quaternion.identity, playerLayer);
        if(hitColliders.Length > 0){
            foreach (var player in hitColliders){
                PlayerControllerB playerControllerB = MeetsStandardPlayerCollisionConditions(player);
                if (playerControllerB != null)
                {
                    creatureSFX.Play();
                    MonsterLogger(playerControllerB.health.ToString());
                    killCoroutine= StartCoroutine(killPlayer(playerControllerB));
                }
            }
        }
        attackCooldown = attackCooldownBeheader;
    }

    IEnumerator killPlayer(PlayerControllerB playerControllerB)
    {
        yield return new WaitForSeconds(0.3f);
        StopCoroutine(killCoroutine);
        playerControllerB.DamagePlayer(currentDamage);
        if (playerControllerB.health <= currentDamage)
        {
            SetWeightPlayerClientRpc(Scp966TargetPlayer.playerClientId,0f);
        }
        
    }
    /// <summary>
    /// Called by us
    /// </summary>
    [ClientRpc]
    public void PlayScreamClientRpc(int x)
    {
        creatureVoice.PlayOneShot(echoes[x]);
        
    }
    /// <summary>
    /// Called for Triggers
    /// </summary>
    /// <param name="animationName"></param>
    [ClientRpc]
    public void DoAnimationClientRpc(string animationName) {
        creatureAnimator.SetTrigger(animationName);
    }
    /// <summary>
    /// Called for booleans
    /// </summary>
    /// <param name="animationName"></param>
    /// <param name="value"></param>
    [ClientRpc]
    public void DoAnimationClientRpc(string animationName, bool value) {
        creatureAnimator.SetBool(animationName,value);
    }
    /// <summary>
    /// Will be called to start each attack animation
    /// </summary>
    /// <param name="x"> The damage that should be dealt</param>
    /// <param name="animation">The animation number from 0 to 2</param>
    [ClientRpc]
    public void SetCurrentDamageClientRpc(int x, int animation)
    {
        currentDamage = x;
        creatureAnimator.SetInteger("AttackNumber", animation);
        
    }
    /// <summary>
    /// Change the weight for a specific client. Will be called on all clients and sorted out later
    /// </summary>
    /// <param name="clientId">The client Id we want to modify the weight</param>
    /// <param name="weight">The new Weight</param>
    [ClientRpc]
    public void SetWeightPlayerClientRpc(ulong clientId, float weight)
    {
        if (RoundManager.Instance.playersManager.localPlayerController.playerClientId == clientId)
        {
            if (weight == 0f)
            {
                PlayerControllerB player = RoundManager.Instance.playersManager.localPlayerController;
                float totalWeight = 0;
                foreach (var item in player.ItemSlots)
                {
                    if (item == null) continue;
                    if (item.gameObject.GetComponent<GrabbableObject>() == null) continue;
                    totalWeight+= item.gameObject.GetComponent<GrabbableObject>().itemProperties.weight;
                    
                }

                player.carryWeight = 1 + totalWeight;

            }
            else if(weight ==1f)
            {
                PlayerControllerB player = RoundManager.Instance.playersManager.localPlayerController;
                float totalWeight = 0;
                foreach (var item in player.ItemSlots)
                {
                    if (item == null) continue;
                    if (item.gameObject.GetComponent<GrabbableObject>() == null) continue;
                    totalWeight+= item.gameObject.GetComponent<GrabbableObject>().itemProperties.weight;
                
                }

                player.carryWeight = 1 + totalWeight + 0.5f;
            }
        }
    }
    /// <summary>
    /// Send to the client to check if their stamina is low! They will respond with SendIfTargetHasLowStamserverRpc if true and they are the target player
    /// </summary>
    /// <param name="clientId">The client id of the target player</param>
    [ClientRpc]
    public void CheckIfTargetHasLowStamClientRpc(ulong clientId)
    {
        if (RoundManager.Instance.playersManager.localPlayerController == Scp966TargetPlayer)
        {
            if (Scp966TargetPlayer.sprintMeter <= 0.3f)
            {
                SendIfTargetHasLowStamServerRpc();
            }
        }
    }
    
    
    /// <summary>
    /// Called by client to tell the server that They are low stam and should change to Chasing
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void SendIfTargetHasLowStamServerRpc()
    {
        SwitchToBehaviourClientRpc((int)State.Chasing);
    }

    private void MonsterLogger(String x, bool pleaseReport = false)
    {
        Debug.Log($"[SCP966SleepKiller][SCP966 - AI CLASS][{(pleaseReport? "This Error should be reported with the context of the error":"NO NEED REPORT")}] :: " + x);
    }
}