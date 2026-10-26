using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Transoceanic.Runtime.Reflection;

namespace Anomalies.Assets.Effects;

public sealed class AnomalyEffects : IContentLoader
{
    //所有嵌套类、字段按字母顺序排列

    public sealed record MiscShaderContainer(Asset<Effect> Asset, MiscShaderData Data);

    [AttributeUsage(AttributeTargets.Field)]
    private sealed class ShaderPassAttribute(string name) : Attribute
    {
        public string Name { get; } = name;
    }

    public static class BossBars
    {
        public static MiscShaderContainer BrainofCthulhu;
        public static MiscShaderContainer EaterofWorlds;
        public static MiscShaderContainer EyeofCthulhu;
        public static MiscShaderContainer KingSlime;
        public static MiscShaderContainer QueenBee;
        public static MiscShaderContainer QueenSlime;
        public static MiscShaderContainer RainbowJewel;
        public static MiscShaderContainer Skeletron;
    }

    public static class Overlays
    {
        public static MiscShaderContainer EventideDeathVignette;
        public static MiscShaderContainer LegendaryNightOverlay;
        public static MiscShaderContainer SolidColor;
        public static MiscShaderContainer SolidColorMask;
    }

    public static class Primitives
    {
        public static MiscShaderContainer CircularAoETelegraph;
        public static MiscShaderContainer EventideDeathMark;
        public static MiscShaderContainer EventideDeathSweep;
        public static MiscShaderContainer EventideSamsaraMark;
        public static MiscShaderContainer EventideSamsaraThread;
        public static MiscShaderContainer EventideSweep;
        public static MiscShaderContainer GlowCircle;
        public static MiscShaderContainer GlowLine;
    }

    public static class Texts
    {
        public static MiscShaderContainer AnomalyTitle;
        public static MiscShaderContainer AnomalyUltramundaneTitle;
        public static MiscShaderContainer EventideReunion;
        public static MiscShaderContainer LegendaryNightGold;
    }

    public static class ScreenFilters
    {
        public static Filter BrainOfCthulhuForcefield;
        public static Filter RadialBlur;
    }

    #region 处理逻辑
    void IContentLoader.PostSetupContent()
    {
        if (Main.dedServ || !AnomalyClientConfig.Instance.EnableShaders) //配置选项关闭时着色器不会加载
            return;

        HandleType(typeof(AnomalyEffects), true);
        HandleType(typeof(BossBars));
        HandleType(typeof(Overlays));
        HandleType(typeof(Primitives));
        HandleType(typeof(Texts));
        ScreenFilters.BrainOfCthulhuForcefield = LoadAndRegisterScreenFilter("BrainOfCthulhuForcefield", "Pass0");
        ScreenFilters.RadialBlur = LoadAndRegisterScreenFilter("RadialBlur", "Pass0");

        void HandleType(Type type, bool isRoot = false)
        {
            foreach (FieldInfo field in type.GetFields(TOReflectionUtils.StaticBindingFlags))
                field.SetValue(null, LoadAndRegisterMiscShader(isRoot ? "" : type.Name, field.Name, field.GetCustomAttribute<ShaderPassAttribute>()?.Name ?? "Pass0"));
        }
    }

    void IContentLoader.OnModUnload()
    {
        if (!Main.dedServ)
        {
            ScreenFilters.BrainOfCthulhuForcefield?.Deactivate();
            ScreenFilters.RadialBlur?.Deactivate();
        }
        ClearType(typeof(ScreenFilters));
        ClearType(typeof(AnomalyEffects));
        ClearType(typeof(BossBars));
        ClearType(typeof(Overlays));
        ClearType(typeof(Primitives));
        ClearType(typeof(Texts));
    }

    private static Filter LoadAndRegisterScreenFilter(string name, string passName)
    {
        Asset<Effect> asset = AnomalyMain.Instance.Assets.Request<Effect>($"Assets/Effects/ScreenFilters/{name}");
        Filter filter = new(new ScreenShaderData(asset, passName), EffectPriority.VeryHigh);
        Filters.Scene[$"Anomalies:{name}"] = filter;
        filter.Load();
        return filter;
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
