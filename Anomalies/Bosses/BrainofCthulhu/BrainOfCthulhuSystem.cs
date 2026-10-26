using Anomalies.Assets.Effects;
using Terraria.Graphics.Effects;

namespace Anomalies.Bosses.BrainofCthulhu;

public class BrainOfCthulhuSystem : ModSystem
{
    public static bool IsBrainOfCthulhuTextureVanilla => _VanillaBoCTexture;

    private static bool _VanillaBoCTexture = true;
    [LoadTexture("Terraria/Images/Chain12")]
    internal static Asset<Texture2D> Tendril;

    [LoadTexture("Anomalies/Bosses/BrainofCthulhu/Assets/VerticalSmearRagged")]
    private static Asset<Texture2D> _BloodScytheSource;

    [LoadTexture("Anomalies/Bosses/BrainofCthulhu/ReferenceBrainOfCthulhu")]
    private static Asset<Texture2D> _ReferenceBrain;
    private static Texture2D _TendrilGlow = null;
    private static Texture2D _BrainGlow = null;
    private static Texture2D _CreeperGlow = null;
    private static Texture2D _BloodScytheMask = null;
    internal static float ScreenBlurStrength = 0f;
    internal static (int creeper, List<VerletSimulatedSegment> tendril, int reelInTimer)[] VerletTendrils;
    private static int _PreviousMusic = -1;
    public static int PreviousMusic => _PreviousMusic;

    public override void OnModLoad()
    {
        On_NPC.SpawnBoss += SpawnBrainNoMessage;
        On_Player.ItemCheck_UseBossSpawners += BlockRoar;
        On_Main.UpdateAudio_DecideOnNewMusic += StopBoss3FromStarting;
        On_Main.UpdateAudio_DecideOnTOWMusic += StopOWBoss1FromStarting;
    }

    private void StopBoss3FromStarting(On_Main.orig_UpdateAudio_DecideOnNewMusic orig, Main self)
    {
        orig(self);
        if (NPC.crimsonBoss == -1 || !AnomalySharedData.Anomaly || !Main.npc[NPC.crimsonBoss].active || !Main.npc[NPC.crimsonBoss].TryGetBrainBehavior(out BrainofCthulhu brainAI))
            return;
        if (_PreviousMusic < 0 || _PreviousMusic >= Main.musicFade.Length)
            return;
        // The last part leaves one frame at the end of the spawn animation where the boss music starts playing, so that it can be instantly maxed out
        if (brainAI.CurrentPhase == BrainofCthulhu.Phase.Introduction && (brainAI.Time - Math.Abs(brainAI.SpawnTime) < 420))
        {
            if (Main.newMusic == MusicID.Boss3)
                Main.newMusic = _PreviousMusic;
            if (Main.curMusic == MusicID.Boss3)
                Main.curMusic = _PreviousMusic;
        }

        if (Main.newMusic == MusicID.Boss1)
            Main.newMusic = _PreviousMusic;
        if (Main.curMusic == MusicID.Boss1)
            Main.curMusic = _PreviousMusic;
    }

    private void StopOWBoss1FromStarting(On_Main.orig_UpdateAudio_DecideOnTOWMusic orig, Main self)
    {
        orig(self);
        if (NPC.crimsonBoss == -1 || !AnomalySharedData.Anomaly || !Main.npc[NPC.crimsonBoss].TryGetBrainBehavior(out BrainofCthulhu brainAI))
            return;
        if (_PreviousMusic < 0 || _PreviousMusic >= Main.musicFade.Length)
            return;
        // The last part leaves one frame at the end of the spawn animation where the boss music starts playing, so that it can be instantly maxed out
        if (brainAI.CurrentPhase == BrainofCthulhu.Phase.Introduction && (brainAI.Time - Math.Abs(brainAI.SpawnTime) < 420))
        {
            if (Main.newMusic == MusicID.OtherworldlyBoss1)
                Main.newMusic = _PreviousMusic;
            if (Main.curMusic == MusicID.OtherworldlyBoss1)
                Main.curMusic = _PreviousMusic;
        }
    }

    private void BlockRoar(On_Player.orig_ItemCheck_UseBossSpawners orig, Player self, int onWhichPlayer, Item sItem)
    {
        if (sItem.type != ItemID.BloodySpine || !AnomalySharedData.Anomaly || !self.ItemTimeIsZero || self.itemAnimation <= 0)
        {
            orig(self, onWhichPlayer, sItem);
            return;
        }

        SoundEngine.PlaySound(SoundID.NPCDeath1, Main.LocalPlayer.Center);
        self.ApplyItemTime(sItem);
        TOCombatUtils.SpawnBossUsingItem(self, NPCID.BrainofCthulhu);
    }

    private void SpawnBrainNoMessage(On_NPC.orig_SpawnBoss orig, int spawnPositionX, int spawnPositionY, int Type, int targetPlayerIndex)
    {
        if (Type != NPCID.BrainofCthulhu || !AnomalySharedData.Anomaly)
        {
            orig(spawnPositionX, spawnPositionY, Type, targetPlayerIndex);
            return;
        }

        NPC.NewNPCAction(NPC.GetBossSpawnSource(targetPlayerIndex), new Vector2(spawnPositionX, spawnPositionY), Type, 1, action: brain =>
        {
            if (Main.player[targetPlayerIndex].HeldItem.type == ItemID.BloodySpine)
                brain.GetBrainBehavior().SummonedViaItem = true;
            NPC.crimsonBoss = brain.whoAmI;
            brain.target = targetPlayerIndex;
            brain.timeLeft *= 20;
            _PreviousMusic = Main.curMusic;
        });
    }

    internal static Texture2D GetBloodScytheMask()
    {
        if (_BloodScytheMask == null)
        {
            Texture2D source = _BloodScytheSource.Value;
            Color[] pixels = new Color[source.Width * source.Height];
            source.GetData(pixels);
            for (int i = 0; i < pixels.Length; i++)
            {
                // This particle texture stores its silhouette in RGB on black.
                // RGB is already premultiplied by source alpha: use brightness
                // as coverage and emit a premultiplied white alpha mask.
                byte coverage = Math.Max(pixels[i].R, Math.Max(pixels[i].G, pixels[i].B));
                pixels[i] = new Color(coverage, coverage, coverage, coverage);
            }
            _BloodScytheMask = new Texture2D(Main.graphics.GraphicsDevice, source.Width, source.Height);
            _BloodScytheMask.SetData(pixels);
        }
        return _BloodScytheMask;
    }

    internal static Texture2D GetTendrilGlow()
    {
        if (_TendrilGlow == null)
        {
            Texture2D tex = new(Main.graphics.GraphicsDevice, Tendril.Value.Width, Tendril.Value.Height);
            Color[] BaseArray = new Color[tex.Width * tex.Height];
            Color[] ColorArray = new Color[tex.Width * tex.Height];
            Tendril.Value.GetData(BaseArray);
            for (int i = 0; i < BaseArray.Length; i++)
            {
                ColorArray[i] = new Color(255, 255, 255) * (BaseArray[i].A / 255f);
            }

            tex.SetData(ColorArray);
            _TendrilGlow = tex;
        }

        return _TendrilGlow;
    }

    internal static Texture2D GetBrainGlow()
    {
        if (_BrainGlow == null)
        {
            Texture2D tex = new(Main.graphics.GraphicsDevice, TextureAssets.Npc[NPCID.BrainofCthulhu].Value.Width, TextureAssets.Npc[NPCID.BrainofCthulhu].Value.Height);
            Color[] BaseArray = new Color[tex.Width * tex.Height];
            Color[] ColorArray = new Color[tex.Width * tex.Height];
            TextureAssets.Npc[NPCID.BrainofCthulhu].Value.GetData(BaseArray);
            for (int i = 0; i < BaseArray.Length; i++)
            {
                ColorArray[i] = new Color(255, 255, 255) * (BaseArray[i].A / 255f);
            }

            tex.SetData(ColorArray);
            _BrainGlow = tex;
        }

        return _BrainGlow;
    }

    internal static Texture2D GetCreeperGlow()
    {
        if (_CreeperGlow == null)
        {
            Texture2D tex = new(Main.graphics.GraphicsDevice, TextureAssets.Npc[NPCID.Creeper].Value.Width, TextureAssets.Npc[NPCID.Creeper].Value.Height);
            Color[] BaseArray = new Color[tex.Width * tex.Height];
            Color[] ColorArray = new Color[tex.Width * tex.Height];
            TextureAssets.Npc[NPCID.Creeper].Value.GetData(BaseArray);
            for (int i = 0; i < BaseArray.Length; i++)
            {
                ColorArray[i] = new Color(255, 255, 255) * (BaseArray[i].A / 255f);
            }

            tex.SetData(ColorArray);
            _CreeperGlow = tex;
        }

        return _CreeperGlow;
    }

    public override void OnWorldLoad()
    {
        if (!Main.dedServ)
        {
            Main.QueueMainThreadAction(() =>
            {
                Texture2D reference = _ReferenceBrain.Value;
                Main.instance.LoadNPC(NPCID.BrainofCthulhu);
                Texture2D usedTexture = TextureAssets.Npc[NPCID.BrainofCthulhu].Value;
                int refSize = reference.Width * reference.Height;
                Color[] referenceArray = new Color[refSize];
                reference.GetData(referenceArray);
                int texSize = usedTexture.Width * usedTexture.Height;
                Color[] textureArray = new Color[texSize];
                usedTexture.GetData(textureArray);
                bool match = reference.Width == usedTexture.Width && reference.Height == usedTexture.Height;
                for (int i = 0; match && i < referenceArray.Length; i++)
                    if (referenceArray[i] != textureArray[i])
                    {
                        match = false;
                        break;
                    }

                _VanillaBoCTexture = match;
            });
        }
    }

    public override void PostUpdateNPCs()
    {
        if (!BrainBehaviorAccess.ActiveBrain || VerletTendrils is null)
            return;
        if (Main.netMode != NetmodeID.Server)
        {
            if (NPC.crimsonBoss != -1 && AnomalySharedData.Anomaly && Main.npc[NPC.crimsonBoss].GetBrainBehavior().CurrentBehavior <= BrainofCthulhu.Behavior.Phase2TransitionOpen)
            {
                NPC brain = Main.npc[NPC.crimsonBoss];
                bool transitioning = brain.GetBrainBehavior().CurrentPhase == BrainofCthulhu.Phase.Transition;
                for (int index = 0; index < VerletTendrils.Length; index++)
                {
                    NPC creeper = Main.npc[VerletTendrils[index].creeper];
                    List<VerletSimulatedSegment> segments = VerletTendrils[index].tendril;
                    if (segments is null || segments.Count < 2)
                        continue;
                    if (!creeper.active || creeper.type != NPCID.Creeper)
                    {
                        segments.Clear();
                        continue;
                    }
                    Vector2 startPoint = brain.Center + brain.netOffset + Vector2.UnitY * 32;
                    float creeperRatio = index / (float)BrainofCthulhu.CreeperCount;
                    startPoint.X += index % 2 == 0 ? MathHelper.Lerp(-24, 0, creeperRatio) : MathHelper.Lerp(24, 0, creeperRatio);
                    Vector2 endPoint = creeper.Center + creeper.netOffset;
                    if (transitioning)
                    {
                        // Preserve the existing curve and move its anchors without
                        // releasing the end or injecting spring energy during the fade.
                        Vector2 startDelta = startPoint - segments[0].Position;
                        Vector2 endDelta = endPoint - segments[^1].Position;
                        for (int i = 0; i < segments.Count; i++)
                        {
                            segments[i].Position += Vector2.Lerp(startDelta, endDelta, i / (float)(segments.Count - 1));
                            segments[i].OldPosition = segments[i].Position;
                        }
                        continue;
                    }
                    segments[0].Position = startPoint;
                    segments[0].OldPosition = startPoint;
                    segments[0].Locked = true;
                    segments[^1].Position = endPoint;
                    segments[^1].OldPosition = endPoint;
                    segments[^1].Locked = true;
                    float segmentDistance = Math.Max(16f, Vector2.Distance(startPoint, endPoint) / (segments.Count - 1));
                    VerletSimulatedSegment.SimpleSimulation(segments, segmentDistance, 10, 3, 0.85f);
                }
            }
        }
    }

    public override void PostDrawTiles()
    {
        if (Main.dedServ || AnomalyEffects.ScreenFilters.BrainOfCthulhuForcefield is null || AnomalyEffects.ScreenFilters.RadialBlur is null)
            return;
        if (Main.netMode != NetmodeID.Server)
        {
            if (!BrainBehaviorAccess.ActiveBrain || !AnomalySharedData.Anomaly || Main.npc[NPC.crimsonBoss].GetBrainBehavior().CurrentBehavior >= BrainofCthulhu.Behavior.Phase2TransitionClosed || !Main.npc[NPC.crimsonBoss].TryGetBrainBehavior(out BrainofCthulhu ai))
            {
                Filters.Scene["Anomalies:BrainOfCthulhuForcefield"].GetShader().UseOpacity(0);
                if (Filters.Scene["Anomalies:BrainOfCthulhuForcefield"].IsActive())
                    Filters.Scene.Deactivate("Anomalies:BrainOfCthulhuForcefield");
            }
            else
            {
                if (!Filters.Scene["Anomalies:BrainOfCthulhuForcefield"].IsActive())
                    Filters.Scene.Activate("Anomalies:BrainOfCthulhuForcefield");
                NPC target = Main.npc[NPC.crimsonBoss];
                Vector2 targetPos = target.Center + target.netOffset;
                float shieldOpacity = ai.ShieldOpacity;
                float shieldScale = ai.ShieldScale;
                targetPos = Vector2.Transform(targetPos - Main.screenPosition, Main.GameViewMatrix.ZoomMatrix) / Main.ScreenSize.ToVector2();
                Texture2D voronoi = AnomalyTextures.Extra.Voronoi;
                Texture2D depthNoise = AnomalyTextures.Extra.Vein2;
                Filters.Scene["Anomalies:BrainOfCthulhuForcefield"].GetShader().Shader.Parameters["voronoi"].SetValue(voronoi);
                Filters.Scene["Anomalies:BrainOfCthulhuForcefield"].GetShader().Shader.Parameters["depthNoise"].SetValue(depthNoise);
                Filters.Scene["Anomalies:BrainOfCthulhuForcefield"].GetShader().Shader.Parameters["uScreenResolution"].SetValue(new Vector2(Main.graphics.GraphicsDevice.Viewport.Width, Main.graphics.GraphicsDevice.Viewport.Height));
                Filters.Scene["Anomalies:BrainOfCthulhuForcefield"].GetShader().UseProgress(0.15f * shieldScale);
                Filters.Scene["Anomalies:BrainOfCthulhuForcefield"].GetShader().UseOpacity(shieldOpacity);
                Filters.Scene["Anomalies:BrainOfCthulhuForcefield"].GetShader().UseColor(Color.Red);
                Filters.Scene["Anomalies:BrainOfCthulhuForcefield"].GetShader().UseSecondaryColor(new Color(255, 0, 90)); //Crimson color (R:220,G:20,B:60,A:255). // Magenta color (R:255,G:0,B:255,A:255).
                Filters.Scene["Anomalies:BrainOfCthulhuForcefield"].GetShader().UseDirection(targetPos);
            }

            if (ScreenBlurStrength == 0f)
            {
                Filters.Scene["Anomalies:RadialBlur"].GetShader().UseIntensity(0);
                if (Filters.Scene["Anomalies:RadialBlur"].IsActive())
                    Filters.Scene.Deactivate("Anomalies:RadialBlur");
                return;
            }

            if (!BrainBehaviorAccess.ActiveBrain)
            {
                ScreenBlurStrength = Filters.Scene["Anomalies:RadialBlur"].GetShader().Intensity * 0.9f;
                Filters.Scene["Anomalies:RadialBlur"].GetShader().UseIntensity(ScreenBlurStrength);
                if (ScreenBlurStrength < 0.01f)
                    ScreenBlurStrength = 0f;
                return;
            }

            if (Filters.Scene["Anomalies:RadialBlur"].IsLoaded)
            {
                if (!Filters.Scene["Anomalies:RadialBlur"].IsActive())
                    Filters.Scene.Activate("Anomalies:RadialBlur");
                NPC boss = Main.npc[NPC.crimsonBoss];
                float counter = boss.ai[1] - boss.ai[2] - 240;
                float distSQ = Main.LocalPlayer.DistanceSQ(boss.Center);
                float distanceScaleFactor = 1;
                if (distSQ > 592900) //770^2
                    distanceScaleFactor = 1 / (1 + (((float)Math.Sqrt(distSQ) - 770) / 32f));
                Filters.Scene["Anomalies:RadialBlur"].GetShader().UseIntensity((ScreenBlurStrength + (((float)Math.Cos(counter * MathHelper.TwoPi / 15f) / 2f + 0.5f) * (0.4f * ScreenBlurStrength))) * distanceScaleFactor);
                Filters.Scene["Anomalies:RadialBlur"].GetShader().Shader.Parameters["uSaturation"].SetValue(20);
                Vector2 targetPos = Vector2.Transform(boss.Center - Main.screenPosition, Main.GameViewMatrix.ZoomMatrix) / Main.ScreenSize.ToVector2();
                Filters.Scene["Anomalies:RadialBlur"].GetShader().UseDirection(targetPos);
            }
        }
    }

    public override void PostUpdateEverything()
    {
        bool allowBossMusic = false;
        if (NPC.crimsonBoss != -1 && Main.npc[NPC.crimsonBoss].active && Main.npc[NPC.crimsonBoss].TryGetBrainBehavior(out BrainofCthulhu brainAI) && brainAI.CurrentBehavior > BrainofCthulhu.Behavior.SurfaceSpawnAnimation)
            allowBossMusic = true;
        if (((Main.curMusic != MusicID.Boss3 && Main.curMusic != MusicID.OtherworldlyBoss1) || allowBossMusic) && Main.curMusic != MusicID.Boss1)
            _PreviousMusic = Main.curMusic;
        if (_PreviousMusic == -1)
            _PreviousMusic = MusicID.Crimson;
    }

    public override void OnWorldUnload()
    {
        VerletTendrils = null;
        ScreenBlurStrength = 0f;
        if (!Main.dedServ && AnomalyEffects.ScreenFilters.BrainOfCthulhuForcefield is not null)
        {
            Filters.Scene.Deactivate("Anomalies:BrainOfCthulhuForcefield");
            Filters.Scene.Deactivate("Anomalies:RadialBlur");
        }
    }

    public override void Unload()
    {
        OnWorldUnload();
        // Unload can run on a worker thread; capture the owned textures before clearing the cache.
        Texture2D[] textures = [_TendrilGlow, _BrainGlow, _CreeperGlow, _BloodScytheMask];
        _BloodScytheMask = null;
        _TendrilGlow = _BrainGlow = _CreeperGlow = null;
        Main.QueueMainThreadAction(() =>
        {
            foreach (Texture2D texture in textures)
                texture?.Dispose();
        });
        _PreviousMusic = -1;
    }
}
