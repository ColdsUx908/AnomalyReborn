// Developed by ColdsUx

namespace Anomalies.Common;

public sealed class AnomalySynchronization : IAnomalyLoader
{
    public static class ID
    {
        public const byte SyncAnomalyMode = 0;
        public const byte SyncAnomalyModeFromServer = 1;
    }

    internal static ModPacket GetCAPacket() => AnomalyMain.Instance.GetPacket();

    public static void SyncAnomalyMode(int ignoreClient = -1)
    {
        if (!TOSharedData.Multiplayer)
            return;

        ModPacket packet = GetCAPacket();
        packet.Write(ID.SyncAnomalyMode);
        packet.Write(AnomalySharedData.Anomaly);
        packet.Send(-1, ignoreClient);
    }

    public static void SyncAnomalyModeFromServer(int toClient = -1)
    {
        if (!TOSharedData.Multiplayer)
            return;

        ModPacket packet = GetCAPacket();
        packet.Write(ID.SyncAnomalyModeFromServer);
        if (Main.dedServ)
            packet.Write(AnomalySharedData.Anomaly);
        packet.Send(toClient);
    }

    public static void HandlePacket(AnomalyMain mod, BinaryReader reader, int whoAmI)
    {
        byte id = reader.ReadByte();
        switch (id)
        {
            case ID.SyncAnomalyMode:
                AnomalySharedData.Anomaly = reader.ReadBoolean();
                if (Main.dedServ)
                    SyncAnomalyMode(whoAmI);
                break;
            case ID.SyncAnomalyModeFromServer:
                if (Main.dedServ)
                    SyncAnomalyModeFromServer(whoAmI);
                else
                    AnomalySharedData.Anomaly = reader.ReadBoolean();
                break;
        }
    }

    void IAnomalyLoader.Load()
    {
        TOSharedData.SyncEnabled = true;
    }
}
