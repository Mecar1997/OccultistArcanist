using TabletopTweaks.Core.Config;

namespace OccultistArcanist.Config {
    public class AddedContent : IUpdatableSettings {
        public bool NewSettingsOffByDefault = false;


        public void Init() {
        }

        public void OverrideSettings(IUpdatableSettings userSettings) {
            var loadedSettings = userSettings as AddedContent;
            NewSettingsOffByDefault = loadedSettings.NewSettingsOffByDefault;

        }
    }
}
