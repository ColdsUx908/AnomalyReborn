namespace Transoceanic.Framework.Publicizers.Terraria;

#pragma warning disable IDE1006

public record Main_Publicizer(Main Source) : Publicizer<Main>(Source)
{
    // _currentGameModInfo (static field)
    public static readonly FieldInfo s_f__currentGameModInfo = GetStaticField("_currentGameModeInfo");

    public static GameModeData _currentGameModeInfo
    {
        get => (GameModeData)s_f__currentGameModInfo.GetValue(null);
        set => s_f__currentGameModInfo.SetValue(null, value);
    }
}
