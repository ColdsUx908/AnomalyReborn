using CalamityMod.Projectiles.Boss;

namespace Anomalies.Bosses.EaterofWorlds;

public sealed partial class EaterofWorldsHead : AnomalyNPCBehavior<EaterofWorldsHead>
{
    public static event Action<EaterofWorldsHead> OnRunningPreAI;

    public override bool PreAI()
    {
        switch (CurrentPhase)
        {
            case Phase.Initialize:
                Initialize();
                break;
            case Phase.Phase1 or Phase.Phase1_2:
                Phase1AI();
                break;
        }

        OnRunningPreAI?.Invoke(this);

        return false;

        #region 行为函数
        void Initialize()
        {
            //生成体节

            int bodyCount = Ultra ? 88 : 58;
            int randomXLimit = 80;
            int randomYLimit = 80;
            Vector2 additionalWormSpawnLocation = NPC.Center;

            NPC previous = NPC;

            for (int i = 0; i < bodyCount; i++)
            {
                additionalWormSpawnLocation += new Vector2(Main.rand.NextFloat(randomXLimit, randomXLimit * 2f) * Main.rand.NextDirectionInt(), Main.rand.NextFloat(randomYLimit, randomYLimit * 2f));
                NPC.NewNPCAction(SourceAI, additionalWormSpawnLocation, NPCID.EaterofWorldsBody, action: n =>
                {
                    EaterofWorldsBody behavior = EaterofWorldsBody.GetInstance(n);
                    behavior.Head = NPC;
                    behavior.Previous = previous;
                    behavior.BodyIndex = i;
                    previous = n;
                });
            }

            additionalWormSpawnLocation += new Vector2(Main.rand.NextFloat(randomXLimit, randomXLimit * 2f) * Main.rand.NextDirectionInt(), Main.rand.NextFloat(randomYLimit, randomYLimit * 2f));
            NPC.NewNPCAction(SourceAI, additionalWormSpawnLocation, NPCID.EaterofWorldsTail, action: n =>
            {
                EaterofWorldsTail behavior = EaterofWorldsTail.GetInstance(n);
                behavior.Head = NPC;
                behavior.Previous = previous;
            });

            CurrentPhase = Phase.Phase1;
            CurrentBehavior = Behavior.Phase1_Normal;
        }

        void NormalHeadMovement()
        {
            // ---- 计算是否与物块碰撞（飞行/掘地判定） ----
            int tilePositionX = (int)(NPC.position.X / 16f) - 1;
            int tileWidthPosX = (int)((NPC.position.X + NPC.width) / 16f) + 2;
            int tilePositionY = (int)(NPC.position.Y / 16f) - 1;
            int tileWidthPosY = (int)((NPC.position.Y + NPC.height) / 16f) + 2;
            if (tilePositionX < 0) tilePositionX = 0;
            if (tileWidthPosX > Main.maxTilesX) tileWidthPosX = Main.maxTilesX;
            if (tilePositionY < 0) tilePositionY = 0;
            if (tileWidthPosY > Main.maxTilesY) tileWidthPosY = Main.maxTilesY;

            for (int i = tilePositionX; i < tileWidthPosX; i++)
            {
                for (int j = tilePositionY; j < tileWidthPosY; j++)
                {
                    Tile tile = Main.tile[i, j];
                    if (tile != null &&
                        ((tile.HasUnactuatedTile && (Main.tileSolid[tile.TileType] || (Main.tileSolidTop[tile.TileType] && tile.TileFrameY == 0)))
                        || tile.LiquidAmount > 64))
                    {
                        Vector2 vector = new Vector2(i * 16, j * 16);
                        if (NPC.position.X + NPC.width > vector.X && NPC.position.X < vector.X + 16f &&
                            NPC.position.Y + NPC.height > vector.Y && NPC.position.Y < vector.Y + 16f)
                        {
                            if (Main.rand.NextBool(100) && tile.HasUnactuatedTile)
                                WorldGen.KillTile(i, j, true, true, false);
                        }
                    }
                }
            }

            // ---- 速度与加速度参数 ----
            float velocityScale = 4.8f;
            float velocityBoost = velocityScale * (1f - (float)NPC.LifeRatio);
            float accelerationScale = 0.06f;
            float accelerationBoost = accelerationScale * (1f - (float)NPC.LifeRatio);
            float segmentVelocity = 14.4f + velocityBoost;
            float segmentAcceleration = 0.27f + accelerationBoost;

            segmentVelocity += NPC.justHit ? 8f : 2f;
            segmentAcceleration += NPC.justHit ? 0.16f : 0.04f;
            if (Main.getGoodWorld)
            {
                segmentVelocity += 4f;
                segmentAcceleration += 0.05f;
            }

            // ---- 头部专属移动逻辑 ----
            // 新生成头部的初始速度（防止瞬间突进）
            /*
            if (calamityGlobalNPC.newAI[2] < 3f)
            {
                calamityGlobalNPC.newAI[2] += 1f;
                if (NPC.Distance(Target.Center) > segmentVelocity * 20f)
                    NPC.velocity = (Target.Center - NPC.Center).SafeNormalize(Vector2.UnitY)
                                   * (segmentVelocity * 0.75f);
            }
            */

            // 目标位置（按格对齐）
            Vector2 dest = Target.Center;
            float targetPosX = (int)(dest.X / 16f) * 16 - (int)(NPC.Center.X / 16f) * 16;
            float targetPosY = (int)(dest.Y / 16f) * 16 - (int)(NPC.Center.Y / 16f) * 16;
            float targetDistance = (float)Math.Sqrt(targetPosX * targetPosX + targetPosY * targetPosY);

            // 追踪玩家
            // 挖掘音效
            if (NPC.soundDelay == 0)
            {
                float delay = Math.Clamp(targetDistance / 40f, 10f, 20f);
                NPC.soundDelay = (int)delay;
                SoundEngine.PlaySound(SoundID.WormDig, NPC.Center);
            }

            // 重新计算目标向量（速度方向）
            targetDistance = (float)Math.Sqrt(targetPosX * targetPosX + targetPosY * targetPosY);
            float absX = Math.Abs(targetPosX), absY = Math.Abs(targetPosY);
            float timeFactor = segmentVelocity / targetDistance;
            targetPosX *= timeFactor;
            targetPosY *= timeFactor;

            // 如果玩家不在腐化/血腥地且全体死亡 → 消失（保留原逻辑）
            bool shouldDespawn = Target.dead;
            if (shouldDespawn)
            {
                bool allDead = true;
                foreach (Player p in Main.ActivePlayers)
                    if (!p.dead && p.ZoneCorrupt) { allDead = false; break; }
                if (allDead && Main.netMode != NetmodeID.MultiplayerClient && (NPC.position.Y / 16f) > (Main.rockLayer + Main.maxTilesY) / 2.0)
                {
                    // 整条蠕虫消失
                    NPC.active = false;
                    int seg = (int)NPC.ai[0];
                    while (seg > 0 && seg < Main.maxNPCs && Main.npc[seg].active && Main.npc[seg].aiStyle == NPC.aiStyle)
                    {
                        int next = (int)Main.npc[seg].ai[0];
                        Main.npc[seg].active = false;
                        NPC.life = 0;
                        if (Main.dedServ) NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, seg);
                        seg = next;
                    }
                    if (Main.dedServ) NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, NPC.whoAmI);
                    targetPosX = 0f;
                    targetPosY = segmentVelocity;
                }
            }

            // 加速/减速到目标速度
            if ((NPC.velocity.X > 0f && targetPosX > 0f) || (NPC.velocity.X < 0f && targetPosX < 0f) ||
                (NPC.velocity.Y > 0f && targetPosY > 0f) || (NPC.velocity.Y < 0f && targetPosY < 0f))
            {
                if (NPC.velocity.X < targetPosX) NPC.velocity.X += segmentAcceleration;
                else if (NPC.velocity.X > targetPosX) NPC.velocity.X -= segmentAcceleration;
                if (NPC.velocity.Y < targetPosY) NPC.velocity.Y += segmentAcceleration;
                else if (NPC.velocity.Y > targetPosY) NPC.velocity.Y -= segmentAcceleration;

                if (Math.Abs(targetPosY) < segmentVelocity * 0.2 &&
                    ((NPC.velocity.X > 0f && targetPosX < 0f) || (NPC.velocity.X < 0f && targetPosX > 0f)))
                {
                    NPC.velocity.Y += (NPC.velocity.Y > 0f ? segmentAcceleration : -segmentAcceleration) * 2f;
                }
                if (Math.Abs(targetPosX) < segmentVelocity * 0.2 &&
                    ((NPC.velocity.Y > 0f && targetPosY < 0f) || (NPC.velocity.Y < 0f && targetPosY > 0f)))
                {
                    NPC.velocity.X += (NPC.velocity.X > 0f ? segmentAcceleration : -segmentAcceleration) * 2f;
                }
            }
            else if (absX > absY)
            {
                if (NPC.velocity.X < targetPosX) NPC.velocity.X += segmentAcceleration * 1.1f;
                else if (NPC.velocity.X > targetPosX) NPC.velocity.X -= segmentAcceleration * 1.1f;
                if ((Math.Abs(NPC.velocity.X) + Math.Abs(NPC.velocity.Y)) < segmentVelocity * 0.5)
                    NPC.velocity.Y += NPC.velocity.Y > 0f ? segmentAcceleration : -segmentAcceleration;
            }
            else
            {
                if (NPC.velocity.Y < targetPosY) NPC.velocity.Y += segmentAcceleration * 1.1f;
                else if (NPC.velocity.Y > targetPosY) NPC.velocity.Y -= segmentAcceleration * 1.1f;
                if ((Math.Abs(NPC.velocity.X) + Math.Abs(NPC.velocity.Y)) < segmentVelocity * 0.5)
                    NPC.velocity.X += NPC.velocity.X > 0f ? segmentAcceleration : -segmentAcceleration;
            }

            int headCount = NPC.CountNPCS(NPC.type);
            if (headCount > 0)
            {
                headCount = Math.Min(headCount - 1, 7);
                float pushDist = MathHelper.Lerp(14f - headCount, 140f - headCount * 10f, 1f - (float)NPC.LifeRatio) * NPC.scale;
                const float pushSpeed = 0.25f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC other = Main.npc[i];
                    if (other.active && i != NPC.whoAmI && other.type == NPC.type)
                    {
                        if (Vector2.Distance(NPC.Center, other.Center) < pushDist)
                        {
                            if (NPC.position.X < other.position.X) NPC.velocity.X -= pushSpeed;
                            else NPC.velocity.X += pushSpeed;
                            if (NPC.position.Y < other.position.Y) NPC.velocity.Y -= pushSpeed;
                            else NPC.velocity.Y += pushSpeed;
                        }
                    }
                }
            }

            // 更新旋转角度（朝向速度方向）
            NPC.VelocityToRotation(MathHelper.PiOver2);

            // 触发网络同步（若状态变化）
            if (((NPC.velocity.X > 0f && NPC.oldVelocity.X < 0f) || (NPC.velocity.X < 0f && NPC.oldVelocity.X > 0f)
                || (NPC.velocity.Y > 0f && NPC.oldVelocity.Y < 0f) || (NPC.velocity.Y < 0f && NPC.oldVelocity.Y > 0f))
                && !NPC.justHit)
            {
                NPC.netUpdate = true;
            }
        }

        void Phase1AI()
        {
            switch (CurrentBehavior)
            {
                case Behavior.Phase1_Normal:
                    Normal();
                    break;
            }
        }

        void Normal()
        {
            NormalHeadMovement();

            //诅咒焰

            Timer1++;

            if (Timer1 >= 60
                && TOSharedData.NotClient
                && Collision.CanHitLine(NPC.Center, 1, 1, Target.Center, 1, 1)
                && Vector2.IncludedAngle(NPC.velocity, Target.Center - NPC.Center) <= TOMathUtils.PiOver3) //发射
            {
                Timer1 = 0;

                Vector2 velocity = NPC.GetVelocityTowards(Target.Center, Ultra ? 20f : 15f) + NPC.velocity * 0.25f;
                int type = ProjectileID.CursedFlameHostile;
                Projectile.NewProjectileAction(SourceAI, NPC.Center + NPC.velocity, velocity, type, CursedFireballDamage, 0f);
            }

            if (Timer1 >= 40) //预警尘埃
            {
                Vector2 dustCenter = NPC.Center + Main.rand.NextPolarVector2(10f, 30f);
                int dustType = DustID.CursedTorch;
                Dust.NewDustPerfectAction(dustCenter, dustType, d =>
                {
                    d.scale = 2.5f;
                    d.noGravity = true;
                });
            }
        }
        #endregion 行为函数
    }
}
