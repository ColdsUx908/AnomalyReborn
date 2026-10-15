using Terraria.Graphics.Effects;

namespace Anomalies.Bosses.Skeletron.LegendaryNight.Misc;

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

        public void Draw() => FallingStar.DrawFallingStar(Position + Main.screenPosition, Rotation, Scale, Velocity, 1, true);
    }

    public const string SkyKey = "Anomalies:LegendaryNightSky";

    [LoadTexture(LegendaryNightHandler.LegendaryNightPath + "Misc/LegendaryNightSky")]
    private static Asset<Texture2D> _SkyTexture;
    public static Texture2D SkyTexture => _SkyTexture.Value;

    public static LegendaryNightSky Instance { get; private set; }

    private List<Star> _Stars = [];
    private bool _IsActive;
    private int _Timer;
    private int _SkeletronLegendHeadIndex = -1;
    public float Intensity => _Timer / 120f;

    public override void Update(GameTime gameTime)
    {
        bool foundSkeletronLegend = false;

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
        }
        else
        {
            _SkeletronLegendHeadIndex = -1;
            _Timer = Math.Clamp(_Timer - 2, 0, 120);
        }

        if (Intensity <= 0f) //如果强度为0，则清除所有星星
        {
            _Stars.Clear();
            return;
        }

        //更新星星
        foreach (Star star in _Stars)
            star.Update(Intensity);

        //删除已死的星星
        _Stars.RemoveAll(star => star.Scale <= 0.01f || star.Position.Y > Main.screenHeight * 1.5f);

        if (Intensity >= 1f && Main.rand.NextBool(30)) //随机生成星星
        {
            _Stars.Add(new Star(
                new Vector2(Main.rand.NextFloat(Main.screenWidth), -50f),
                new Vector2(Main.rand.NextFloat(-1.24f, 1.25f), Main.rand.NextFloat(2f, 4.5f)),
                Main.rand.NextFloat(MathHelper.TwoPi),
                Main.rand.NextFloat(0.05f, 0.1f)
            ));
        }

        bool CheckSingleNPC(NPC n) => n.active && n.type == NPCID.SkeletronHead && n.Anomaly?.CustomAIType == 1;
    }

    public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
    {
        using (spriteBatch.Scope(blendState: BlendState.Additive))
        {
            if (maxDepth >= float.MaxValue && minDepth < float.MaxValue)
                spriteBatch.Draw(SkyTexture, new Rectangle(0, Math.Max(0, (int)(((float)Main.worldSurface * 16f - Main.screenPosition.Y - 2400f) * 0.1f)), Main.screenWidth, Main.screenHeight), Color.White * Math.Min(1f, (Main.screenPosition.Y - 800f) / 1000f) * Intensity * 0.8f);
        }
    }

    public override bool IsActive() => _IsActive;

    public override void Reset()
    {
        _IsActive = false;
        _Timer = 0;
        _SkeletronLegendHeadIndex = -1;
    }

    public override void Activate(Vector2 position, params object[] args)
    {
        _IsActive = true;
        if (args.Length > 0 && args[0] is int index)
            _SkeletronLegendHeadIndex = index;
    }

    public override void Deactivate(params object[] args) => _IsActive = false;

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
            star.Draw();
        }
    }

    void IContentLoader.OnWorldLoad()
    {
        if (!_IsActive)
            SkyManager.Instance.Activate("Anomalies:SkeletronLegendSky");

        _Stars.Clear();
        _Timer = 0;
    }

    void IContentLoader.OnWorldUnload()
    {
        _Stars.Clear();
        _Timer = 0;
    }
}
