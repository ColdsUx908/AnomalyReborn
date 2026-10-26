namespace Anomalies.Bosses.EaterofWorlds;

public sealed partial class EaterofWorldsHead
{
    internal float CoilMaskStrength
    {
        get
        {
            // The first hunt starts immediately; its lingering birth cloud is visual cover, not an intro state.
            if (CurrentBehavior == Behavior.Phase1_Hunt && CycleIndex == 0)
                return MathHelper.SmoothStep(0f, 1f, Math.Clamp(Timer1 / 18f, 0f, 1f))
                    * MathHelper.SmoothStep(0f, 1f, Math.Clamp((HuntDuration - Timer1) / 30f, 0f, 1f));
            if (CurrentBehavior is not (Behavior.Phase1_Split or Behavior.Phase1_Combine))
                return 0f;
            float progress = CurrentLocalPhase switch
            {
                0 => Timer1 / 24f,
                1 or 2 => 1f,
                3 => ((CurrentBehavior == Behavior.Phase1_Combine ? 42 : 30 + 2 * SplitLaunchDelay) + 30 - Timer1) / 24f,
                _ => 0f
            };
            return MathHelper.SmoothStep(0f, 1f, Math.Clamp(progress, 0f, 1f));
        }
    }

    private void SpawnCoilDust()
    {
        if (Main.dedServ || CoilMaskStrength <= 0f)
            return;
        for (int i = 0; i < 5; i++)
        {
            if (Main.rand.NextFloat() > CoilMaskStrength)
                continue;
            Vector2 radial = Main.rand.NextVector2Unit() * Main.rand.NextFloat(100f, 190f);
            Dust.NewDustPerfectAction(CoilingCenter + radial, Main.rand.NextBool(5) ? DustID.CorruptGibs : DustID.Smoke, dust =>
            {
                dust.velocity = radial.RotatedBy(MathHelper.PiOver2) * 0.012f - radial * 0.006f;
                dust.noGravity = true;
                dust.scale = 2.2f * CoilMaskStrength;
                dust.alpha = 80 + (int)(150f * (1f - CoilMaskStrength));
                dust.color = GetCoilSmokeColor();
            });
        }
    }

    internal void DrawCoilMask(SpriteBatch spriteBatch, Vector2 screenPos)
    {
        float opacity = CoilMaskStrength;
        Texture2D cloud = EaterofWorldsWarningSystem.CoilCloud;
        if (opacity <= 0f || cloud is null)
            return;
        Color smoke = GetCoilSmokeColor();
        float time = Main.GlobalTimeWrappedHourly;
        // Two irregular, feathered smoke layers replace the concentric disc and repeated petal pattern.
        spriteBatch.Draw(cloud, CoilingCenter - screenPos, null, smoke * (opacity * 0.97f),
            time * 0.12f, cloud.Size() * 0.5f, 460f / cloud.Width, SpriteEffects.None, 0f);
        spriteBatch.Draw(cloud, CoilingCenter - screenPos, null,
            Color.Lerp(smoke, new Color(81, 66, 92), 0.22f) * (opacity * 0.97f),
            -time * 0.09f + 1.7f, cloud.Size() * 0.5f, 430f / cloud.Width, SpriteEffects.FlipHorizontally, 0f);
    }

    private Color GetCoilSmokeColor()
    {
        Color ambient = Lighting.GetColor((int)(CoilingCenter.X / 16f), (int)(CoilingCenter.Y / 16f));
        return Color.Lerp(new Color(76, 71, 82), ambient, 0.38f);
    }

    internal void DrawAttackWarnings(SpriteBatch spriteBatch, Vector2 screenPos)
    {
        if (!IsFirstSegment || CurrentPhase == Phase.Initialize)
            return;
        Color color = new(128, 78, 159);
        switch (CurrentBehavior)
        {
            case Behavior.Phase1_Hunt when Timer1 % 110 >= 50 && Timer1 % 110 < 86:
                Warning(NPC.Center, WarningDirection(NPC), Timer1 % 110 - 50, 36, 280f);
                break;
            case Behavior.Phase1_BurrowCharge when CurrentLocalPhase == 1:
                Warning(NPC.Center, WarningDirection(NPC), Timer1, ChargeWarningTime);
                break;
            case Behavior.Phase1_TripleHunt when CurrentLocalPhase == 1:
                List<NPC> hunters = GetEncounterHeads();
                if (hunters.Count > 0)
                {
                    NPC hunter = hunters[AttackIndex % hunters.Count];
                    Warning(hunter.Center, WarningDirection(hunter), Timer1, TripleWarningTime);
                    Ring(hunter.Center, 42f, color * Envelope(Timer1, TripleWarningTime));
                    if (Timer1 < 30 && hunters.Count == 3)
                        Ring(hunters[(AttackIndex + 1) % 3].Center, 28f, color * Envelope(Timer1, 30));
                }
                break;
            case Behavior.Phase1_CrossSweep when CurrentLocalPhase is 1 or 2:
                List<NPC> sweepers = GetEncounterHeads();
                if (sweepers.Count != 3)
                    break;
                if (CurrentLocalPhase == 1)
                    Warning(sweepers[0].Center, WarningDirection(sweepers[0], new Vector2(0f, -SweepHalfGap)), Timer1, SweepWarningTime);
                Warning(sweepers[1].Center, WarningDirection(sweepers[1], new Vector2(0f, SweepHalfGap)),
                    CurrentLocalPhase == 1 ? Timer1 : SweepWarningTime + Timer1, SweepWarningTime + 24);
                break;
            case Behavior.Phase1_Split:
            case Behavior.Phase1_Combine:
                bool combine = CurrentBehavior == Behavior.Phase1_Combine;
                if (CurrentLocalPhase < 3)
                {
                    if (CurrentLocalPhase == 2)
                    {
                        for (int i = 0; i < (combine ? 1 : 3); i++)
                            Warning(CoilingCenter, CoilLaunchDirection(i, combine), Timer1, CoilDuration,
                                warningColor: color);
                    }
                }
                else if (CurrentLocalPhase == 3)
                {
                    List<NPC> heads = GetEncounterHeads();
                    for (int i = 0; i < heads.Count; i++)
                    {
                        if (Timer1 <= (combine ? 42 : 30 + i * SplitLaunchDelay))
                        {
                            float emergence = Smooth((heads[i].Distance(CoilingCenter) - 180f) / 100f);
                            Vector2 origin = Vector2.Lerp(CoilingCenter, heads[i].Center, emergence);
                            Warning(origin, CoilLaunchDirection(i, combine),
                                Timer1, combine ? 42 : 30 + i * SplitLaunchDelay, warningColor: color);
                        }
                    }
                }
                break;
        }

        // Same layered beam as SlimeMount; keep the shared renderer independent of Boss modules.
        // Ease both ends of the warning window and grow the beam without a one-frame length jump.
        void Warning(Vector2 start, Vector2 direction, float elapsed, float duration, float maxLength = 520f, Color? warningColor = null)
        {
            if (direction.LengthSquared() < 0.001f)
                return;
            float growth = Smooth(elapsed / 16f);
            Color tint = warningColor ?? color;
            AnomalyDrawUtils.DrawLayeredWarning(spriteBatch, start, direction.ToRotation(),
                maxLength * growth, MathHelper.Lerp(20f, 60f, growth), tint,
                Color.Lerp(tint, new Color(198, 163, 215), 0.65f), Envelope(elapsed, duration) * 0.85f,
                startFadeLength: 120f, coreWidth: 5f);
        }

        static float Smooth(float value) => MathHelper.SmoothStep(0f, 1f, Math.Clamp(value, 0f, 1f));

        static float Envelope(float elapsed, float duration) => Smooth(elapsed / 10f) * Smooth((duration - elapsed) / 8f);

        void Line(Vector2 start, Vector2 end, Color tint)
        {
            Vector2 delta = end - start;
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, start - screenPos, new Rectangle(0, 0, 1, 1), tint,
                delta.ToRotation(), new Vector2(0f, 0.5f), new Vector2(delta.Length(), 2f), SpriteEffects.None, 0f);
        }

        void Ring(Vector2 center, float radius, Color tint)
        {
            for (int i = 0; i < 48; i++)
                Line(center + new Vector2(radius, 0f).RotatedBy(i * MathHelper.TwoPi / 48f),
                    center + new Vector2(radius, 0f).RotatedBy((i + 1) * MathHelper.TwoPi / 48f), tint);
        }
    }
}
