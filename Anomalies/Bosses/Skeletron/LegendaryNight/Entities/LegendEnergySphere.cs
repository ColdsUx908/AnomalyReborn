using Anomalies.Assets.Effects;
using ReLogic.Utilities;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>ai[0]：主人索引+1；ai[1]：光球序号0至3；ai[2]：0蓄力、1飞行、2抵住竞技场边缘。</summary>
public sealed class LegendEnergySphere : AnomalyModProjectile
{
    private const string AssetPath = LegendaryNightHandler.LegendaryNightEntityPath + "EnergySphere/";
    private static readonly SoundStyle _ChargeSound = new(AssetPath + "TeslaSphereLoop") { IsLooped = true, Volume = 0.45f };
    private SlotId _ChargeSoundSlot;
    private bool _SoundStarted;

    public override string Texture => TOTextures.InvisibleTexturePath;
    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;
    private int Cycle => Math.Clamp((int)Projectile.ai[1], 0, 3);
    private NPC Master => NPC.GetNPCFromIndex((int)Projectile.ai[0] - 1);
    private SkeletronLegendHead Head => Master is { active: true, type: NPCID.SkeletronHead } master
        && master.Anomaly.CustomAIType == 1 ? SkeletronLegendHead.GetInstance(master) : null;
    private float SphereRadius => Head is { } head ? SkeletronLegendHead.Mechanic6Data.Radius(head.Phase2MusicElapsedTime, Cycle) : 0f;
    // 着色器亮边位于 UV 半径0.25，噪声最多偏移0.006；绘制边长为4倍基础半径。
    private float ContactRadius => SphereRadius * 1.024f;
    private const float ArenaBoundaryHalfWidth = 1.5f;


    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 16;
        Projectile.hostile = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.netImportant = true;
        Projectile.timeLeft = 1200;
    }

    public override void AI()
    {
        SkeletronLegendHead head = Head;
        if (head is not { Mechanic6Active: true } || head.CurrentBaseBehavior == SkeletronLegendHead.BaseBehavior.Despawn)
        {
            if (TONetworkEnvironment.NotClient)
                Projectile.Kill();
            return;
        }
        int time = head.Phase2MusicElapsedTime;
        if (time >= SkeletronLegendHead.Mechanic6Data.ExplosionTime(Cycle))
        {
            if (TONetworkEnvironment.NotClient)
                Projectile.Kill();
            return;
        }
        if (Projectile.ai[2] == 0f)
        {
            Projectile.Center = Master.Center + SkeletronLegendHead.Mechanic6Data.SphereOffset(time, Cycle);
            Projectile.velocity = Vector2.Zero;
            if (time >= SkeletronLegendHead.Mechanic6Data.ThrowTime(Cycle) && TONetworkEnvironment.NotClient)
            {
                Projectile.ai[2] = 1f;
                Projectile.velocity = (head.Target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY)
                    * SkeletronLegendHead.Mechanic6Data.ThrowSpeed;
                Projectile.netUpdate = true;
            }
        }
        bool collidedWithArena = false;
        Vector2 flightVelocity = Projectile.velocity;
        if (Projectile.ai[2] == 2f && head.Phase2Arena is { } restingArena)
        {
            Vector2 normal = (Projectile.Center - restingArena.Projectile.Center).SafeNormalize(Vector2.UnitY);
            float radius = Math.Max(0f, restingArena.Radius - ArenaBoundaryHalfWidth - ContactRadius);
            Projectile.Center = restingArena.Projectile.Center + normal * radius;
            Projectile.velocity = Vector2.Zero;
        }
        if (Projectile.ai[2] == 1f && Projectile.velocity != Vector2.Zero)
        {
            // 预测下一帧的完整球体边界；高速投掷抵达外圈后停留，仍按音乐节点爆炸。
            if (head.Phase2Arena is { } arena)
            {
                float allowedRadius = Math.Max(0f, arena.Radius - ArenaBoundaryHalfWidth - ContactRadius);
                Vector2 nextOffset = Projectile.Center + Projectile.velocity - arena.Projectile.Center;
                if (nextOffset.LengthSquared() > allowedRadius * allowedRadius)
                {
                    Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitY);
                    Vector2 offset = Projectile.Center - arena.Projectile.Center;
                    float projection = Vector2.Dot(offset, direction);
                    float distance = -projection + MathF.Sqrt(Math.Max(0f,
                        projection * projection + allowedRadius * allowedRadius - offset.LengthSquared()));
                    Projectile.Center += direction * MathHelper.Clamp(distance, 0f, Projectile.velocity.Length());
                    // 将圆心归一到精确内切圆，消除浮点误差、网络修正造成的越界或缝隙。
                    Vector2 normal = (Projectile.Center - arena.Projectile.Center).SafeNormalize(direction);
                    Projectile.Center = arena.Projectile.Center + normal * allowedRadius;
                    Projectile.velocity = Vector2.Zero;
                    collidedWithArena = true;
                    if (TONetworkEnvironment.NotClient)
                    {
                        Projectile.ai[2] = 2f;
                        Projectile.netUpdate = true;
                    }
                }
            }
            else
            {
                // 场地引用尚未恢复时暂停移动，避免无边界约束地继续飞行。
                Projectile.velocity = Vector2.Zero;
                if (TONetworkEnvironment.NotClient)
                    Projectile.netUpdate = true;
            }
        }
        if (Main.dedServ)
            return;
        Lighting.AddLight(Projectile.Center, LegendaryNightHandler.StarColor.ToVector3() * 1.5f);
        if (!_SoundStarted)
        {
            _SoundStarted = true;
            _ChargeSoundSlot = SoundEngine.PlaySound(_ChargeSound, Projectile.Center, sound => Projectile.active);
        }
        if (SoundEngine.TryGetActiveSound(_ChargeSoundSlot, out ActiveSound activeSound))
        {
            activeSound.Position = Projectile.Center;
            activeSound.Volume = 0.15f + 0.5f * SkeletronLegendHead.Mechanic6Data.ChargeProgress(time, Cycle);
        }
        if (Projectile.ai[2] >= 1f)
        {
            if (flightVelocity.LengthSquared() > 1f)
                EmitFlightParticles(flightVelocity);
            if (head.Phase2Arena is { } contactArena)
            {
                Vector2 offset = Projectile.Center - contactArena.Projectile.Center;
                float contactRadius = Math.Max(0f, contactArena.Radius - ArenaBoundaryHalfWidth - ContactRadius);
                // 停止后仍按几何位置持续检测接触，不依赖只发生一次的撞击分支。
                if (offset.LengthSquared() >= Math.Max(0f, contactRadius - 2f) * Math.Max(0f, contactRadius - 2f))
                    EmitContactParticles(contactArena, offset.SafeNormalize(Vector2.UnitY), collidedWithArena);
            }
        }
        if (Projectile.ai[2] == 0f)
        {
            foreach (NPC hand in NPC.ActiveNPCs)
            {
                if (hand.type != NPCID.SkeletronHand || (int)hand.ai[1] != (int)Projectile.ai[0] - 1)
                    continue;
                for (int i = 0; i < 2; i++)
                {
                    Vector2 origin = hand.Center + Main.rand.NextVector2Circular(16f, 16f);
                    ParticleHandler.SpawnParticle(new LegendaryNightStellarMatterParticle(Projectile, origin,
                        Main.rand.NextFloat(0.25f, 0.5f), Main.rand.Next(18, 31)));
                }
            }
        }

        void EmitFlightParticles(Vector2 velocity)
        {
            Vector2 backward = -velocity.SafeNormalize(Vector2.UnitY);
            for (int i = 0; i < 6; i++)
            {
                // 沿球体后半部喷出，生成点位于表面之外，不被不透明球体遮住。
                Vector2 direction = backward.RotatedBy(Main.rand.NextFloat(-1.4f, 1.4f));
                Vector2 position = Projectile.Center + direction * (SphereRadius + Main.rand.NextFloat(8f, 22f));
                Vector2 particleVelocity = direction * Main.rand.NextFloat(3f, 8f) + velocity * 0.12f;
                Color color = Color.Lerp(Color.Orange, Color.LightGoldenrodYellow, Main.rand.NextFloat());
                if (i < 2)
                    ParticleHandler.SpawnParticle(new GenericSparkle(position, particleVelocity, color,
                        LegendaryNightHandler.StarColor, Main.rand.NextFloat(0.3f, 0.6f), Main.rand.Next(20, 34), 0.04f, 1.5f));
                else
                    ParticleHandler.SpawnParticle(new OrbParticle(position, particleVelocity, Main.rand.Next(18, 30),
                        Main.rand.NextFloat(0.3f, 0.65f), color, lifeEndRatio: 0.1f));
            }
        }

        void EmitContactParticles(LegendaryNightPhase2Arena arena, Vector2 normal, bool impact)
        {
            Vector2 contact = arena.Projectile.Center + normal * (arena.Radius - ArenaBoundaryHalfWidth);
            Vector2 tangent = new(-normal.Y, normal.X);
            Lighting.AddLight(contact, new Vector3(1.4f, 0.55f, 0.08f));
            int count = impact ? 24 : 6;
            for (int i = 0; i < count; i++)
            {
                float side = i % 2 == 0 ? -1f : 1f;
                Vector2 position = contact + tangent * Main.rand.NextFloat(-10f, 10f);
                Vector2 velocity = tangent * (side * Main.rand.NextFloat(5f, impact ? 22f : 14f))
                    + normal * Main.rand.NextFloat(0.5f, 3f);
                Color color = Color.Lerp(new Color(255, 85, 12), Color.LightGoldenrodYellow, Main.rand.NextFloat());
                ParticleHandler.SpawnParticle(new PointingParticle(position, velocity, true,
                    Main.rand.Next(16, 30), Main.rand.NextFloat(0.35f, 0.75f), color));
                if (i % 3 == 0)
                    ParticleHandler.SpawnParticle(new OrbParticle(position, velocity * 0.4f, Main.rand.Next(12, 22),
                        Main.rand.NextFloat(0.35f, 0.7f), Color.OrangeRed, lifeEndRatio: 0.1f));
            }
            if (impact)
                ParticleHandler.SpawnParticle(new BloomParticle(contact, Vector2.Zero, Color.Orange,
                    0.2f, 0.8f, 12, lifeEndRatio: 0.1f));
        }
    }

    public override bool CanHitPlayer(Player target) => Projectile.ai[2] >= 1f;
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => new Circle(Projectile.Center, SphereRadius).Collides(targetHitbox);

    public override void OnKill(int timeLeft)
    {
        if (!Main.dedServ && _SoundStarted && SoundEngine.TryGetActiveSound(_ChargeSoundSlot, out ActiveSound sound))
            sound.Stop();
        SkeletronLegendHead head = Head;
        // 清场或主人退场时不爆出弹幕；仅音乐指定的爆炸节点产生伤害。
        if (head is not { Mechanic6Active: true } || head.CurrentBaseBehavior == SkeletronLegendHead.BaseBehavior.Despawn
            || head.Phase2MusicElapsedTime < SkeletronLegendHead.Mechanic6Data.ExplosionTime(Cycle))
            return;
        if (!Main.dedServ)
        {
            SoundEngine.PlaySound(Meteor.ExplosionSound, Projectile.Center);
            LegendaryNightHandler.SpawnBlessingBurst(Projectile.Center);
        }
        if (!TONetworkEnvironment.NotClient)
            return;
        int count = SkeletronLegendHead.Mechanic6Data.StarCount(Cycle);
        // 与 Meteor 一样按隔点相连的五条边插值速度，扩散后形成五角星。
        Vector2 aim = (head.Target.Center - Projectile.Center).SafeNormalize(-Vector2.UnitY) * 16f;
        for (int i = 0; i < count; i++)
        {
            float edge = i * 5f / count;
            int vertex = (int)edge;
            Vector2 velocity = Vector2.Lerp(aim.RotatedBy(TOMathUtils.PiOver5 * 4f * vertex),
                aim.RotatedBy(TOMathUtils.PiOver5 * 4f * (vertex + 1)), edge - vertex);
            Projectile.NewProjectileAction<FallingStar>(Projectile.GetSource_Death(), Projectile.Center, velocity,
                SkeletronLegendHead.FallingStarDamage, 0f, action: p =>
                {
                    p.ai[2] = Projectile.ai[0];
                    p.scale = 0.2f;
                    p.Resize(16, 16);
                    FallingStar star = p.GetModProjectile<FallingStar>();
                    star.TrailType = 2;
                    star.AccelerationStartTime = 90;
                    p.netUpdate = true;
                });
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        if (Main.dedServ || SphereRadius <= 0f)
            return false;
        SpriteBatch spriteBatch = Main.spriteBatch;
        Vector2 center = Projectile.Center - Main.screenPosition;
        Texture2D bloom = ParticleHandler.GetTexture<BloomParticle>();
        spriteBatch.DrawFromCenter(bloom, center, null, new Color(255, 190, 20, 0) * 0.16f,
            scale: SphereRadius * 5f / bloom.Width);
        AnomalyEffects.MiscShaderContainer shader = AnomalyClientConfig.Instance.EnableShaders
            ? AnomalyEffects.Overlays.LegendEnergySphere : null;
        if (shader is null)
        {
            Texture2D body = AnomalyTextures.Extra.BasicCircleLarge;
            spriteBatch.DrawFromCenter(body, center, null, new Color(255, 105, 8),
                scale: SphereRadius * 2f / body.Width);
            spriteBatch.DrawFromCenter(bloom, center, null, new Color(255, 235, 120, 0) * 0.7f,
                scale: SphereRadius * 2f / bloom.Width);
        }
        else
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            Effect effect = shader.Asset.Value;
            effect.Parameters["globalTime"].SetValue((float)Main.timeForVisualEffects / 60f);
            effect.Parameters["nebulaNoiseTexture"].SetValue(AnomalyTextures.Noise.Perlin2);
            effect.CurrentTechnique.Passes["Pass0"].Apply();
            Texture2D pixel = TOTextures.FillerTexture;
            spriteBatch.DrawFromCenter(pixel, center, null, Color.White, scale: SphereRadius * 4f / pixel.Width);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
        // 关闭着色器时同样保留可见的流动星辰，不只显示纯光晕。
        if (shader is null)
        {
            Texture2D sparkle = ParticleHandler.GetTexture<GenericSparkle>();
            float time = (float)Main.timeForVisualEffects / 60f;
            for (int i = 0; i < 48; i++)
            {
                float distance = MathF.Sqrt((i + 0.5f) / 48f);
                float angle = i * 2.399963f + time * (0.2f + distance * 0.4f);
                Vector2 offset = angle.ToRotationVector2() * (SphereRadius * distance * 0.92f);
                float twinkle = 0.6f + 0.4f * MathF.Sin(time * 2f + i * 1.7f);
                spriteBatch.DrawFromCenter(sparkle, center + offset, null,
                    new Color(255, 235, 175, 0) * twinkle, time * 0.15f,
                    (4f + 4f * twinkle) / sparkle.Width);
            }
        }
        return false;
    }
}
