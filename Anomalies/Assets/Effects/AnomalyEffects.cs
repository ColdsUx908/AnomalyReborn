using Terraria.Graphics.Shaders;

namespace Anomalies.Assets.Effects;

public sealed class AnomalyEffects : IContentLoader
{
    public sealed record MiscShaderContainer(Asset<Effect> Asset, MiscShaderData Data);

    public static MiscShaderContainer LightingShader;

    public static class CustomBossBars
    {
        public static MiscShaderContainer EyeofCthulhu;
        public static MiscShaderContainer KingSlime;
        public static MiscShaderContainer QueenBee;
        public static MiscShaderContainer QueenSlime;
        public static MiscShaderContainer RainbowJewel;
    }

    #region 处理逻辑
    void IContentLoader.PostSetupContent()
    {
        LightingShader = LoadAndRegisterMiscShader("", nameof(LightingShader));

        CustomBossBars.EyeofCthulhu = LoadAndRegisterMiscShader(nameof(CustomBossBars), nameof(CustomBossBars.EyeofCthulhu));
        CustomBossBars.KingSlime = LoadAndRegisterMiscShader(nameof(CustomBossBars), nameof(CustomBossBars.KingSlime));
        CustomBossBars.QueenBee = LoadAndRegisterMiscShader(nameof(CustomBossBars), nameof(CustomBossBars.QueenBee));
        CustomBossBars.QueenSlime = LoadAndRegisterMiscShader(nameof(CustomBossBars), nameof(CustomBossBars.QueenSlime));
        CustomBossBars.RainbowJewel = LoadAndRegisterMiscShader(nameof(CustomBossBars), nameof(CustomBossBars.RainbowJewel));
    }

    void IContentLoader.OnModUnload()
    {
        ClearType(typeof(AnomalyEffects));
    }

    private static MiscShaderContainer LoadAndRegisterMiscShader(string subDirectory, string registrationName, string passName = "Pass0")
    {
        AssetRepository assets = AnomalyMain.Instance.Assets;

        string path = "Assets/Effects/";
        if (!string.IsNullOrEmpty(subDirectory))
            path += subDirectory + "/";
        path += registrationName;
        Asset<Effect> shader = assets.Request<Effect>(path);
        MiscShaderData data = new(shader, passName);
        GameShaders.Misc[$"Anomalies:{registrationName}"] = data;

        return new MiscShaderContainer(shader, data);
    }

    private static void ClearType(Type type)
    {
        foreach (FieldInfo field in type.GetFields(TOReflectionUtils.StaticBindingFlags))
            field.SetValue(null, null);
    }
    #endregion 处理逻辑
}
