namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed class SkeletronLegendHand : AnomalyNPCBehavior<SkeletronLegendHand>
{
    private const int AttackCycleDuration = 300;
    private const int WindupStart = 180;
    private const int StrikeTime = 210;
    private const int MinimumStrikeDuration = 20;
    private const int MaximumStrikeDuration = 50;
    private const float MaximumAttackDistance = 800f;

    /// <summary>常态闲置偏移；相较原版约 (200,190)，略微靠近头部。</summary>
    public static Vector2 GetIdleOffset(int side) => new(side * 140f, 120f);

    /// <summary>沿用原版 atan2(参考点-手部)+π/2；参考点取原版第二段骨节的 (±50,80)，使手掌顺接前臂。</summary>
    public static float GetPositionRotation(Vector2 handCenter, Vector2 headCenter, int side)
    {
        Vector2 direction = headCenter + new Vector2(side * 50f, 80f) - handCenter;
        return direction.LengthSquared() > 0.001f ? MathHelper.WrapAngle(direction.ToRotation() + MathHelper.PiOver2) : 0f;
    }

    // 保留原版手的 ai 布局：ai[0] 左右侧，ai[1] 头部索引，ai[2] 动作，ai[3] 待机计时。
    public NPC Master => NPC.Master;
    public override int ApplyingType => NPCID.SkeletronHand;

    public override decimal Priority => 10m;
    public override bool ShouldProcess
    {
        get
        {
            if (!base.ShouldProcess)
                return false;
            if (AnomalyNPC.CustomAIType == 1)
                return true;

            // 参照召唤仪式，在 SetDefaults 等早期环节识别正在生成的手。
            foreach (NPC head in NPC.ActiveNPCs)
            {
                if (head.type == NPCID.SkeletronHead && head.Anomaly.CustomAIType == 1
                    && SkeletronLegendHead.GetInstance(head).IsSummoningHands)
                    return true;
            }
            return false;
        }
    }

    public override bool AllowCalamityLogic(CalamityLogicType_NPCBehavior type) => type switch
    {
        CalamityLogicType_NPCBehavior.VanillaOverrideAI or CalamityLogicType_NPCBehavior.PreAI or CalamityLogicType_NPCBehavior.GetAlpha => false,
        _ => true
    };

    public override void SetDefaultsFinal()
    {
        NPC.lifeMax = 1500;
        NPC.defense = 9999;
    }

    public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
    {
        modifiers.SetMaxDamage(1);
        modifiers.ModifyHitInfo += static (ref hit) => hit.Damage = 1;
    }

    public override bool PreAI()
    {
        using BossNetUpdateScope netUpdate = new(NPC);

        //手部无敌，且不可追踪
        NPC.chaseable = false;
        NPC.dontTakeDamage = true;

        if (!NPC.TryGetMaster(NPCID.SkeletronHead, out NPC master) || master.Anomaly.CustomAIType != 1)
        {
            if (TONetworkEnvironment.NotClient)
            {
                NPC.active = false;
                NPC.netUpdate = true;
            }
            return false;
        }

        SkeletronLegendHead head = SkeletronLegendHead.GetInstance(master);
        NPC.spriteDirection = -(int)NPC.ai[0];
        NPC.target = master.target;
        NPC.dontTakeDamage = !head.Phase2;
        int increasedLife = Math.Min(NPC.lifeMax - NPC.life, Math.Max(1, NPC.lifeMax / 60));
        if (increasedLife > 0)
        {
            NPC.life += increasedLife;
            NPC.HealEffect(increasedLife, true);
        }
        if (!NPC.TargetClosestIfInvalid(true, SkeletronLegendHead.DespawnDistance))
        {
            NPC.velocity *= 0.8f;
            return false;
        }
        if (head.Mechanic2InBackground)
            Mechanic2HandAI(head);
        else
        {
            AnomalyNPC.Z = 0f;
            VanillaHandAI(head);
            NPC.rotation = GetPositionRotation(NPC.Center, Master.Center, -Math.Sign(NPC.ai[0]));
        }
        return false;

        // 从原版 NPC.AI 的 aiStyle == 12（type 36）复制，保留 0/3 待机、1/2 下拍、4/5 横扫。
        // 改动：显式 Master、头部共享时序、更近更快的回收，以及更快且距离受限的挥击。
        void VanillaHandAI(SkeletronLegendHead head)
        {
            bool canAttack = head.Phase2 && !head.Mechanic1Rotating;
            int timer = canAttack ? head.Timer1 % AttackCycleDuration : 0;
            NPC.ai[3] = timer;
            if (!canAttack || timer >= StrikeTime + MaximumStrikeDuration)
                ChangeAction(0);
            else if (timer == WindupStart)
                ChangeAction(head.Timer1 / AttackCycleDuration % 2 == 0 ? 1 : 4);

            if (NPC.ai[2] is 0f or 3f)
            {
                // 一阶段、转阶段和二阶段常态共用较靠近头部的斜下方闲置点。
                Vector2 destination = Master.Center + GetIdleOffset(-Math.Sign(NPC.ai[0]));
                Vector2 offset = destination - NPC.Center;
                Vector2 desiredVelocity = offset.ToCustomLength(Math.Min(28f, offset.Length() * 0.15f));
                NPC.velocity = Vector2.Lerp(NPC.velocity, desiredVelocity, 0.2f);
                return;
            }

            if (NPC.ai[2] == 1f)
            {
                Vector2 handCenter = NPC.Center;
                float targetOffsetX = Master.position.X + Master.width / 2 - 300f * NPC.ai[0] - handCenter.X;
                float targetOffsetY = Master.position.Y + 160f - handCenter.Y;
                float targetDistance = Math.Max(0.01f, (float)Math.Sqrt(targetOffsetX * targetOffsetX + targetOffsetY * targetOffsetY));
                NPC.velocity.X *= 0.95f;
                NPC.velocity.Y -= 0.1f;
                NPC.velocity.Y -= 0.06f;
                if (NPC.velocity.Y < -13f)
                    NPC.velocity.Y = -13f;

                if (head.Timer1 % AttackCycleDuration == StrikeTime)
                {
                    NPC.ai[2] = 2f;
                    handCenter = NPC.Center;
                    targetOffsetX = Target.position.X + Target.width / 2 - handCenter.X;
                    targetOffsetY = Target.position.Y + Target.height / 2 - handCenter.Y;
                    targetDistance = Math.Max(0.01f, (float)Math.Sqrt(targetOffsetX * targetOffsetX + targetOffsetY * targetOffsetY));
                    float strikeSpeedFactor = 28f / targetDistance;
                    NPC.velocity.X = targetOffsetX * strikeSpeedFactor;
                    NPC.velocity.Y = targetOffsetY * strikeSpeedFactor;
                    NPC.netUpdate = true;
                }
            }
            else if (NPC.ai[2] == 2f)
            {
                if (timer >= StrikeTime + MinimumStrikeDuration
                    && (NPC.position.Y > Target.position.Y || Vector2.Dot(NPC.velocity, Target.Center - NPC.Center) <= 0f || NPC.Distance(Target.position) > MaximumAttackDistance || NPC.velocity.Y < 0f))
                    NPC.ai[2] = 3f;
            }
            else if (NPC.ai[2] == 4f)
            {
                Vector2 handCenter = NPC.Center;
                float targetOffsetX = Master.position.X + Master.width / 2 - 200f * NPC.ai[0] - handCenter.X;
                float targetOffsetY = Master.position.Y + 230f - handCenter.Y;
                float targetDistance = Math.Max(0.01f, (float)Math.Sqrt(targetOffsetX * targetOffsetX + targetOffsetY * targetOffsetY));
                NPC.velocity.Y *= 0.95f;
                NPC.velocity.X += 0.1f * (0f - NPC.ai[0]);
                NPC.velocity.X += 0.07f * (0f - NPC.ai[0]);
                if (NPC.velocity.X < -12f)
                    NPC.velocity.X = -12f;
                else if (NPC.velocity.X > 12f)
                    NPC.velocity.X = 12f;

                if (head.Timer1 % AttackCycleDuration == StrikeTime)
                {
                    NPC.ai[2] = 5f;
                    handCenter = NPC.Center;
                    targetOffsetX = Target.position.X + Target.width / 2 - handCenter.X;
                    targetOffsetY = Target.position.Y + Target.height / 2 - handCenter.Y;
                    targetDistance = Math.Max(0.01f, (float)Math.Sqrt(targetOffsetX * targetOffsetX + targetOffsetY * targetOffsetY));
                    float strikeSpeedFactor = 28f / targetDistance;
                    NPC.velocity.X = targetOffsetX * strikeSpeedFactor;
                    NPC.velocity.Y = targetOffsetY * strikeSpeedFactor;
                    NPC.netUpdate = true;
                }
            }
            else if (NPC.ai[2] == 5f && timer >= StrikeTime + MinimumStrikeDuration
                && ((NPC.velocity.X > 0f && NPC.position.X + NPC.width / 2 > Target.position.X + Target.width / 2) || (NPC.velocity.X < 0f && NPC.position.X + NPC.width / 2 < Target.position.X + Target.width / 2) || Vector2.Dot(NPC.velocity, Target.Center - NPC.Center) <= 0f || NPC.Distance(Target.position) > MaximumAttackDistance))
            {
                NPC.ai[2] = 0f;
            }
        }

        void ChangeAction(int action)
        {
            if (NPC.ai[2] == action)
                return;
            NPC.ai[2] = action;
            NPC.netUpdate = true;
        }
    }

    public override bool CanHitPlayer(Player target, ref int cooldownSlot) =>
        NPC.TryGetMaster(NPCID.SkeletronHead, out NPC master) && SkeletronLegendHead.GetInstance(master) is { Phase2: true, Mechanic2InBackground: false };

    private void Mechanic2HandAI(SkeletronLegendHead head)
    {
        NPC.ai[2] = 0f;
        int age = head.Mechanic2Age;
        int side = -Math.Sign(NPC.ai[0]);
        int strike = SkeletronLegendHead.Mechanic2Data.GetStrikeIndex(age);
        int relativeAge = age - SkeletronLegendHead.Mechanic2Data.GetStrikeTime(strike);
        bool selected = head.Mechanic2HandPattern != 0 && side == head.Mechanic2StrikeSide(strike);
        Vector2 center = head.Phase2Arena?.Projectile.Center ?? Master.Center;
        // 两手默认自然下垂；只有当前拍击手在预警期间抬起。
        // 闲置手保持实际左右位置，随头部一起掠过，不额外改变左右侧。
        Vector2 restingCenter = Master.Center + SkeletronLegendHead.Mechanic2Data.GetRestingHandOffset(side);
        Vector2 destination = restingCenter;
        float z = head.AnomalyNPC.Z;
        if (selected && relativeAge >= -SkeletronLegendHead.Mechanic2Data.HandRaiseLeadTime && age < SkeletronLegendHead.Mechanic2Data.ReturnStart)
        {
            float attackProgress = SkeletronLegendHead.Mechanic2Data.HandAttackProgress(age);
            float slam = SkeletronLegendHead.Mechanic2Data.HandSlamProgress(relativeAge);
            Vector2 raised = center + new Vector2(side * 540f, -700f);
            Vector2 impact = center + new Vector2(side * 540f, 200f);
            destination = Vector2.Lerp(destination, Vector2.Lerp(raised, impact, slam), attackProgress);
            z = MathHelper.Lerp(head.AnomalyNPC.Z, 0f, slam * attackProgress);
        }
        NPC.velocity = Vector2.Zero;
        NPC.Center = destination;
        AnomalyNPC.Z = z;
        if (!selected && head.AnomalyNPC.Depth_DrawAnchor is { } anchor)
        {
            // 仅复制 Z 会漏掉头部为贴近玩家头顶而产生的屏幕平移。
            // 以同一世界锚点投影，使闲置手和头部获得完全相同的视觉平移。
            Vector2 localOffset = Master.Center + Master.netOffset + anchor.LocalOffset - NPC.Center - NPC.netOffset;
            AnomalyNPC.Depth_DrawAnchor = (localOffset, anchor.Target, anchor.Weight);
        }
        // 原版从手掌连接点朝 (±200,130) 绘制第一段骨节；手掌朝向沿同一条轴自然下垂。
        // 始终以闲置位置计算基准，避免抬手时基准朝向随攻击位置变化。
        Vector2 wrist = restingCenter + new Vector2(side * 5f, 20f - NPC.height * 0.5f);
        Vector2 forearm = Master.Center + new Vector2(side * 200f, 130f) - wrist;
        float restingRotation = MathHelper.WrapAngle(forearm.ToRotation() + MathHelper.PiOver2);
        NPC.rotation = SkeletronLegendHead.Mechanic2Data.GetHandRotation(age, selected, restingRotation);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        // 原版骨节与血尘生成先执行；血尘由独立列表在本体的 Z 投影作用域内绘制。
        AnomalyNPC.Skeletron_HandBloodDust?.UpdateAndDraw(spriteBatch, NPC.netOffset - screenPos, NPC.Opacity);
        if (!NPC.TryGetMaster(NPCID.SkeletronHead, out NPC master))
            return true;
        SkeletronLegendHead head = SkeletronLegendHead.GetInstance(master);
        if (!head.Mechanic2InBackground)
            return true;
        float size = GetMechanic2VisualScale(head);
        Texture2D texture = TextureAssets.Npc[NPC.type].Value;
        spriteBatch.Draw(texture, NPC.Center + NPC.netOffset - screenPos, NPC.frame, Color.White * NPC.Opacity,
            NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale * size,
            NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
        return false;
    }

    private float GetMechanic2VisualScale(SkeletronLegendHead head)
    {
        int strike = SkeletronLegendHead.Mechanic2Data.GetStrikeIndex(head.Mechanic2Age);
        bool selected = head.Mechanic2HandPattern != 0 && -Math.Sign(NPC.ai[0]) == head.Mechanic2StrikeSide(strike);
        int relativeAge = head.Mechanic2Age - SkeletronLegendHead.Mechanic2Data.GetStrikeTime(strike);
        float growth = SkeletronLegendHead.Mechanic2Data.HandRaiseProgress(relativeAge);
        float recovery = 1f - MathHelper.SmoothStep(0f, 1f,
            MathHelper.Clamp((head.Mechanic2Age - SkeletronLegendHead.Mechanic2Data.ReturnStart) / (float)SkeletronLegendHead.Mechanic2Data.ReturnDuration, 0f, 1f));
        return selected ? 1f + 4f * growth * recovery * SkeletronLegendHead.Mechanic2Data.HandAttackProgress(head.Mechanic2Age) : 1f;
    }

    public override void ModifyHoverBoundingBox(ref Rectangle boundingBox)
    {
        if (NPC.TryGetMaster(NPCID.SkeletronHead, out NPC master)
            && SkeletronLegendHead.GetInstance(master) is { Mechanic2InBackground: true } head)
        {
            // 原版 hover box 不知道拍击手的额外放大；先匹配手的视觉尺寸，再由通用系统投影 Z。
            // HoverOverNPCs 已临时加入 netOffset，这里不再重复加入。
            boundingBox = Utils.CenteredRectangle(NPC.Center, NPC.frame.Size() * NPC.scale * GetMechanic2VisualScale(head));
        }
    }

    public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
    {
        if (!NPC.TryGetMaster(NPCID.SkeletronHead, out NPC master)
            || SkeletronLegendHead.GetInstance(master) is not { Mechanic2InBackground: true } head)
            return null;
        float halfHeight = NPC.frame.Height * NPC.scale * GetMechanic2VisualScale(head) * 0.5f;
        position = NPC.Center + new Vector2(0f, hbPosition == 1 ? halfHeight + 10f : -halfHeight - 24f);
        return true;
    }

    public override bool CheckDead()
    {
        int increasedLife = NPC.lifeMax - NPC.life;
        NPC.life = NPC.lifeMax;
        if (increasedLife > 0)
            NPC.HealEffect(increasedLife, true);
        NPC.active = true;
        NPC.netUpdate = true;
        return false;
    }

    private float AttackFlashIntensity
    {
        get
        {
            if (!NPC.TryGetMaster(NPCID.SkeletronHead, out NPC master))
                return 0f;
            SkeletronLegendHead head = SkeletronLegendHead.GetInstance(master);
            if (head.Mechanic2Active)
            {
                int strike = SkeletronLegendHead.Mechanic2Data.GetStrikeIndex(head.Mechanic2Age);
                int remaining = SkeletronLegendHead.Mechanic2Data.GetStrikeTime(strike) - head.Mechanic2Age;
                return head.Mechanic2HandPattern != 0 && -Math.Sign(NPC.ai[0]) == head.Mechanic2StrikeSide(strike)
                    && remaining > 0 && remaining < 45 ? MathF.Sin(remaining * MathHelper.Pi / 45f) : 0f;
            }
            int timer = head.Timer1 % AttackCycleDuration;
            if (!head.Phase2 || head.Mechanic1Rotating || timer < WindupStart || timer >= StrikeTime)
                return 0f;
            return MathF.Pow(MathF.Sin((timer - WindupStart) / (float)(StrikeTime - WindupStart) * MathHelper.Pi), 2f);
        }
    }

    public override Color? GetAlpha(Color drawColor)
    {
        float flash = AttackFlashIntensity;
        return flash > 0f ? Color.Lerp(drawColor, Color.White, flash) : null;
    }

    public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        float flash = AttackFlashIntensity;
        if (flash <= 0f)
            return;

        // 全亮环境下仍可见的叠加预警，不再依赖实体取色变亮。
        Vector2 position = NPC.Center + NPC.netOffset - screenPos;
        Texture2D bloom = BloomParticle.BloomCircleLarge;
        Color gold = (LegendaryNightHandler.StarColor with { A = 0 }) * (flash * NPC.Opacity);
        Color white = (Color.White with { A = 0 }) * (flash * NPC.Opacity);
        float size = NPC.scale * MathHelper.Lerp(90f, 170f, flash);
        spriteBatch.DrawFromCenter(bloom, position, null, gold * 0.65f, 0f, size / bloom.Width);

        Texture2D rays = TextureAssets.Extra[ExtrasID.SharpTears].Value;
        Vector2 rayScale = new(14f * NPC.scale / rays.Width, size / rays.Height);
        spriteBatch.Draw(rays, position, null, white * 0.9f, 0f, rays.Size() * 0.5f, rayScale, SpriteEffects.None, 0f);
        spriteBatch.Draw(rays, position, null, white * 0.9f, MathHelper.PiOver2, rays.Size() * 0.5f, rayScale, SpriteEffects.None, 0f);
    }
}
