namespace Anomalies.Visuals.MainMenu;

public sealed class AnomaliesMenuBackground : ModSurfaceBackgroundStyle
{
    public override void ModifyFarFades(float[] fades, float transitionSpeed)
    {
        for (int i = 0; i < fades.Length; i++)
            fades[i] = MathHelper.Clamp(fades[i] + (i == Slot ? transitionSpeed : -transitionSpeed), 0f, 1f);
    }

    // BackgroundTextureLoader treats negative slots as disabled layers.
    public override int ChooseFarTexture() => -1;
    public override int ChooseMiddleTexture() => -1;
    public override int ChooseCloseTexture(ref float scale, ref double parallax, ref float a, ref float b) => -1;
    public override bool PreDrawCloseBackground(SpriteBatch spriteBatch) => false;
}
