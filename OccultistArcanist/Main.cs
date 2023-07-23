using HarmonyLib;
using Kingmaker.Blueprints.Classes;
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

        internal static bool IsHomebrewArchetypesEnabled() //
        {
            var duplicate_archetype = BlueprintTools.GetBlueprint<BlueprintArchetype>("23d88af5a9470b845b893b31895b20c9");
            return !(duplicate_archetype is null);
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry) {
            HaddeqiModContext.SaveAllSettings();
        }
    }
}
