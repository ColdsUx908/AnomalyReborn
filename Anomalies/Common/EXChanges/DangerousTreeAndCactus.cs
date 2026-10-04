namespace Anomalies.Common.EXChanges;

public sealed class DangerousTreeAndCactus : GlobalTile
{
    public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (!EX)
            return;

        if (type == TileID.Trees)
        {
            int mossHornetType = Main.rand.Next(5) switch
            {
                0 => NPCID.TinyMossHornet,
                1 => NPCID.LittleMossHornet,
                2 => NPCID.MossHornet,
                3 => NPCID.BigMossHornet,
                4 => NPCID.GiantMossHornet,
                _ => NPCID.MossHornet
            };
            NPC.NewNPCAction(null, new Point(i, j).ToWorldCoordinates(), mossHornetType);
        }
    }

    public override void Load()
    {
        TileID.Sets.TouchDamageImmediate[TileID.Trees] = 30;
        On_Collision.CanTileHurt += On_Collision_CanTileHurt;
    }

    private static bool On_Collision_CanTileHurt(On_Collision.orig_CanTileHurt orig, ushort type, int i, int j, Player player)
    {
        if (type == TileID.Cactus && EX)
            return true;

        if (type == TileID.Trees)
            return EX;

        return orig(type, i, j, player);
    }
}
