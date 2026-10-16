namespace Anomalies.Bosses.Skeletron.LegendaryNight.Entities;

public sealed class FlyingSlime : AnomalyModNPC
{
    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;

    public override void SetStaticDefaults() => Main.npcFrameCount[Type] = 5;

    public override void SetDefaults()
    {
        NPC.width = NPC.height = 30;
        NPC.scale = 1f;
        AnimationType = NPCID.Slimer;
        NPC.lifeMax = 30;
        NPC.damage = 20;
        NPC.defense = 0;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.knockBackResist = 0.05f;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.IsImportantBossMinion = true;
    }

    public override bool CheckActive() => false;

    public override void AI()
    {
        if (!NPC.TryGetMaster(NPCID.SkeletronHead, out NPC master) || master.Anomaly.CustomAIType != 1
            || SkeletronLegendHead.GetInstance(master).CurrentAvatar != SkeletronLegendHead.Avatar.FlyingSlime
            || !NPC.TargetClosestIfInvalid(true, SkeletronLegendHead.DespawnDistance) || ++NPC.ai[0] >= 180f)
        {
            SpawnDisappearanceParticles();
            NPC.active = false;
            if (TOSharedData.NotClient)
                NetMessage.SendData(MessageID.SyncNPC, number: NPC.whoAmI);
            return;
        }

        Lighting.AddLight(NPC.Center, LegendaryNightHandler.SlimeColor.ToVector3());
        if (NPC.ai[0] < 30f)
        {
            NPC.damage = 0;
            NPC.velocity *= 0.9f;
        }
        else
        {
            NPC.damage = NPC.defDamage;
            if (NPC.ai[0] == 30f && TOSharedData.NotClient)
            {
                NPC.velocity = NPC.GetVelocityTowards(NPC.PlayerTarget, 20f);
                NPC.netUpdate = true;
            }
        }
    }

    public override void FindFrame(int frameHeight)
    {
        // AnimationType 的原版 FindFrame 先执行，这里覆盖其较大的倾斜幅度，保留振翅帧动画。
        NPC.rotation = NPC.velocity.X * 0.03f;
        // 当前贴图默认朝左；默认 NPC 绘制在 spriteDirection == 1 时水平翻转。
        // 静止时保留上一朝向，避免蓄力期间左右抖动。
        if (NPC.velocity.X != 0f)
            NPC.spriteDirection = NPC.velocity.X > 0f ? 1 : -1;
    }

    public override void HitEffect(NPC.HitInfo hit)
    {
        if (NPC.life <= 0)
            SpawnDisappearanceParticles();
    }

    private void SpawnDisappearanceParticles()
    {
        if (Main.dedServ)
            return;
        for (int i = 0; i < 20; i++)
            ParticleHandler.SpawnParticle(new OrbParticle(NPC.Center, Main.rand.NextPolarVector2(1f, 5f),
                Main.rand.Next(20, 35), Main.rand.NextFloat(0.2f, 0.5f), LegendaryNightHandler.SlimeColor, lifeEndRatio: 0.2f));
    }
}
