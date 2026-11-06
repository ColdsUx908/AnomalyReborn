using static Anomalies.Bosses.Skeletron.LegendaryNight.LegendaryNightStarburstTimeline;
using static Anomalies.Bosses.Skeletron.LegendaryNight.LegendaryNightStarburstParameters;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>
/// 单次星爆，负责预警、音效、帧伤与淡出。
/// </summary>
/// <remarks>
/// <para>ai[0]：主体 NPC 索引 + 1；ai[1]：星爆序号（0..11），偶数实心圆、奇数圆环；ai[2]：未使用。</para>
/// <para>localAI：未使用；框架 Timer1..5：未使用；年龄由主体音乐帧减去星爆节点计算。</para>
/// <para>无实体专属 ExtraAI；爆点与半径通过所属竞技场几何恢复，服务器生成。</para>
/// </remarks>
public sealed class LegendStarburst : AnomalyModProjectile
{
    internal SkeletronLegendHead Head => LegendaryNightHandler.TryGetLegendaryNightHead((int)Projectile.ai[0], out SkeletronLegendHead head, oneBased: true) ? head : null;
    private LegendaryNightPhase2Arena Arena => Head?.Phase2Arena;
    private int Index => (int)Projectile.ai[1];
    private int Age => (Head?.Phase2MusicElapsedTime ?? 0) - GetStarburstTime(Index);
    private bool IsRing => Index % 2 == 1;
    private float Radius => (Arena?.Radius ?? 0f) * (IsRing ? 2.175f : 1.45f);
    private float SafeRadius => (Arena?.Radius ?? 0f) * 0.625f;
    private float Opacity => Age < -StarburstWarningDuration ? 0f
        : Age < 0 ? 0.22f * (1f + Age / (float)StarburstWarningDuration)
        : 0.65f * MathHelper.Clamp((StarburstDamageDuration + StarburstFadeDuration - Age) / (float)StarburstFadeDuration, 0f, 1f);
    private bool _Hit;

    public override string Texture => TOTextures.InvisibleTexturePath;
    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;
    public override bool? CanDamage() => false;
    public override bool ShouldUpdatePosition() => false;
    public override bool PreDraw(ref Color lightColor) => false;
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 16;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.netImportant = true;
        Projectile.hide = true;
        Projectile.timeLeft = 100;
    }
    public override void AI()
    {
        if (Head is not { StarburstActive: true } || Arena is not LegendaryNightPhase2Arena arena || Age >= StarburstDamageDuration + StarburstFadeDuration)
        {
            if (TONetworkEnvironment.NotClient)
                Projectile.Kill();
            return;
        }
        Projectile.timeLeft = 100;
        Projectile.Center = arena.PredictStarburstPosition(Index);
        if (Main.dedServ)
            return;
        if (Age == -StarburstSoundLeadTime)
            SoundEngine.PlaySound(StarburstSound);
        if (Age is >= 0 and < StarburstDamageDuration && Colliding(Projectile.Hitbox, Main.LocalPlayer.Hitbox) == true)
        {
            if (_Hit || !Main.LocalPlayer.active || Main.LocalPlayer.dead || Main.LocalPlayer.ghost)
                return;
            _Hit = true;
            LegendaryNightDamage.HurtLocalPlayer(Projectile, Projectile.damage, true);
        }
    }
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => IsRing
        ? new Annulus(Projectile.Center, SafeRadius, Radius).Collides(targetHitbox)
        : new Circle(Projectile.Center, Radius).Collides(targetHitbox);
    internal static void EnsureAttack(SkeletronLegendHead head)
    {
        if (!TONetworkEnvironment.NotClient || !head.StarburstActive || head.Phase2Arena is not LegendaryNightPhase2Arena arena)
            return;
        int time = head.Phase2MusicElapsedTime;
        int index = GetStarburstIndex(time);
        if (index < 0 || time >= GetStarburstTime(index) + StarburstDamageDuration + StarburstFadeDuration)
            return;
        foreach (Projectile other in Projectile.ActiveProjectiles)
            if (other.ModProjectile is LegendStarburst && (int)other.ai[0] == head.NPC.whoAmI + 1 && (int)other.ai[1] == index)
                return;
        Projectile.NewProjectileAction<LegendStarburst>(head.NPC.GetSource_FromAI(), arena.PredictStarburstPosition(index),
            Vector2.Zero, arena.Projectile.damage, 0f, action: p =>
            {
                p.ai[0] = head.NPC.whoAmI + 1;
                p.ai[1] = index;
            });
    }
    internal void Draw(SpriteBatch spriteBatch)
    {
        if (Arena is null || Opacity <= 0f)
            return;
        Vector2 center = Arena.PredictStarburstPosition(Index);
        float opacity = Opacity;
        Texture2D circle = AnomalyTextures.Extra.BasicCircleLarge;
        using RenderTargetLease lease = RenderTargetPool.Shared.Rent(Main.instance.GraphicsDevice,
            Main.screenWidth, Main.screenHeight, RenderTargetDescriptor.Default);
        using (lease.Scope(clearColor: Color.Transparent))
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            spriteBatch.DrawFromCenter(circle, center - Main.screenPosition, null, Color.White,
                scale: Radius * 2f / circle.Width);
            spriteBatch.End();
            if (IsRing)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, TODrawUtils.EraseBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                spriteBatch.DrawFromCenter(circle, center - Main.screenPosition, null, Color.White,
                    scale: SafeRadius * 2f / circle.Width);
                spriteBatch.End();
            }
        }
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
            DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);
        spriteBatch.Draw(lease.Target, Vector2.Zero, (Age < 0 ? Color.Red : Color.White) * opacity);
        spriteBatch.End();
        Color color = Age < 0 ? Color.Red : Color.White;
        AnomalyDrawUtils.DrawGlowingCircle(spriteBatch, center, Radius, color, opacity,
            Age < 0 ? 3f : LegendaryNightPhase2Arena.LaserWidth);
        if (IsRing)
            AnomalyDrawUtils.DrawGlowingCircle(spriteBatch, center, SafeRadius, color, opacity,
                Age < 0 ? 3f : LegendaryNightPhase2Arena.LaserWidth);
    }
}