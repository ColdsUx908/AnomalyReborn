namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>
/// 终幕陨星命中的冲击波，成功时仅显示柔和光环。
/// </summary>
/// <remarks>
/// <para>ai[0]、ai[2]：保持0，使用独立寿命计时；ai[1]：最大扩张半径；damage：0为成功演出，1为失败秒杀。</para>
/// <para>localAI：未使用；框架 Timer1..5：未使用；继承共享冲击波协议，无额外载荷。</para>
/// <para>主体死亡后独立完成扩张和失败判定，使用原版绘制入口保持可见。</para>
/// </remarks>
public sealed class LegendFinalShockwave : LegendStellarShockwave
{
    private bool _Hit;
    private bool _ImpactPlayed;
    private bool Success => Projectile.damage == 0;
    protected override bool YellowPalette => true;
    protected override string ExplosionPass => Success ? "CurtainCall" : "Pass0";
    protected override float VisualOpacity => 1f;
    protected override Color OuterColor => Success ? new Color(235, 211, 155, 0) : base.OuterColor;
    protected override Color InnerColor => Success ? new Color(255, 243, 211, 0) : base.InnerColor;
    protected override void UpdateExplosion(int age)
    {
        PlayImpactOnce();
        if (Success || _Hit)
            return;
        Player player = Main.LocalPlayer;
        float radius = Projectile.ai[1] * CompanionShockwaveProgress(age);
        if (player.active && !player.dead && !player.ghost && new Circle(Projectile.Center, radius).Collides(player.Hitbox))
        {
            _Hit = true;
            player.KillMe(PlayerDeathReason.ByCustomReason(Language.GetText(
                "Mods.Anomalies." + LegendaryNightHandler.LocalizationCategory + ".TrialFailedDeath").ToNetworkText(player.name)), SkeletronLegendHead.TrialFailedDamage, 0);
        }

        void PlayImpactOnce()
        {
            if (!_ImpactPlayed)
            {
                _ImpactPlayed = true;
                if (!Success)
                {
                    const int ParticleCount = 320;
                    for (int i = 0; i < ParticleCount; i++)
                    {
                        Color color = i % 3 == 0 ? Color.Gold
                            : Color.Lerp(LegendaryNightHandler.BoneColor, Color.White, Main.rand.NextFloat());
                        ParticleHandler.SpawnParticle(new OrbParticle(Projectile.Center,
                            Main.rand.NextPolarVector2(5f, 35f), Main.rand.Next(40, 85),
                            Main.rand.NextFloat(0.5f, 1.6f), color, lifeEndRatio: 0.2f));
                    }
                }
                SoundEngine.PlaySound(Meteor.ExplosionSound with { Volume = Success ? 0.45f : 1f }, Projectile.Center);
                Main.LocalPlayer.SetScreenshake(Success ? 3f : 18f);
            }
        }
    }
}