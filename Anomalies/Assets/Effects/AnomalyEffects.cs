using Terraria.Graphics.Shaders;

namespace Anomalies.Assets.Effects;

public sealed class AnomalyEffects : IContentLoader
{
    public sealed record MiscShaderContainer(Asset<Effect> Asset, MiscShaderData Data);

    public static MiscShaderContainer SolidColor;
    public static MiscShaderContainer SolidColorMask;

    public static class BossBars
    {
        public static MiscShaderContainer EaterofWorlds;
        public static MiscShaderContainer EyeofCthulhu;
        public static MiscShaderContainer KingSlime;
        public static MiscShaderContainer QueenBee;
        public static MiscShaderContainer QueenSlime;
        public static MiscShaderContainer RainbowJewel;
        public static MiscShaderContainer Skeletron;
    }

    public static class Texts
    {
        public static MiscShaderContainer AnomalyTitle;
        public static MiscShaderContainer AnomalyUltramundaneTitle;
        public static MiscShaderContainer EventideReunion;
    }

    #region 处理逻辑
    void IContentLoader.PostSetupContent()
    {
        if (!AnomalyClientConfig.Instance.EnableShaders) //配置选项关闭时着色器不会加载
            return;

        HandleType(typeof(AnomalyEffects), true);
        HandleType(typeof(BossBars));
        HandleType(typeof(Texts));

        void HandleType(Type type, bool isRoot = false)
        {
            foreach (FieldInfo field in type.GetFields(TOReflectionUtils.StaticBindingFlags))
                field.SetValue(null, LoadAndRegisterMiscShader(isRoot ? "" : type.Name, field.Name));
        }
    }

    void IContentLoader.OnModUnload()
    {
        ClearType(typeof(AnomalyEffects));
        ClearType(typeof(BossBars));
        ClearType(typeof(Texts));
    }

    private static MiscShaderContainer LoadAndRegisterMiscShader(string subDirectory, string registrationName, string passName = "Pass0")
    {
        string path = "Assets/Effects/";
        if (!string.IsNullOrEmpty(subDirectory))
            path += subDirectory + "/";
        path += registrationName;
        Asset<Effect> shader = AnomalyMain.Instance.Assets.Request<Effect>(path);
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
