namespace Anomalies.Bosses.BrainofCthulhu;

internal static class BrainIllusionHit
{
    public static void Send(int illusionIndex, int playerIndex)
    {
        if (Main.netMode == NetmodeID.Server)
            return;
        if (Main.netMode == NetmodeID.SinglePlayer)
        {
            Apply(illusionIndex, playerIndex);
            return;
        }

        if (playerIndex != Main.myPlayer)
            return;
        ModPacket packet = AnomalySynchronization.GetCAPacket();
        packet.Write(AnomalySynchronization.ID.BrainIllusionHit);
        packet.Write((byte)illusionIndex);
        packet.Send();
    }

    internal static void Apply(int illusionIndex, int playerIndex)
    {
        if (!BrainBehaviorAccess.ActiveBrain || (uint)illusionIndex >= Main.maxNPCs || (uint)playerIndex >= Main.maxPlayers)
            return;
        NPC illusion = Main.npc[illusionIndex];
        Player player = Main.player[playerIndex];
        if (!illusion.active || illusion.ModNPC is not FalseBrain brain || brain.BeenHit || !player.active || player.dead)
            return;
        if (Main.npc[NPC.crimsonBoss].GetBrainBehavior().CurrentBehavior != BrainofCthulhu.Behavior.IllusionTrick)
            return;
        brain.BeenHit = true;
        illusion.dontTakeDamage = true;
        illusion.netUpdate = true;
        Projectile.NewProjectileAction(source: illusion.GetSource_FromAI(), position: illusion.Center, velocity: Vector2.Zero, type: ModContent.ProjectileType<TelekineticBlast>(), damage: BrainofCthulhu.TelekineticBlastDamage, knockback: 0.5f, owner: Main.myPlayer, action: entity =>
        {
            entity.ai[0] = playerIndex;
            entity.ai[1] = 8;
            entity.ai[2] = illusionIndex;
        });
    }
}
