using System.Collections;

namespace SoVUtilities;

internal static class IExtensions
{
    public static void Start(this IEnumerator routine)
    {
        Core.StartCoroutine(routine);
    }
}
