using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using TabletopTweaks.Core.Utilities;
using static Kingmaker.Blueprints.Classes.BlueprintProgression;

namespace OccultistArcanist.Engine.WizardSchools {
    public class WizardSchool
    {
        public BlueprintProgression wizard_school_progression;
        public BlueprintFeature third_base_feature;
        public BlueprintFeature second_base_feature;
        public BlueprintFeature first_base_feature;

        public BlueprintCharacterClass school_class;
        public BlueprintArchetype school_archetype;
        public bool use_archetype;
        public string guid_used;
        public bool only_first_power;
        public bool add_spell_list;
        public StatType stat_used;

        public SpellSchool school_used;

        public BlueprintFeature first_feature;
        public BlueprintFeature second_feature;
        public BlueprintFeature third_feature;

        public LevelEntry[] new_level_entry_progression;
        public BlueprintProgression new_progression;

        public BlueprintFeatureSelection opposition_school_selection;
        public BlueprintFeature opposition_school_feature;

        public WizardSchool(BlueprintCharacterClass class_to_use, bool archetype_used, string guid_used, bool only_first_power, bool add_spell_list, StatType stat_used, BlueprintFeatureSelection opposition_school_selection = null, BlueprintArchetype archetype_to_use = null)
        {
            school_class = class_to_use;
            school_archetype = archetype_to_use;
            use_archetype = archetype_used;
            this.opposition_school_selection = opposition_school_selection;
            this.guid_used = guid_used;
            this.only_first_power = only_first_power;
            this.add_spell_list = add_spell_list;
            this.stat_used = stat_used;
            initializeBaseFeatures();
        }

        public WizardSchool() 
        {
            school_class = BlueprintTools.GetBlueprint<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e"); //wizard
            use_archetype = false;
            opposition_school_selection = BlueprintTools.GetBlueprint<BlueprintFeatureSelection>("6c29030e9fea36949877c43a6f94ff31");
            guid_used = "";
            only_first_power = false;
            stat_used = StatType.Intelligence;

            initializeBaseFeatures();

            first_feature = first_base_feature;
            second_feature = second_base_feature;
            third_feature = third_base_feature;
            new_progression = wizard_school_progression;
            if (school_used != SpellSchool.Universalist)
            {
                add_spell_list = true;
                opposition_school_feature = opposition_school_selection.AllFeatures.First(a => a.GetComponent<AddOppositionSchool>().School == school_used);
            } else
            {
                add_spell_list = false;
            }

        } //quickly register the vanilla wizard schools

        public WizardSchool(string guid_used)
        {
            this.guid_used = guid_used;
        }

        public virtual void initializeBaseFeatures()
        {

        }

        public virtual void createFirstSchoolPower()
        {

        }


        public virtual void createSecondSchoolPower()
        {


        }


        public virtual BlueprintProgression createProgression()
        {
            new_level_entry_progression = new LevelEntry[] { Helpers.CreateLevelEntry(1, opposition_school_selection, opposition_school_selection, RegisterSchools.associated_requisite_feature[school_used], first_feature) };

            new_progression = wizard_school_progression.CreateCopy(Main.HaddeqiModContext, guid_used + wizard_school_progression.name, bp => {
                bp.m_Classes = new ClassWithLevel[] {
                            new ClassWithLevel {
                                m_Class = school_class.ToReference<BlueprintCharacterClassReference>(),
                                AdditionalLevel = 0,
                    }
                };

                bp.LevelEntries = new_level_entry_progression;
                if (bp.GetComponents<PrerequisiteNoFeature>().Any() && !(opposition_school_feature is null))
                {
                    bp.ReplaceComponent<PrerequisiteNoFeature>(a => a.m_Feature = opposition_school_feature.ToReference<BlueprintFeatureReference>());
                    opposition_school_feature.ReplaceComponent<PrerequisiteNoFeature>(a => a.m_Feature = bp.ToReference<BlueprintFeatureReference>());
                }

            });

            if (!only_first_power)
            {
                second_feature.addFeatureToProgression(new_progression, 8);
            }

            return new_progression;
        }


        public virtual void SetAsSubschool(WizardSchool main_school)
        {
            school_class = main_school.school_class;
            school_archetype = main_school.school_archetype;
            use_archetype = main_school.use_archetype;
            this.opposition_school_selection = main_school.opposition_school_selection;
            this.guid_used = main_school.guid_used;
            this.only_first_power = main_school.only_first_power;
            this.add_spell_list = main_school.add_spell_list;
            this.stat_used = main_school.stat_used;
            school_used = main_school.school_used;
            wizard_school_progression = main_school.wizard_school_progression;
            first_base_feature = main_school.first_base_feature;
            second_base_feature = main_school.second_base_feature;
            third_base_feature = main_school.third_base_feature;
            first_feature = main_school.first_feature;
            second_feature = main_school.second_feature;
            third_feature = main_school.third_feature;
            opposition_school_feature = main_school.opposition_school_feature;
        }
    }
}