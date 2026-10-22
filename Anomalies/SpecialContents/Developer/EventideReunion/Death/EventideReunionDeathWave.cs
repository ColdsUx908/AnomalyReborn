namespace Anomalies.SpecialContents.Developer.EventideReunion;

public sealed partial class EventideReunionDeathWave : AnomalyModProjectile
{
    public override string LocalizationCategory => EventideReunion.LocalizationCategoryForEventideReunion;
    public override string Texture => EventideReunionHoldout.AssetPath + "SpearTrail";
    private const int Lifetime = 45;
    private float Progress => MathHelper.Clamp((Lifetime - Projectile.timeLeft + 1f) / EventideReunionDeathSkill.SlashDuration, 0f, 1f);
    private Vector2 Aim => Projectile.velocity.SafeNormalize(Vector2.UnitX);
    private float SwingSign => Projectile.ai[1] < 0f ? -1f : 1f;
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 112;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = Lifetime;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }
    public override void AI()
    {
        Projectile.rotation = Projectile.velocity.ToRotation();
        Projectile.velocity *= 0.97f;
        EmitFlightTrail();
    }
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        // 与绘制使用同一个投影弧面，避免只在弹幕中心的方框内造成伤害。
        for (int i = 0; i <= 48; i++)
        {
            Vector2 vector = EventideReunionDeathGeometry.SlashVector(Progress * i / 48f, Aim, SwingSign);
            Vector2 outer = Projectile.Center + vector;
            Vector2 inner = Projectile.Center + vector * EventideReunionDeathGeometry.InnerRatio;
            float distance = 0f;
            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), inner, outer, 16f, ref distance))
                return true;
            // 包含本帧运动扫过的刃口，快速飞行时不漏掉薄目标。
            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), outer - Projectile.velocity,
                outer, 16f, ref distance))
                return true;
        }
        return false;
    }
    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;
        // 命中结算前登记，首次镰刀波直接击杀也会生成灵魂。
        EventideReunionDeathMark.Apply(target, (int)Projectile.ai[0]);
    }
    public override bool PreDraw(ref Color lightColor)
    {
        float fade = MathHelper.SmoothStep(0f, 1f, Math.Min(1f, Projectile.timeLeft / 12f));
        Main.spriteBatch.End();
        EventideReunionDeathDrawing.DrawCrescent(Projectile.Center, Aim, SwingSign, Progress, fade);
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
            DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        return false;
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => EventideReunionDeathMark.CompleteHit(target);
}
