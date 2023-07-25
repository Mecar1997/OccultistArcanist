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
    public class EvocationSchool : WizardSchool
    { 

        public EvocationSchool(BlueprintCharacterClass class_to_use, bool archetype_used, string guid_used, bool only_first_power, bool add_spell_list, StatType stat_used, BlueprintFeatureSelection opposition_school_selection = null, BlueprintArchetype archetype_to_use = null) : base(class_to_use, archetype_used, guid_used, only_first_power, add_spell_list, stat_used, opposition_school_selection, archetype_to_use)
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


        public EvocationSchool() : base()
        {

        }
        public override void initializeBaseFeatures()
        {
            school_used = SpellSchool.Evocation;
            wizard_school_progression = BlueprintTools.GetBlueprint<BlueprintProgression>("f8019b7724d72a241a97157bc37f1c3b");
            first_base_feature = BlueprintTools.GetBlueprint<BlueprintFeature>("c46512b796216b64899f26301241e4e6");
            second_base_feature = BlueprintTools.GetBlueprint<BlueprintFeature>("de877714f4d224949b403205e8582de4");
            third_base_feature = BlueprintTools.GetBlueprint<BlueprintFeature>("f50f2336ee59d004c8a37f9c8665bb98");
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
                bp.GetComponent<IntenseSpells>().m_Wizard = school_class.ToReference<BlueprintCharacterClassReference>();

                var old_fact = bp.GetComponent<AddFacts>().Facts[0];
                var new_fact = old_fact.CreateCopy(Main.HaddeqiModContext, guid_used + old_fact.name, bl => {
                    var old_resource = bl.GetComponent<AddFacts>().Facts[0].GetComponent<AddAbilityResources>().Resource;
                    var new_resource = old_resource.CreateCopy(Main.HaddeqiModContext, guid_used + old_resource.name, bl => {
                        bl.m_MaxAmount.ResourceBonusStat = stat_used;
                    });

                    var new_resource_feat = bl.GetComponent<AddFacts>().Facts[0].CreateCopy(Main.HaddeqiModContext, guid_used + bl.GetComponent<AddFacts>().Facts[0].name, bk => {
                        bk.GetComponent<AddAbilityResources>().m_Resource = new_resource.ToReference<BlueprintAbilityResourceReference>();
                    });
                    bl.GetComponent<AddFacts>().m_Facts[0] = new_resource_feat.ToReference<BlueprintUnitFactReference>();

                    var old_ability = bl.GetComponent<AddFacts>().Facts[1];
                    var new_ability = old_ability.CreateCopy(Main.HaddeqiModContext, guid_used + old_ability.name, bk => {
                        bk.GetComponent<AbilityResourceLogic>().m_RequiredResource = new_resource.ToReference<BlueprintAbilityResourceReference>();
                    });
                    bl.GetComponent<AddFacts>().m_Facts[1] = new_ability.ToReference<BlueprintUnitFactReference>();

                    bl.GetComponent<ReplaceAbilitiesStat>().Stat = stat_used;
                    bl.GetComponent<ReplaceAbilitiesStat>().m_Ability[0] = new_ability.ToReference<BlueprintAbilityReference>();
                });
                bp.GetComponent<AddFacts>().m_Facts[0] = new_fact.ToReference<BlueprintUnitFactReference>();
            });

            first_feature = copied_feature;


            //TODO: Locket of Magic Missile Mastery
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
                foreach (BlueprintAbility old_ability in second_base_feature.GetComponent<AddFacts>().Facts)
                {
                    var new_ability = old_ability.CreateCopy(Main.HaddeqiModContext, guid_used + old_ability.name, bl => {
                        bl.GetComponent<AbilityResourceLogic>().m_RequiredResource = new_resource.ToReference<BlueprintAbilityResourceReference>();
                        bl.GetComponent<ContextRankConfig>().m_Class[0] = school_class.ToReference<BlueprintCharacterClassReference>();
                        var old_effect = (bl.GetComponent<AbilityEffectRunAction>().Actions.Actions[0] as ContextActionSpawnAreaEffect).AreaEffect;
                        var new_effect = old_effect.CreateCopy(Main.HaddeqiModContext, guid_used + old_effect.name, bk => {
                            bk.GetComponent<ContextRankConfig>().m_Class[0] = school_class.ToReference<BlueprintCharacterClassReference>();
                        });
                        (bl.GetComponent<AbilityEffectRunAction>().Actions.Actions[0] as ContextActionSpawnAreaEffect).m_AreaEffect = new_effect.ToReference<BlueprintAbilityAreaEffectReference>();
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