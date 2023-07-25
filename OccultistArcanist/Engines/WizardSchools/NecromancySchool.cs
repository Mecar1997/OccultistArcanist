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
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using TabletopTweaks.Core.Utilities;
using static Kingmaker.Blueprints.Classes.BlueprintProgression;

namespace OccultistArcanist.Engine.WizardSchools {
    public class NecromancySchool : WizardSchool
    {

        public NecromancySchool(BlueprintCharacterClass class_to_use, bool archetype_used, string guid_used, bool only_first_power, bool add_spell_list, StatType stat_used, BlueprintFeatureSelection opposition_school_selection = null, BlueprintArchetype archetype_to_use = null) : base(class_to_use, archetype_used, guid_used, only_first_power, add_spell_list, stat_used, opposition_school_selection, archetype_to_use)
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


        public NecromancySchool() : base()
        {

        }

        public override void initializeBaseFeatures()
        {
            school_used = SpellSchool.Necromancy;
            wizard_school_progression = BlueprintTools.GetBlueprint<BlueprintProgression>("e9450978cc9feeb468fb8ee3a90607e3");
            first_base_feature = BlueprintTools.GetBlueprint<BlueprintFeature>("927707dce06627d4f880c90b5575125f");
            second_base_feature = BlueprintTools.GetBlueprint<BlueprintFeature>("82371e899df830e4bb955429d89b755c");
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

            });

            //base ability
            var old_resource = first_base_feature.GetComponents<AddAbilityResources>().Where(a => a.Resource.name.Contains("Base")).First().Resource;
            var new_resource = old_resource.CreateCopy(Main.HaddeqiModContext, guid_used + old_resource.name, bl => {
                bl.m_MaxAmount.ResourceBonusStat = stat_used;
            });


            var old_ability = first_base_feature.GetComponent<AddFacts>().Facts[0];
            var new_ability = old_ability.CreateCopy(Main.HaddeqiModContext, guid_used + old_ability.name, bl => {
                bl.GetComponent<AbilityResourceLogic>().m_RequiredResource = new_resource.ToReference<BlueprintAbilityResourceReference>();
                foreach (var comp in bl.GetComponents<ContextRankConfig>())
                {
                    comp.m_Class[0] = school_class.ToReference<BlueprintCharacterClassReference>();
                };
            });

            copied_feature.GetComponent<ReplaceAbilitiesStat>().Stat = stat_used;
            copied_feature.GetComponent<ReplaceAbilitiesStat>().m_Ability[0] = new_ability.ToReference<BlueprintAbilityReference>();

            copied_feature.GetComponent<ReplaceCasterLevelOfAbility>().m_Class = school_class.ToReference<BlueprintCharacterClassReference>();
            copied_feature.GetComponent<ReplaceCasterLevelOfAbility>().m_Spell = new_ability.ToReference<BlueprintAbilityReference>();

            //turn_undead
            var old_resource_turn_undead = first_base_feature.GetComponents<AddAbilityResources>().Where(a => a.Resource.name.Contains("Turn")).First().Resource;
            var new_resource_turn_undead = old_resource_turn_undead.CreateCopy(Main.HaddeqiModContext, guid_used + old_resource_turn_undead.name, bl => {
                bl.m_MaxAmount.ResourceBonusStat = stat_used;
            });


            var old_turn_undead_fact = first_base_feature.GetComponent<AddFacts>().Facts[1];
            var new_turn_undead_fact = old_turn_undead_fact.CreateCopy(Main.HaddeqiModContext, guid_used + old_turn_undead_fact.name, bl => {

                bl.GetComponent<ContextRankConfig>().m_Class[0] = school_class.ToReference<BlueprintCharacterClassReference>();
                bl.GetComponent<AbilityResourceLogic>().m_RequiredResource = new_resource_turn_undead.ToReference<BlueprintAbilityResourceReference>();
            });



            copied_feature.RemoveComponents<AddAbilityResources>();
            copied_feature.RemoveComponents<AddFacts>();

            copied_feature.AddComponents(
                Helpers.Create<AddFacts>(a => a.m_Facts = new BlueprintUnitFactReference[] {
                    new_ability.ToReference<BlueprintUnitFactReference>(),
                    new_turn_undead_fact.ToReference<BlueprintUnitFactReference>(),
                }),
                Helpers.Create<AddAbilityResources>(a => {
                    a.m_Resource = new_resource_turn_undead.ToReference<BlueprintAbilityResourceReference>();
                    a.RestoreAmount = true;
                })
            );

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
                var old_ability = bp.GetComponent<AddFacts>().Facts[0] as BlueprintActivatableAbility;
                var new_ability = old_ability.CreateCopy(Main.HaddeqiModContext, guid_used + old_ability.name, bl => {
                    bl.GetComponent<ActivatableAbilityResourceLogic>().m_RequiredResource = new_resource.ToReference<BlueprintAbilityResourceReference>();
                });
                bp.GetComponent<AddFacts>().m_Facts[0] = new_ability.ToReference<BlueprintUnitFactReference>();
            });


            second_feature = copied_feature;

        }
    }
}