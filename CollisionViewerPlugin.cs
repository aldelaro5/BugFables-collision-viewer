using BepInEx;
using HarmonyLib;

namespace CollisionViewer
{
  [BepInPlugin("com.aldelaro5.BugFables.plugins.CollisionViewer", "Collision Viewer", "2.0.1")]
  [BepInProcess("Bug Fables.exe")]
  public class CollisionViewerPlugin : BaseUnityPlugin
  {
    void Awake()
    {
      var harmony = new Harmony("com.aldelaro5.BugFables.harmony.CollisionViewer");
      harmony.PatchAll();
    }
  }
}
