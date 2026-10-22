namespace Anomalies.SpecialContents.Developer.EventideReunion;

public sealed partial class EventideReunionDeathSkill
{
    private readonly record struct Afterimage(Vector2 Center, float WeaponAngle, float WeaponScale, int Direction,
        Player.CompositeArmData FrontArm, Player.CompositeArmData BackArm, Rectangle BodyFrame, Rectangle LegFrame);
    private readonly Afterimage[] _Afterimages = new Afterimage[3];
    private readonly bool[] _Recalled = new bool[3];
    private int _AfterimageCount;
    private int _RecallAge = -1;

    private void UpdateAfterimages()
    {
        Player player = Projectile.Owner;
        if (IsDashing && Timer % 3 == 0 && _AfterimageCount < _Afterimages.Length)
            _Afterimages[_AfterimageCount++] = new(player.Center, WeaponAngle, WeaponVector.Length(), player.direction,
                player.compositeFrontArm, player.compositeBackArm, player.bodyFrame, player.legFrame);
        if (Phase < DeathPhase.Slash)
            return;
        _RecallAge++;
        for (int i = 0; i < _AfterimageCount; i++)
        {
            if (_Recalled[i] || EventideReunionDeathRecall.Pose(_RecallAge, i).Flight < 1f)
                continue;
            _Recalled[i] = true;
            EmitRecallBurst(i == _AfterimageCount - 1);
        }
    }

    private void DrawAfterimages(Texture2D weapon, Vector2 origin)
    {
        Player player = Projectile.Owner;
        for (int i = 0; i < _AfterimageCount; i++)
        {
            if (_Recalled[i])
                continue;
            Afterimage image = _Afterimages[i];
            (float Raise, float Flight, float Scale) pose = EventideReunionDeathRecall.Pose(_RecallAge, i);
            float scale = pose.Scale;
            if (scale <= 0.001f)
                continue;
            Vector2 center = EventideReunionDeathRecall.Center(image.Center, player.MountedCenter, pose.Flight);
            float upright = -MathHelper.PiOver2 * player.gravDir;
            float angle = image.WeaponAngle + MathHelper.WrapAngle(upright - image.WeaponAngle) * pose.Raise;
            float opacity = (0.35f - i * 0.05f) * (1f - pose.Flight * 0.5f);
            int direction = player.direction;
            int heldProj = player.heldProj;
            Player.CompositeArmData front = player.compositeFrontArm;
            Player.CompositeArmData back = player.compositeBackArm;
            Rectangle body = player.bodyFrame;
            Rectangle leg = player.legFrame;
            try
            {
                player.direction = image.Direction;
                player.heldProj = -1;
                player.compositeFrontArm = image.FrontArm;
                player.compositeBackArm = image.BackArm;
                if (pose.Raise > 0f)
                    player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, angle - MathHelper.PiOver2);
                player.bodyFrame = image.BodyFrame;
                player.legFrame = image.LegFrame;
                // 原版从脚底缩放，补偿脚底到中心的距离，保证最终缩回玩家中心。
                Vector2 drawPosition = center - new Vector2(player.width * 0.5f, player.height * (1f - scale * 0.5f));
                Main.PlayerRenderer.DrawPlayer(Main.Camera, player, drawPosition,
                    0f, player.fullRotationOrigin, 1f - opacity, scale);
            }
            finally
            {
                player.direction = direction;
                player.heldProj = heldProj;
                player.compositeFrontArm = front;
                player.compositeBackArm = back;
                player.bodyFrame = body;
                player.legFrame = leg;
            }
            Main.EntitySpriteDraw(weapon, center - Main.screenPosition, null,
                new Color(105, 60, 150) * opacity, angle + MathHelper.PiOver4, origin,
                image.WeaponScale * scale, SpriteEffects.None);
        }
    }

    private void EmitRecallBurst(bool final)
    {
        Vector2 center = Projectile.Owner.MountedCenter;
        int count = final ? 28 : 12;
        for (int i = 0; i < count; i++)
        {
            Vector2 motion = (MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.1f, 0.1f)).ToRotationVector2()
                * Main.rand.NextFloat(2f, final ? 10f : 5f);
            Color purple = Color.Lerp(new Color(80, 20, 145), new Color(200, 120, 255), Main.rand.NextFloat());
            ParticleHandler.SpawnParticle(new PointingParticle(center, motion, false, Main.rand.Next(18, 32),
                Main.rand.NextFloat(0.25f, 0.6f), purple));
            if (i % 3 == 0)
                ParticleHandler.SpawnParticle(new GenericSparkle(center, motion * 0.45f, new Color(220, 190, 255),
                    purple, final ? 0.5f : 0.3f, 26, bloomScale: 1.4f));
            Dust.NewDustPerfectAction(center, DustID.Smoke, d =>
            {
                d.noGravity = true;
                d.color = new Color(25, 5, 38);
                d.velocity = motion * 0.4f;
                d.scale = 0.9f;
            });
        }
        ParticleHandler.SpawnParticle(new BloomParticle(center, Vector2.Zero, new Color(110, 30, 190),
            0.1f, final ? 0.9f : 0.4f, 18, lifeEndRatio: 0.05f));
    }
}
