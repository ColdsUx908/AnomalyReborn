using Transoceanic.DataStructures;

namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(UnifiedRandom rand)
    {
        public float NextRadian() => rand.NextFloat(MathHelper.TwoPi);

        public PolarVector2 NextPolarVector2(float length) => new(length, rand.NextRadian());

        public PolarVector2 NextPolarVector2(float minLength, float maxLength) => new(rand.NextFloat(minLength, maxLength), rand.NextRadian());

        public bool NextProbability(float probability) => rand.NextFloat() < probability;
    }
}
