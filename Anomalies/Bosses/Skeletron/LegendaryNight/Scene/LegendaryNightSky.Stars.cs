namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class LegendaryNightSky
{
    public sealed class Star
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Rotation;
        public float Scale;

        public Star(Vector2 position, Vector2 velocity, float rotation, float scale)
        {
            Position = position;
            Velocity = velocity;
            Rotation = rotation;
            Scale = scale;
        }

        public void Update(float intensity)
        {
            Position += Velocity;
            Rotation += 0.1f * (Velocity.X >= 0).ToDirectionInt();

            if (intensity < 1f)
                Scale *= intensity;
        }

        public void Draw(Color trailColor) => LegendaryNightStarDrawing.DrawFallingStar(Position + Main.screenPosition, Rotation, Scale, Velocity, 1, true, trailColorOverride: trailColor);
    }

    public sealed class RisingStarlight
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Scale;
        public float Phase;
        public int Age;
        public int LifeTime;

        public void Update()
        {
            Age++;
            Position += Velocity + new Vector2(MathF.Sin(Age * 0.03f + Phase) * 0.35f, 0f);
        }

        public void Draw(float skyIntensity)
        {
            Texture2D sparkle = AnomalyTextures.Extra.CrystalTextSparkle;
            float fade = MathHelper.Clamp(Age / 30f, 0f, 1f) * MathHelper.Clamp((LifeTime - Age) / 45f, 0f, 1f);
            float pulse = 0.75f + 0.25f * MathF.Sin(Age * 0.08f + Phase);
            float opacity = fade * pulse * skyIntensity;
            Color gold = (Color.Gold with { A = 0 }) * (opacity * 0.5f);
            Color white = (Color.LightGoldenrodYellow with { A = 0 }) * (opacity * 0.7f);
            float rotation = Age * 0.01f + Phase;
            // 参照 Calamity 特殊字体星光：两层旋转星芒，细长外辉与明亮内芯。
            Main.spriteBatch.Draw(sparkle, Position, null, gold, rotation, sparkle.Size() * 0.5f,
                new Vector2(Scale * 0.65f, Scale * 1.8f), SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(sparkle, Position, null, white, rotation, sparkle.Size() * 0.5f,
                Scale * 0.5f, SpriteEffects.None, 0f);
        }
    }
}