using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using TabletopTweaks.Core.Utilities;

namespace OccultistArcanist.Engine.WizardSchools {
    public static class RegisterSchools
    {
        public static BlueprintProgression[] wizard_schools_progression = new BlueprintProgression[] { };
        public static SpellSchool[] wizard_associated_school = new SpellSchool[] { };
        public static WizardSchool[] wizard_schools = new WizardSchool[] { };


        public static Dictionary<SpellSchool, BlueprintFeature> associated_requisite_feature = new Dictionary<SpellSchool, BlueprintFeature>();


        static internal void register()
        {

            var wizard_school_selection = BlueprintTools.GetBlueprint<BlueprintFeatureSelection>("5f838049069f1ac4d804ce0862ab5110");
            foreach (BlueprintProgression progression_feature in wizard_school_selection.AllFeatures)
            {
                if (progression_feature.Name.Contains("Universalist"))
                {
                    break;
                }
                var school = progression_feature.LevelEntries[0].Features[0].GetComponent<AddSpecialSpellList>().SpellList.FilterSchool;
                wizard_schools_progression = wizard_schools_progression.AddToArray(progression_feature);
                var requisite_feature = Helpers.CreateBlueprint<BlueprintFeature>(Main.HaddeqiModContext, school.ToString() + "RequisiteFeature", bp => {
                    bp.IsClassFeature = true;
                    bp.HideInUI = true;
                    bp.Ranks = 1;
                    bp.HideInCharacterSheetAndLevelUp = true;
                    bp.SetName("School Feature - " + school.ToString());
                    bp.SetDescription(school.ToString() + " School Requisite Feature");
                });
                associated_requisite_feature.Add(school, requisite_feature);
                requisite_feature.addFeatureToProgression(progression_feature, 1);
            }

            wizard_schools = new WizardSchool[]
            {
                new AbjurationSchool(),
                new ConjurationSchool(),
                new DivinationSchool(),
                new EnchantmentSchool(),
                new EvocationSchool(),
                new IllusionSchool(),
                new NecromancySchool(),
                new TransmutationSchool(),
                new UniversalistSchool(),
            };

            var arcane_crafter = new ArcaneCrafter(wizard_schools[8], "");

            wizard_school_selection.m_AllFeatures = wizard_school_selection.m_AllFeatures.AddToArray(arcane_crafter.createProgression().ToReference<BlueprintFeatureReference>());
        }

    }
}