namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>
/// 自身无伤害的终幕陨星，落点生成成功或失败冲击波。
/// </summary>
/// <remarks>
/// <para>ai[0]：主体 NPC 索引 + 1；ai[1]：失败冲击波最大半径；ai[2]：成功击杀标记（1为成功）。</para>
/// <para>localAI：未使用；框架 Timer1..5：未使用；主体存在时跟随音乐，消失后由 timeLeft 接续。</para>
/// <para>Projectile.Center 保存命中点；无实体专属 ExtraAI。</para>
/// </remarks>
public sealed class LegendFinalMeteor : AnomalyModProjectile
{
    public override string Texture => TOTextures.InvisibleTexturePath;
    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;
    public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 2400;
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 16;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.netImportant = true;
        Projectile.timeLeft = SkeletronLegendHead.Mechanic7Parameters.MeteorFallDuration + 1;
    }

    public override bool ShouldUpdatePosition() => false;
    public override bool? CanDamage() => false;

    private int Age => LegendaryNightHandler.TryGetLegendaryNightHead((int)Projectile.ai[0], out SkeletronLegendHead head, oneBased: true)
        ? head.Phase2MusicElapsedTime - SkeletronLegendHead.Mechanic7Timeline.FinalMeteorStartMusicTime
        : SkeletronLegendHead.Mechanic7Parameters.MeteorFallDuration + 1 - Projectile.timeLeft;

    public override void AI()
    {
        if (LegendaryNightHandler.TryGetLegendaryNightHead((int)Projectile.ai[0], out SkeletronLegendHead head, oneBased: true))
            Projectile.Center = head.NPC.Top + new Vector2(0f, 8f);
        int age = Age;
        // 在同一帧先更新天空，再结算命中并移除陨星。
        if (!Main.dedServ)
            LegendaryNightSky.Instance?.UpdateFinalMeteorBlend(this, (int)Projectile.ai[0] - 1, age);
        if (age >= SkeletronLegendHead.Mechanic7Parameters.MeteorFallDuration)
        {
            if (TONetworkEnvironment.NotClient)
            {
                bool success = Projectile.ai[2] == 1f;
                if (!success && head is not null)
                {
                    head.CurrentBehavior = SkeletronLegendHead.Behavior.Despawn;
                    head.NPC.active = false;
                    head.NPC.netUpdate = true;
                    if (Main.netMode == NetmodeID.Server)
                        NetMessage.SendData(MessageID.SyncNPC, number: head.NPC.whoAmI);
                }
                Projectile.NewProjectileAction<LegendFinalShockwave>(SourceAI, Projectile.Center, Vector2.Zero,
                    success ? 0 : 1, 0f, action: p =>
                    {
                        // 独立计时，主体死亡后仍完成终幕。
                        p.ai[1] = Projectile.ai[1];
                        p.netUpdate = true;
                    });
            }
            Projectile.Kill();
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        SkeletronLegendHead.DrawDescendingStar(Main.spriteBatch, Main.screenPosition, Math.Max(0, Age), Projectile.scale, PositionAtAge);
        return false;
    }

    private Vector2 PositionAtAge(int age)
    {
        float progress = MathHelper.Clamp(age / (float)SkeletronLegendHead.Mechanic7Parameters.MeteorFallDuration, 0f, 1f);
        return Projectile.Center - Vector2.UnitY * (1600f * (1f - progress * progress));
    }
}
