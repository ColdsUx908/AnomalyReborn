namespace Anomalies.Bosses.Skeletron;

/// <summary>骷髅王发射的旋转加速十字骨。</summary>
/// <remarks>ai[0] 为加速上限，由生成方指定。ai[1] 为所属头部索引（1-based）。</remarks>
public sealed class Crossbone : AnomalyModProjectile
{
    public override string Texture => TOAssetUtils.FormatVanillaProjectileTexturePath(ProjectileID.BoneGloveProj);
    public override string LocalizationCategory => "Bosses.Skeletron";

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 26;
        Projectile.alpha = 255;
        Projectile.hostile = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = 3;
        Projectile.timeLeft = 600;
    }

    public override void AI()
    {
        int headIndex = (int)Projectile.ai[1] - 1;
        // 主体死亡后继续飞行，仅在仍存活的主体主动退场时清理。
        if ((uint)headIndex < Main.maxNPCs
            && Main.npc[headIndex] is { active: true, type: NPCID.SkeletronHead }
            && SkeletronHead.GetInstance(Main.npc[headIndex]) is
                { ShouldProcess: true, CurrentBehavior: SkeletronHead.Behavior.Despawn })
        {
            if (TONetworkEnvironment.NotClient)
                Projectile.Kill();
            return;
        }
        UpdateAcceleratingFlight(Projectile, Projectile.ai[0]);
        Projectile.spriteDirection = Projectile.direction;
        Projectile.rotation += MathHelper.Pi / 90f * Projectile.velocity.Length() * Projectile.direction;
    }

    private static void UpdateAcceleratingFlight(Projectile projectile, float maxSpeed)
    {
        projectile.alpha = Math.Max(0, projectile.alpha - 75);
        float speed = projectile.velocity.Length();
        if (speed < maxSpeed && speed > 0f)
        {
            float nextSpeed = Math.Min(speed * 1.015f, maxSpeed);
            projectile.velocity *= nextSpeed / speed;
        }

        if (Main.dedServ)
            return;

        Dust.NewDustAction(projectile.Center, projectile.width - 8, projectile.height - 8, DustID.GemDiamond, projectile.velocity * 0.2f, dust =>
        {
            dust.alpha = 100;
            dust.position -= projectile.velocity;
            dust.noGravity = true;
            dust.velocity *= 0.3f;
        });
    }

    public override Color? GetAlpha(Color lightColor) =>
        new Color(255, 255, 255, (int)Utils.WrappedLerp(0f, 255f, Projectile.timeLeft % 40 / 40f));

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor),
            Projectile.rotation, texture.Size() / 2f, Projectile.scale,
            Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
        return false;
    }

    public override void OnKill(int timeLeft)
    {
        if (Main.dedServ)
            return;

        SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
        for (int i = 0; i < 20; i++)
        {
            Dust.NewDustAction(Projectile.Center, Projectile.width, Projectile.height, DustID.Bone, action: dust =>
            {
                dust.alpha = 100;
                dust.noGravity = true;
                dust.scale = 1.3f;
                dust.velocity = dust.velocity * 1.2f - Projectile.oldVelocity * 0.3f;
            });
            Dust.NewDustAction(Projectile.Center, Projectile.width - 8, Projectile.height - 8, DustID.Blood, action: dust =>
            {
                dust.alpha = 100;
                dust.noGravity = true;
                dust.scale *= 1.5f;
                dust.velocity *= 3f;
            });
        }
    }

    internal static void Fire(NPC source, Vector2 position, Vector2 velocity, int damage, float maxSpeed) =>
        Projectile.NewProjectileAction<Crossbone>(source.GetSource_FromAI(), position, velocity, damage, 0f, Main.myPlayer,
            action: p =>
            {
                p.ai[0] = maxSpeed;
                NPC head = source.type == NPCID.SkeletronHand ? NPC.GetNPCFromIndex((int)source.ai[1]) : source;
                if (head is { active: true, type: NPCID.SkeletronHead } && SkeletronHead.GetInstance(head).ShouldProcess)
                    p.ai[1] = head.whoAmI + 1;
            });

}
