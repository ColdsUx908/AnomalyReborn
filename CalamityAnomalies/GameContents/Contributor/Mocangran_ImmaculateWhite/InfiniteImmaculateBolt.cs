using Terraria.GameContent.ItemDropRules;

namespace CalamityAnomalies.GameContents.Contributor.Mocangran_ImmaculateWhite;

public sealed class InfiniteImmaculateBolt : CAModProjectile
{
    //似乎是存了一个最大timelife，但是好像有些错位
    //方便热重载暂时从常量改成字段，不过这玩意真有必要做个常量吗
    public int LifeTime = 1200;

    public NPC Target
    {
        get
        {
            int temp = (int)Projectile.ai[0];
            return temp >= 0 && temp < Main.maxNPCs ? Main.npc[temp] : null;
        }

        set => Projectile.ai[0] = value?.whoAmI ?? -1;
    }
    public override string LocalizationCategory => "GameContents.Contributor";

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
        Projectile.timeLeft = 750;
        Projectile.tileCollide = false;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.ignoreWater = true;
        Projectile.extraUpdates = 4;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.ArmorPenetration = 200;

        Target = NPC.DummyNPC;
    }

    public override void AI()
    {
        Timer1++;

        float linearAccelerationLimit = 10f;
        if (Projectile.velocity.Modulus <= linearAccelerationLimit)
        {
            float acceleration = 2f;
            Projectile.velocity.Modulus += acceleration;
        }
        else
            Projectile.velocity *= 1.01f;

        int starTime = 10;
        int maxLifeTime = 750;
        if (Timer1 > starTime)
        {
            bool targetIsValid = false;
            if (Target is not null && CanBeChaseable(Target)
                && Projectile.localNPCImmunity[Target.whoAmI] <= 0
                && Projectile.localNPCImmunity[Target.whoAmI] != -1
                && Target.immune[Projectile.owner] <= 0)
            {
                targetIsValid = true;
            }
            float homingRatio = Utils.Remap(Timer1, starTime, maxLifeTime, 0.2f, 0.6f); 
            if (!targetIsValid)
            {
                Target = GetNPCTarget(Projectile, Projectile.Center, 8000f, ignoreTiles: true, bossPriority: true, respectIFrames: true, canBeChaseable: true);
            }

            if (Target is not null && targetIsValid)
            {
                Projectile.HomeIn(Target, HomingAlgorithm.SmootherStep, homingRatio, 8000f, MathHelper.TwoPi, false);
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
        Rectangle sourceRectangle = new Rectangle(0, sourceY, projectileTexture.Width, frameHeight);
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

    public override bool? CanHitNPC(NPC target) => !target.friendly;

    /*public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        NPCMaxLife += target.life;
    }*/

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        Projectile.damage = (int)(Projectile.damage * 0.975f);
        Projectile.timeLeft = 300;
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

    //先暂时放这里  从灾厄抄过来的能看无敌帧索敌npc  合并到TO的方法里 （妈呀AI还能自动补全注释）
    //respectIFrames控制无敌帧开关
    //CanBeChaseable控制是否能追踪一些特殊NPC（比如正常情况下不被追踪弹幕索敌的地牢门口邪教徒）
    //可以改一下命名，我都随便命名的。
    public static NPC GetNPCTarget(Projectile projectile, Vector2 origin, float maxDistanceToCheck, bool ignoreTiles = true, bool bossPriority = false, PriorityType priorityType = PriorityType.Closest, bool respectIFrames = false, bool canBeChaseable = false)
    {
        float maxDistanceToCheckSquared = maxDistanceToCheck * maxDistanceToCheck;
        NPC target = null;
        bool hasPriority = false;
        switch (priorityType)
        {
            case PriorityType.LifeMax:
                if (bossPriority)
                {
                    foreach (NPC npc in NPC.ActiveNPCs)
                    {
                        bool canBeChased = canBeChaseable ? CanBeChaseable(npc) : npc.CanBeChasedBy();
                        if (!canBeChased || Vector2.DistanceSquared(origin, npc.Center) > maxDistanceToCheckSquared
                            || (!ignoreTiles && !Collision.CanHit(origin, 1, 1, npc.Center, 1, 1))
                            || (respectIFrames && (projectile.localNPCImmunity[npc.whoAmI] > 0 
                            || projectile.localNPCImmunity[npc.whoAmI] == -1 || npc.immune[projectile.owner] > 0)))
                            continue;

                        if (target is null)
                        {
                            target = npc;
                            hasPriority = npc.IsBossEnemy;
                            continue;
                        }
                        switch (hasPriority, npc.IsBossEnemy)
                        {
                            case (true, false):
                                break;
                            case (false, true):
                                target = npc;
                                hasPriority = true;
                                break;
                            case (true, true) or (false, false) when npc.lifeMax > target.lifeMax:
                                target = npc;
                                break;
                        }
                    }
                }
                else
                {
                    foreach (NPC npc in NPC.ActiveNPCs)
                    {
                        bool canBeChased = canBeChaseable ? CanBeChaseable(npc) : npc.CanBeChasedBy();
                        if (!canBeChased || Vector2.DistanceSquared(origin, npc.Center) > maxDistanceToCheckSquared 
                            || (!ignoreTiles && !Collision.CanHit(origin, 1, 1, npc.Center, 1, 1))
                            || (respectIFrames && (projectile.localNPCImmunity[npc.whoAmI] > 0
                            || projectile.localNPCImmunity[npc.whoAmI] == -1 || npc.immune[projectile.owner] > 0)))
                            continue;

                        if (target is null || npc.lifeMax > target.lifeMax)
                            target = npc;
                    }
                }
                return target;
            case PriorityType.Life:
                if (bossPriority)
                {
                    foreach (NPC npc in NPC.ActiveNPCs)
                    {
                        bool canBeChased = canBeChaseable ? CanBeChaseable(npc) : npc.CanBeChasedBy();
                        if (!canBeChased || Vector2.DistanceSquared(origin, npc.Center) > maxDistanceToCheckSquared 
                            || (!ignoreTiles && !Collision.CanHit(origin, 1, 1, npc.Center, 1, 1))
                            || (respectIFrames && (projectile.localNPCImmunity[npc.whoAmI] > 0
                            || projectile.localNPCImmunity[npc.whoAmI] == -1 || npc.immune[projectile.owner] > 0)))
                            continue;

                        if (target is null)
                        {
                            target = npc;
                            hasPriority = npc.IsBossEnemy;
                            continue;
                        }
                        switch (hasPriority, npc.IsBossEnemy)
                        {
                            case (true, false):
                                break;
                            case (false, true):
                                target = npc;
                                hasPriority = true;
                                break;
                            case (true, true) or (false, false) when npc.life > target.life:
                                target = npc;
                                break;
                        }
                    }
                }
                else
                {
                    foreach (NPC npc in NPC.ActiveNPCs)
                    {
                        bool canBeChased = canBeChaseable ? CanBeChaseable(npc) : npc.CanBeChasedBy();
                        if (!canBeChased || Vector2.DistanceSquared(origin, npc.Center) > maxDistanceToCheckSquared 
                            || (!ignoreTiles && !Collision.CanHit(origin, 1, 1, npc.Center, 1, 1))
                            || (respectIFrames && (projectile.localNPCImmunity[npc.whoAmI] > 0
                            || projectile.localNPCImmunity[npc.whoAmI] == -1 || npc.immune[projectile.owner] > 0)))
                            continue;

                        if (target is null || npc.life < target.life)
                            target = npc;
                    }
                }
                return target;
            case PriorityType.Closest:
            default:
                float distanceTemp = maxDistanceToCheckSquared;
                if (bossPriority)
                {
                    foreach (NPC npc in NPC.ActiveNPCs)
                    {
                        float distanceSquared = Vector2.DistanceSquared(origin, npc.Center);
                        bool canBeChased = canBeChaseable ? CanBeChaseable(npc) : npc.CanBeChasedBy();
                        if (!canBeChased || distanceSquared > maxDistanceToCheckSquared 
                            || (!ignoreTiles && !Collision.CanHit(origin, 1, 1, npc.Center, 1, 1))
                            || (respectIFrames && (projectile.localNPCImmunity[npc.whoAmI] > 0
                            || projectile.localNPCImmunity[npc.whoAmI] == -1 || npc.immune[projectile.owner] > 0)))
                            continue;

                        if (target is null)
                        {
                            target = npc;
                            hasPriority = npc.IsBossEnemy;
                            distanceTemp = distanceSquared;
                            continue;
                        }
                        switch (hasPriority, npc.IsBossEnemy)
                        {
                            case (true, false):
                                break;
                            case (false, true):
                                target = npc;
                                distanceTemp = distanceSquared;
                                hasPriority = true;
                                break;
                            case (true, true) or (false, false) when distanceSquared < distanceTemp:
                                target = npc;
                                distanceTemp = distanceSquared;
                                break;
                        }
                    }
                }
                else
                {
                    foreach (NPC npc in NPC.ActiveNPCs)
                    {
                        float distanceSquared = Vector2.DistanceSquared(origin, npc.Center);
                        bool canBeChased = canBeChaseable ? CanBeChaseable(npc) : npc.CanBeChasedBy();
                        if (!canBeChased || distanceSquared > maxDistanceToCheckSquared 
                            || (!ignoreTiles && !Collision.CanHit(origin, 1, 1, npc.Center, 1, 1))
                            || (respectIFrames && (projectile.localNPCImmunity[npc.whoAmI] > 0
                            || projectile.localNPCImmunity[npc.whoAmI] == -1 || npc.immune[projectile.owner] > 0)))
                            continue;

                        if (target is null || distanceSquared < distanceTemp)
                        {
                            target = npc;
                            distanceTemp = distanceSquared;
                        }
                    }
                }
                return target;
        }
    }
    
    //一个看起来很扯的方法,返回一些特殊NPC（比如正常情况下不被追踪弹幕索敌的地牢门口邪教徒）
    public static bool CanBeChaseable(NPC npc, object attacker = null, bool ignoreDontTakeDamage = false)
    {
        if (npc.active && npc.lifeMax > 5 && (!npc.dontTakeDamage || ignoreDontTakeDamage) && !npc.friendly)
            return !npc.immortal;

        return false;
    }
}
