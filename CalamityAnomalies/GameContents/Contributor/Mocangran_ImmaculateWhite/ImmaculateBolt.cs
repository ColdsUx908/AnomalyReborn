// Developed by ColdsUx

namespace CalamityAnomalies.GameContents.Contributor.Mocangran_ImmaculateWhite;

public sealed class ImmaculateBolt : CAModProjectile
{
    public const int LifeTime = 2400;

    public NPC Target
    {
        get
        {
            int temp = (int)Projectile.ai[0];
            return temp >= 0 && temp < Main.maxNPCs ? Main.npc[temp] : null;
        }

        set => Projectile.ai[0] = value?.whoAmI ?? -1;
    }

    public bool IsSmallProjectile
    {
        get => AI_Union_2.bits[0];
        set
        {
            Union32 union = AI_Union_2;
            union.bits[0] = value;
            AI_Union_2 = union;
        }
    }

    public bool IsInfiniteProjectile
    {
        get => AI_Union_2.bits[1];
        set
        {
            Union32 union = AI_Union_2;
            union.bits[1] = value;
            AI_Union_2 = union;
        }
    }

    public bool IsSplittableProjectile
    {
        get => AI_Union_2.bits[2];
        set
        {
            Union32 union = AI_Union_2;
            union.bits[2] = value;
            AI_Union_2 = union;
        }
    }

    public bool IsNotFirstHit
    {
        get => AI_Union_2.bits[3];
        set
        {
            Union32 union = AI_Union_2;
            union.bits[3] = value;
            AI_Union_2 = union;
        }
    }

    public override string LocalizationCategory => "GameContents.Contributor";

    public string LocalizationPrefix => CASharedData.ModLocalizationPrefix + LocalizationCategory + "ImmaculateWhite";

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 60;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 8000;
        ProjectileID.Sets.CanHitPastShimmer[Projectile.type] = true;
    }

    public override void SetDefaults()
    {
        Projectile.width = 30;
        Projectile.height = 30;
        Projectile.alpha = 255;
        Projectile.friendly = true;
        Projectile.timeLeft = LifeTime;
        Projectile.tileCollide = false;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.ignoreWater = true;
        Projectile.extraUpdates = 4;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 50;
        Projectile.ArmorPenetration = 200;

        Target = NPC.DummyNPC;
    }

    public override void AI()
    {
        if (Timer1 == 0) //初始化
        {
            if (IsInfiniteProjectile)
            {
                Projectile.timeLeft /= 4;
                Projectile.localNPCHitCooldown = -1;
            }
        }

        Timer1++;

        if (IsInfiniteProjectile)
        {
            float linearAccelerationLimit = 10f;
            if (Projectile.velocity.Modulus <= linearAccelerationLimit)
            {
                float acceleration = 2f;
                Projectile.velocity.Modulus += acceleration;
            }
            else
                Projectile.velocity *= 1.01f;

            int starTime = 20;
            if (Timer1 > starTime)
            {
                bool targetIsValid = false;
                if (Target is not null && Target.CanBeChasedBy_IgnoreChaseable()
                    && Projectile.localNPCImmunity[Target.whoAmI] <= 0
                    && Projectile.localNPCImmunity[Target.whoAmI] != -1
                    && Target.immune[Projectile.owner] <= 0)
                {
                    targetIsValid = true;
                }

                float homingRatio = Utils.Remap(Timer1, starTime, LifeTime / 4, 0.2f, 0.6f);
                if (!targetIsValid && Projectile.IsFinalUpdate)
                    Target = TOKinematicUtils.GetNPCTarget_Advanced(Projectile.Center, 8000f, ignoreTiles: true, respectImmuneFrames: true, projectile: Projectile, ignoreChaseable: true, bossPriority: true);

                if (Target is not null && targetIsValid)
                    Projectile.HomeIn(Target, HomingAlgorithm.SmootherStep, homingRatio, 8000f, MathHelper.TwoPi, false);
            }
        }
        else
        {
            float linearAccelerationLimit = !IsSmallProjectile ? 10f : 6f;
            if (Projectile.velocity.Modulus <= linearAccelerationLimit)
            {
                float acceleration = !IsSmallProjectile ? 0.5f : 0.3f;
                Projectile.velocity.Modulus += acceleration;
            }
            else
                Projectile.velocity *= 1.005f;

            int starTime = 20;
            if (Timer1 > starTime)
            {
                float homingRatio = Utils.Remap(Timer1, starTime, LifeTime / 2, 0.2f, 0.5f);
                if (Target is null || !Target.CanBeChasedBy() || !Projectile.HomeIn(Target, HomingAlgorithm.SmootherStep, homingRatio, 8000f, MathHelper.TwoPi, false) && Projectile.IsFinalUpdate)
                    Target = TOKinematicUtils.GetNPCTarget(Projectile.Center, 8000f, ignoreTiles: true);
            }
        }

        Projectile.VelocityToRotation(MathHelper.PiOver2);
    }


    public override Color? GetAlpha(Color lightColor) => Color.White * Projectile.Opacity;

    public override bool PreDraw(ref Color drawColor)
    {
        SpriteEffects spriteEffects = SpriteEffects.None;
        int type = Projectile.type;
        Asset<Texture2D> asset = TextureAssets.Projectile[type];
        Texture2D projectileTexture = asset.Value;
        int frameHeight = asset.Height() / Main.projFrames[type];
        int sourceY = frameHeight * Projectile.frame;
        Rectangle sourceRectangle = new(0, sourceY, projectileTexture.Width, frameHeight);
        Vector2 origin = sourceRectangle.Size() / 2f;
        Vector2 positionOffset = Vector2.Zero;
        float rotationOffset = 0f;
        Rectangle trailSourceRect = sourceRectangle;

        // Trail settings
        // Actual trail values after override
        int trailStart = 39;
        float scaleLerpMax = 40f;
        int trailEnd = 0;
        int trailStep = -1;
        float targetScale = 1.4f * Projectile.scale;

        // Draw trailing afterimages
        for (int i = trailStart; (trailStep > 0 && i < trailEnd) || (trailStep < 0 && i > trailEnd); i += trailStep)
        {
            if (i >= Projectile.oldPos.Length)
                continue;

            Color trailColor = Color.White;
            trailColor.A /= 2;
            trailColor *= Utils.GetLerpValue(0f, 50f, Projectile.timeLeft, clamped: true);

            float fadeIndex = trailEnd - i;
            if (trailStep < 0)
                fadeIndex = trailStart - i;

            trailColor *= fadeIndex / (ProjectileID.Sets.TrailCacheLength[type] * 1.5f);

            Vector2 oldPosition = Projectile.oldPos[i];
            float oldRotation = Projectile.rotation;
            SpriteEffects oldEffects = spriteEffects;
            if (ProjectileID.Sets.TrailingMode[type] is 2 or 3 or 4)
            {
                oldRotation = Projectile.oldRot[i];
                oldEffects = (Projectile.oldSpriteDirection[i] == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            }

            if (oldPosition == Vector2.Zero)
                continue;

            Vector2 drawPosition = oldPosition + positionOffset + Projectile.Size / 2f - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            float trailScale = MathHelper.Lerp(Projectile.scale, targetScale, i / scaleLerpMax);
            Main.EntitySpriteDraw(projectileTexture, drawPosition, trailSourceRect, trailColor,
                oldRotation + rotationOffset,
                origin, trailScale, oldEffects);
        }

        // Base glow layer
        Color baseColor = Color.White;
        baseColor.A = 0;

        Vector2 centerScreenPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
        Main.EntitySpriteDraw(projectileTexture, centerScreenPos, sourceRectangle, baseColor, Projectile.rotation, origin, Projectile.scale * 0.9f, spriteEffects);

        // Sharp tears effect overlay。七彩矢额外闪烁特效：其实就是把七彩矢贴图拉长然后横着一个竖着一个
        Texture2D sharpTearsTexture = TextureAssets.Extra[ExtrasID.SharpTears].Value;
        Color tearsBaseColor = baseColor;
        Vector2 tearsOrigin = sharpTearsTexture.Size() / 2f;
        Color tearsDimColor = baseColor * 0.5f;

        float glowFactor = Utils.GetLerpValue(15f, 30f, Projectile.timeLeft, clamped: true)//消失时的停止闪烁，但是对这个弹幕来说没太多必要（
            * Utils.GetLerpValue(LifeTime, LifeTime - 10, Projectile.timeLeft, clamped: true)//刚生成时逐渐开始闪烁.
            * (1f + 0.2f * (float)Math.Cos(Main.GlobalTimeWrappedHourly % 30f / 0.5f * ((float)Math.PI * 2f) * 3f))//这里是控制大小闪烁
            * 0.8f * Projectile.scale;

        Vector2 glowScaleVertical = new Vector2(0.5f, 5f) * glowFactor;
        Vector2 glowScaleHorizontal = new Vector2(0.5f, 2f) * glowFactor;
        tearsBaseColor *= glowFactor;
        tearsDimColor *= glowFactor;

        // Offset multiplier is zero, so position stays the same
        int glowOffsetMultiplier = 0;
        Vector2 glowPosition = centerScreenPos + Projectile.velocity.SafeNormalize(Vector2.Zero) * MathHelper.Lerp(0.5f, 1f, Projectile.localAI[0] / 60f) * glowOffsetMultiplier;

        Main.EntitySpriteDraw(sharpTearsTexture, glowPosition, null, tearsBaseColor, (float)Math.PI / 2f, tearsOrigin, glowScaleVertical, spriteEffects);
        Main.EntitySpriteDraw(sharpTearsTexture, glowPosition, null, tearsBaseColor, 0f, tearsOrigin, glowScaleHorizontal, spriteEffects);
        Main.EntitySpriteDraw(sharpTearsTexture, glowPosition, null, tearsDimColor, (float)Math.PI / 2f, tearsOrigin, glowScaleVertical * 0.6f, spriteEffects);
        Main.EntitySpriteDraw(sharpTearsTexture, glowPosition, null, tearsDimColor, 0f, tearsOrigin, glowScaleHorizontal * 0.6f, spriteEffects);

        // Final semi-transparent draw of the projectile itself
        Color finalColor = Projectile.GetAlpha(drawColor);
        float finalScale = Projectile.scale;
        float finalRotation = Projectile.rotation + rotationOffset;
        finalColor.A /= 2;

        Main.EntitySpriteDraw(projectileTexture, Projectile.Center + positionOffset - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY),
            sourceRectangle, finalColor, finalRotation, origin, finalScale, spriteEffects);

        return false;
    }

    public override bool? CanHitNPC(NPC target)
    {
        if (target.friendly || IsSmallProjectile && Timer1 <= 40)
            return false;

        if (!IsInfiniteProjectile && target != Target)
            return false;

        return true;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        TOCombatTextUtils.ChangeHitNPCText(t => t.color = Color.White);

        if (IsInfiniteProjectile)
        {
            Projectile.damage = (int)Math.Round(Projectile.damage * 0.96f);
        }
        else
        {
            if (IsSplittableProjectile && !IsSmallProjectile)
            {
                for (int i = 0; i < 2; i++)
                {
                    Projectile.NewProjectileAction<ImmaculateBolt>(Projectile.GetSource_FromAI(), Projectile.Center, Main.rand.NextPolarVector2(2f, 2.5f), Projectile.damage / 3, Projectile.knockBack * 0.4f, Projectile.owner, p =>
                    {
                        ImmaculateBolt modP = p.GetModProjectile<ImmaculateBolt>();

                        p.scale /= 2f;
                        modP.IsSmallProjectile = true;
                    });
                }
            }

            Projectile.Kill();
        }
    }

    public override void OnKill(int timeLeft)
    {
        int dustAmount = (int)Math.Round(20 * Projectile.scale);
        float singleRotation = MathHelper.TwoPi / dustAmount;
        float initialRotation = Projectile.velocity.ToRotation();
        for (int i = 0; i < dustAmount; i++)
        {
            float rotation = initialRotation + singleRotation * i;
            Dust.NewDustPerfectAction(Projectile.Center, DustID.RainbowMk2, d =>
            {
                d.fadeIn = 1f;
                d.noGravity = true;
                d.alpha = 100;
                d.color = Color.White;
                if (i % 4 == 0)
                {
                    d.velocity = rotation.ToRotationVector2() * 3.2f;
                    d.scale = 2.3f;
                }
                else if (i % 2 == 0)
                {
                    d.velocity = rotation.ToRotationVector2() * 1.8f;
                    d.scale = 1.9f;
                }
                else
                {
                    d.velocity = rotation.ToRotationVector2();
                    d.scale = 1.6f;
                }
                d.velocity += Projectile.velocity * Main.rand.NextFloat();
            });
        }
    }
}
