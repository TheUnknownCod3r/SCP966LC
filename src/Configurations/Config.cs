using System;
using BepInEx.Configuration;
using SCP966;
using Unity.Collections;
using Unity.Netcode;

namespace SCP1507.Configurations;

[Serializable]
public class Config : SyncedInstance<Config>
{ 
    public ConfigEntry<float> TIME_YOU_SEE_MONSTER { get; private set; } //Implemented
    public ConfigEntry<int> RARITY { get; private set; } //Implemented
    
    public Config(ConfigFile cfg)
    {
        InitInstance(this);
        TIME_YOU_SEE_MONSTER = cfg.Bind("Visibility", "Time you see the monster", 0.2f,
            "The amount of time you will see SCP 966 upon scan. It should not be higher than 1.2"
        );
        RARITY = cfg.Bind("Spawning", "Rarity", 30,
            "The rarity of SCp 966 on all the moons"
        );



    }
    public static void RequestSync() {
        if (!IsClient) return;

        using FastBufferWriter stream = new(IntSize, Allocator.Temp);
        MessageManager.SendNamedMessage("Xilef992SCP966_OnRequestConfigSync", 0uL, stream);
    }
    public static void OnRequestSync(ulong clientId, FastBufferReader _) {
        if (!IsHost) return;

        Plugin.Logger.LogInfo($"Config sync request received from client: {clientId}");

        byte[] array = SerializeToBytes(Instance);
        int value = array.Length;

        using FastBufferWriter stream = new(value + IntSize, Allocator.Temp);

        try {
            stream.WriteValueSafe(in value, default);
            stream.WriteBytesSafe(array);

            MessageManager.SendNamedMessage("Xilef992SCP966_OnReceiveConfigSync", clientId, stream);
        } catch(Exception e) {
            Plugin.Logger.LogInfo($"Error occurred syncing config with client: {clientId}\n{e}");
        }
    }
    public static void OnReceiveSync(ulong _, FastBufferReader reader) {
        if (!reader.TryBeginRead(IntSize)) {
            Plugin.Logger.LogError("Config sync error: Could not begin reading buffer.");
            return;
        }

        reader.ReadValueSafe(out int val, default);
        if (!reader.TryBeginRead(val)) {
            Plugin.Logger.LogError("Config sync error: Host could not sync.");
            return;
        }

        byte[] data = new byte[val];
        reader.ReadBytesSafe(ref data, val);

        SyncInstance(data);

        Plugin.Logger.LogInfo("Successfully synced config with host.");
    }
    
}