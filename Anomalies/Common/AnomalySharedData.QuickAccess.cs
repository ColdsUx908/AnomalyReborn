// Developed by ColdsUx

namespace Anomalies.Common;

public sealed partial class AnomalySharedData : ModSystem
{
    public static class QuickAccess
    {
        /// <summary>
        /// 灾厄模组的名称。
        /// </summary>
        public const string CalamityModName = "CalamityMod";

        /// <summary>
        /// 灾厄模组是否已加载。
        /// </summary>
        public static bool CalamityEnabled => ModReference.Calamity.IsLoaded;

        /// <inheritdoc cref="AnomalyUltramundane"/>
        public static bool Ultra => AnomalyUltramundane;

        /// <inheritdoc cref="Aromaly"/>
        public static bool Aroma => Aromaly;

        /// <inheritdoc cref="StoryMode"/>
        public static bool Story => StoryMode;
    }
}