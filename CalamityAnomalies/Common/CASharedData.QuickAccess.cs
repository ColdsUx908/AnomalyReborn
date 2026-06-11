// Developed by ColdsUx

namespace CalamityAnomalies.Common;

public sealed partial class CASharedData
{
    public static class QuickAccess
    {
        /// <inheritdoc cref="AnomalyUltramundane"/>
        public static bool Ultra => AnomalyUltramundane;

        /// <inheritdoc cref="Aromaly"/>
        public static bool Aroma => Aromaly;

        /// <inheritdoc cref="StoryMode"/>
        public static bool Story => StoryMode;
    }
}