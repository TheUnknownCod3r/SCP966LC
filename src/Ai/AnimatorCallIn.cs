using UnityEngine;

namespace SCP966.Ai;

public class AnimatorCallIn :MonoBehaviour
{
    public Scp966 scpScript;
    public void Attack()
    {
        scpScript.SwingAttackHitClientRpc();
    }
}