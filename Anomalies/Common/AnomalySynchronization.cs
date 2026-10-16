namespace Anomalies.Common;

public sealed class AnomalySynchronization : IAnomalyLoader
{
    public static class ID
    {
        public const byte SyncAnomalyMode = 0;
        public const byte SyncAnomalyModeFromServer = 1;
        public const byte SyncWorldState = 2;
    }

    internal static ModPacket GetCAPacket() => AnomalyMain.Instance.GetPacket();

    internal static bool ReceivingWorldState { get; private set; }

    public static void RequestAnomalyMode(bool enabled)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            return;
        ModPacket packet = GetCAPacket();
        packet.Write(ID.SyncAnomalyMode);
        packet.Write(enabled);
        packet.Send();
    }

    public static void SyncAnomalyMode(int ignoreClient = -1)
    {
        if (Main.netMode != NetmodeID.Server || ReceivingWorldState)
            return;

        ModPacket packet = GetCAPacket();
        packet.Write(ID.SyncWorldState);
        WriteWorldState(packet);
        packet.Send(-1, ignoreClient);
    }

    public static void SyncAnomalyModeFromServer(int toClient = -1)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            return;

        ModPacket packet = GetCAPacket();
        packet.Write(Main.netMode == NetmodeID.Server ? ID.SyncWorldState : ID.SyncAnomalyModeFromServer);
        if (Main.netMode == NetmodeID.Server)
            WriteWorldState(packet);
        packet.Send(toClient);
    }

    public static void HandlePacket(AnomalyMain mod, BinaryReader reader, int whoAmI)
    {
        byte id = reader.ReadByte();
        switch (id)
        {
            case ID.SyncAnomalyMode:
                bool requested = reader.ReadBoolean();
                if (Main.netMode == NetmodeID.Server && (uint)whoAmI < Main.maxPlayers && Main.player[whoAmI].active)
                {
                    if (!TOSharedData.BossActive && (!requested || TOSharedData.MasterMode))
                        AnomalySharedData.Anomaly = requested;
                    // Always reply, including when the request was rejected or was a no-op.
                    SyncAnomalyModeFromServer(whoAmI);
                }
                break;
            case ID.SyncAnomalyModeFromServer:
                if (Main.netMode == NetmodeID.Server)
                    SyncAnomalyModeFromServer(whoAmI);
                break;
            case ID.SyncWorldState:
                if (Main.netMode == NetmodeID.MultiplayerClient)
                    ReadWorldState(reader);
                break;
        }
    }

    internal static void WriteWorldState(BinaryWriter writer)
    {
        BitsByte flags = new();
        flags[0] = AnomalySharedData.Anomaly;
        flags[1] = AnomalySharedData.AnomalyUltramundane;
        flags[2] = AnomalySharedData.StoryMode;
        flags[3] = AnomalySharedData.EXMode;
        writer.Write((byte)flags);
    }

    internal static void ReadWorldState(BinaryReader reader)
    {
        ApplyWorldState(reader.ReadByte());
    }

    internal static void ApplyWorldState(BitsByte flags)
    {
        bool wasReceiving = ReceivingWorldState;
        ReceivingWorldState = true;
        try
        {
            AnomalySharedData.Anomaly = flags[0];
            AnomalySharedData.AnomalyUltramundane = flags[0] && flags[1];
            AnomalySharedData.StoryMode = flags[2];
            AnomalySharedData.EXMode = flags[3];
        }
        finally
        {
            ReceivingWorldState = wasReceiving;
        }
    }

    void IAnomalyLoader.Load()
    {
        TOSharedData.SyncEnabled = true;
    }
}

