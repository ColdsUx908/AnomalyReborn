using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Transoceanic.Runtime.Reflection;

namespace Anomalies.Assets.Effects;

/// <summary>通过原生加载入口注册本模组着色器；配置模板与筛选器引用由 TOCleanup 清理。</summary>
public sealed class AnomalyEffects : ILoadable
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
    /// <summary>在所属模组资产仓库就绪后加载自身着色器；服务器及关闭着色器时跳过。</summary>
    /// <param name="mod">由 tML 提供的所属模组，早于 AnomalyMain.Load 可安全使用。</param>
    void ILoadable.Load(Mod mod)
    {
        TOCleanup.Register(ClearReferences);
        if (Main.dedServ || !AnomalyClientConfig.Instance.EnableShaders) //配置选项关闭时着色器不会加载
            return;

        HandleType(typeof(AnomalyEffects), true);
        HandleType(typeof(BossBars));
        HandleType(typeof(Overlays));
        HandleType(typeof(Primitives));
        HandleType(typeof(Texts));
        ScreenFilters.BrainOfCthulhuForcefield = LoadAndRegisterScreenFilter(mod, "BrainOfCthulhuForcefield", "Pass0");
        ScreenFilters.RadialBlur = LoadAndRegisterScreenFilter(mod, "RadialBlur", "Pass0");

        void HandleType(Type type, bool isRoot = false)
        {
            foreach (FieldInfo field in type.GetFields(TOReflectionUtils.StaticBindingFlags))
                field.SetValue(null, LoadAndRegisterMiscShader(mod, isRoot ? "" : type.Name, field.Name, field.GetCustomAttribute<ShaderPassAttribute>()?.Name ?? "Pass0"));
        }
    }

    // 资产仓库由 tML 管理，自有容器与筛选器状态由 TOCleanup 清理。
    void ILoadable.Unload() { }

    private static void ClearReferences()
    {
        // 每个筛选器分别登记失活，单项失败不会阻止此处释放其余容器引用。
        ClearType(typeof(ScreenFilters));
        ClearType(typeof(AnomalyEffects));
        ClearType(typeof(BossBars));
        ClearType(typeof(Overlays));
        ClearType(typeof(Primitives));
        ClearType(typeof(Texts));
    }

    private static Filter LoadAndRegisterScreenFilter(Mod mod, string name, string passName)
    {
        Asset<Effect> asset = mod.Assets.Request<Effect>($"Assets/Effects/ScreenFilters/{name}");
        Filter filter = new(new ScreenShaderData(asset, passName), EffectPriority.VeryHigh);
        // 必须先于 Load 登记：Load 失败时尚未返回给 ScreenFilters 字段。
        TOCleanup.Register(() => filter.Deactivate());
        Filters.Scene[$"Anomalies:{name}"] = filter;
        filter.Load();
        return filter;
    }

    private static MiscShaderContainer LoadAndRegisterMiscShader(Mod mod, string subDirectory, string registrationName, string passName = "Pass0")
    {
        string path = "Assets/Effects/";
        if (!string.IsNullOrEmpty(subDirectory))
            path += subDirectory + "/";
        path += registrationName;
        Asset<Effect> shader = mod.Assets.Request<Effect>(path);
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
