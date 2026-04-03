namespace Transoceanic.Framework.Publicizers.Terraria.ModLoader.UI;

#pragma warning disable IDE1006
public record UIModItem_Publicizer : IPublicizer
{
    public static Type C_type => field ??= TOReflectionUtils.GetTerrariaType("UIModItem");

    // Draw (static method)
    public static readonly MethodInfo i_m_Draw = PublicizerHandler.GetInstanceMethod(C_type, "Draw");
    public delegate void Orig_Draw(object self, SpriteBatch spriteBatch);
    public static void Draw(object self, SpriteBatch spriteBatch) => i_m_Draw.Invoke(self, [spriteBatch]);
}
