using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using TabletopTweaks.Core.Utilities;
using static Kingmaker.Blueprints.Classes.BlueprintProgression;

namespace OccultistArcanist.Engine.WizardSchools {
    public class ConjurationSchool : WizardSchool
    {
        public ConjurationSchool(BlueprintCharacterClass class_to_use, bool archetype_used, string guid_used, bool only_first_power, bool add_spell_list, StatType stat_used, BlueprintFeatureSelection opposition_school_selection = null, BlueprintArchetype archetype_to_use = null) : base(class_to_use, archetype_used, guid_used, only_first_power, add_spell_list, stat_used, opposition_school_selection, archetype_to_use)
        {
            initializeBaseFeatures();

            createFirstSchoolPower();
            if (!only_first_power)
            {
                createSecondSchoolPower();
            }

            if (add_spell_list)
            {
                opposition_school_feature = opposition_school_selection.AllFeatures.First(a => a.GetComponent<AddOppositionSchool>().School == school_used);
            }
        }

        public ConjurationSchool() : base()
        {

        }

        public override void initializeBaseFeatures()
        {
            school_used = SpellSchool.Conjuration;
            wizard_school_progression = BlueprintTools.GetBlueprint<BlueprintProgression>("567801abe990faf4080df566fadcd038");
            first_base_feature = BlueprintTools.GetBlueprint<BlueprintFeature>("cee0f7edbd874a042952ee150f878b84");
            second_base_feature = BlueprintTools.GetBlueprint<BlueprintFeature>("71293f6177954334697ca44a9ddf7090");
        }
        public override void createFirstSchoolPower()
        {
            var copied_feature = first_base_feature.CreateCopy(Main.HaddeqiModContext, guid_used + first_base_feature.name, bp => {
                if (add_spell_list)
                {
                    bp.ReplaceComponent<AddSpecialSpellList>(a => a.m_CharacterClass = school_class.ToReference<BlueprintCharacterClassReference>());
                    if (use_archetype)
                    {
                        bp.GetComponent<AddSpecialSpellList>().ForArchetypeOnly = true;
                        bp.GetComponent<AddSpecialSpellList>().m_Archetype = school_archetype.ToReference<BlueprintArchetypeReference>();
                    }
                }
                else
                {
                    bp.RemoveComponents<AddSpecialSpellList>();
                }
                bp.GetComponent<AddClassLevelToSummonDuration>().m_CharacterClass = school_class.ToReference<BlueprintCharacterClassReference>();
            });



            var acid_dart_feature = BlueprintTools.GetBlueprint<BlueprintFeature>("b4b575d14c3bab6489aafa17ac2c909e");


            var copied_acid_dart_feature = acid_dart_feature.CreateCopy(Main.HaddeqiModContext, guid_used + acid_dart_feature.name, bp => {
                var old_resource = acid_dart_feature.GetComponent<AddAbilityResources>().Resource;
                var new_resource = old_resource.CreateCopy(Main.HaddeqiModContext, guid_used + old_resource.name, bl => {
                    bl.m_MaxAmount.ResourceBonusStat = stat_used;
                });
                bp.GetComponent<AddAbilityResources>().m_Resource = new_resource.ToReference<BlueprintAbilityResourceReference>();
                var old_ability = acid_dart_feature.GetComponent<AddFacts>().Facts[0];
                var new_ability = old_ability.CreateCopy(Main.HaddeqiModContext, guid_used + old_ability.name, bl => {
                    bl.GetComponent<AbilityResourceLogic>().m_RequiredResource = new_resource.ToReference<BlueprintAbilityResourceReference>();
                    bl.GetComponent<ContextRankConfig>().m_Class[0] = school_class.ToReference<BlueprintCharacterClassReference>();
                });

                bp.GetComponent<ReplaceAbilitiesStat>().Stat = stat_used;
                bp.GetComponent<ReplaceAbilitiesStat>().m_Ability[0] = new_ability.ToReference<BlueprintAbilityReference>();
                bp.GetComponent<AddFacts>().m_Facts[0] = new_ability.ToReference<BlueprintUnitFactReference>();

            });


            copied_feature.GetComponents<AddFacts>().First().m_Facts = new BlueprintUnitFactReference[] {
                copied_acid_dart_feature.ToReference<BlueprintUnitFactReference>(),
            };
            first_feature = copied_feature;
        }


        public override void createSecondSchoolPower()
        {


            var copied_feature = second_base_feature.CreateCopy(Main.HaddeqiModContext, guid_used + second_base_feature.name, bp => {
                var old_resource = bp.GetComponent<AddAbilityResources>().Resource;
                var new_resource = old_resource.CreateCopy(Main.HaddeqiModContext, guid_used + old_resource.name, bl => {
                    bl.m_MaxAmount.m_Class[0] = school_class.ToReference<BlueprintCharacterClassReference>();
                });
                bp.GetComponent<AddAbilityResources>().m_Resource = new_resource.ToReference<BlueprintAbilityResourceReference>();
                var old_ability = bp.GetComponent<AddFacts>().Facts[0];
                var new_ability = old_ability.CreateCopy(Main.HaddeqiModContext, guid_used + old_ability.name, bl => {
                    bl.GetComponent<AbilityResourceLogic>().m_RequiredResource = new_resource.ToReference<BlueprintAbilityResourceReference>();
                });
                bp.GetComponent<AddFacts>().m_Facts[0] = new_ability.ToReference<BlueprintUnitFactReference>();
            });


            second_feature = copied_feature;

        }
    }
}