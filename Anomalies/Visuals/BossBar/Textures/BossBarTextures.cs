namespace Anomalies.Visuals.BossBar;

public sealed class BossBarTextures : ILoadable
{
    public const string Path = "Anomalies/Visuals/BossBar/Textures/";

    internal static Asset<DynamicSpriteFont> _BigLifeFont;
    /// <summary>
    /// 血条百分比数字所用字体。
    /// </summary>
    public static DynamicSpriteFont BigLifeFont => _BigLifeFont.Value;

    public static class Fancy
    {
        public const string Path = BossBarTextures.Path + "Fancy/";

        [LoadTexture(Path + "BaseBar")]
        internal static Asset<Texture2D> _BaseBar;
        /// <summary>
        /// 基础血量条纹理。
        /// </summary>
        public static Texture2D BaseBar => _BaseBar.Value;

        [LoadTexture(Path + "BaseBarFiller")]
        internal static Asset<Texture2D> _BaseBarFiller;
        /// <summary>
        /// 血量条填充背景纹理。
        /// </summary>
        public static Texture2D BaseBarFiller => _BaseBarFiller.Value;

        [LoadTexture(Path + "BaseBarSmall")]
        internal static Asset<Texture2D> _BaseBarSmall;
        /// <summary>
        /// 小尺寸基础血量条纹理。
        /// </summary>
        public static Texture2D BaseBarSmall => _BaseBarSmall.Value;

        [LoadTexture(Path + "BaseBarFillerSmall")]
        internal static Asset<Texture2D> _BaseBarFillerSmall;
        /// <summary>
        /// 小尺寸血量条填充背景纹理。
        /// </summary>
        public static Texture2D BaseBarFillerSmall => _BaseBarFillerSmall.Value;

        [LoadTexture(Path + "DefaultHealthBar")]
        /// <summary>
        /// 默认血条纹理。
        /// </summary>
        internal static Asset<Texture2D> _DefaultHealthBar;
        public static Texture2D DefaultHealthBar => _DefaultHealthBar?.Value;

        [LoadTexture(Path + "PhaseIndicator")]
        internal static Asset<Texture2D> _PhaseIndicator;
        /// <summary>
        /// 阶段血量阈值指示器纹理。
        /// </summary>
        public static Texture2D PhaseIndicator => _PhaseIndicator?.Value;

        [LoadTexture(Path + "SubPhaseIndicator")]
        internal static Asset<Texture2D> _SubPhaseIndicator;
        /// <summary>
        /// 亚阶段血量阈值指示器纹理。
        /// </summary>
        public static Texture2D SubPhaseIndicator => _SubPhaseIndicator?.Value;

        [LoadTexture(Path + "PhaseIndicatorCenter")]
        internal static Asset<Texture2D> _PhaseIndicatorCenter;
        /// <summary>
        /// 阶段血量阈值指示器中心纹理。
        /// </summary>
        public static Texture2D PhaseIndicatorCenter => _PhaseIndicatorCenter?.Value;

        [LoadTexture(Path + "SubPhaseIndicatorCenter")]
        internal static Asset<Texture2D> _SubPhaseIndicatorCenter;
        /// <summary>
        /// 亚阶段血量阈值指示器中心纹理。
        /// </summary>
        public static Texture2D SubPhaseIndicatorCenter => _SubPhaseIndicatorCenter?.Value;

        [LoadTexture(Path + "BarLock")]
        internal static Asset<Texture2D> _BarLock;
        /// <summary>
        /// 血条锁纹理。
        /// </summary>
        public static Texture2D BarLock => _BarLock?.Value;
    }

    public static class Retro
    {
        public const string Path = BossBarTextures.Path + "Retro/";

        [LoadTexture(Path + "MainBar")]
        internal static Asset<Texture2D> _MainBar;
        /// <summary>
        /// 复古主血条纹理。
        /// </summary>
        public static Texture2D MainBar => _MainBar.Value;

        [LoadTexture(Path + "ComboBar")]
        internal static Asset<Texture2D> _ComboBar;
        /// <summary>
        /// 复古连击残影条纹理。
        /// </summary>
        public static Texture2D ComboBar => _ComboBar.Value;

        [LoadTexture(Path + "SeperatorBar")]
        internal static Asset<Texture2D> _SeperatorBar;
        /// <summary>
        /// 复古分隔条纹理。
        /// </summary>
        public static Texture2D SeperatorBar => _SeperatorBar.Value;
    }

    /// <summary>在 tML 内容加载时请求本模组字体，不依赖实体 ID 或其他加载器。</summary>
    /// <param name="mod">由 tML 提供的所属模组。</param>
    void ILoadable.Load(Mod mod)
    {
        TOCleanup.Register(() => _BigLifeFont = null);
        if (!Main.dedServ)
            _BigLifeFont = mod.Assets.Request<DynamicSpriteFont>("Visuals/BossBar/Textures/BigLifeFont");
    }

    // 资产引用由 TOCleanup 清空，纹理由 tML 资产仓库管理。
    void ILoadable.Unload() { }
}
