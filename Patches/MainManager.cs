using HarmonyLib;

namespace CollisionViewer.Patches
{
  [HarmonyPatch(typeof(MainManager), "Start")]
  public class MainManagerPatch
  {
    static bool Prefix(MainManager __instance)
    {
      __instance.gameObject.AddComponent<CollisionViewerManager>();
      __instance.gameObject.AddComponent<DisplayPlayerDirection>();

      return true;
    }
  }
}
