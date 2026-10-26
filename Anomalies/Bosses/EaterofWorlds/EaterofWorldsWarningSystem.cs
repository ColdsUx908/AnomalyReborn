namespace Anomalies.Bosses.EaterofWorlds;

// Draw independently of NPC screen culling: a head outside the viewport can still telegraph a charge into it.
public sealed class EaterofWorldsWarningSystem : ModSystem
{
    internal static Texture2D CoilCloud { get; private set; }
    private static bool _CloudEnabled;

    public override void Load()
    {
        if (!Main.dedServ)
        {
            _CloudEnabled = true;
            On_Main.DrawNPCs += DrawMasks;
            Main.QueueMainThreadAction(CreateCloudTexture);
        }
    }

    public override void Unload()
    {
        _CloudEnabled = false;
        Texture2D cloud = CoilCloud;
        CoilCloud = null;
        if (cloud is not null)
            Main.QueueMainThreadAction(cloud.Dispose);
    }

    private static void CreateCloudTexture()
    {
        if (!_CloudEnabled)
            return;
        const int Size = 256;
        Color[] pixels = new Color[Size * Size];
        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                float u = (x + 0.5f - Size * 0.5f) / (Size * 0.5f);
                float v = (y + 0.5f - Size * 0.5f) / (Size * 0.5f);
                float noise = MathF.Sin(u * 13f + MathF.Sin(v * 9f)) * MathF.Sin(v * 11f + MathF.Cos(u * 7f));
                float radius = MathF.Sqrt(u * u + v * v) + noise * 0.085f;
                float alpha = 1f - MathHelper.SmoothStep(0f, 1f, Math.Clamp((radius - 0.58f) / 0.4f, 0f, 1f));
                pixels[y * Size + x] = Color.White * alpha;
            }
        }
        CoilCloud = new Texture2D(Main.graphics.GraphicsDevice, Size, Size);
        CoilCloud.SetData(pixels);
    }

    private static void DrawMasks(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles)
    {
        orig(self, behindTiles);
        if (behindTiles || !AnomalySharedData.Anomaly)
            return;
        // DrawNPCs is called inside an active world-space batch; cover the NPCs after both NPC passes.
        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (npc.type != NPCID.EaterofWorldsHead || !npc.Anomaly.ShouldRunAnomalyAI)
                continue;
            EaterofWorldsHead behavior = EaterofWorldsHead.GetInstance(npc);
            if (behavior.IsFirstSegment)
                behavior.DrawCoilMask(Main.spriteBatch, Main.screenPosition);
        }
    }

    public override void PostDrawTiles()
    {
        if (Main.dedServ || !AnomalySharedData.Anomaly)
            return;
        bool begun = false;
        try
        {
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.type != NPCID.EaterofWorldsHead || !npc.Anomaly.ShouldRunAnomalyAI)
                    continue;
                EaterofWorldsHead behavior = EaterofWorldsHead.GetInstance(npc);
                if (!behavior.IsFirstSegment)
                    continue;
                if (!begun)
                {
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                        DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                    begun = true;
                }
                behavior.DrawAttackWarnings(Main.spriteBatch, Main.screenPosition);
            }
        }
        finally
        {
            if (begun)
                Main.spriteBatch.End();
        }
    }
}
