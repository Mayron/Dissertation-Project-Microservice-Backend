using System.Collections.Immutable;

namespace OpenSpark.Domain
{
    public static class VisibilityStatus
    {
        public static string Public { get; } = "Public";
        public static string Private { get; } = "Private";
        public static string Unlisted { get; } = "Unlisted";

        private static readonly string[] All = { "Public", "Private", "Unlisted" };

        public static ImmutableArray<string> GetAll() => All.ToImmutableArray();
    }
}