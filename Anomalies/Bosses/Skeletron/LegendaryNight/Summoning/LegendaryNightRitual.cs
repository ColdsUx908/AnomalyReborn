namespace Anomalies.Bosses.Skeletron.LegendaryNight.Summoning;

using Anomalies.Bosses.Skeletron.LegendaryNight.Arena;

public sealed class LegendaryNightRitual : AnomalyModProjectile
{
    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;

    public const int RitualTime = 360;
    public const int FrameCount = 11;

    public Point AltarTopLeftCoordinate;

    public Vector2 RitualCenter => new Point(AltarTopLeftCoordinate.X, AltarTopLeftCoordinate.Y).ToWorldCoordinates() + new Vector2(32f, -20f);

    public bool IsSummoningSkeletron;
    public bool HasFinishedSummoning => Timer1 >= RitualTime;

    public int SummonedSkeletronHeadIndex = -1;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;
        Main.projFrames[Type] = FrameCount;
    }

    public override void SetDefaults()
    {
        Projectile.netImportant = true;
        Projectile.width = Projectile.height = 2;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = RitualTime;
        Projectile.scale = 1.5f;
    }

    public override void AI()
    {
        Timer1++;
        Projectile.timeLeft = RitualTime;

        if (HasFinishedSummoning)
        {
            if (NPC.TryGetNPCFromIndex(SummonedSkeletronHeadIndex, out NPC skeletronHead) && skeletronHead.active && skeletronHead.type == NPCID.SkeletronHead)
            {
            }
            else if (TOSharedData.NotClient)
                Projectile.Kill();

            Projectile.Center = RitualCenter + new Vector2(0f, 8f * MathF.Sin((Timer1 - RitualTime) / 60f)); //上下摆动
        }

        Projectile.frame = Math.Clamp(Timer1 - 18 / 2, 0, FrameCount - 1);
        if (Projectile.frame != 0)
            Lighting.AddLight(Projectile.Center, LegendaryNightHandler.StarColor.ToVector3() * 1.5f);

        #region 召唤仪式
        switch (Timer1)
        {
            case > 50 and <= 170: //向上移动
                float interpolation = TOMathUtils.Interpolation.QuadraticEaseInOut((Timer1 - 50) / 120f);
                Projectile.Center = RitualCenter - new Vector2(0f, 180f * interpolation);
                break;
            case >= 180 and <= 270: //粒子效果
                Vector2 dustVelocity = Main.rand.NextPolarVector2(3f, 5.5f);
                ParticleHandler.SpawnParticle(new SquashParticle(Projectile.Center - dustVelocity * Main.rand.NextFloat(25, 30), dustVelocity, 20, Main.rand.NextFloat(1.2f, 1.8f), Color.Lerp(LegendaryNightHandler.StarColor, Color.White, Main.rand.NextFloat(0.3f)), baseSize: null, noGravity: true) { FadeIn = 0.66f });

                if (Timer1 == 220)
                {
                    ParticleHandler.SpawnParticle(new BloomParticle(Projectile.Center, Vector2.Zero, Color.LightGoldenrodYellow, 0f, 1f, 75, 0.9f));
                    ParticleHandler.SpawnParticle(new BloomParticle(Projectile.Center, Vector2.Zero, LegendaryNightHandler.StarColor, 0f, 0.7f, 75, 0.9f));
                    ParticleHandler.SpawnParticle(new BloomParticle(Projectile.Center, Vector2.Zero, Color.White, 0f, 0.53f, 75, 0.9f));
                }

                if (Timer1 == 265)
                {
                    ParticleHandler.SpawnParticle(new BloomParticle(Projectile.Center, Vector2.Zero, Color.LightGoldenrodYellow, 0f, 3.5f, 75, 0.8f));
                    ParticleHandler.SpawnParticle(new BloomParticle(Projectile.Center, Vector2.Zero, LegendaryNightHandler.StarColor, 0f, 2.5f, 75, 0.8f));
                    ParticleHandler.SpawnParticle(new BloomParticle(Projectile.Center, Vector2.Zero, Color.White, 0f, 1.8f, 75, 0.8f));
                }
                break;
            case 300: //召唤骷髅王和竞技场
                IsSummoningSkeletron = true;

                SoundEngine.PlaySound(SoundID.ForceRoar);

                for (int i = 0; i < 100; i++)
                    ParticleHandler.SpawnParticle(new OrbParticle(Projectile.Center, Main.rand.NextPolarVector2(15f, 40f), Main.rand.Next(40, 60), Main.rand.NextFloat(1f, 1.6f), Color.Lerp(LegendaryNightHandler.StarColor, Color.White, Main.rand.NextFloat(0.5f))));

                if (TOSharedData.NotClient)
                {
                    TOLocalizationUtils.ChatLocalizedText(AnomalySharedData.ModLocalizationPrefix + LocalizationCategory + ".Summon", LegendaryNightHandler.StarColor);

                    NPC.NewNPCAction(new EntitySource_BossSpawn(Projectile.FindTargetWithinRange(2000f)), Projectile.Center, NPCID.SkeletronHead, action: n =>
                    {
                        n.Center = Projectile.Center;
                        n.netUpdate = true;
                        SummonedSkeletronHeadIndex = n.whoAmI;
                        n.Anomaly.CustomAIType = 1;
                        Projectile.NewProjectileAction<LegendaryNightArena>(SourceAI, n.Center, Vector2.Zero, SkeletronLegendHead.ArenaDamage, 0f, action: p =>
                        {
                            p.GetModProjectile<LegendaryNightArena>().Master = n;
                            SkeletronLegendHead.GetInstance(n).ArenaProjectile = p;
                        });
                    });
                }
                
                if (TOSharedData.NotClient)
                    Projectile.netUpdate = true;
                IsSummoningSkeletron = false;
                break;
            case > 300 and <= 360: //向下移动
                float interpolation2 = TOMathUtils.Interpolation.QuadraticEaseInOut((Timer1 - 300) / 60f);
                Projectile.Center = RitualCenter - new Vector2(0f, 180f * (1f - interpolation2));
                break;
        }
        #endregion 召唤仪式
    }

    protected override void SendAnomalyExtraAI(BinaryWriter writer)
    {
        base.SendAnomalyExtraAI(writer);
        writer.Write(AltarTopLeftCoordinate.X);
        writer.Write(AltarTopLeftCoordinate.Y);
        writer.Write(SummonedSkeletronHeadIndex);
        writer.Write(IsSummoningSkeletron);
    }

    protected override void ReceiveAnomalyExtraAI(BinaryReader reader)
    {
        base.ReceiveAnomalyExtraAI(reader);
        AltarTopLeftCoordinate = new Point(reader.ReadInt32(), reader.ReadInt32());
        SummonedSkeletronHeadIndex = reader.ReadInt32();
        IsSummoningSkeletron = reader.ReadBoolean();
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Main.spriteBatch.DrawFromCenter(Projectile.Texture, Projectile.Center - Main.screenPosition, Projectile.Texture.Frame(1, FrameCount, 0, Projectile.frame), Color.White, Projectile.rotation, Projectile.scale);
        return false;
    }

    public override void OnKill(int timeLeft)
    {
        for (int i = 0; i < 50; i++)
            ParticleHandler.SpawnParticle(new OrbParticle(Projectile.Center, Main.rand.NextPolarVector2(10f, 25f), Main.rand.Next(20, 40), Main.rand.NextFloat(0.5f, 1f), Color.Lerp(LegendaryNightHandler.StarColor, Color.White, Main.rand.NextFloat(0.5f))));

    }
}
