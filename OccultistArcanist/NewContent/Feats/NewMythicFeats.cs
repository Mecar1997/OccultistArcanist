using Epic.OnlineServices;
using HarmonyLib;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics;
using TabletopTweaks.Core.ModLogic;
using TabletopTweaks.Core.Utilities;
using static UnityModManagerNet.UnityModManager;
using System.Linq;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Utility;
using OccultistArcanist.NewComponents;

namespace OccultistArcanist.NewContent.Feats
{
    public static class NewMythicFeats
    {
        static BlueprintCharacterClass arcanist_class = BlueprintTools.GetBlueprint<BlueprintCharacterClass>("52dbfd8505e22f84fad8d702611f60b7");


        static internal void load()
        {
            metamagicMastery();
        }

        static internal void metamagicMastery()
        {
            var wizard_feats = BlueprintTools.GetBlueprint<BlueprintFeatureSelection>("8c3102c2ff3b69444b139a98521a4899");
            var mythic_brew_potion = BlueprintTools.GetBlueprint<BlueprintFeature>("a155dc95e3fb2d44481c76bc4aea26b9");

            var spontaneous_metamagic_mastery = mythic_brew_potion.CreateCopy(Main.HaddeqiModContext, "MythicMetamagicMasteryFeature", bp => {
                bp.SetName("Spontaneous Metamagic Mastery");
                bp.SetDescription("You apply metamagic feats to spontaneous spells without increasing the casting time.");
                var metamagic_feats = wizard_feats.Features.Where(b => b.GetComponents<AddMetamagicFeat>().Any()).ToArray();
                bp.SetComponents(Helpers.Create<SpontaneousMetamagicMasteryComponent>());
                bp.AddPrerequisiteFeaturesFromList(1, metamagic_feats);
                bp.AddPrerequisite<PrerequisiteCasterTypeSpellLevel>(c => {
                    c.RequiredSpellLevel = 1;
                    c.OnlySpontaneous = true;
                    c.IsArcane = false;
                    c.Group = Prerequisite.GroupType.Any;
                });
                bp.AddPrerequisite<PrerequisiteCasterTypeSpellLevel>(c => {
                    c.RequiredSpellLevel = 2;
                    c.IsArcane = true;
                    c.Group = Prerequisite.GroupType.Any;
                });
                bp.m_Icon = AssetLoader.LoadInternal(Main.HaddeqiModContext, folder: "Icons", file: "Metamixing.png");
            });


            FeatTools.AddAsMythicAbility(spontaneous_metamagic_mastery);
        }


    }
}
