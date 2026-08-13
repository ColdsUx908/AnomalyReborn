// Developed by ColdsUx

namespace Anomalies.Assets;

public sealed class AnomalyTextures
{
    public const string TexturePathPrefix = "Anomalies/Assets/Textures/";

    public static class Extra
    {
        public const string Path = TexturePathPrefix + "Extra/";

        [LoadTexture(Path + "BasicCircle")]
        internal static Asset<Texture2D> _BasicCircle;
        public static Texture2D BasicCircle => _BasicCircle?.Value;

        [LoadTexture(Path + "CrystalTextGlow")]
        internal static Asset<Texture2D> _CrystalTextGlow;
        public static Texture2D CrystalTextGlow => _CrystalTextGlow?.Value;

        [LoadTexture(Path + "CrystalTextSparkle")]
        internal static Asset<Texture2D> _CrystalTextSparkle;
        public static Texture2D CrystalTextSparkle => _CrystalTextSparkle?.Value;
    }


    public static class Noise
    {
        public const string Path = TexturePathPrefix + "Noise/";
        [LoadTexture(Path + "Melt")]
        internal static Asset<Texture2D> _Melt;
        public static Texture2D Melt => _Melt?.Value;

        [LoadTexture(Path + "Milky")]
        internal static Asset<Texture2D> _Milky;
        public static Texture2D Milky => _Milky?.Value;

        [LoadTexture(Path + "Perlin")]
        internal static Asset<Texture2D> _Perlin;
        public static Texture2D Perlin => _Perlin?.Value;

        [LoadTexture(Path + "Smear")]
        internal static Asset<Texture2D> _Smear;
        public static Texture2D Smear => _Smear?.Value;

        [LoadTexture(Path + "Turbulence")]
        internal static Asset<Texture2D> _Turbulence;
        public static Texture2D Turbulence => _Turbulence?.Value;

        [LoadTexture(Path + "Vein")]
        internal static Asset<Texture2D> _Vein;
        public static Texture2D Vein => _Vein?.Value;
    }

    public static class Touhou
    {
        public const string Path = TexturePathPrefix + "Touhou/";

        [LoadTexture(Path + "Ice1")]
        internal static Asset<Texture2D> _Ice1;
        public static Texture2D Ice1 => _Ice1?.Value;

        [LoadTexture(Path + "Ice2")]
        internal static Asset<Texture2D> _Ice2;
        public static Texture2D Ice2 => _Ice2?.Value;

        [LoadTexture(Path + "Ice3")]
        internal static Asset<Texture2D> _Ice3;
        public static Texture2D Ice3 => _Ice3?.Value;

        internal static Asset<Texture2D> _Ice4;
        public static Texture2D Ice4 => _Ice4?.Value;

        internal static Asset<Texture2D> _Ice5;
        public static Texture2D Ice5 => _Ice5?.Value;

        [LoadTexture(Path + "Ice6")]
        internal static Asset<Texture2D> _Ice6;
        public static Texture2D Ice6 => _Ice6?.Value;

        [LoadTexture(Path + "Ice7")]
        internal static Asset<Texture2D> _Ice7;
        public static Texture2D Ice7 => _Ice7?.Value;

        [LoadTexture(Path + "Ice8")]
        internal static Asset<Texture2D> _Ice8;
        public static Texture2D Ice8 => _Ice8?.Value;

        [LoadTexture(Path + "Scale1")]
        internal static Asset<Texture2D> _Scale1;
        public static Texture2D Scale1 => _Scale1?.Value;

        [LoadTexture(Path + "Scale2")]
        internal static Asset<Texture2D> _Scale2;
        public static Texture2D Scale2 => _Scale2?.Value;
    }
}
