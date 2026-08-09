using Terraria.Enums;

namespace CalamityAnomalies.Anomaly.Bosses.EyeofCthulhu;

public sealed class ServantofCthulhu : AnomalyNPCBehavior<ServantofCthulhu>
{
    public override int ApplyingType => NPCID.ServantofCthulhu;

    public override bool AllowCalamityLogic(CalamityLogicType_NPCBehavior type) => type switch
    {
        CalamityLogicType_NPCBehavior.VanillaOverrideAI => false,
        _ => true,
    };

    public override bool PreAI()
    {
        NPC npc = NPC;

        // ---- 1. 目标选择 ----

        if (npc.target < 0 || npc.target <= Main.maxPlayers || Main.player[npc.target].dead)
            npc.TargetClosest();

        // ---- 2. 获取目标数据 ----

        NPCAimedTarget targetData = npc.GetTargetData();
        bool targetDead = false;
        if (targetData.Type == NPCTargetType.Player)
            targetDead = Main.player[npc.target].dead;

        // ---- 3. 设置 速度参数 ----

        float maxVelocity = 5f + npc.ai[2] * 2f;
        float acceleration = 0.03f + npc.ai[2] * 0.03f;
        maxVelocity *= 1.25f;
        acceleration *= 1.25f;
        maxVelocity *= 1.25f;
        acceleration *= 1.25f;

        // ---- 4. 进入移动逻辑 ----

        Vector2 vector = npc.Center;
        float targetXDist = Main.player[npc.target].Center.X;
        float targetYDist = Main.player[npc.target].Center.Y;
        targetXDist = (int)(targetXDist / 8f) * 8;
        targetYDist = (int)(targetYDist / 8f) * 8;
        vector.X = (int)(vector.X / 8f) * 8;
        vector.Y = (int)(vector.Y / 8f) * 8;
        targetXDist -= vector.X;
        targetYDist -= vector.Y;
        float targetDistance = (float)Math.Sqrt(targetXDist * targetXDist + targetYDist * targetYDist);

        // 处理目标死亡的情况
        if (targetDead)
        {
            targetXDist = npc.direction * maxVelocity / 2f;
            targetYDist = -maxVelocity / 2f;
        }
        else if (targetDistance == 0f)
        {
            targetXDist = npc.velocity.X;
            targetYDist = npc.velocity.Y;
        }
        else
        {
            targetDistance = maxVelocity / targetDistance;
            targetXDist *= targetDistance;
            targetYDist *= targetDistance;
        }

        // ---- 5. 同类型推挤（防止堆叠） ----
        float pushVelocity = 0.5f + npc.ai[2] * 0.25f;
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            if (Main.npc[i].active && i != npc.whoAmI && Main.npc[i].type == npc.type)
            {
                if (Vector2.Distance(npc.Center, Main.npc[i].Center) < 48f * npc.scale)
                {
                    if (npc.position.X < Main.npc[i].position.X)
                        npc.velocity.X -= pushVelocity;
                    else
                        npc.velocity.X += pushVelocity;

                    if (npc.position.Y < Main.npc[i].position.Y)
                        npc.velocity.Y -= pushVelocity;
                    else
                        npc.velocity.Y += pushVelocity;
                }
            }
        }

        // ---- 6. 通用加减速度移动 ----
        if (npc.velocity.X < targetXDist)
        {
            npc.velocity.X += acceleration;
        }
        else if (npc.velocity.X > targetXDist)
        {
            npc.velocity.X -= acceleration;
        }

        if (npc.velocity.Y < targetYDist)
        {
            npc.velocity.Y += acceleration;
        }
        else if (npc.velocity.Y > targetYDist)
        {
            npc.velocity.Y -= acceleration;
        }

        // ---- 7. 旋转设置 ----
        npc.rotation = (float)Math.Atan2(npc.velocity.Y, npc.velocity.X) - MathHelper.PiOver2;

        // ---- 8. 灰尘生成 ----
        if (Main.rand.NextBool(20))
        {
            int dustType = 18; // 默认烟尘
            if (npc.type == NPCID.Crimera) // 不匹配
                dustType = 5;
            int idleDust = Dust.NewDust(new Vector2(npc.position.X, npc.position.Y + npc.height * 0.25f), npc.width, (int)(npc.height * 0.5f), dustType, npc.velocity.X, 2f, 75, npc.color, npc.scale);
            Dust dust = Main.dust[idleDust];
            dust.velocity.X *= 0.5f;
            dust.velocity.Y *= 0.1f;
        }

        // 第二个独立的灰尘生成（不是 Parrot 且随机）
        if (npc.type != NPCID.Parrot && Main.rand.NextBool(40))
        {
            int otherIdleDust = Dust.NewDust(new Vector2(npc.position.X, npc.position.Y + npc.height * 0.25f), npc.width, (int)(npc.height * 0.5f), DustID.Blood, npc.velocity.X, 2f, 0, default, 1f);
            Dust dust = Main.dust[otherIdleDust];
            dust.velocity.X *= 0.5f;
            dust.velocity.Y *= 0.1f;
        }

        // ---- 9. 死亡强制下降（通用，且 Servant 不在排除列表中） ----
        if (Main.player[npc.target].dead)
        {
            npc.velocity.Y -= acceleration * 2f;
            if (npc.timeLeft > 10)
                npc.timeLeft = 10;
        }

        // ---- 10. 网络同步（通用） ----
        if (((npc.velocity.X > 0f && npc.oldVelocity.X < 0f)
            || (npc.velocity.X < 0f && npc.oldVelocity.X > 0f)
            || (npc.velocity.Y > 0f && npc.oldVelocity.Y < 0f)
            || (npc.velocity.Y < 0f && npc.oldVelocity.Y > 0f))
            && !npc.justHit)
        {
            npc.netUpdate = true;
        }

        return false;
    }
}