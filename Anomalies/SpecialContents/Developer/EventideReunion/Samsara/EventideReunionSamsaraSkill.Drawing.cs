namespace Anomalies.SpecialContents.Developer.EventideReunion;

public sealed partial class EventideReunionSamsaraSkill
{
    private readonly EventideReunionSamsaraTrail _CausalTrail = new();
    private SamsaraPhase _TrailPhase;
    private float _TrailProgress;
    private Vector2 _TrailGrip;

    private float VisualOpacity => Phase == SamsaraPhase.Charge ? MathHelper.SmoothStep(0f, 1f, Progress)
        : Phase == SamsaraPhase.Recovery ? 1f - MathHelper.SmoothStep(0f, 1f, Progress) : 1f;

    internal void DrawGear()
    {
        float opacity = VisualOpacity;
        // shader 的轮齿外沿约为网格半径的 0.81，保持旧齿轮的视觉大小。
        Vector2 center = Projectile.Owner.Center + new Vector2(0f, Projectile.Owner.gfxOffY);
        EventideReunionMarkDrawing.DrawSamsara(center,
            156f * 1.1f / 0.81f * (0.4f + 0.6f * opacity), Main.screenPosition,
            opacity * 0.9f, 1f, 0f);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        DrawBladeTrails();
        DrawCausalThreads();
        Texture2D texture = EventideReunionAssets.Weapon;
        Main.EntitySpriteDraw(texture, WeaponCenter - Main.screenPosition, null,
            Color.Lerp(lightColor, new Color(175, 205, 255), 0.35f), WeaponAngle + MathHelper.PiOver4,
            new Vector2(144f, 156f), WeaponScale, SpriteEffects.None);
        DrawThrustGhost();
        return false;
    }

    internal void DrawAfterimages()
    {
        if (Phase == SamsaraPhase.Charge)
            return;
        Player player = Projectile.Owner;
        float expansion = Phase == SamsaraPhase.Spin ? MathHelper.SmoothStep(0f, 1f, Math.Min(1f, Progress * 2f))
            : Phase == SamsaraPhase.Recovery ? 1f - MathHelper.SmoothStep(0f, 1f, Progress) : 1f;
        int heldProj = player.heldProj;
        int animation = player.itemAnimation;
        int itemTime = player.itemTime;
        try
        {
            // DrawPlayer 复制当前身体、手臂和装备姿态；隐藏持有弹幕与手持物品。
            player.heldProj = -1;
            player.itemAnimation = player.itemTime = 0;
            for (int i = 0; i < 6; i++)
            {
                float scale = 0.7f + 0.3f * expansion;
                Vector2 offset = EventideReunionSamsaraVisualGeometry.AfterimageOffset(i, player.width, player.height, scale);
                Vector2 position = Grip + offset - new Vector2(player.width * 0.5f, player.height * (1f - scale * 0.5f));
                Main.PlayerRenderer.DrawPlayer(Main.Camera, player, position, player.fullRotation,
                    player.fullRotationOrigin, 1f - 0.3f * VisualOpacity, scale);
            }
        }
        finally
        {
            player.heldProj = heldProj;
            player.itemAnimation = animation;
            player.itemTime = itemTime;
        }
    }

    private void UpdateCausalTrail()
    {
        if (Phase == SamsaraPhase.Recovery)
        {
            _CausalTrail.Freeze();
            return;
        }
        if (Phase == SamsaraPhase.Charge)
            return;
        bool samePhase = Phase == _TrailPhase;
        float from = samePhase ? _TrailProgress : 0f;
        Vector2 startGrip = samePhase ? _TrailGrip : Grip;
        Vector2 currentGrip = Grip;
        _CausalTrail.AppendCurve(sample =>
        {
            float movement = Progress > from ? (sample - from) / (Progress - from) : 1f;
            Vector2 vector;
            if (Phase == SamsaraPhase.Thrust)
                vector = Aim * (WeaponTipDistance + EventideReunionSamsaraVisualGeometry.ThrustReach(sample));
            else if (Phase == SamsaraPhase.Pullback)
            {
                float t = MathHelper.SmoothStep(0f, 1f, sample);
                float angle = _PullbackFromAim.ToRotation() + MathHelper.WrapAngle(Aim.ToRotation() - _PullbackFromAim.ToRotation()) * t;
                vector = angle.ToRotationVector2() * (WeaponTipDistance - EventideReunionSamsaraVisualGeometry.PullbackDistance * t);
            }
            else
                vector = BladeVector(Phase == SamsaraPhase.Turn, sample);
            return Vector2.Lerp(startGrip, currentGrip, movement) + vector;
        }, from, Progress);
        _TrailPhase = Phase;
        _TrailProgress = Progress;
        _TrailGrip = currentGrip;
    }

    private void DrawCausalThreads()
    {
        List<Vector2> points = _CausalTrail.Points;
        if (points.Count < 2)
            return;
        float recall = Phase == SamsaraPhase.Recovery ? MathHelper.SmoothStep(0f, 1f, Progress) : 0f;
        Main.spriteBatch.End();
        try
        {
            EventideReunionSamsaraThreadDrawing.Draw(_CausalTrail, WeaponTip, recall);
        }
        finally
        {
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }

    private void EmitParticles()
    {
        if (Phase is SamsaraPhase.Spin or SamsaraPhase.Turn)
        {
            EmitBladeParticles();
            return;
        }
        if (Timer % 3 != 0)
            return;
        Vector2 position = Phase == SamsaraPhase.Recovery ? Grip + Main.rand.NextVector2Circular(100f, 100f) : WeaponTip;
        Vector2 motion = Phase == SamsaraPhase.Recovery ? (WeaponTip - position) * 0.12f : Main.rand.NextVector2Circular(2f, 2f);
        ParticleHandler.SpawnParticle(new GenericSparkle(position, motion, new Color(210, 255, 255),
            new Color(135, 90, 235), 0.22f, 18, rotationSpeed: 0.025f, bloomScale: 1.2f));
    }
}
