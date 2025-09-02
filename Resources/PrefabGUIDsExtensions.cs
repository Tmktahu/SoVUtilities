using Stunlock.Core;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace SoVUtilities.Resources
{
  /// <summary>
  /// Extension methods for working with the PrefabGUIDs class
  /// </summary>
  public static class PrefabGUIDsExtensions
  {
    /// <summary>
    /// Gets all static readonly PrefabGUID fields defined in the PrefabGUIDs class
    /// </summary>
    public static IEnumerable<PrefabGUID> GetAllPrefabGuids()
    {
      return typeof(PrefabGUIDs)
          .GetFields(BindingFlags.Public | BindingFlags.Static)
          .Where(f => f.FieldType == typeof(PrefabGUID) && f.IsInitOnly)
          .Select(f => (PrefabGUID)f.GetValue(null));
    }

    /// <summary>
    /// Gets all static readonly PrefabGUID fields defined in the PrefabGUIDs class as a dictionary
    /// </summary>
    public static Dictionary<string, PrefabGUID> GetAllPrefabGuidsAsDictionary()
    {
      var fieldInfos = typeof(PrefabGUIDs)
          .GetFields(BindingFlags.Public | BindingFlags.Static)
          .Where(f => f.FieldType == typeof(PrefabGUID) && f.IsInitOnly);

      var result = new Dictionary<string, PrefabGUID>();
      foreach (var fieldInfo in fieldInfos)
      {
        result[fieldInfo.Name] = (PrefabGUID)fieldInfo.GetValue(null);
      }

      return result;
    }

    /// <summary>
    /// Finds a PrefabGUID by name (case sensitive)
    /// </summary>
    public static bool TryGetPrefabGUIDByName(string name, out PrefabGUID prefabGUID)
    {
      var fieldInfo = typeof(PrefabGUIDs).GetField(name, BindingFlags.Public | BindingFlags.Static);
      if (fieldInfo != null && fieldInfo.FieldType == typeof(PrefabGUID))
      {
        prefabGUID = (PrefabGUID)fieldInfo.GetValue(null);
        return true;
      }

      prefabGUID = PrefabGUID.Empty;
      return false;
    }

    /// <summary>
    /// Gets the name of a field that contains the specified PrefabGUID value
    /// </summary>
    public static string GetPrefabGUIDName(PrefabGUID prefabGUID)
    {
      var fields = typeof(PrefabGUIDs)
          .GetFields(BindingFlags.Public | BindingFlags.Static)
          .Where(f => f.FieldType == typeof(PrefabGUID) && f.IsInitOnly);

      foreach (var fieldInfo in fields)
      {
        var value = (PrefabGUID)fieldInfo.GetValue(null);
        if (value.GuidHash == prefabGUID.GuidHash)
        {
          return fieldInfo.Name;
        }
      }

      return null;
    }
  }
}
