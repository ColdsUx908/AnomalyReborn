// Developed by ColdsUx

namespace Anomalies.Common;

public sealed partial class AnomalySharedData : ModSystem
{
    public static class QuickAccess
    {
        public const string CalamityModName = "CalamityMod";

        /// <inheritdoc cref="AnomalyUltramundane"/>
        public static bool Ultra => AnomalyUltramundane;

        /// <inheritdoc cref="Aromaly"/>
        public static bool Aroma => Aromaly;

        /// <inheritdoc cref="StoryMode"/>
        public static bool Story => StoryMode;
    }
}