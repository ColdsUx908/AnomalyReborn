namespace Anomalies.SpecialContents.Developer.EventideReunion;

// 仅同步结算表现；真实伤害已经在服务器直接扣除，此弹幕不参与战斗。
public sealed class EventideReunionSamsaraEcho : AnomalyModProjectile
{
    public override string LocalizationCategory => EventideReunion.LocalizationCategoryForEventideReunion;
    public override string Texture => EventideReunionAssets.SpearTrailPath;
    internal int SettledDamage;
    protected override void SendAnomalyExtraAI(BinaryWriter writer) => writer.Write(SettledDamage);
    protected override void ReceiveAnomalyExtraAI(BinaryReader reader) => SettledDamage = reader.ReadInt32();
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 2;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 30;
        Projectile.netImportant = true;
    }
    public override bool? CanDamage() => false;
    public override void AI()
    {
        if (Main.dedServ || Projectile.localAI[0]++ != 0f)
            return;
        CombatText.NewText(new Rectangle((int)Projectile.Center.X - 16, (int)Projectile.Center.Y - 16, 32, 32),
            new Color(140, 230, 255), SettledDamage.ToString(), true);
        SoundEngine.PlaySound(SoundID.Item35 with { Pitch = -0.7f, Volume = 0.65f }, Projectile.Center);
        for (int i = 0; i < 24; i++)
        {
            Vector2 motion = (i * MathHelper.TwoPi / 24).ToRotationVector2() * Main.rand.NextFloat(3f, 7f);
            ParticleHandler.SpawnParticle(new GenericSparkle(Projectile.Center, motion,
                new Color(225, 255, 255), new Color(135, 100, 255), 0.3f, 28, rotationSpeed: 0.04f, bloomScale: 1.3f));
        }
    }
    public override bool PreDraw(ref Color lightColor)
    {
        float progress = 1f - Projectile.timeLeft / 30f;
        // 与持续标记共用 shader、全局动画时钟与双标记配色，仅放大淡出。
        Main.spriteBatch.End();
        try
        {
            EventideReunionMarkDrawing.DrawSamsara(Projectile.Center,
                Projectile.ai[1] * (1f + progress * 0.8f), Main.screenPosition,
                (1f - progress) * (1f - progress), 0f, Projectile.ai[2]);
        }
        finally
        {
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
        return false;
    }
}
