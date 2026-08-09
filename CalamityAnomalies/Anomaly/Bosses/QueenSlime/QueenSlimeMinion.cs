// Developed by ColdsUx

using CalamityAnomalies.DataStructures;

namespace CalamityAnomalies.Anomaly.Bosses.QueenSlime;

public sealed class QueenSlimeMinionBlue : AnomalyNPCBehavior<QueenSlimeMinionBlue>
{
    private static readonly ProjectileDamageContainer _SpikeDamage = new(30, 64, 90, 120, 90, 120);
    public static int SpikeDamage => _SpikeDamage.Value;

    public override int ApplyingType => NPCID.QueenSlimeMinionBlue;

    public override bool AllowCalamityLogic(CalamityLogicType_NPCBehavior type) => type switch
    {
        CalamityLogicType_NPCBehavior.GetAlpha => false,
        _ => true,
    };

    public override bool PreAI()
    {
        bool death = true;

        if (NPC.localAI[0] > 0f)
            NPC.localAI[0] -= 1f;

        if (!NPC.wet && Main.player[NPC.target].active && !Main.player[NPC.target].dead && !Main.player[NPC.target].npcTypeNoAggro[NPC.type])
        {
            Player obj = Main.player[NPC.target];
            Vector2 center = NPC.Center;
            float num19 = obj.Center.X - center.X;
            float num20 = obj.Center.Y - center.Y;
            float num21 = (float)Math.Sqrt(num19 * num19 + num20 * num20);
            int num22 = NPC.CountNPCS(NPCID.QueenSlimeMinionBlue);
            if (num22 < 5 && Math.Abs(num19) < 500f && Math.Abs(num20) < 550f && Collision.CanHit(NPC.position, NPC.width, NPC.height, Main.player[NPC.target].position, Main.player[NPC.target].width, Main.player[NPC.target].height) && NPC.velocity.Y == 0f)
            {
                NPC.ai[0] = -40f;
                if (NPC.velocity.Y == 0f)
                    NPC.velocity.X *= 0.9f;

                if (Main.netMode != NetmodeID.MultiplayerClient && NPC.localAI[0] == 0f)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        Vector2 vector6 = new Vector2(k - 1, -4f);
                        vector6.X *= 1f + Main.rand.Next(-50, 51) * 0.005f;
                        vector6.Y *= 1f + Main.rand.Next(-50, 51) * 0.005f;
                        vector6.Normalize();
                        vector6 *= 6f + Main.rand.Next(-50, 51) * 0.01f;
                        if (num21 > 350f)
                            vector6 *= 2f;
                        else if (num21 > 250f)
                            vector6 *= 1.5f;

                        int type = ProjectileID.QueenSlimeMinionBlueSpike;
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), center, vector6 * (death ? 0.7f : 0.5f), type, SpikeDamage, 0f, Main.myPlayer);
                        NPC.localAI[0] = death ? 50f : 25f;
                        if (num22 > 4)
                            break;
                    }
                }
            }
            else if (Math.Abs(num19) < 500f && Math.Abs(num20) < 550f && Collision.CanHit(NPC.position, NPC.width, NPC.height, Main.player[NPC.target].position, Main.player[NPC.target].width, Main.player[NPC.target].height) && NPC.velocity.Y == 0f)
            {
                float num23 = num21;
                NPC.ai[0] = -40f;
                if (NPC.velocity.Y == 0f)
                    NPC.velocity.X *= 0.9f;

                if (Main.netMode != NetmodeID.MultiplayerClient && NPC.localAI[0] == 0f)
                {
                    num20 = Main.player[NPC.target].position.Y - center.Y - Main.rand.Next(0, 200);
                    num21 = (float)Math.Sqrt(num19 * num19 + num20 * num20);
                    num21 = 4.5f / num21;
                    num21 *= 2f;
                    if (num23 > 350f)
                        num21 *= 2f;
                    else if (num23 > 250f)
                        num21 *= 1.5f;

                    num19 *= num21;
                    num20 *= num21;
                    NPC.localAI[0] = death ? 100f : 50f;
                    int type = ProjectileID.QueenSlimeMinionBlueSpike;
                    Vector2 spikeVelocity = new Vector2(num19, num20) * (death ? 0.7f : 0.5f);
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), center, spikeVelocity, type, SpikeDamage, 0f, Main.myPlayer);
                }
            }
        }

        if (NPC.ai[2] > 1f)
            NPC.ai[2] -= 1f;

        if (NPC.wet)
        {
            if (NPC.collideY)
                NPC.velocity.Y = -2f;

            if (NPC.velocity.Y < 0f && NPC.ai[3] == NPC.position.X)
            {
                NPC.direction *= -1;
                NPC.ai[2] = 200f;
            }

            if (NPC.velocity.Y > 0f)
                NPC.ai[3] = NPC.position.X;

            if (NPC.velocity.Y > 2f)
                NPC.velocity.Y *= 0.9f;

            NPC.velocity.Y -= 0.5f;
            if (NPC.velocity.Y < -4f)
                NPC.velocity.Y = -4f;

            if (NPC.ai[2] == 1f)
                NPC.TargetClosest();
        }

        NPC.aiAction = 0;
        if (NPC.ai[2] == 0f)
        {
            NPC.ai[0] = -100f;
            NPC.ai[2] = 1f;
            NPC.TargetClosest();
        }

        if (NPC.velocity.Y == 0f)
        {
            if (NPC.collideY && NPC.oldVelocity.Y != 0f && Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                NPC.position.X -= NPC.velocity.X + NPC.direction;

            if (NPC.ai[3] == NPC.position.X)
            {
                NPC.direction *= -1;
                NPC.ai[2] = 200f;
            }

            NPC.ai[3] = 0f;
            NPC.velocity.X *= 0.8f;
            if (NPC.velocity.X > -0.1 && NPC.velocity.X < 0.1)
                NPC.velocity.X = 0f;

            NPC.ai[0] += death ? 16f : 10f;

            float num33 = -1000f;

            int num34 = 0;
            if (NPC.ai[0] >= 0f)
                num34 = 1;

            if (NPC.ai[0] >= num33 && NPC.ai[0] <= num33 * 0.5f)
                num34 = 2;

            if (NPC.ai[0] >= num33 * 2f && NPC.ai[0] <= num33 * 1.5f)
                num34 = 3;

            if (num34 > 0)
            {
                NPC.netUpdate = true;
                if (NPC.ai[2] == 1f)
                    NPC.TargetClosest();

                if (num34 == 3)
                {
                    NPC.velocity.Y = -8f;
                    NPC.velocity.X += (death ? 9 : 6) * NPC.direction;
                    NPC.ai[0] = -200f;
                    NPC.ai[3] = NPC.position.X;
                }
                else
                {
                    NPC.velocity.Y = -6f;
                    NPC.velocity.X += (death ? 6 : 4) * NPC.direction;
                    NPC.ai[0] = -120f;
                    if (num34 == 1)
                        NPC.ai[0] += num33;
                    else
                        NPC.ai[0] += num33 * 2f;
                }
            }
            else if (NPC.ai[0] >= -30f)
                NPC.aiAction = 1;
        }
        else if (NPC.target < Main.maxPlayers && ((NPC.direction == 1 && NPC.velocity.X < 3f) || (NPC.direction == -1 && NPC.velocity.X > -3f)))
        {
            if (NPC.collideX && Math.Abs(NPC.velocity.X) == 0.2f)
                NPC.position.X -= 1.4f * NPC.direction;

            if (NPC.collideY && NPC.oldVelocity.Y != 0f && Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                NPC.position.X -= NPC.velocity.X + NPC.direction;

            if ((NPC.direction == -1 && NPC.velocity.X < 0.01) || (NPC.direction == 1 && NPC.velocity.X > -0.01))
                NPC.velocity.X += 0.2f * NPC.direction;
            else
                NPC.velocity.X *= 0.93f;
        }

        return false;
    }

    public override Color? GetAlpha(Color drawColor) => drawColor;
}

public sealed class QueenSlimeMinionPink : AnomalyNPCBehavior<QueenSlimeMinionPink>
{
    private static readonly ProjectileDamageContainer _SmallGelDamage = new(30, 64, 90, 120, 90, 120);
    public static int SmallGelDamage => _SmallGelDamage.Value;

    public override int ApplyingType => NPCID.QueenSlimeMinionPink;

    public override bool AllowCalamityLogic(CalamityLogicType_NPCBehavior type) => type switch
    {
        CalamityLogicType_NPCBehavior.GetAlpha => false,
        _ => true,
    };

    public override bool PreAI()
    {
        bool death = true;

        if (NPC.localAI[0] > 0f)
            NPC.localAI[0] -= 1f;

        if (!NPC.wet && Main.player[NPC.target].active && !Main.player[NPC.target].dead && !Main.player[NPC.target].npcTypeNoAggro[NPC.type])
        {
            Player obj2 = Main.player[NPC.target];
            Vector2 center2 = NPC.Center;
            float num24 = obj2.Center.X - center2.X;
            float num25 = obj2.Center.Y - center2.Y;
            float num26 = (float)Math.Sqrt(num24 * num24 + num25 * num25);
            float num27 = num26;
            if (Math.Abs(num24) < 500f && Math.Abs(num25) < 550f && Collision.CanHit(NPC.position, NPC.width, NPC.height, Main.player[NPC.target].position, Main.player[NPC.target].width, Main.player[NPC.target].height) && NPC.velocity.Y == 0f)
            {
                NPC.ai[0] = -40f;
                if (NPC.velocity.Y == 0f)
                    NPC.velocity.X *= 0.9f;

                if (Main.netMode != NetmodeID.MultiplayerClient && NPC.localAI[0] == 0f)
                {
                    num25 = Main.player[NPC.target].position.Y - center2.Y - Main.rand.Next(0, 200);
                    num26 = (float)Math.Sqrt(num24 * num24 + num25 * num25);
                    num26 = 4.5f / num26;
                    num26 *= 2f;
                    if (num27 > 350f)
                        num26 *= 1.75f;
                    else if (num27 > 250f)
                        num26 *= 1.25f;

                    num24 *= num26;
                    num25 *= num26;
                    NPC.localAI[0] = death ? 60f : 30f;

                    int type = ProjectileID.QueenSlimeMinionPinkBall;
                    Vector2 pinkBallVelocity = new Vector2(num24, num25) * (death ? 0.7f : 0.5f);
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), center2, pinkBallVelocity, type, SmallGelDamage, 0f, Main.myPlayer);
                }
            }
        }

        if (NPC.ai[2] > 1f)
            NPC.ai[2] -= 1f;

        if (NPC.wet)
        {
            if (NPC.collideY)
                NPC.velocity.Y = -2f;

            if (NPC.velocity.Y < 0f && NPC.ai[3] == NPC.position.X)
            {
                NPC.direction *= -1;
                NPC.ai[2] = 200f;
            }

            if (NPC.velocity.Y > 0f)
                NPC.ai[3] = NPC.position.X;

            if (NPC.velocity.Y > 2f)
                NPC.velocity.Y *= 0.9f;

            NPC.velocity.Y -= 0.5f;
            if (NPC.velocity.Y < -4f)
                NPC.velocity.Y = -4f;

            if (NPC.ai[2] == 1f)
                NPC.TargetClosest();
        }

        NPC.aiAction = 0;
        if (NPC.ai[2] == 0f)
        {
            NPC.ai[0] = -100f;
            NPC.ai[2] = 1f;
            NPC.TargetClosest();
        }

        if (NPC.velocity.Y == 0f)
        {
            if (NPC.collideY && NPC.oldVelocity.Y != 0f && Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                NPC.position.X -= NPC.velocity.X + NPC.direction;

            if (NPC.ai[3] == NPC.position.X)
            {
                NPC.direction *= -1;
                NPC.ai[2] = 200f;
            }

            NPC.ai[3] = 0f;
            NPC.velocity.X *= 0.8f;
            if (NPC.velocity.X > -0.1 && NPC.velocity.X < 0.1)
                NPC.velocity.X = 0f;

            NPC.ai[0] += death ? 11f : 7f;

            float num33 = -500f;

            int num34 = 0;
            if (NPC.ai[0] >= 0f)
                num34 = 1;

            if (NPC.ai[0] >= num33 && NPC.ai[0] <= num33 * 0.5f)
                num34 = 2;

            if (NPC.ai[0] >= num33 * 2f && NPC.ai[0] <= num33 * 1.5f)
                num34 = 3;

            if (num34 > 0)
            {
                NPC.netUpdate = true;
                if (NPC.ai[2] == 1f)
                    NPC.TargetClosest();

                if (num34 == 3)
                {
                    NPC.velocity.Y = -8f;
                    NPC.velocity.X += (death ? 9 : 6) * NPC.direction;
                    NPC.ai[0] = -200f;
                    NPC.ai[3] = NPC.position.X;
                }
                else
                {
                    NPC.velocity.Y = -6f;
                    NPC.velocity.X += (death ? 6 : 4) * NPC.direction;
                    NPC.ai[0] = -120f;
                    if (num34 == 1)
                        NPC.ai[0] += num33;
                    else
                        NPC.ai[0] += num33 * 2f;
                }

                NPC.velocity.Y *= 1.6f;
                NPC.velocity.X *= 1.2f;
            }
            else if (NPC.ai[0] >= -30f)
                NPC.aiAction = 1;
        }
        else if (NPC.target < Main.maxPlayers && ((NPC.direction == 1 && NPC.velocity.X < 3f) || (NPC.direction == -1 && NPC.velocity.X > -3f)))
        {
            if (NPC.collideX && Math.Abs(NPC.velocity.X) == 0.2f)
                NPC.position.X -= 1.4f * NPC.direction;

            if (NPC.collideY && NPC.oldVelocity.Y != 0f && Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                NPC.position.X -= NPC.velocity.X + NPC.direction;

            if ((NPC.direction == -1 && NPC.velocity.X < 0.01) || (NPC.direction == 1 && NPC.velocity.X > -0.01))
                NPC.velocity.X += 0.2f * NPC.direction;
            else
                NPC.velocity.X *= 0.93f;
        }

        return false;
    }

    public override Color? GetAlpha(Color drawColor) => drawColor;
}

public sealed class QueenSlimeMinionPurple : AnomalyNPCBehavior<QueenSlimeMinionPurple>
{
    public override int ApplyingType => NPCID.QueenSlimeMinionPurple;

    public override bool AllowCalamityLogic(CalamityLogicType_NPCBehavior type) => type switch
    {
        CalamityLogicType_NPCBehavior.GetAlpha => false,
        _ => true,
    };

    public override Color? GetAlpha(Color drawColor) => drawColor;
}
