using HarmonyLib;
using Kingmaker.Blueprints.JsonSystem;
using OccultistArcanist.Config;

namespace OccultistArcanist {
    internal class BlueprintInitLoader {

        [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.Init))]
        static class BlueprintsCache_Init_Patch {
            static bool Initialized;

            static void Postfix() {
                if (Initialized) return;
                Initialized = true;


                Changes.MetamagicsUpdate.load();
                Engine.WizardSchools.RegisterSchools.register();

                NewContent.Classes.Arcanist.load();
                NewContent.Classes.Magus.load();
                NewContent.Classes.Shaman.load();
                NewContent.Classes.Wizard.load();
            }
        }
    }
}