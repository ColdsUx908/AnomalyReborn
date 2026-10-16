using Anomalies.DataStructures;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed class SkeletronLegendHand : AnomalyNPCBehavior<SkeletronLegendHand>
{
    private const int AttackCycleDuration = 300;
    private const int WindupStart = 180;
    private const int StrikeTime = 210;
    private const int MinimumStrikeDuration = 16;
    private const int MaximumStrikeDuration = 30;
    private const float MaximumAttackDistance = 800f;

    // 保留原版手的 ai 布局：ai[0] 左右侧，ai[1] 头部索引，ai[2] 动作，ai[3] 待机计时。
    public NPC Master => NPC.Master;
    public override int ApplyingType => NPCID.SkeletronHand;
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
        CalamityLogicType_NPCBehavior.VanillaOverrideAI or CalamityLogicType_NPCBehavior.PreAI => false,
        _ => true
    };

    public override void SetDefaultsFinal()
    {
        NPC.lifeMax = 3750;
        NPC.defense = 9999;
    }

    public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
    {
        modifiers.SetMaxDamage(1);
        modifiers.ModifyHitInfo += static (ref NPC.HitInfo hit) => hit.Damage = 1;
    }

    public override bool PreAI()
    {
        using BossNetUpdateScope netUpdate = new(NPC);

        if (!NPC.TryGetMaster(NPCID.SkeletronHead, out NPC master) || master.Anomaly.CustomAIType != 1)
        {
            if (TOSharedData.NotClient)
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
        NPC.life = Math.Min(NPC.lifeMax, NPC.life + Math.Max(1, NPC.lifeMax / 60));
        if (!NPC.TargetClosestIfInvalid(true, SkeletronLegendHead.DespawnDistance))
        {
            NPC.velocity *= 0.8f;
            return false;
        }
        VanillaHandAI(head);
        return false;

        // 从原版 NPC.AI 的 aiStyle == 12（type 36）复制，保留 0/3 待机、1/2 下拍、4/5 横扫。
        // 改动：显式 Master、头部共享时序、更近更快的回收，以及更快且距离受限的挥击。
        void VanillaHandAI(SkeletronLegendHead head)
        {
            int timer = head.Phase2 ? head.Timer1 % AttackCycleDuration : 0;
            NPC.ai[3] = timer;
            if (!head.Phase2 || timer >= StrikeTime + MaximumStrikeDuration)
                ChangeAction(0);
            else if (timer == WindupStart)
                ChangeAction(head.Timer1 / AttackCycleDuration % 2 == 0 ? 1 : 4);

            if (NPC.ai[2] is 0f or 3f)
            {
                // 原版的侧下方待机点；缩小偏移并加强回收，避免挥击后远离头部。
                Vector2 destination = Master.Center + new Vector2(-160f * NPC.ai[0], 140f);
                Vector2 offset = destination - NPC.Center;
                Vector2 desiredVelocity = offset.ToCustomLength(Math.Min(28f, offset.Length() * 0.15f));
                NPC.velocity = Vector2.Lerp(NPC.velocity, desiredVelocity, 0.2f);
                NPC.rotation = offset.ToRotation() + MathHelper.PiOver2;
                return;
            }

            if (NPC.ai[2] == 1f)
            {
                Vector2 handCenter = NPC.Center;
                float targetOffsetX = Master.position.X + Master.width / 2 - 200f * NPC.ai[0] - handCenter.X;
                float targetOffsetY = Master.position.Y + 230f - handCenter.Y;
                float targetDistance = Math.Max(0.01f, (float)Math.Sqrt(targetOffsetX * targetOffsetX + targetOffsetY * targetOffsetY));
                NPC.rotation = (float)Math.Atan2(targetOffsetY, targetOffsetX) + 1.57f;
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
                NPC.rotation = (float)Math.Atan2(targetOffsetY, targetOffsetX) + 1.57f;
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
        NPC.TryGetMaster(NPCID.SkeletronHead, out NPC master) && SkeletronLegendHead.GetInstance(master).Phase2;

    public override bool CheckDead()
    {
        NPC.life = NPC.lifeMax;
        NPC.active = true;
        NPC.netUpdate = true;
        return false;
    }

    public override Color? GetAlpha(Color drawColor)
    {
        if (!NPC.TryGetMaster(NPCID.SkeletronHead, out NPC master))
            return null;
        SkeletronLegendHead head = SkeletronLegendHead.GetInstance(master);
        int timer = head.Timer1 % AttackCycleDuration;
        if (!head.Phase2 || timer < WindupStart || timer >= StrikeTime)
            return null;
        float flash = MathF.Pow(MathF.Sin((timer - WindupStart) / (float)(StrikeTime - WindupStart) * MathHelper.Pi), 2f);
        return Color.Lerp(drawColor, LegendaryNightHandler.BoneColor, flash);
    }
}
