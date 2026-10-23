namespace Anomalies.Bosses.BrainofCthulhu;

public class TelekineticBlast : AnomalyModProjectile
{
    public override string LocalizationCategory => "Bosses.BrainofCthulhu";
    public override string Texture => TOTextures.InvisibleTexturePath;

    public override void SetDefaults()
    {
        Projectile.width = 1;
        Projectile.height = 1;
        Projectile.penetrate = -1;
        Projectile.Opacity = 1f;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 10;
        Projectile.damage = 0;
        Projectile.scale = 1;
        Projectile.hostile = true;
        Projectile.netImportant = true;
    }

    public Player target => Main.player[(int)Projectile.ai[0]];
    public float debuffMultiplier => Main.npc[NPCSource].type == NPCID.BrainofCthulhu ? 2f : 1f;
    public int delay { get => (int)Projectile.ai[1]; set => Projectile.ai[1] = value; }
    public int NPCSource => (int)Projectile.ai[2];

    public override void OnSpawn(IEntitySource source)
    {
        Projectile.netUpdate = true;
    }

    public override void AI()
    {
        if (!BrainBehaviorAccess.ActiveBrain || (uint)NPCSource >= Main.maxNPCs || (uint)Projectile.ai[0] >= Main.maxPlayers || !target.active || target.dead || Main.npc[NPC.crimsonBoss].GetBrainBehavior().AttackFlag || Main.npc[NPC.crimsonBoss].GetBrainBehavior().CurrentBehavior == BrainofCthulhu.Behavior.DeathAnimation)
        {
            Projectile.active = false;
            return;
        }

        if (--delay > 0)
            return;
        if (Main.npc[NPCSource].ModNPC is FalseBrain illusion)
        {
            illusion.BeenHit = true;
            Main.npc[NPCSource].netUpdate = true;
        }

        for (int i = 0; i < 6; i++)
        {
            Vector2 dir = target.Center - Projectile.Center;
            int lifeTime = 24;
            dir /= lifeTime / 2f * 5f;
            dir *= i;
            PulseRing pulse = new(Projectile.Center, dir, i % 2 == 0 ? Color.Red : Color.Orange, new Vector2(0.5f, 1), dir.ToRotation(), 0f, i / 5f, lifeTime + 8);
            ParticleHandler.SpawnParticle(pulse);
        }

        SoundEngine.PlaySound(BrainofCthulhu.LaughSound, Projectile.Center);
        // Player damage and debuffs are resolved once, by the affected player's client.
        if (Main.netMode != NetmodeID.SinglePlayer && target.whoAmI != Main.myPlayer)
        {
            Projectile.active = false;
            return;
        }

        target.AddBuff(BuffID.Darkness, (int)Math.Round(900 * debuffMultiplier));
        target.AddBuff(BuffID.Bleeding, (int)Math.Round(900 * debuffMultiplier));
        target.AddBuff(BuffID.Confused, (int)Math.Round(60 * debuffMultiplier));
        target.Hurt(PlayerDeathReason.ByCustomReason(Language.GetText("Mods.Anomalies.Bosses.BrainofCthulhu.Death.BrainIllusion" + Main.rand.Next(1, 3 + 1)).ToNetworkText(target.name)), 100, Main.npc[NPC.crimsonBoss].Center.X > target.Center.X ? -1 : 1, cooldownCounter: 0, dodgeable: false, scalingArmorPenetration: 1f);
        BridgeUtils.ResetCalamityAdrenaline(target);
        Projectile.active = false;
    }
}
