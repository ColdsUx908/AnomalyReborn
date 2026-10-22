namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed class SlimeMount : AnomalyModProjectile
{
    /* 数组使用约定
     * 
     * Projectile.ai
     * [0] 准备时间（单位：帧）；可飞向落点，也可原地等待
     * [1] 阶段：0 准备，1 下落及预警
     * [2] 目标索引（1-based，即 1 表示玩家索引 0，2 表示玩家索引 1，以此类推）
     */

    private const float WarningMaxLength = 1600f;
    private const float WarningGrowthTime = 30f;

    // 所属骷髅王索引（1-based），用于清场和准备状态有效性检查；下落后不再依赖主人。
    public int MasterIndex;

    protected override void SendAnomalyExtraAI(BinaryWriter writer) => writer.Write(MasterIndex);
    protected override void ReceiveAnomalyExtraAI(BinaryReader reader) => MasterIndex = reader.ReadInt32();

    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;

    public override string Texture => TOTextures.InvisibleTexturePath;

    public override void SetStaticDefaults()
    {
        Main.projFrames[Projectile.type] = 4;
        // 与 ThanatosLaser 相同：本体在屏幕上方时仍须提交延伸进屏幕的预警线。
        ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;
    }

    public override void SetDefaults()
    {
        Projectile.netImportant = true;
        Projectile.width = 54;
        Projectile.height = 34;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.timeLeft = 600;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
    }

    public override void AI()
    {
        if (Projectile.ai[1] == 0f && MasterIndex > 0 && TOSharedData.NotClient)
        {
            NPC master = NPC.GetNPCFromIndex(MasterIndex - 1);
            if (!master.active || master.type != NPCID.SkeletronHead || master.Anomaly.CustomAIType != 1
                || !(SkeletronLegendHead.GetInstance(master).CurrentAvatar == SkeletronLegendHead.Avatar.FlyingSlime
                    || SkeletronLegendHead.GetInstance(master).Phase2SlimeRainActive))
            {
                Projectile.Kill();
                return;
            }
        }

        Timer1++;
        if (Projectile.ai[1] == 0f)
        {
            if (Timer1 > Projectile.ai[0])
            {
                Projectile.ai[1] = 1f;
                Timer2 = 0;
                Projectile.velocity = Vector2.Zero;
                Projectile.netUpdate = true;
            }

            Projectile.frame = 3;

            Projectile.tileCollide = false;
        }
        else
        {
            Timer2++;
            Projectile.velocity.X = 0f;
            Projectile.velocity.Y = Math.Min(Projectile.velocity.Y + 0.75f, 25f);

            Projectile.frame = 1;

            if (Player.TryGetPlayerFromIndex((int)Projectile.ai[2] - 1, out Player target) && Projectile.position.Y > target.Bottom.Y)
                Projectile.tileCollide = true;
        }

        if (!Main.dedServ && (Projectile.ai[1] > 0f || Projectile.velocity != Vector2.Zero))
        {
            Texture2D texture = TextureAssets.SlimeMount.Value;
            Rectangle frame = texture.Frame(1, 4, 0, Projectile.frame);
            Projectile.SpawnAfterimage(new AfterimageParticle(texture, frame, Projectile.Center, 3, Projectile.rotation, Projectile.scale, Color.White));
            Lighting.AddLight(Projectile.Center, (LegendaryNightHandler.SlimeColor * 1.5f).ToVector3());
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        if (Projectile.ai[1] > 0f && Timer2 > 0)
            DrawWarning();
        // 原地等待期间不显示坐骑；飞向落点的常态坐骑仍可见。
        if (Projectile.ai[1] == 0f && Projectile.velocity == Vector2.Zero)
            return false;

        Texture2D texture = TextureAssets.SlimeMount.Value;
        Rectangle frame = texture.Frame(1, 4, 0, Projectile.frame);
        Main.spriteBatch.DrawFromCenter(texture, Projectile.Center - new Vector2(0f, 6f) - Main.screenPosition, frame, lightColor, Projectile.rotation, Projectile.scale);

        return false;
    }

    private void DrawWarning()
    {
        float progress = MathHelper.Clamp(Timer2 / WarningGrowthTime, 0f, 1f);
        float length = WarningMaxLength * progress;
        Color outer = Color.Lerp(LegendaryNightHandler.SlimeColor, Color.CornflowerBlue,
            (MathF.Sin(Timer2 * 0.12f) + 1f) * 0.5f);
        Color inner = Color.Lerp(outer, Color.White, 0.75f);
        Vector2 position = Projectile.Bottom - new Vector2(0f, 2f);
        AnomalyDrawUtils.DrawLayeredWarning(Main.spriteBatch, position, MathHelper.PiOver2,
            length, 30f, outer, inner);
    }

    public override bool CanHitPlayer(Player target) => Projectile.ai[1] > 0f;

    public override void OnHitPlayer(Player target, Player.HurtInfo info) => Projectile.velocity *= -0.8f;

    public override void OnKill(int timeLeft)
    {
        if (Projectile.ai[1] == 0f && Projectile.velocity == Vector2.Zero)
            return;
        SpawnSlimeMountDust(Projectile.Center, 30, Projectile.velocity * 0.8f);
        SpawnSlimeMountDust(Projectile.Center, 30, Vector2.Zero);
    }

    public static void SpawnSlimeMountDust(Vector2 position, int amount, Vector2 extraVelocity, Action<Dust> action = null)
    {
        if (Main.dedServ)
            return;

        Color newColor = Color.Transparent;

        for (int i = 0; i < amount; i++)
        {
            Dust.NewDustAction(position, 32, 20, DustID.BlueFairy, action: d =>
            {
                d.color = newColor;
                d.scale += Main.rand.NextFloat(-0.10f, 0.2f);
                d.velocity *= 0.5f;
                d.velocity += extraVelocity;
                if (Main.rand.NextBool(2))
                {
                    d.scale *= 1.3f;
                    d.noGravity = true;
                }
                else
                    d.velocity *= 0.7f;

                action?.Invoke(d);
            });
        }
    }
}
