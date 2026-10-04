namespace Anomalies.Bosses.Skeletron.SkeletronLegend;

public sealed partial class SkeletronLegendHead : AnomalyNPCBehavior<SkeletronLegendHead>
{
    public void Phase1AI()
    {
        switch (CurrentBaseBehavior)
        {
            case BaseBehavior.None:
            default:
                CurrentBaseBehavior = BaseBehavior.NormalMovement;
                break;
            case BaseBehavior.NormalMovement:
                float maxSpeedX = 20f;
                float maxSpeedY = 8f;
                float accelerationX = 0.2f;
                float accelerationY = 0.08f;
                float targetDistanceAboveTarget = 300f;
                float rotationFactor = 20f;

                SkeletronHead.NormalMovement(NPC, Target, maxSpeedX, maxSpeedY, accelerationX, accelerationY, targetDistanceAboveTarget, rotationFactor);

                Timer1++;

                if (Timer1 % 30 == 0 && Timer1 <= 300)
                    SkeletronHead.FireSkull(NPC, Target, Main.rand.NextBool(3) ? 2 : 1, TOMathUtils.PiOver10, 15f, 10);

                if (Timer1 == 300)
                {
                    SoundEngine.PlaySound(SoundID.ForceRoar, NPC.Center);
                    Projectile.NewProjectileAction<LegendShockwave>(SourceAI, NPC.Center, Vector2.Zero, 100, 0f, action: p =>
                    {
                        p.scale = 0f;
                        p.ai[0] = 1f;
                    });

                    SkeletronHead.FireSkull(NPC, Target, 8, TOMathUtils.PiOver10, 15f, 10, ai0: 1);
                }

                if (Timer1 is 400 or 450 or 500)
                {
                    int num = Timer1 switch
                    {
                        400 => 0,
                        450 => 1,
                        500 => 2,
                        _ => 0
                    };

                    SoundEngine.PlaySound(SoundID.Item81, NPC.Center);

                    for (int i = 0; i <= 300;i++)
                    {
                        Dust.NewDustAction(NPC.Center, 10, 10, DustID.TintableDustLighted, action: d =>
                        {
                            d.scale *= Main.rand.NextFloat(1.5f, 3f);
                            d.color = LegendShockwave.SlimeColor;
                            d.velocity *= 5f;
                            d.fadeIn = 0.5f;
                        });
                    }

                    if (TOSharedData.NotClient)
                    {
                        int delay = 30;
                        float distance = 300f - num * 35f;
                        int amount = 40;

                        float y = Math.Clamp((Target.Center.Y - NPC.Center.Y + Math.Min(Target.velocity.Y, 0f) * delay) * 1.2f, -1000f, 0f) - 160f;
                        Vector2 left = new(-distance * ((amount - 1) / 2f), y);
                        Vector2 interval = new(distance, 0f);
                        for (int i = 0; i < amount; i++)
                        {
                            Vector2 destination = left + interval * i;
                            Vector2 velocity = destination / delay;
                            Projectile.NewProjectileAction<SlimeMount>(SourceAI, NPC.Center, velocity, 10, 0f, action: p => p.ai[0] = delay);
                        }
                    }
                }

                if (Timer1 >= 700)
                {
                    CurrentBaseBehavior = BaseBehavior.RotatingMovement;
                    Timer1 = 0;
                }
                break;
            case BaseBehavior.RotatingMovement:
                switch (CurrentLocalPhase)
                {
                    case 0:
                        SoundEngine.PlaySound(SoundID.ForceRoar, NPC.Center);
                        NPC.Face(Target);
                        CurrentLocalPhase = 1;
                        break;
                    case 1:
                        float rotationSpeed = 0.3f;
                        float moveSpeed = 10f;
                        SkeletronHead.RotatingMovement(NPC, Target, rotationSpeed, moveSpeed);

                        if (++Timer1 >= 480)
                        {
                            CurrentBaseBehavior = BaseBehavior.NormalMovement;
                            CurrentLocalPhase = 0;
                            Timer1 = 0;
                        }
                        break;
                }
                break;
        }
    }
}
