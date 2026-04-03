namespace CalamityAnomalies.Core;

public sealed class CANetSync : ICALoader
{
    public static class ID
    {
        public const byte SyncAnomalyMode = 0;
        public const byte SyncAnomalyModeFromServer = 1;
    }

    internal static ModPacket GetCAPacket() => CAMain.Instance.GetPacket();

    public static void SyncAnomalyMode(int ignoreClient = -1)
    {
        if (!TOSharedData.Multiplayer)
            return;

        ModPacket packet = GetCAPacket();
        packet.Write(ID.SyncAnomalyMode);
        packet.Write(CASharedData.Anomaly);
        packet.Send(-1, ignoreClient);
    }

    public static void SyncAnomalyModeFromServer(int toClient = -1)
    {
        if (!TOSharedData.Multiplayer)
            return;

        ModPacket packet = GetCAPacket();
        packet.Write(ID.SyncAnomalyModeFromServer);
        if (Main.dedServ)
            packet.Write(CASharedData.Anomaly);
        packet.Send(toClient);
    }

    public static void HandlePacket(CAMain mod, BinaryReader reader, int whoAmI)
    {
        byte id = reader.ReadByte();
        switch (id)
        {
            case ID.SyncAnomalyMode:
                CASharedData.Anomaly = reader.ReadBoolean();
                if (Main.dedServ)
                    SyncAnomalyMode(whoAmI);
                break;
            case ID.SyncAnomalyModeFromServer:
                if (Main.dedServ)
                    SyncAnomalyModeFromServer(whoAmI);
                else
                    CASharedData.Anomaly = reader.ReadBoolean();
                break;
        }
    }

    void ICALoader.Load()
    {
        TOSharedData.SyncEnabled = true;
    }
}
