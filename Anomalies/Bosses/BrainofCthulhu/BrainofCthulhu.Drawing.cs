namespace Anomalies.Bosses.BrainofCthulhu;

public sealed partial class BrainofCthulhu
{
    public override void FindFrame(int frameHeight)
    {
        if (BoCFrame == Rectangle.Empty)
            BoCFrame = TextureAssets.Npc[NPCID.BrainofCthulhu].Frame(verticalFrames: 8);
        if (NPC.frameCounter == 0)
            BoCFrame.Y += frameHeight;
        if (CurrentBehavior <= Behavior.Phase2TransitionClosed)
        {
            if (BoCFrame.Y > frameHeight * 3)
                BoCFrame.Y = 0;
            return;
        }

        if (BoCFrame.Y < frameHeight * 4)
            BoCFrame.Y = frameHeight * 4;
        if (BoCFrame.Y > frameHeight * 7)
            BoCFrame.Y = frameHeight * 4;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        bool phase1 = CurrentBehavior <= Behavior.Phase2TransitionOpen;
        bool drawBrain = true;
        if (phase1)
        {
            List<NPC> creepers = [..Main.npc.Where(n => n.active && n.type == NPCID.Creeper)];
            creepers.Sort((a, b) => b.DistanceSQ(NPC.Center).CompareTo(a.DistanceSQ(NPC.Center)));
            List<(int creeper, List<VerletSimulatedSegment> tendril, int reelInTimer)> tendrils = [..BrainOfCthulhuSystem.VerletTendrils];
            tendrils.Sort((a, b) =>
            {
                int bIndex = creepers.IndexOf(Main.npc[b.creeper]);
                if (bIndex == -1)
                    bIndex = int.MaxValue;
                int aIndex = creepers.IndexOf(Main.npc[a.creeper]);
                if (aIndex == -1)
                    aIndex = int.MaxValue;
                return bIndex.CompareTo(aIndex);
            });
            foreach ((int creeper, List<VerletSimulatedSegment> tendril, int reelInTimer) v in tendrils)
            {
                List<VerletSimulatedSegment> curvePoints = v.tendril;
                if (curvePoints is null)
                    continue;
                NPC creeper = Main.npc[v.creeper];
                if (creeper == null || !creeper.active || creeper.type != NPCID.Creeper)
                    creeper = null;
                float glowIntensity = creeper == null ? 0 : creeper.GetCreeperBehavior().ConnectionOpacity;
                Color ichorLess = Color.Lerp(Color.Transparent, Color.OrangeRed * 0.333f, glowIntensity);
                Color ichorful = Color.Lerp(Color.OrangeRed * 0.25f, Color.Orange * 0.666f, glowIntensity);
                for (int i = 0; i < curvePoints.Count; i++)
                {
                    Vector2 start = curvePoints[i].position;
                    Vector2 end = i == curvePoints.Count - 1 ? (creeper == null ? curvePoints[i].position : creeper.Center) : curvePoints[i + 1].position;
                    Vector2 center = (end + start) / 2f;
                    start -= Main.screenPosition;
                    end -= Main.screenPosition;
                    float rotation = (end - start).ToRotation() - MathHelper.PiOver2;
                    float ichorRatio = 0;
                    if (creeper != null)
                    {
                        float flowTime = creeper.GetCreeperBehavior().FlowTime;
                        float flowAmt = creeper.GetCreeperBehavior().FlowAmount;
                        ichorRatio = BrainCombatUtils.ExpInEasing((float)Math.Sin((flowTime + (i * flowAmt)) * 2f) / 2f + 0.5f, 1);
                    }

                    Color glowColor = Color.Lerp(ichorLess, ichorful, ichorRatio);
                    float dist = Vector2.Distance(start, end);
                    Vector2 tendrilScale = new(1f + (0.5f * (ichorRatio * (1 + (glowIntensity * 0.5f)))), dist / BrainOfCthulhuSystem.tendril.Height());
                    spriteBatch.Draw(BrainOfCthulhuSystem.tendril.Value, start, null, Lighting.GetColor(center.ToTileCoordinates()) * NPC.Opacity, rotation, BrainOfCthulhuSystem.tendril.Size() * Vector2.UnitX * 0.5f, tendrilScale, SpriteEffects.None, 0f);
                    spriteBatch.Draw(BrainOfCthulhuSystem.GetTendrilGlow(), start, null, glowColor * NPC.Opacity, rotation, BrainOfCthulhuSystem.GetTendrilGlow().Size() * Vector2.UnitX * 0.5f, tendrilScale, SpriteEffects.None, 0f);
                }
            }

            foreach (NPC creeper in creepers)
            {
                float glowIntensity = creeper.GetCreeperBehavior().ConnectionOpacity;
                spriteBatch.Draw(BrainOfCthulhuSystem.GetCreeperGlow(), creeper.Center - Main.screenPosition, null, Color.Orange * glowIntensity, creeper.rotation, TextureAssets.Npc[NPCID.Creeper].Size() * 0.5f, creeper.scale * 1.15f, 0, 0);
                spriteBatch.Draw(TextureAssets.Npc[NPCID.Creeper].Value, creeper.Center - Main.screenPosition, null, Lighting.GetColor(creeper.Center.ToTileCoordinates()).MultiplyRGB(Color.Lerp(Color.White, new Color(255, 180, 180), glowIntensity)), creeper.rotation, TextureAssets.Npc[NPCID.Creeper].Size() * 0.5f, creeper.scale, 0, 0);
            }
        }
        else
        {
            List<NPC> falseBrains = [..Main.npc.Where(n => n.active && n.type == ModContent.NPCType<FalseBrain>())];
            if (falseBrains.Count > 0)
            {
                drawBrain = false;
                falseBrains.Add(NPC);
                falseBrains.Sort((a, b) =>
                {
                    float aValue;
                    float bValue;
                    if (a.ModNPC is FalseBrain falseA)
                        aValue = falseA.DrawPriority;
                    else
                        aValue = a.GetBrainBehavior().CachedRatio;
                    if (b.ModNPC is FalseBrain falseB)
                        bValue = falseB.DrawPriority;
                    else
                        bValue = b.GetBrainBehavior().CachedRatio;
                    return aValue.CompareTo(bValue);
                });
                foreach (NPC n in falseBrains)
                {
                    if (n.ModNPC is FalseBrain falseBrain)
                        falseBrain.DrawSelf(spriteBatch, screenPos, Lighting.GetColor(n.Center.ToTileCoordinates()));
                    else if (CurrentBehavior == Behavior.DeathAnimation)
                        drawBrain = true;
                    else
                        DrawBrainLikeFakes(spriteBatch, n);
                }
            }
        }

        if (drawBrain)
            DrawBrain(spriteBatch, NPC);
        return false;
    }

    private static void DrawBrain(SpriteBatch spriteBatch, NPC brain)
    {
        BrainofCthulhu ai = brain.GetBrainBehavior();
        bool phase1 = ai.CurrentBehavior < Behavior.Phase2TransitionClosed;
        Vector2 drawPos = brain.Center + ai.BoCDrawOffset + (Vector2.UnitY * 16) - Main.screenPosition;
        Vector2 scale = Vector2.One;
        if (!phase1 && ai.TeleportTime != 0)
        {
            ai.BoCAfterImages.RemoveAll(p => p.Dead || p.Timer >= p.Lifetime);
            foreach (BrainOfCthulhuAfterImage p in ai.BoCAfterImages)
                p.Draw(spriteBatch);
            //Color glowColor = Color.White * (teleportCounter / 30f);
            scale = Vector2.Lerp(Vector2.One, new Vector2(0.5f + ((float)Math.Cos(ai.Time / (ai.TeleportDuration / 2f) * MathHelper.TwoPi) / 2f + 0.5f), 0.5f + ((float)Math.Sin(ai.Time / (ai.TeleportDuration / 2f) * MathHelper.TwoPi) / 2f + 0.5f)), BrainCombatUtils.SineInOutEasing(ai.TeleportTime / (ai.TeleportDuration / 2f), 1));
            spriteBatch.Draw(TextureAssets.Npc[NPCID.BrainofCthulhu].Value, drawPos, ai.BoCFrame, Lighting.GetColor(brain.Center.ToTileCoordinates()) * brain.Opacity, brain.rotation, ai.BoCFrame.Size() * 0.5f, scale * brain.scale, 0, 0);
        //spriteBatch.Draw(GetBrainGlow(), drawPos, NPC.frame, glowColor, NPC.rotation, NPC.frame.Size() * 0.5f, scale, 0, 0);
        }
        else
            spriteBatch.Draw(TextureAssets.Npc[NPCID.BrainofCthulhu].Value, drawPos, ai.BoCFrame, Lighting.GetColor(brain.Center.ToTileCoordinates()) * brain.Opacity, brain.rotation, ai.BoCFrame.Size() * 0.5f, scale * brain.scale, 0, 0);
    }

    private static void DrawBrainLikeFakes(SpriteBatch spriteBatch, NPC brain)
    {
        BrainofCthulhu ai = brain.GetBrainBehavior();
        Vector2 scaleDistort = new((float)Math.Cos(Main.GlobalTimeWrappedHourly * MathHelper.TwoPi * 2) / 2f, (float)Math.Sin(Main.GlobalTimeWrappedHourly * MathHelper.TwoPi * 2) / 2f);
        int spawnTime = 150 - ((int)ai.Time);
        float startLerp = spawnTime / 60f;
        Color drawColor = Lighting.GetColor(brain.Center.ToTileCoordinates());
        if (spawnTime > 0)
        {
            drawColor *= 1 - startLerp;
            scaleDistort *= startLerp;
        }
        else
            scaleDistort = Vector2.Zero;
        spriteBatch.Draw(TextureAssets.Npc[NPCID.BrainofCthulhu].Value, brain.Center + (Vector2.UnitY * 16) - Main.screenPosition, ai.BoCFrame, drawColor, brain.rotation, ai.BoCFrame.Size() * 0.5f, (Vector2.One + scaleDistort) * brain.scale, 0, 0);
    }
}
