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
    public class DivinationSchool : WizardSchool
    {

        public DivinationSchool(BlueprintCharacterClass class_to_use, bool archetype_used, string guid_used, bool only_first_power, bool add_spell_list, StatType stat_used, BlueprintFeatureSelection opposition_school_selection = null, BlueprintArchetype archetype_to_use = null) : base(class_to_use, archetype_used, guid_used, only_first_power, add_spell_list, stat_used, opposition_school_selection, archetype_to_use)
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

        public DivinationSchool() : base()
        {

        }
        public override void initializeBaseFeatures()
        {
            school_used = SpellSchool.Divination;
            wizard_school_progression = BlueprintTools.GetBlueprint<BlueprintProgression>("d7d18ce5c24bd324d96173fdc3309646");
            first_base_feature = BlueprintTools.GetBlueprint<BlueprintFeature>("54d21b3221ea82a4d90d5a91b7872f3d");
            second_base_feature = BlueprintTools.GetBlueprint<BlueprintFeature>("ef01f802ebc01a94cbc25c89c365563b");
            third_base_feature = BlueprintTools.GetBlueprint<BlueprintFeature>("a6839f2a709288e43b68d3281b3bbd8f");
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
                var old_resource = first_base_feature.GetComponent<AddAbilityResources>().Resource;
                var new_resource = old_resource.CreateCopy(Main.HaddeqiModContext, guid_used + old_resource.name, bl => {
                    bl.m_MaxAmount.ResourceBonusStat = stat_used;
                });
                bp.GetComponent<AddAbilityResources>().m_Resource = new_resource.ToReference<BlueprintAbilityResourceReference>();
                bp.RemoveComponents<AddFacts>();
            });



            var divination_school_base_ability = BlueprintTools.GetBlueprint<BlueprintAbility>("0997652c1d8eb164caae8a462401a25d");

            var copied_divination_school_base_ability = divination_school_base_ability.CreateCopy(Main.HaddeqiModContext, guid_used + divination_school_base_ability.name, bp => {
                bp.GetComponent<AbilityResourceLogic>().m_RequiredResource = copied_feature.GetComponent<AddAbilityResources>().m_Resource;
                var old_ability = bp.GetComponent<AbilityEffectStickyTouch>().TouchDeliveryAbility;
                var new_ability = old_ability.CreateCopy(Main.HaddeqiModContext, guid_used + old_ability.name, bl => {
                    var old_buff = (bl.GetComponent<AbilityEffectRunAction>().Actions.Actions[0] as ContextActionApplyBuff).Buff;
                    var new_buff = old_buff.CreateCopy(Main.HaddeqiModContext, guid_used + old_buff.name, bl_2 => {
                        bl_2.GetComponent<ContextRankConfig>().m_Class[0] = school_class.ToReference<BlueprintCharacterClassReference>();
                    });
                    (bl.GetComponent<AbilityEffectRunAction>().Actions.Actions[0] as ContextActionApplyBuff).m_Buff = new_buff.ToReference<BlueprintBuffReference>();
                    bl.GetComponent<ContextRankConfig>().m_Class[0] = school_class.ToReference<BlueprintCharacterClassReference>();
                });
                bp.GetComponent<AbilityEffectStickyTouch>().m_TouchDeliveryAbility = new_ability.ToReference<BlueprintAbilityReference>();

            });


            var divination_school_initiative_feature = BlueprintTools.GetBlueprint<BlueprintFeature>("f8ee4f0f4cf741fa9b561ba4eef9ebef");
            var copied_divination_school_initiative_feature = divination_school_initiative_feature.CreateCopy(Main.HaddeqiModContext, guid_used + divination_school_initiative_feature.name, bp => {
                bp.GetComponent<ContextRankConfig>().m_Class[0] = school_class.ToReference<BlueprintCharacterClassReference>();
            });

            copied_feature.AddComponent<AddFacts>(a => a.m_Facts = new BlueprintUnitFactReference[] {
                copied_divination_school_base_ability.ToReference<BlueprintUnitFactReference>(),
                copied_divination_school_initiative_feature.ToReference<BlueprintUnitFactReference>(),
            });
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
                int i = 0;
                foreach (BlueprintActivatableAbility old_ability in second_base_feature.GetComponent<AddFacts>().Facts)
                {
                    var new_ability = old_ability.CreateCopy(Main.HaddeqiModContext, guid_used + old_ability.name, bl => {
                        bl.GetComponent<ActivatableAbilityResourceLogic>().m_RequiredResource = new_resource.ToReference<BlueprintAbilityResourceReference>();
                    });
                    bp.GetComponent<AddFacts>().m_Facts[i] = new_ability.ToReference<BlueprintUnitFactReference>();
                    i++;
                }
            });


            second_feature = copied_feature;

        }

        public override BlueprintProgression createProgression()
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
                third_base_feature.addFeatureToProgression(new_progression, 20);
            }

            return new_progression;
        }
    }
}