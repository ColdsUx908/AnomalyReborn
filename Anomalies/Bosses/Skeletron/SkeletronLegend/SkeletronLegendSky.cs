using Terraria.Graphics.Effects;

namespace Anomalies.Bosses.Skeletron.SkeletronLegend;

public sealed class SkeletronLegendSky : CustomSky, IContentLoader
{
    public const string SkyKey = "Anomalies:SkeletronLegendSky";

    [LoadTexture(SkeletronLegendHandler.SkeletronLegendPath + "SkeletronLegendSky")]
    private static Asset<Texture2D> _SkyTexture;
    public static Texture2D SkyTexture => _SkyTexture.Value;

    public static SkeletronLegendSky Instance { get; private set; }

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

        bool CheckSingleNPC(NPC n) => n.type == NPCID.SkeletronHead && SkeletronLegendHead.GetInstance(n)?.LegendFlag == true;
    }

    public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
    {
        //spriteBatch.ChangeBlendState(BlendState.Additive);
        using (spriteBatch.Scope(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp))
        {
            spriteBatch.Draw(SkyTexture,
                new Rectangle(0, /* Math.Max(0, (int)((Main.worldSurface * 16.0 - Main.screenPosition.Y - 2400.0) / 2.0))*/ 0, Main.screenWidth, Main.screenHeight * 2),
                Color.White * Math.Min(1f, Intensity));
        }
        //spriteBatch.ChangeBlendState(BlendState.AlphaBlend);
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
        SkyManager.Instance["Anomalies:SkeletronLegendSky"] = this;
    }

    void IContentLoader.OnWorldLoad()
    {
        if (!_IsActive)
            SkyManager.Instance.Activate("Anomalies:SkeletronLegendSky");
    }
}
