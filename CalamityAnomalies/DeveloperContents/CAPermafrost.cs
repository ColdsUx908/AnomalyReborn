// Designed by ColdsUx

/*
using CalamityAnomalies.Publicizers.CalamityMod;
using CalamityAnomalies.Visuals;
using CalamityMod.NPCs.Cryogen;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.NPCs.TownNPCs;
using CalamityMod.Projectiles.Boss;
using CalamityMod.Skies;
using Terraria.Graphics.Shaders;
using Transoceanic.Framework.Helpers.AbstractionHandlers;
using Transoceanic.Framework.RuntimeEditing;
using static CalamityMod.NPCs.SupremeCalamitas.SupremeCalamitas;
using static CalamityMod.Projectiles.Boss.SCalRitualDrama;

namespace CalamityAnomalies.DeveloperContents;

public sealed class Permafrost_Handler : IContentLoader
{
    private static Asset<Texture2D> _centerTexture;
    public static Texture2D CenterTexture => _centerTexture?.Value;

    private static Asset<Texture2D> _immuneTexture;
    public static Texture2D ImmuneTexture => _immuneTexture?.Value;

    void IContentLoader.PostSetupContent()
    {
        AssetRepository assets = CalamityMod_Publicizer.Instance.Assets;
        _centerTexture = assets.Request<Texture2D>("Particles/CentralGold");
        _immuneTexture = assets.Request<Texture2D>("Particles/SemiCircularSmearVertical");
    }

    void IContentLoader.OnModUnload()
    {
        _centerTexture = null;
        _immuneTexture = null;
    }
}

public sealed class CAPermafrost : CASingleNPCBehavior<SupremeCalamitas>, ILocalizationPrefix
{
    #region 枚举、数值、属性、AI状态
    public enum AttackType
    {
        Welcome = 0,
        NonSpell1 = 1,
    }

    private static class Data
    {
        public static Color BlueColor => Color.Lerp(Color.LightCyan, Color.Cyan, TOMathUtils.TimeWrappingFunction.GetTimeSin(0.2f, 1f, 0f, true));

        public static readonly List<Color> NameColors =
        [
            Color.Cyan,
            Color.LightYellow,
            Color.LightPink,
            Color.SkyBlue,
            Color.LightSkyBlue
        ];

        public static Color NameColor => Color.LerpMany(NameColors, TOMathUtils.TimeWrappingFunction.GetTimeSin(0.5f, 0.6f, 0f, true));

        public const float DespawnDistance = 15000f;

        public const int ArenaSize = 201;
    }

    public Point Arena_TopLeft
    {
        get => AnomalyNPC.AnomalyAI64[0].p;
        set
        {
            if (AnomalyNPC.AnomalyAI64[0].p != value)
            {
                AnomalyNPC.AnomalyAI64[0].p = value;
                AnomalyNPC.AIChanged64[0] = true;
            }
        }
    }

    public Vector2 Arena_TopLeftPosition => Arena_TopLeft.ToWorldCoordinates();

    public Vector2 GetArenaTilePosition(int i, int j) => (Arena_TopLeft + new Point(i, j)).ToWorldCoordinates();

    public Vector2 Arena_CenterPosition => GetArenaTilePosition(Data.ArenaSize / 2, Data.ArenaSize / 2);

    public AttackType CurrentAttack
    {
        get => (AttackType)AnomalyNPC.AnomalyAI32[0].i;
        set
        {
            int temp = (int)value;
            if (AnomalyNPC.AnomalyAI32[0].i != temp)
            {
                AnomalyNPC.AnomalyAI32[0].i = temp;
                AnomalyNPC.AIChanged32[0] = true;
            }
        }
    }

    #endregion 枚举、数值、属性、AI状态

    public string LocalizationPrefix => CASharedData.ModLocalizationPrefix + "DeveloperContents.Permafrost";

    public override decimal Priority => 935m; //ICE

    public override bool ShouldProcess => ModNPC?.permafrost ?? false;

    public override void OnSpawn(IEntitySource source)
    {
        NPC.dontTakeDamage = true;
    }

    public override bool PreAI()
    {
        StartUp();

        switch (CurrentAttack)
        {
            case AttackType.Welcome:
                Welcome();
                break;
        }

        return false;

        void StartUp()
        {
            NPC.damage = 0;

            ModNPC.FrameType = FrameAnimationType.UpwardDraft;
            ModNPC.FrameChangeSpeed = 0.15f;

            CalamityGlobalNPC.SCal = NPC.whoAmI;
            ModNPC.HandleMusicVariables();

            if (Main.slimeRain)
            {
                Main.StopSlimeRain(true);
                CalamityNetcode.SyncWorld();
            }


            #region 目标与脱战
            if (!NPC.TargetClosestIfInvalid(true, Data.DespawnDistance))
            {
                if (SoundEngine.TryGetActiveSound(ModNPC.BulletHellRumbleSlot, out ActiveSound rumbleSound) && rumbleSound.IsPlaying)
                    rumbleSound.Stop();

                ModNPC.canDespawn = true;

                NPC.Opacity = MathHelper.Lerp(NPC.Opacity, 0f, 0.065f);
                NPC.velocity = Vector2.Lerp(Vector2.UnitY * -4f, Vector2.Zero, (float)Math.Sin(MathHelper.Pi * NPC.Opacity));
                ModNPC.forcefieldOpacity = Utils.GetLerpValue(0.3f, 1f, NPC.Opacity, true);
                if (NPC.alpha >= 230)
                {
                    if (DownedBossSystem_Bridged.downedCalamitas)
                    {
                        // Create a teleport line effect
                        Dust.QuickDustLine(NPC.Center, ModNPC.initialRitualPosition, 500f, Color.Cyan);
                        NPC.Center = ModNPC.initialRitualPosition;

                        // Make the town NPC spawn.
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                            NPC.NewNPCAction<Archmage>(NPC.GetSource_FromAI(), NPC.Center + new Vector2(0f, 12f));
                    }

                    NPC.active = false;
                    NPC.netUpdate = true;
                }

                for (int i = 0; i < MathHelper.Lerp(2f, 6f, 1f - NPC.Opacity); i++)
                {
                    Dust.NewDustPerfectAction(NPC.Center + Main.rand.NextVector2Square(-24f, 24f), DustID.BlueTorch, iceFire =>
                    {
                        iceFire.velocity = Vector2.UnitY * -Main.rand.NextFloat(2f, 3.25f);
                        iceFire.color = Color.Cyan;
                        iceFire.scale = Main.rand.NextFloat(0.95f, 1.15f);
                        iceFire.noGravity = true;
                    });
                }
            }
            else
                ModNPC.canDespawn = false;
            #endregion 目标与脱战

            #region 判定方向
            bool currentlyCharging = NPC.ai[1] == 2f;
            if (!currentlyCharging && Math.Abs(Target.Center.X - NPC.Center.X) > 16f)
                NPC.spriteDirection = (Target.Center.X < NPC.Center.X).ToDirectionInt();
            #endregion 判定方向

            #region 力场和护盾
            // Shield effect rotation
            ModNPC.rotateToPlayer = ModNPC.rotateToPlayer.AngleLerp((Target.Center - NPC.Center).SafeNormalize(Vector2.UnitY).ToRotation(MathHelper.PiOver2), 0.04f);
            ModNPC.rotateAwayPlayer = ModNPC.rotateAwayPlayer.AngleLerp((Target.Center - NPC.Center).SafeNormalize(Vector2.UnitY).ToRotation(-MathHelper.PiOver2), 0.04f);

            if (ModNPC.hitTimer > 0)
                ModNPC.hitTimer--;

            if (NPC.dontTakeDamage && !ModNPC.hasDoneDeathAnim) // Dust visuals for shield when immune
            {
                Vector2 sustVel = new Vector2(-78 * Main.rand.NextFloat(0.95f, 1.05f), 0).RotatedBy(ModNPC.rotateToPlayer + MathHelper.PiOver2).RotatedByRandom(1.4);
                Dust.NewDustPerfectAction(NPC.Center + sustVel, DustID.Sandnado, sust =>
                {
                    sust.velocity = sustVel * Main.rand.NextFloat(0.001f, 0.03f);
                    sust.alpha = 200;
                    sust.color = Main.rand.NextBool(3) ? Color.Goldenrod : CASharedData.AnomalyUltramundaneColor;
                    sust.scale = Main.rand.NextFloat(0.5f, 0.9f);
                    sust.noGravity = true;
                });
            }

            Vector2 hitboxSize = new(ModNPC.forcefieldScale * 216f / 1.4142f);
            hitboxSize = Vector2.Max(hitboxSize, new Vector2(42, 44));
            if (NPC.Size != hitboxSize)
                NPC.Size = hitboxSize;
            bool shouldNotUseShield = ModNPC.bulletHellCounter2 % BulletHellDuration != 0 || ModNPC.attackCastDelay > 0
                || NPC.ai[0] == 1f || NPC.ai[0] == 2f;

            // Make the shield and forcefield fade away in SCal's acceptance phase.
            if (NPC.LifeRatio <= 0.01f && ModNPC.hasDoneDeathAnim)
            {
                ModNPC.shieldOpacity = MathHelper.Lerp(ModNPC.shieldOpacity, 0f, 0.08f);
                ModNPC.forcefieldScale = MathHelper.Lerp(ModNPC.forcefieldScale, 0f, 0.08f);
            }

            // Summon a shield if the next attack will be a charge.
            // Make it go away if certain triggers happen during this, such as a bullet hell starting, however.
            else if ((ModNPC.willCharge && ModNPC.AttackCloseToBeingOver || NPC.ai[1] == 2f) && !shouldNotUseShield)
            {
                if (NPC.ai[1] != 2f)
                {
                    float idealRotation = NPC.AngleTo(Target.Center);
                    float angularOffset = Math.Abs(MathHelper.WrapAngle(ModNPC.shieldRotation - idealRotation));

                    if (angularOffset > 0.04f)
                    {
                        ModNPC.shieldRotation = ModNPC.shieldRotation.AngleLerp(idealRotation, 0.125f);
                        ModNPC.shieldRotation = ModNPC.shieldRotation.AngleTowards(idealRotation, 0.18f);
                    }
                }

                // Shrink the force-field since it looks strange when charging.
                ModNPC.forcefieldScale = MathHelper.Lerp(ModNPC.forcefieldScale, 0.45f, 0.08f);
                ModNPC.shieldOpacity = MathHelper.Lerp(ModNPC.shieldOpacity, 1f, 0.08f);
            }
            // Make the shield disappear if it is no longer relevant and regenerate the forcefield.
            else
            {
                ModNPC.shieldOpacity = MathHelper.Lerp(ModNPC.shieldOpacity, 0f, 0.08f);
                ModNPC.forcefieldScale = MathHelper.Lerp(ModNPC.forcefieldScale, 1f, 0.08f);
            }
            #endregion 力场和护盾

            #region 激怒和伤害减免
            if (ModNPC.spawnArena && !Target.Hitbox.Intersects(ModNPC.safeBox))
            {
                float projectileVelocityMultCap = !Target.Hitbox.Intersects(ModNPC.safeBox) && ModNPC.spawnArena ? 2f : 1.5f;
                ModNPC.uDieLul = MathHelper.Clamp(ModNPC.uDieLul * 1.01f, 1f, projectileVelocityMultCap);
                ModNPC.protectionBoost = !Target.Hitbox.Intersects(ModNPC.safeBox);
            }
            else
            {
                ModNPC.uDieLul = MathHelper.Clamp(ModNPC.uDieLul * 0.99f, 1f, 2f);
                ModNPC.protectionBoost = false;
            }
            CalamityNPC.CurrentlyEnraged = !Target.Hitbox.Intersects(ModNPC.safeBox);

            // Set DR to be 99% and unbreakable if enraged. Boost DR during the 5th attack.
            if (ModNPC.protectionBoost && !ModNPC.gettingTired5)
            {
                CalamityNPC.DR = enragedDR;
                CalamityNPC.unbreakableDR = true;
            }
            else
            {
                CalamityNPC.DR = normalDR;
                CalamityNPC.unbreakableDR = false;
                if (ModNPC.startFifthAttack)
                    CalamityNPC.DR *= 1.2f;
            }
            #endregion 激怒和伤害减免
        }

        void Welcome()
        {
            NPC.dontTakeDamage = true;

            switch (Timer1++)
            {
                case 180:
                    if (TOSharedData.GeneralClient)
                        TOLocalizationUtils.ChatLocalizedText(this, "Welcome1", Color.LightCyan);
                    break;
                case 300:
                    if (TOSharedData.GeneralClient)
                        TOLocalizationUtils.ChatLocalizedText(this, "Welcome2", Color.LightCyan);
                    break;
                case 480:
                    if (TOSharedData.GeneralClient)
                        TOLocalizationUtils.ChatLocalizedText(this, "Beginning", Color.LightCyan);

                    ParticleHandler.SpawnParticle(new PulseRing(NPC.Center, Vector2.Zero, Color.Cyan, new Vector2(1f), 0, 0.1f, 7f, 30));
                    ParticleHandler.SpawnParticle(new PulseRing(NPC.Center, Vector2.Zero, CASharedData.AnomalyUltramundaneColor * 0.8f, new Vector2(2f), 0, 0.05f, 6f, 36));
                    ParticleHandler.SpawnParticle(new PulseRing(NPC.Center, Vector2.Zero, Color.LightCyan, new Vector2(1f), 0, 0.05f, 4f, 45));
                    for (int i = 0; i < 100; i++)
                    {
                        Vector2 dustVel = new PolarVector2(15f, Main.rand.NextFloat(100f));
                        Dust.NewDustPerfectAction(NPC.Center + dustVel * 3f, Main.rand.NextBool(4) ? DustID.BlueTorch : DustID.IceTorch, d =>
                        {
                            d.velocity = dustVel * Main.rand.NextFloat(0.3f, 1.3f);
                            d.scale = Main.rand.NextFloat(2f, 3.2f);
                            d.noGravity = true;
                        });
                    }
                    SoundEngine.PlaySound(BulletHellEndSound, NPC.Center);
                    SoundEngine.PlaySound(SoundID.DD2_DarkMageCastHeal, Target.Center);

                    CurrentAttack = AttackType.NonSpell1;
                    Timer1 = 0;
                    break;
            }
        }
    }

    public delegate void Orig_DrawForcefield(SupremeCalamitas self, SpriteBatch spriteBatch);

    [DetourMethodTo<SupremeCalamitas>]
    public static void Detour_DrawForcefield(Orig_DrawForcefield orig, SupremeCalamitas self, SpriteBatch spriteBatch)
    {
        if (!self.permafrost)
        {
            orig(self, spriteBatch);
            return;
        }

        spriteBatch.EnterShaderRegion();

        float lifeRatio = self.NPC.LifeRatio;

        if (lifeRatio < 0.05f)
            self.forcefieldOpacity = 0.75f;
        if (lifeRatio <= 0.01f)
            self.forcefieldOpacity = 0.6f;

        float flickerPower = 0f;
        if (lifeRatio < 0.6f)
            flickerPower += 0.1f;
        if (lifeRatio < 0.3f)
            flickerPower += 0.25f;
        if (self.postMusicHit)
            flickerPower += 0.61f;
        if (lifeRatio < 0.05f)
            flickerPower += Main.rand.NextFloat(0.7f, 1f);
        if (lifeRatio <= 0.01f)
            flickerPower += 0.08f;
        float opacity = self.forcefieldOpacity;
        opacity *= MathHelper.Lerp(1f, MathHelper.Max(1f - flickerPower, 0.56f), MathF.Pow(MathF.Cos(Main.GlobalTimeWrappedHourly * MathHelper.Lerp(3f, 5f, flickerPower)), 24));
        opacity *= self.musicSyncCounter is <= 0 and > -30 ? Utils.GetLerpValue(120, 0, self.musicSyncCounter, true) : 0.75f;

        Texture2D forcefieldTexture = ForcefieldTexture.Value;
        MiscShaderData miscShaderData = GameShaders.Misc["CalamityMod:SupremeShield"];
        miscShaderData.UseImage1("Images/Misc/Perlin");

        Color forcefieldColor = Color.Cyan;
        Color secondaryForcefieldColor = Color.SkyBlue;

        forcefieldColor *= opacity;
        secondaryForcefieldColor *= opacity;

        miscShaderData.UseSecondaryColor(secondaryForcefieldColor);
        miscShaderData.UseColor(forcefieldColor);
        miscShaderData.UseSaturation(1);
        miscShaderData.UseOpacity(0.65f);
        miscShaderData.Apply();

        if (self.postMusicHit)
            spriteBatch.Draw(Permafrost_Handler.CenterTexture, self.NPC.Center - Main.screenPosition, null, Color.White with { A = 0 } * opacity * 2f, self.rotateAwayPlayer, Permafrost_Handler.CenterTexture.Size() * 0.5f, self.forcefieldScale * 0.088f * self.forcefieldPureVisualScale, SpriteEffects.None, 0f);
        if (!self.NPC.dontTakeDamage)
            spriteBatch.Draw(forcefieldTexture, self.NPC.Center - Main.screenPosition, null, Color.White * opacity, self.postMusicHit ? self.rotateToPlayer : 0, forcefieldTexture.Size() * 0.5f, self.forcefieldScale * 3f * self.forcefieldPureVisualScale, SpriteEffects.None, 0f);
        else
            spriteBatch.Draw(Permafrost_Handler.ImmuneTexture, self.NPC.Center - Main.screenPosition, null, Color.White * opacity * 0.3f, self.rotateToPlayer, Permafrost_Handler.ImmuneTexture.Size() * 0.5f, self.forcefieldScale * 1.35f * self.forcefieldPureVisualScale, SpriteEffects.None, 0f);
        spriteBatch.ExitShaderRegion();
    }

    public override bool PreDrawCalBossBar(BetterBossHPUI newBar, SpriteBatch spriteBatch, ref int x, ref int y)
    {
        newBar.DrawMainBar(spriteBatch, x, y);
        newBar.DrawComboBar(spriteBatch, x, y);
        newBar.DrawSeperatorBar(spriteBatch, x, y, Data.BlueColor * newBar.AnimationCompletionRatio2);
        newBar.DrawNPCName(spriteBatch, x, y, null,
            Data.BlueColor * newBar.AnimationCompletionRatio2,
            Data.NameColor * newBar.AnimationCompletionRatio2,
            Math.Clamp(NPC.ActiveTime, 0f, 360f) / 240f + TOMathUtils.TimeWrappingFunction.GetTimeSin(0.5f, 1f, TOMathUtils.PiOver3, true) + NPC.LostLifeRatio / 2f);
        newBar.DrawBigLifeText(spriteBatch, x, y);
        newBar.DrawExtraSmallText(spriteBatch, x, y);

        return false;
    }
}

public sealed class PermafrostRitualDrama : CASingleProjectileBehavior<SCalRitualDrama>, ILocalizationPrefix
{
    public string LocalizationPrefix => CASharedData.ModLocalizationPrefix + "DeveloperContents.Permafrost";

    public override decimal Priority => 935m; //ICE

    public override bool ShouldProcess => Projectile.ai[1] == 1f; //将召唤永冻的弹幕

    public override bool PreAI()
    {
        CalamityPlayer calamityPlayer = CalamityUtils_Bridged.GetCalamityPlayer(Main.LocalPlayer);

        if (Projectile.timeLeft == 689)
        {
            for (int i = 0; i < 2; i++)
                ParticleHandler.SpawnParticle(new BloomParticle(Projectile.Center, Vector2.Zero, Color.Lerp(Color.Blue, Color.Cyan, 0.7f), 0f, 0.55f, 270, 1f));
            ParticleHandler.SpawnParticle(new BloomParticle(Projectile.Center, Vector2.Zero, Color.White, 0f, 0.5f, 270, 1f));
        }

        if (Projectile.timeLeft == 689 - 180)
            ParticleHandler.SpawnParticle(new BloomParticle(Projectile.Center, Vector2.Zero, CASharedData.AnomalyUltramundaneColor, 0f, 0.85f, 90, 1f));

        // If needed, these effects may continue after the ritual timer, to ensure that there are no awkward
        // background changes between the time it takes for SCal to appear after this projectile is gone.
        // If SCal is already present, this does not happen.
        if (!NPC.AnyNPCs<SupremeCalamitas>())
        {
            SCalSky.OverridingIntensity = Utils.GetLerpValue(90f, TotalRitualTime - 25f, ModProjectile.Time, true);
            calamityPlayer.GeneralScreenShakePower = Utils.GetLerpValue(90f, TotalRitualTime - 25f, ModProjectile.Time, true);
            calamityPlayer.GeneralScreenShakePower *= Utils.GetLerpValue(3400f, 1560f, Main.LocalPlayer.Distance(Projectile.Center), true) * 4f;
        }

        // Summon SCal right before the ritual effect ends.
        // The projectile lingers a little longer, however, to ensure that desync delays in MP do not interfere with the background transition.
        if (ModProjectile.Time == TotalRitualTime - 1f)
        {
            // Summon Permafrost.
            // All the other acoustic and visual effects can happen client-side.
            if (TOSharedData.GeneralClient)
            {
                Vector2 spawnPosition = Projectile.Center - new Vector2(53f, 39f);
                NPC.NewNPCAction<SupremeCalamitas>(NPC.GetBossSpawnSource(Player.FindClosest(spawnPosition, 1, 1)), spawnPosition, action: n =>
                {
                    TOLocalizationUtils.ChatLocalizedText(this, "Spawn", Color.Lerp(Color.Blue, Color.Cyan, 0.7f));
                    n.GetModNPC<SupremeCalamitas>().permafrost = true;
                });
            }

            // Make sound.
            SoundEngine.PlaySound(Cryogen.DeathSound, Projectile.Center);

            // Make a sudden screen shake.
            calamityPlayer.GeneralScreenShakePower = Utils.GetLerpValue(3400f, 1560f, Main.LocalPlayer.Distance(Projectile.Center), true) * 16f;

            // Generate a dust explosion at the ritual's position.
            for (int i = 0; i < 90; i++)
            {
                Dust.NewDustPerfectAction(Projectile.Center, DustID.IceGolem, d =>
                {
                    d.velocity = new PolarVector2(Main.rand.NextFloat(1.5f, 36f), Main.rand.NextFloat(100f));
                    d.scale = Main.rand.NextFloat(1.2f, 2.3f);
                    d.noGravity = true;
                });
            }
            for (int i = 0; i < 40; i++)
            {
                Vector2 sparkVel = new PolarVector2(Main.rand.NextFloat(2f, 22f), Main.rand.NextFloat(100f));
                ParticleHandler.SpawnParticle(new OrbParticle(Projectile.Center + sparkVel * 2, sparkVel, 120, Main.rand.NextFloat(1.55f, 2.75f), Color.Cyan));
            }
            ParticleHandler.SpawnParticle(new PulseRing(Projectile.Center, Vector2.Zero, Color.Cyan, new Vector2(2f), 0, 0f, 4f, 55));
            ParticleHandler.SpawnParticle(new PulseRing(Projectile.Center, Vector2.Zero, CASharedData.AnomalyUltramundaneColor * 0.8f, new Vector2(2f), 0, 0f, 2.8f, 58));
            ParticleHandler.SpawnParticle(new PulseRing(Projectile.Center, Vector2.Zero, Color.LightCyan, new Vector2(2f), 0, 0f, 2f, 60));
        }

        if (ModProjectile.Time >= TotalRitualTime)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient && !NPC.AnyNPCs<SupremeCalamitas>())
                Projectile.Kill();
            return false;
        }

        int fireReleaseRate = ModProjectile.Time > 150f ? 2 : 1;
        for (int i = 0; i < fireReleaseRate; i++)
        {
            if (Main.rand.NextBool())
            {
                float variance = Main.rand.NextFloat(-25f, 25f);
                Dust.NewDustPerfectAction(Projectile.Center + new Vector2(variance, 20), DustID.RainbowMk2, d =>
                {
                    d.velocity = new PolarVector2(ModProjectile.Time * 0.023f * Main.rand.NextFloat(1.1f, 2.1f), variance * 0.02f - MathHelper.PiOver2);
                    d.color = Main.rand.NextBool() ? Color.Cyan : CASharedData.AnomalyUltramundaneColor;
                    d.scale = Main.rand.NextFloat(0.35f, 1.2f);
                    d.fadeIn = 0.7f;
                    d.noGravity = true;
                });
            }
        }

        ModProjectile.Time++;

        return false;
    }
}
*/