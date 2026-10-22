using Terraria.Graphics.Effects;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed class LegendaryNightSky : CustomSky, IContentLoader
{
    public sealed class Star
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Rotation;
        public float Scale;

        public Star(Vector2 position, Vector2 velocity, float rotation, float scale)
        {
            Position = position;
            Velocity = velocity;
            Rotation = rotation;
            Scale = scale;
        }

        public void Update(float intensity)
        {
            Position += Velocity;
            Rotation += 0.1f * (Velocity.X >= 0).ToDirectionInt();

            if (intensity < 1f)
                Scale *= intensity;
        }

        public void Draw(Color trailColor) => FallingStar.DrawFallingStar(Position + Main.screenPosition, Rotation, Scale, Velocity, 1, true, trailColorOverride: trailColor);
    }

    public sealed class RisingStarlight
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Scale;
        public float Phase;
        public int Age;
        public int LifeTime;

        public void Update()
        {
            Age++;
            Position += Velocity + new Vector2(MathF.Sin(Age * 0.03f + Phase) * 0.35f, 0f);
        }

        public void Draw(float skyIntensity)
        {
            Texture2D sparkle = AnomalyTextures.Extra.CrystalTextSparkle;
            float fade = MathHelper.Clamp(Age / 30f, 0f, 1f) * MathHelper.Clamp((LifeTime - Age) / 45f, 0f, 1f);
            float pulse = 0.75f + 0.25f * MathF.Sin(Age * 0.08f + Phase);
            float opacity = fade * pulse * skyIntensity;
            Color gold = (Color.Gold with { A = 0 }) * (opacity * 0.5f);
            Color white = (Color.LightGoldenrodYellow with { A = 0 }) * (opacity * 0.7f);
            float rotation = Age * 0.01f + Phase;
            // 参照 Calamity 特殊字体星光：两层旋转星芒，细长外辉与明亮内芯。
            Main.spriteBatch.Draw(sparkle, Position, null, gold, rotation, sparkle.Size() * 0.5f,
                new Vector2(Scale * 0.65f, Scale * 1.8f), SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(sparkle, Position, null, white, rotation, sparkle.Size() * 0.5f,
                Scale * 0.5f, SpriteEffects.None, 0f);
        }
    }

    public const string SkyKey = "Anomalies:LegendaryNightSky";

    [LoadTexture(LegendaryNightHandler.LegendaryNightPath + "Misc/LegendaryNightSky")]
    private static Asset<Texture2D> _SkyTexture;
    public static Texture2D SkyTexture => _SkyTexture.Value;

    [LoadTexture(LegendaryNightHandler.LegendaryNightPath + "Misc/LegendaryNightSky2")]
    private static Asset<Texture2D> _OrangeSkyTexture;
    public static Texture2D OrangeSkyTexture => _OrangeSkyTexture.Value;

    public static LegendaryNightSky Instance { get; private set; }

    private List<Star> _Stars = [];
    private List<RisingStarlight> _RisingStarlights = [];
    private bool _IsActive;
    private int _Timer;
    private int _SkeletronLegendHeadIndex = -1;
    private float _OrangeBlend;
    private bool _FullBrightActive;
    private bool _CloudSuppressionActive;
    public float Intensity => _Timer / 120f;
    public bool FullBrightActive => _IsActive && _FullBrightActive && !Main.gameMenu;
    public float EntityLightingIntensity => FullBrightActive ? _OrangeBlend : 0f;
    public bool CloudSuppressionActive => _IsActive && _CloudSuppressionActive && !Main.gameMenu;

    public override void Update(GameTime gameTime)
    {
        bool foundSkeletronLegend = false;
        SkeletronLegendHead.Phase currentPhase = SkeletronLegendHead.Phase.Initialize;

        if (NPC.TryGetNPCFromIndex(_SkeletronLegendHeadIndex, out NPC potentialLegendHead) && CheckSingleNPC(potentialLegendHead))
            foundSkeletronLegend = true;

        if (!foundSkeletronLegend)
        {
            foreach (NPC n in NPC.ActiveNPCs)
            {
                if (CheckSingleNPC(n))
                {
                    _SkeletronLegendHeadIndex = n.whoAmI;
                    foundSkeletronLegend = true;
                    break;
                }
            }
        }

        if (foundSkeletronLegend)
        {
            _Timer = Math.Clamp(_Timer + 1, 0, 120);
            _OrangeBlend = SkeletronLegendHead.GetInstance(Main.npc[_SkeletronLegendHeadIndex]).SkyOrangeBlend;
            currentPhase = SkeletronLegendHead.GetInstance(Main.npc[_SkeletronLegendHeadIndex]).CurrentPhase;
        }
        else
        {
            _SkeletronLegendHeadIndex = -1;
            _Timer = Math.Clamp(_Timer - 2, 0, 120);
        }

        bool phase2 = currentPhase >= SkeletronLegendHead.Phase.Phase2_1;
        _CloudSuppressionActive = foundSkeletronLegend
            && SkeletronLegendHead.GetInstance(Main.npc[_SkeletronLegendHeadIndex]).SuppressSkyClouds;
        _FullBrightActive = foundSkeletronLegend && _OrangeBlend > 0f;
        if (foundSkeletronLegend && phase2)
            AnomalyStarVisual.Intensity += 1f / 30f;

        if (Intensity <= 0f) //如果强度为0，则清除所有星星
        {
            _Stars.Clear();
            _RisingStarlights.Clear();
            _OrangeBlend = 0f;
            return;
        }

        //更新星星
        foreach (Star star in _Stars)
            star.Update(Intensity);

        //删除已死的星星
        _Stars.RemoveAll(star => star.Scale <= 0.01f || star.Position.Y > Main.screenHeight * 1.5f);

        foreach (RisingStarlight starlight in _RisingStarlights)
            starlight.Update();
        _RisingStarlights.RemoveAll(starlight => starlight.Age >= starlight.LifeTime || starlight.Position.Y < -80f);

        if (foundSkeletronLegend && Intensity >= 1f && currentPhase != SkeletronLegendHead.Phase.PhaseChange_1To2
            && Main.rand.NextBool(phase2 ? 24 : 30))
        {
            _Stars.Add(new Star(
                new Vector2(Main.rand.NextFloat(Main.screenWidth), -50f),
                new Vector2(Main.rand.NextFloat(-1.24f, 1.25f), Main.rand.NextFloat(2f, 4.5f)),
                Main.rand.NextFloat(MathHelper.TwoPi),
                Main.rand.NextFloat(0.05f, 0.1f)
            ));
        }

        if (foundSkeletronLegend && phase2 && Intensity >= 1f && _RisingStarlights.Count < 180 && Main.rand.NextBool(6))
        {
            float speed = Main.rand.NextFloat(2.5f, 5f);
            _RisingStarlights.Add(new RisingStarlight
            {
                Position = new Vector2(Main.rand.NextFloat(Main.screenWidth), Main.screenHeight + 40f),
                Velocity = new Vector2(Main.rand.NextFloat(-0.35f, 0.35f), -speed),
                Scale = Main.rand.NextFloat(0.2f, 0.5f),
                Phase = Main.rand.NextFloat(MathHelper.TwoPi),
                LifeTime = (int)((Main.screenHeight + 160f) / speed)
            });
        }

        bool CheckSingleNPC(NPC n) => n.active && n.type == NPCID.SkeletronHead && n.Anomaly?.CustomAIType == 1;
    }

    public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
    {
        using (spriteBatch.Scope(blendState: BlendState.Additive))
        {
            if (maxDepth >= float.MaxValue && minDepth < float.MaxValue)
            {
                Rectangle destination = new(0, Math.Max(0, (int)(((float)Main.worldSurface * 16f - Main.screenPosition.Y - 2400f) * 0.1f)), Main.screenWidth, Main.screenHeight);
                Color color = Color.White * MathHelper.Clamp((Main.screenPosition.Y - 800f) / 1000f, 0f, 1f) * Intensity * 0.8f;
                spriteBatch.Draw(SkyTexture, destination, color * (1f - _OrangeBlend));
                spriteBatch.Draw(OrangeSkyTexture, destination, color * _OrangeBlend);
            }
        }
    }

    public override bool IsActive() => _IsActive;

    public override void Reset()
    {
        _IsActive = false;
        _Timer = 0;
        _SkeletronLegendHeadIndex = -1;
        _OrangeBlend = 0f;
        _FullBrightActive = false;
        _CloudSuppressionActive = false;
        _Stars.Clear();
        _RisingStarlights.Clear();
    }

    public override void Activate(Vector2 position, params object[] args)
    {
        _IsActive = true;
        if (args.Length > 0 && args[0] is int index)
            _SkeletronLegendHeadIndex = index;
    }

    public override void Deactivate(params object[] args)
    {
        _IsActive = false;
        _FullBrightActive = false;
        _CloudSuppressionActive = false;
    }

    void IContentLoader.PostSetupContent()
    {
        SkyManager.Instance["Anomalies:SkeletronLegendSky"] = Instance = this;
        On_Main.DrawStarsInBackground += On_Main_DrawStarsInBackground;
    }

    private void On_Main_DrawStarsInBackground(On_Main.orig_DrawStarsInBackground orig, Main self, Main.SceneArea sceneArea, bool artificial)
    {
        orig(self, sceneArea, artificial);
        if (!Main.dedServ && !Main.gameMenu)
        {
            foreach (Star star in _Stars)
                star.Draw(Color.Lerp(Color.Blue, Color.Gold, _OrangeBlend));
            foreach (RisingStarlight starlight in _RisingStarlights)
                starlight.Draw(Intensity);
        }
    }

    void IContentLoader.OnWorldLoad()
    {
        _FullBrightActive = false;
        _CloudSuppressionActive = false;
        _OrangeBlend = 0f;
        if (!_IsActive)
            SkyManager.Instance.Activate("Anomalies:SkeletronLegendSky");

        _Stars.Clear();
        _RisingStarlights.Clear();
        _Timer = 0;
    }

    void IContentLoader.OnWorldUnload()
    {
        _FullBrightActive = false;
        _CloudSuppressionActive = false;
        _OrangeBlend = 0f;
        _Stars.Clear();
        _RisingStarlights.Clear();
        _Timer = 0;
    }
}
