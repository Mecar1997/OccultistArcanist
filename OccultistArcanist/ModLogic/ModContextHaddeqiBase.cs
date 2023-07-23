using Kingmaker.Blueprints;
using OccultistArcanist.Config;
using TabletopTweaks.Core.ModLogic;
using TabletopTweaks.Core.Utilities;
using static UnityModManagerNet.UnityModManager;

namespace OccultistArcanist.ModLogic {
    internal class ModContextHaddeqiBase : ModContextBase {
        public AddedContent AddedContent;
        public Fixes Fixes;

        public ModContextHaddeqiBase(ModEntry ModEntry) : base(ModEntry) {
            LoadAllSettings();
#if DEBUG
            Debug = true;
#endif
        }
        public override void LoadAllSettings() {
            LoadSettings("AddedContent.json", "OccultistArcanist.Config", ref AddedContent);
            LoadSettings("Fixes.json", "OccultistArcanist.Config", ref Fixes);
            LoadBlueprints("OccultistArcanist.Config", Main.HaddeqiModContext);
            LoadLocalization("OccultistArcanist.Localization");
        }

        public override void AfterBlueprintCachePatches() {
            base.AfterBlueprintCachePatches();
            if (Debug)
            {
                Blueprints.RemoveUnused();
                SaveSettings(BlueprintsFile, Blueprints);
                ModLocalizationPack.RemoveUnused();
                SaveLocalization(ModLocalizationPack);
            }
        }

        public override void SaveAllSettings() {
            base.SaveAllSettings();
            SaveSettings("AddedContent.json", AddedContent);
            SaveSettings("Fixes.json", Fixes);
        }

        public T GetModBlueprintReference<T>(string name) where T : BlueprintReferenceBase
            => BlueprintTools.GetModBlueprintReference<T>(this, name);
    }
}
