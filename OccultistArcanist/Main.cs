using HarmonyLib;
using OccultistArcanist.ModLogic;
using TabletopTweaks.Core.Utilities;
using UnityModManagerNet;

namespace OccultistArcanist {
    static class Main {
        public static bool Enabled;
        public static ModContextHaddeqiBase HaddeqiModContext;
        static bool Load(UnityModManager.ModEntry modEntry) {
            var harmony = new Harmony(modEntry.Info.Id);
            HaddeqiModContext = new ModContextHaddeqiBase(modEntry);
            HaddeqiModContext.LoadAllSettings();
            HaddeqiModContext.ModEntry.OnSaveGUI = OnSaveGUI;
            HaddeqiModContext.ModEntry.OnGUI = UMMSettingsUI.OnGUI;
            harmony.PatchAll();
            PostPatchInitializer.Initialize(HaddeqiModContext);
            return true;
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry) {
            HaddeqiModContext.SaveAllSettings();
        }
    }
}
