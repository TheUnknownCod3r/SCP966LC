using System.Collections.Generic;
using SCP966.Ai;
using UnityEngine;

namespace SCP966.SCP966Manager;

public class Scp966Manager : MonoBehaviour
{
    private static Scp966Manager _instance;
    public static Scp966Manager Instance => _instance;

    private List<SCP966.Ai.Scp966> _allScp966Instances = new List<SCP966.Ai.Scp966>();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        _instance = this;
    }

    public void RegisterScp966Instance(SCP966.Ai.Scp966 instance)
    {
        _allScp966Instances.Add(instance);
    }

    public void UnregisterScp966Instance(SCP966.Ai.Scp966 instance)
    {
        _allScp966Instances.Remove(instance);
    }

    public void StartScanOnAllInstances()
    {
        foreach (var scp966 in _allScp966Instances)
        {
            scp966.StartScanCoroutine();
        }
    }
}