namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(Regex regex)
    {
        public bool TryMatch(string input, [NotNullWhen(true)] out Match match)
        {
            ArgumentNullException.ThrowIfNull(input);
            return (match = regex.Match(input)).Success;
        }
    }

    extension(Regex)
    {
        public static bool TryMatch(string input, string pattern, [NotNullWhen(true)] out Match match)
        {
            ArgumentNullException.ThrowIfNull(input);
            ArgumentNullException.ThrowIfNull(pattern);
            return (match = Regex.Match(input, pattern)).Success;
        }
    }
}