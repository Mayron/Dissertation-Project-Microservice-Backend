using System;
using OpenSpark.Shared.Domain;

namespace OpenSpark.Shared
{
    public static class VisibilityHelper
    {
        public static (bool canConnect, string error) CanProjectConnectToGroup(
            string projectVisibility,
            string groupVisibility)
        {
            if (projectVisibility != VisibilityStatus.Private &&
                groupVisibility == VisibilityStatus.Private)
            {
                var message = projectVisibility == VisibilityStatus.Unlisted ? "an unlisted" : "a public";
                return (false, $"Cannot connect a private group to {message} project.");
            }

            if (projectVisibility == VisibilityStatus.Public &&
                groupVisibility == VisibilityStatus.Unlisted)
            {
                return (false, "Cannot connect an unlisted group to a public project.");
            }

            return (true, null);
        }

        public static string GetCleanVisibility(string visibility)
        {
            if (VisibilityStatus.GetAll().Contains(visibility)) return visibility;

            Console.WriteLine($"Unknown visibility status: {visibility}");
            return VisibilityStatus.Public;
        }
    }
}