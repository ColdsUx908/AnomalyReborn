namespace Anomalies.Bosses.Skeletron;

/// <summary>普通骷髅王三阶段的小守卫：固定路线预警后飞行，或作为竖直匀速弹幕雨。</summary>
/// <remarks>ai[0]：头部索引 + 1；ai[1]：年龄；ai[2]：0 对径加速，1 径向收束（保留），2 匀速下落。</remarks>
public sealed class SkeletronPhase3Guardian : AnomalyModProjectile
{
    public override string Texture => TOAssetUtils.FormatVanillaProjectileTexturePath(ProjectileID.BabySkeletronHead);
    public override string LocalizationCategory => "Bosses.Skeletron";
    public Vector2 Destination { get; set; }
    private bool IsRain => Projectile.ai[2] == 2f;
    private int WarningDuration => IsRain ? 0 : Projectile.ai[2] == 0f ? 48 : 60;

    public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 42;
        Projectile.hostile = true;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.netImportant = true;
        Projectile.timeLeft = 240;
    }

    protected override void SendAnomalyExtraAI(BinaryWriter writer)
    {
        base.SendAnomalyExtraAI(writer);
        writer.WriteVector2(Destination);
    }

    protected override void ReceiveAnomalyExtraAI(BinaryReader reader)
    {
        base.ReceiveAnomalyExtraAI(reader);
        Destination = reader.ReadVector2();
    }

    public override bool ShouldUpdatePosition() => false;
    public override bool? CanDamage() => Projectile.ai[1] > WarningDuration ? null : false;

    public override void AI()
    {
        int headIndex = (int)Projectile.ai[0] - 1;
        if ((uint)headIndex >= Main.maxNPCs || Main.npc[headIndex] is not { active: true, type: NPCID.SkeletronHead }
            || SkeletronHead.GetInstance(Main.npc[headIndex]) is not { ShouldProcess: true, CurrentPhase: SkeletronHead.Phase.Phase3 } head
            || head.CurrentBehavior == SkeletronHead.Behavior.Despawn)
        {
            if (TONetworkEnvironment.NotClient)
                Projectile.Kill();
            return;
        }

        int age = (int)Projectile.ai[1]++;
        Projectile.rotation += 0.3f;
        if (IsRain)
        {
            Projectile.Center += Projectile.velocity;
            return;
        }
        if (age < WarningDuration)
            return;
        if (age == WarningDuration && !Main.dedServ)
            SoundEngine.PlaySound(SoundID.Item21, Projectile.Center);

        Vector2 edge = Destination - Projectile.Center;
        float distance = edge.Length();
        float speed = Projectile.ai[2] == 0f ? Math.Min(20f, 12f + (age - WarningDuration) * 0.4f) : 10f;
        if (distance <= speed)
        {
            if (TONetworkEnvironment.NotClient)
                Projectile.Kill();
            return;
        }
        Projectile.velocity = edge * (speed / distance);
        Projectile.Center += Projectile.velocity;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        if (Projectile.ai[1] <= WarningDuration)
            return false;
        float width = Projectile.width * Projectile.scale;
        Vector2 extent = Projectile.velocity.SafeNormalize(Vector2.UnitY) * (width * 0.5f);
        return RotatedRectangle.FromLineSegment(Projectile.Center - Projectile.velocity - extent,
            Projectile.Center + extent, width).Collides(targetHitbox);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        bool warning = Projectile.ai[1] <= WarningDuration;
        if (warning)
        {
            Vector2 edge = Destination - Projectile.Center;
            float opacity = MathHelper.Clamp(Projectile.ai[1] / 10f, 0f, 1f);
            AnomalyDrawUtils.DrawLayeredWarning(Main.spriteBatch, Projectile.Center, edge.ToRotation(), edge.Length(),
                Projectile.width, SkeletronHandler.BoneColor, Color.White, opacity * 0.7f);
        }
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null,
            SkeletronHandler.BoneColor * (warning ? 0.4f : 1f), Projectile.rotation, texture.Size() * 0.5f, Projectile.scale,
            SpriteEffects.None);
        return false;
    }
}
