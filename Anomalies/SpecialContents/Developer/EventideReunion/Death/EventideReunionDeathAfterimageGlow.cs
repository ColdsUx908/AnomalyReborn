namespace Anomalies.SpecialContents.Developer.EventideReunion;

// 仅在一次残影绘制调用内有效的图形上下文，不是玩家实例状态。
internal static class EventideReunionDeathAfterimageDrawing
{
    internal static Player CurrentPlayer { get; private set; }
    internal static float Opacity { get; private set; }
    internal static float Scale { get; private set; }

    internal static void DrawPlayer(Player player, Vector2 position, float opacity, float scale)
    {
        Player previousPlayer = CurrentPlayer;
        float previousOpacity = Opacity;
        float previousScale = Scale;
        try
        {
            CurrentPlayer = player;
            Opacity = opacity;
            Scale = scale;
            Main.PlayerRenderer.DrawPlayer(Main.Camera, player, position, 0f, player.fullRotationOrigin, 1f - opacity, scale);
        }
        finally
        {
            CurrentPlayer = previousPlayer;
            Opacity = previousOpacity;
            Scale = previousScale;
        }
    }
}

public sealed class EventideReunionDeathAfterimageGlow : AnomalyPlayerBehavior
{
    public override void TransformDrawData(ref PlayerDrawSet drawInfo)
    {
        if (EventideReunionDeathAfterimageDrawing.CurrentPlayer != Player)
            return;
        float opacity = EventideReunionDeathAfterimageDrawing.Opacity;
        float scale = EventideReunionDeathAfterimageDrawing.Scale;
        List<DrawData> cache = drawInfo.DrawDataCache;
        List<DrawData> glow = new(cache.Count * 16);
        for (int i = 0; i < cache.Count; i++)
        {
            DrawData data = cache[i];
            if (data.texture is null)
                continue;
            // 不乘环境照明；两层扩散轮廓在人物原图层后叠加发光。
            for (int ring = 0; ring < 2; ring++)
                for (int ray = 0; ray < 8; ray++)
                {
                    DrawData outline = data;
                    outline.shader = 0;
                    outline.position += (MathHelper.TwoPi * ray / 8f).ToRotationVector2() * ((ring == 0 ? 2f : 4.5f) * scale);
                    outline.color = new Color(150, 45, 250, 0) * (opacity * (ring == 0 ? 0.22f : 0.08f));
                    glow.Add(outline);
                }
            // 给残影本体保留最低紫色亮度，暗处也能辨认身体与手臂。
            data.color = new Color(Math.Max(data.color.R, (int)(110f * opacity)),
                Math.Max(data.color.G, (int)(70f * opacity)), Math.Max(data.color.B, (int)(175f * opacity)),
                Math.Max(data.color.A, (int)(255f * opacity)));
            cache[i] = data;
        }
        cache.InsertRange(0, glow);
    }
}
