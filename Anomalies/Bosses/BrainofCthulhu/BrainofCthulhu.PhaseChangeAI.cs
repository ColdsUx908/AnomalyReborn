namespace Anomalies.Bosses.BrainofCthulhu;

public sealed partial class BrainofCthulhu
{
    private void PhaseChangeAI()
    {
        NPC.dontTakeDamage = true;
        NPC.rotation *= 0.9f;
        TeleportTime = 0;
        NPC.damage = 0;
        // 90 frames to settle, then 120 frames to open: 3.5 seconds total.
        // The bar's 28-frame locking animation leaves about 3 seconds fully locked.
        const int settleDuration = 90;
        float animCounter = Time - settleDuration;
        if (animCounter >= 0)
        {
            if (animCounter == 0)
                NPC.velocity = Vector2.UnitY * 2f;
            else if (animCounter < 60)
                NPC.velocity *= 0.99f;
            else if (animCounter == 60)
                NPC.velocity = Vector2.UnitY * -8f;
            else if (animCounter == 65)
            {
                CurrentBehavior = Behavior.Phase2TransitionOpen;
                PreviousAttack = Behavior.Phase1Idle;
                AvailableAttacks.Clear();
                NPC.netUpdate = true;
                SoundEngine.PlaySound(SoundID.NPCHit1, NPC.Center);
                if (!Main.dedServ)
                {
                    //Spawns all of BoC's Phase Transition Gores (GoreIDs 392 -> 395)
                    for (int i = 392; i <= 395; i++)
                        Gore.NewGoreAction(NPC.GetSource_FromAI(), NPC.position, Main.rand.NextVector2Circular(6f, 6f), i, action: gore => gore.position += gore.AABBRectangle.Size() / 2f);
                }

                for (int j = 0; j < 20; j++)
                {
                    Vector2 edgeBloodDir = Main.rand.NextVector2CircularEdge(1, 1);
                    BloodParticle b = new(NPC.Center - (Vector2.UnitY * 32) + (edgeBloodDir * new Vector2(16, 24)), edgeBloodDir.RotatedBy(Main.rand.NextFloat(-MathHelper.Pi / 10f, MathHelper.Pi / 10f)) * Main.rand.NextFloat(4f, 8f), 24, 0.75f, Color.Red);
                    ParticleHandler.SpawnParticle(b);
                    Dust.NewDustPerfectAction(Main.rand.NextVector2FromRectangle(NPC.Hitbox), DustID.Blood, action: dust => dust.velocity = Main.rand.NextVector2Circular(6f, 6f));
                }

                for (int i = 1; i <= 3; i++)
                {
                    Color color = i switch
                    {
                        1 => Color.Yellow,
                        2 => Color.Orange,
                        _ => Color.Red,
                    };
                    PulseRing ring = new(NPC.Center, NPC.velocity * 0.5f, color, 0f, 1f + i * 0.5f, 24);
                    ParticleHandler.SpawnParticle(ring);
                }

                BoCAfterImages = [];
                SoundEngine.PlaySound(_ShortRoarSound, NPC.Center);
            }
            else
                NPC.velocity *= 0.9f;
            if (animCounter < 60f)
                BoCDrawOffset = Main.rand.NextVector2CircularEdge(1, 1) * MathHelper.Lerp(0f, 16f, TOMathUtils.Interpolation.CircularEaseIn(animCounter / 60f, clamped: false));
            else if (animCounter < 70f)
                BoCDrawOffset = Main.rand.NextVector2CircularEdge(1, 1) * MathHelper.Lerp(16f, 0f, TOMathUtils.Interpolation.CircularEaseOut((animCounter - 60) / 10f, clamped: false));
            if (animCounter >= 120f)
            {
                Time = 0;
                ResetAttackValues();
                CurrentBehavior = Behavior.Phase2Idle;
                NPC.dontTakeDamage = false;
                NPC.damage = NPC.defDamage;
            }
        }
        else
        {
            NPC.velocity *= 0.8f;
            if (Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
            {
                NPC.velocity = NPC.DirectionTo(Target.Center) * 4f;
            }
            else if (Collision.SolidCollision(NPC.position + NPC.velocity, NPC.width, NPC.height))
            {
                if (NPC.velocity.X != NPC.oldVelocity.X)
                    NPC.velocity.X = -NPC.oldVelocity.X;
                if (NPC.velocity.Y != NPC.oldVelocity.Y)
                    NPC.velocity.Y = -NPC.oldVelocity.Y;
                NPC.velocity *= 2f;
                NPC.velocity = NPC.velocity.ClampMagnitude(0f, 8f);
            }
        }
    }
}
