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
    public class TransmutationSchool : WizardSchool
    {

        public TransmutationSchool(BlueprintCharacterClass class_to_use, bool archetype_used, string guid_used, bool only_first_power, bool add_spell_list, StatType stat_used, BlueprintFeatureSelection opposition_school_selection = null, BlueprintArchetype archetype_to_use = null) : base(class_to_use, archetype_used, guid_used, only_first_power, add_spell_list, stat_used, opposition_school_selection, archetype_to_use)
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



        public TransmutationSchool() : base()
        {

        }
        public override void initializeBaseFeatures()
        {
            school_used = SpellSchool.Transmutation;
            wizard_school_progression = BlueprintTools.GetBlueprint<BlueprintProgression>("b6a604dab356ac34788abf4ad79449ec");
            first_base_feature = BlueprintTools.GetBlueprint<BlueprintFeature>("c459c8200e666ef4c990873d3e501b91");
            second_base_feature = BlueprintTools.GetBlueprint<BlueprintFeature>("aeb56418768235640a3ee858d5ee05e8");
            third_base_feature = BlueprintTools.GetBlueprint<BlueprintFeature>("6aa7d3496cd68e643adcd439a7306caa");
        }

        public override void createFirstSchoolPower()
        {
            var copied_feature = first_base_feature.CreateCopy(Main.HaddeqiModContext, guid_used + first_base_feature.name, bp => { //pecializationSchoolTransmutation
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


            var telekinetic_fist_feature = BlueprintTools.GetBlueprint<BlueprintFeature>("e015d05ca18b5c24183cd062486fd46b"); //TelekineticFistFeature
            var physical_enhancement_feature = BlueprintTools.GetBlueprint<BlueprintFeature>("93919f8ce64dc5a4cbf058a486a44a1b"); //physical enhancement

            var new_telekinetic_fist_feature = telekinetic_fist_feature.CreateCopy(Main.HaddeqiModContext, guid_used + telekinetic_fist_feature.name, bp => {
                var old_resource_fact = telekinetic_fist_feature.GetComponent<AddFacts>().Facts[0];
                var old_ability = telekinetic_fist_feature.GetComponent<AddFacts>().Facts[1];

                var new_resource = old_resource_fact.GetComponent<AddAbilityResources>().Resource.CreateCopy(Main.HaddeqiModContext, guid_used + old_resource_fact.GetComponent<AddAbilityResources>().Resource.name, bk => {
                    bk.m_MaxAmount.ResourceBonusStat = stat_used;
                });

                var new_resource_fact = old_resource_fact.CreateCopy(Main.HaddeqiModContext, guid_used + old_resource_fact.name, bl => {
                    bl.GetComponent<AddAbilityResources>().m_Resource = new_resource.ToReference<BlueprintAbilityResourceReference>();
                });

                var new_ability = old_ability.CreateCopy(Main.HaddeqiModContext, guid_used + old_ability.name, bl => {
                    bl.GetComponent<AbilityResourceLogic>().m_RequiredResource = new_resource.ToReference<BlueprintAbilityResourceReference>();
                    bl.GetComponent<ContextRankConfig>().m_Class[0] = school_class.ToReference<BlueprintCharacterClassReference>();
                });

                bp.ReplaceComponent<ReplaceAbilitiesStat>(a => {
                    a.Stat = stat_used;
                    a.m_Ability = new BlueprintAbilityReference[] { new_ability.ToReference<BlueprintAbilityReference>() };
                });
                bp.ReplaceComponent<AddFacts>(a => a.m_Facts = new BlueprintUnitFactReference[] {
                        new_resource_fact.ToReference<BlueprintUnitFactReference>(),
                        new_ability.ToReference<BlueprintUnitFactReference>()
                    });
            });

            var new_physical_enhancement_feature = physical_enhancement_feature.CreateCopy(Main.HaddeqiModContext, guid_used + physical_enhancement_feature.name, bl => {
                int i = 0;
                foreach (BlueprintActivatableAbility old_ability in physical_enhancement_feature.GetComponent<AddFacts>().Facts)
                {
                    var new_ability = old_ability.CreateCopy(Main.HaddeqiModContext, guid_used + old_ability.name, bk => {
                        var old_buff = bk.Buff;
                        var new_buff = old_buff.CreateCopy(Main.HaddeqiModContext, guid_used + old_buff.name, bs => {
                            bs.GetComponent<AddStatBonusScaled>().Scaling.m_ChosenClass = school_class.ToReference<BlueprintCharacterClassReference>();
                        });
                        bk.m_Buff = new_buff.ToReference<BlueprintBuffReference>();
                    });
                    bl.GetComponent<AddFacts>().m_Facts[i] = new_ability.ToReference<BlueprintUnitFactReference>();
                    i++;
                }

            });


            copied_feature.ReplaceComponent<AddFacts>(a => a.m_Facts = new BlueprintUnitFactReference[] {
                        telekinetic_fist_feature.ToReference<BlueprintUnitFactReference>(),
                        new_physical_enhancement_feature.ToReference<BlueprintUnitFactReference>()
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