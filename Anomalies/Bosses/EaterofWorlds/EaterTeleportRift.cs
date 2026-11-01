using Anomalies.Assets.Effects;

// Adapted from CalamityMod; Copyright Azafure, LLC. See Portal/README.md.
namespace Anomalies.Bosses.EaterofWorlds;

public sealed class EaterTeleportRift : ModProjectile
{
    public static readonly SoundStyle CrackSound = new(EaterPortalAssets.Path + "CryogenHit", 3);

    public static readonly SoundStyle BreakSound = new(EaterPortalAssets.Path + "CryogenPhaseTransitionCrack");

    private bool _ExplosionShown;
    private bool _VisualsInitialized;

    public int RiftLifetime;

    public NPC OwnerHead => NPC.GetNPCFromIndex((int)Projectile.ai[0] - 1);

    public ref float Timer => ref Projectile.ai[1];

    public ref float CrackScale => ref Projectile.localAI[0];

    // ai[0]: owner + 1; ai[1]: state timer; ai[2]: charging / exploded.
    public ref float AIState => ref Projectile.ai[2];

    public ref float CrackExposure => ref Projectile.localAI[1];

    public ref float MaxExposure => ref Projectile.localAI[2];

    public override string LocalizationCategory => "Bosses.EaterofWorlds";
    public override string Texture => TOTextures.InvisibleTexturePath;

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 30;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.Opacity = 0f;
        Projectile.scale = 0f;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 240;
        Projectile.netImportant = true;
    }

    public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;

    public override bool? CanDamage() => false;

    public override bool ShouldUpdatePosition() => false;

    public override void SendExtraAI(BinaryWriter writer) => writer.Write(RiftLifetime);

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        RiftLifetime = reader.ReadInt32();
        _VisualsInitialized = false;
    }

    // Rebuild local drawing data for a late join or correction; only the state and timer are synchronized.
    private void RestoreVisualState()
    {
        CrackScale = CrackExposure = MaxExposure = 0f;
        int lifetime = Math.Max(1, RiftLifetime);
        int interval = Math.Max(1, lifetime / 3);
        int elapsed = AIState == 1f ? lifetime + 1 : Math.Clamp((int)Timer, 0, lifetime + 1);
        for (int time = 0; time < elapsed; time++)
        {
            if (time % interval == 0)
            {
                float progress = MathHelper.Lerp(1f, 2f, time / (float)lifetime);
                CrackScale += 0.525f * progress * 0.75f;
                MaxExposure = Math.Clamp(MaxExposure + 0.25f * progress, 0f, 0.95f);
            }
            CrackExposure = MathHelper.Lerp(CrackExposure, MaxExposure, 0.075f);
        }
        if (AIState == 1f)
        {
            CrackScale += 3f;
            CrackExposure = MathHelper.Lerp(MaxExposure, 0f, Timer / 45f);
        }
        Projectile.Opacity = AIState == 1f ? 1f - Timer / 45f : Math.Clamp(Timer * 0.05f, 0f, 1f);
        Projectile.scale = AIState == 1f ? 1f : Math.Clamp(Timer * 0.02f, 0f, 1f);
        _VisualsInitialized = true;
    }

    public override void AI()
    {
        if (!OwnerHead.active || OwnerHead.type != NPCID.EaterofWorldsHead
            || EaterofWorldsHead.GetInstance(OwnerHead).CurrentBehavior == EaterofWorldsHead.Behavior.Despawn)
        {
            Projectile.Kill();
            return;
        }

        if (!_VisualsInitialized)
            RestoreVisualState();

        // Pre-explosion, charge-up.
        if (AIState == 0f)
        {
            if (Timer == 0f && !Main.dedServ)
                SoundEngine.PlaySound(new SoundStyle(EaterPortalAssets.Path + "DevourerRiftOpen") { Volume = 1.5f }, Projectile.Center);
            int riftCrackInterval = Math.Max(1, RiftLifetime / 3);
            if (Timer % riftCrackInterval == 0f)
            {
                // Increase the intensity of certain effects as the crack gets bigger.
                float effectsLifetimeInterpolant = MathHelper.Lerp(1f, 2f, Timer / RiftLifetime);

                if (!Main.dedServ)
                {
                    // Original crack sparks, lights and camera pulse.

                    float crackPitch = MathHelper.Lerp(-0.75f, 0f, Timer / RiftLifetime);
                    SoundEngine.PlaySound(CrackSound with { Pitch = crackPitch }, Projectile.Center);

                    for (int i = 0; i < 12; i++)
                    {
                        Vector2 sparkVelocity = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(8f, 12f) * effectsLifetimeInterpolant * 0.7f;
                        float sparkScale = Main.rand.NextFloat(1.2f, 1.6f) * effectsLifetimeInterpolant * 0.62f;
                        Color sparkColor = Color.Lerp(Utils.SelectRandom(Main.rand, Color.Fuchsia, EaterPortalAssets.LightBlue, EaterPortalAssets.Twilight), Color.White, 0.65f);
                        PointingParticle crackSpark = new(Projectile.Center, sparkVelocity, false, Main.rand.Next(30, 45), sparkScale, sparkColor) { Asset = TextureAssets.Extra[ExtrasID.ThePerfectGlow] };
                        ParticleHandler.SpawnParticle(crackSpark);
                    }

                    for (int i = 0; i < 8; i++)
                    {
                        Vector2 lightVelocity = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(12f, 14f) * effectsLifetimeInterpolant * 0.5f;
                        float lightScale = Main.rand.NextFloat(1.2f, 1.6f) * effectsLifetimeInterpolant * 0.62f;
                        Color lightColor = Color.Lerp(Utils.SelectRandom(Main.rand, Color.Fuchsia, EaterPortalAssets.LightBlue, EaterPortalAssets.Twilight), Color.White, 0.65f);
                        EaterRiftLight crackLight = new(Projectile.Center, lightVelocity, lightScale, lightColor, Main.rand.Next(30, 45));
                        ParticleHandler.SpawnParticle(crackLight);
                    }

                    AnomalyCombatUtils.AddScreenshakeAt(Projectile.Center, 6f * effectsLifetimeInterpolant * 0.8f);

                    const float BrightnessMultiplier = 1f;
                    TexturePulseParticle shineExplosion2 = new(Projectile.Center, Vector2.Zero, Color.White * BrightnessMultiplier, EaterPortalAssets.Path + "ShineExplosion2", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), 0f, 0.5f * effectsLifetimeInterpolant, 45);
                    ParticleHandler.SpawnParticle(shineExplosion2);

                }

                // Increase the scale and exposure of the actual crack at each interval.
                CrackScale += 0.525f * effectsLifetimeInterpolant * 0.75f;
                MaxExposure = MathHelper.Clamp(MaxExposure + 0.25f * effectsLifetimeInterpolant, 0f, 0.95f);
            }

            // Perform the explosion on the topology-change and emergence boundary.
            if (EaterofWorldsHead.GetInstance(OwnerHead).CurrentLocalPhase == 3 || Timer >= RiftLifetime)
                SwitchAIStates();

            if (Projectile.timeLeft < 45)
                Projectile.timeLeft = 45;

            CrackExposure = MathHelper.Lerp(CrackExposure, MaxExposure, 0.075f);
            Projectile.Opacity = MathHelper.Clamp(Projectile.Opacity + 0.05f, 0f, 1f);
            Projectile.scale = MathHelper.Clamp(Projectile.scale + 0.02f, 0f, 1f);

        }

        if (AIState == 1f && !_ExplosionShown && !Main.dedServ)
        {
            _ExplosionShown = true;
            if (Timer <= 2f)
                SpawnExplosionVisuals();
        }

        // Post-explosion, slowly fade out.
        if (AIState == 1f)
        {
            Projectile.Opacity = MathHelper.Lerp(1f, 0f, Timer / 45f);
            CrackExposure = MathHelper.Lerp(MaxExposure, 0f, Timer / 45f);
        }

        Timer++;
    }

    public void SwitchAIStates()
    {
        AIState = 1f;
        Timer = 0f;
        CrackScale += 3f;
        Projectile.timeLeft = 30;
        if (TONetworkEnvironment.NotClient)
            Projectile.netUpdate = true;
    }

    public void SpawnExplosionVisuals()
    {
        const float BrightnessMultiplier = 1f;
        // Spawn the rift cracks.
        for (int i = 0; i < 25; i++)
        {
            Vector2 crackOffset = Vector2.UnitX.RotatedBy(i * MathHelper.TwoPi / 25) * Main.rand.NextFloat(20f, 80f);
            Vector2 crackDestination = Projectile.Center + crackOffset;
            Vector2 crackLength = crackDestination - Projectile.Center;
            float riftWidth = Main.rand.NextFloat(20f, 30f);
            EaterDistortionSystem.SpawnCrack(Projectile.Center, crackLength, riftWidth);
        }

        // Spawn a bunch of square-shaped metaballs at the impact zone as well.
        for (int i = 0; i < 35; i++)
            EaterDistortionSystem.SpawnParticle(Projectile.Center + Main.rand.NextVector2Circular(25f, 25f), Vector2.Zero, 500f, true, Main.rand.NextFloat(MathHelper.TwoPi));

        // More metaballs flying away from the impact zone for extra flavor.
        int metaballAmount = Main.rand.Next(15, 21);
        for (int i = 0; i < metaballAmount; i++)
            EaterDistortionSystem.SpawnParticle(Projectile.Center, Main.rand.NextVector2Unit() * Main.rand.NextFloat(25f, 35f), Main.rand.NextFloat(30f, 50f));

        // Addtional particles and effects.
        for (int i = 0; i < 25; i++)
        {
            Vector2 sparkVelocity = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(12f, 16f);
            float sparkScale = Main.rand.NextFloat(1.8f, 2f);
            Color sparkColor = Color.Lerp(Utils.SelectRandom(Main.rand, Color.Fuchsia, EaterPortalAssets.LightBlue, EaterPortalAssets.Twilight), Color.White, 0.65f);
            PointingParticle crackSpark = new(Projectile.Center, sparkVelocity, false, Main.rand.Next(30, 45), sparkScale, sparkColor) { Asset = TextureAssets.Extra[ExtrasID.ThePerfectGlow] };
            ParticleHandler.SpawnParticle(crackSpark);
        }

        for (int i = 0; i < 20; i++)
        {
            Vector2 lightVelocity = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(16f, 20f);
            float lightScale = Main.rand.NextFloat(1.8f, 2f);
            Color lightColor = Color.Lerp(Utils.SelectRandom(Main.rand, Color.Fuchsia, EaterPortalAssets.LightBlue, EaterPortalAssets.Twilight), Color.White, 0.65f);
            EaterRiftLight crackLight = new(Projectile.Center, lightVelocity, lightScale, lightColor, Main.rand.Next(30, 45));
            ParticleHandler.SpawnParticle(crackLight);
        }

        // Explosion visuals.
        for (int i = 0; i < 3; i++)
        {
            TexturePulseParticle plasmaExplosion = new(Projectile.Center, Vector2.Zero, EaterPortalAssets.Twilight * 0.8f * BrightnessMultiplier, EaterPortalAssets.Path + "PlasmaExplosion", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), 0f, 1.25f + i * 0.05f, 45);
            ParticleHandler.SpawnParticle(plasmaExplosion);
        }

        TexturePulseParticle shineExplosion = new(Projectile.Center, Vector2.Zero, EaterPortalAssets.Twilight * BrightnessMultiplier, EaterPortalAssets.Path + "ShineExplosion1", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), 0f, 0.75f, 45);
        ParticleHandler.SpawnParticle(shineExplosion);
        TexturePulseParticle shineExplosion2 = new(Projectile.Center, Vector2.Zero, Color.White * 0.6f * BrightnessMultiplier, EaterPortalAssets.Path + "ShineExplosion2", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), 0f, 0.5f, 45);
        ParticleHandler.SpawnParticle(shineExplosion2);
        EaterRiftBloom bloom = new(Projectile.Center, Vector2.Zero, Color.White * 0.8f * BrightnessMultiplier, 8f, 30);
        ParticleHandler.SpawnParticle(bloom);

        SoundEngine.PlaySound(BreakSound, Projectile.Center);
        AnomalyCombatUtils.AddScreenshakeAt(Projectile.Center, 14f);
    
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D crackTexture = EaterPortalAssets.Get(EaterPortalAssets.Path + "CrackedGlass_Glowing").Value;
        Texture2D starTexture = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;
        Texture2D bloomRingTexture = AnomalyTextures.Extra.BloomRing;

        float chargeInterpolant = (1f - MathF.Cos(MathHelper.Pi * Timer / Math.Max(1, RiftLifetime))) * 0.5f;
        const float BrightnessMultiplier = 1f;

        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Vector2 verticalStarPartScale = new Vector2(1f, 8f) * chargeInterpolant * Projectile.scale * 3.25f;
        Vector2 horizontalStarPartScale = new Vector2(1f, 5f) * chargeInterpolant * Projectile.scale * 3.25f;

        using SpriteBatchScope additive = new(Main.spriteBatch, blendState: BlendState.Additive);

        // Draw the glowing star.
        Main.EntitySpriteDraw(starTexture, drawPosition, null, Color.White * 0.7f * BrightnessMultiplier * chargeInterpolant, 0f, starTexture.Size() * 0.5f, verticalStarPartScale, 0);
        Main.EntitySpriteDraw(starTexture, drawPosition, null, Color.White * 0.7f * BrightnessMultiplier * chargeInterpolant, MathHelper.PiOver2, starTexture.Size() * 0.5f, horizontalStarPartScale, 0);

        // Draw the smaller twilight-colored star at the center.

        Main.EntitySpriteDraw(starTexture, drawPosition, null, EaterPortalAssets.Twilight * 0.9f * BrightnessMultiplier * chargeInterpolant, 0f, starTexture.Size() * 0.5f, verticalStarPartScale * 0.8f, 0);
        Main.EntitySpriteDraw(starTexture, drawPosition, null, EaterPortalAssets.Twilight * 0.9f * BrightnessMultiplier * chargeInterpolant, MathHelper.PiOver2, starTexture.Size() * 0.5f, horizontalStarPartScale * 0.8f, 0);

        // Draw the crack.
        Effect crackShader = AnomalyEffects.Overlays.EaterRealityCrack?.Asset.Value;
        float crackOpacity = (AIState == 1f) ? Projectile.Opacity * 0.1f : 0.1f;
        Color darkerPixelColor = EaterPortalAssets.Twilight;

        crackShader?.Parameters["opacityCutoffValue"]?.SetValue(CrackExposure);
        crackShader?.Parameters["fadeoutPower"]?.SetValue(1f);
        crackShader?.Parameters["overallOpacity"]?.SetValue(crackOpacity * BrightnessMultiplier);
        crackShader?.Parameters["minBrightnessValue"]?.SetValue(0.75f);
        crackShader?.Parameters["darkerPixelColor"]?.SetValue(darkerPixelColor.ToVector3());
        crackShader?.Parameters["brighterPixelColor"]?.SetValue(Color.White.ToVector3());
        if (crackShader is not null)
        {
            using SpriteBatchScope crackScope = new(Main.spriteBatch, SpriteSortMode.Immediate,
                BlendState.Additive, SamplerState.PointClamp, customEffect: crackShader);
            crackShader.CurrentTechnique.Passes["Pass0"].Apply();
            const int CrackCount = 3;
            for (int i = 0; i < CrackCount; i++)
            {
                float crackRotation = i * MathHelper.TwoPi / CrackCount;
                Main.EntitySpriteDraw(crackTexture, drawPosition + Vector2.UnitY * 8f, null,
                    Color.White * Projectile.Opacity, crackRotation, crackTexture.Size() * 0.5f, CrackScale, 0);
            }
        }

        if (AIState != 1f)
        {
            // Draw the bloom ring which closes inwards.
            float ringScale = MathHelper.Lerp(8f, 0f, chargeInterpolant);
            Color ringColor = Color.Lerp(Color.White, EaterPortalAssets.Twilight, chargeInterpolant);
            Main.EntitySpriteDraw(bloomRingTexture, drawPosition, null, ringColor * BrightnessMultiplier * chargeInterpolant * 0.6f, 0f, bloomRingTexture.Size() * 0.5f, ringScale, 0);
        }

        return false;
    }
}
