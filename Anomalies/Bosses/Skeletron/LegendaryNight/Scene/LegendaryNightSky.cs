using Terraria.Graphics.Effects;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class LegendaryNightSky : CustomSky, IContentLoader
{
    /// <summary>天空注册键。</summary>
    public const string SkyKey = "Anomalies:SkeletronLegendSky";

    /// <summary>天空贴图。</summary>
    [LoadTexture(LegendaryNightHandler.LegendaryNightPath + "Scene/LegendaryNightSky")]
    private static Asset<Texture2D> _SkyTexture;
    /// <summary>天空贴图。</summary>
    public static Texture2D SkyTexture => _SkyTexture.Value;

    /// <summary>橙色天空贴图。</summary>
    [LoadTexture(LegendaryNightHandler.LegendaryNightPath + "Scene/LegendaryNightSky2")]
    private static Asset<Texture2D> _OrangeSkyTexture;
    /// <summary>橙色天空贴图。</summary>
    public static Texture2D OrangeSkyTexture => _OrangeSkyTexture.Value;

    /// <summary>当前天空实例，卸载时清空。</summary>
    public static LegendaryNightSky Instance { get; private set; }

    private List<Star> _Stars = [];
    private List<RisingStarlight> _RisingStarlights = [];
    private bool _IsActive;
    private int _Timer;
    private float _OrangeBlend;
    private bool _FullBrightActive;
    private bool _CloudSuppressionActive;
    public float Intensity => _Timer / 120f;
    public bool FullBrightActive => _IsActive && _FullBrightActive && !Main.gameMenu;
    public float EntityLightingIntensity => FullBrightActive ? _OrangeBlend : 0f;
    public bool CloudSuppressionActive => _IsActive && _CloudSuppressionActive && !Main.gameMenu;

    public override void Update(GameTime gameTime)
    {
        bool foundSkeletronLegend = LegendaryNightHandler.TryGetSceneHead(out SkeletronLegendHead head);
        SkeletronLegendHead.Phase currentPhase = head?.CurrentPhase ?? SkeletronLegendHead.Phase.Initialize;
        if (foundSkeletronLegend)
        {
            _Timer = Math.Clamp(_Timer + 1, 0, 120);
            _OrangeBlend = head.SkyOrangeBlend;
        }
        else
            _Timer = Math.Clamp(_Timer - 2, 0, 120);

        bool phase2 = head?.Phase2 ?? false;
        _CloudSuppressionActive = head?.SuppressSkyClouds ?? false;
        _FullBrightActive = foundSkeletronLegend && _OrangeBlend > 0f;
        if (head?.AnomalyStarActive == true)
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
        _OrangeBlend = 0f;
        _FullBrightActive = false;
        _CloudSuppressionActive = false;
        _Stars.Clear();
        _RisingStarlights.Clear();
    }

    public override void Activate(Vector2 position, params object[] args) => _IsActive = true;

    public override void Deactivate(params object[] args)
    {
        _IsActive = false;
        _FullBrightActive = false;
        _CloudSuppressionActive = false;
    }

    void IContentLoader.PostSetupContent()
    {
        SkyManager.Instance[SkyKey] = Instance = this;
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
            SkyManager.Instance.Activate(SkyKey);

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