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
    public class AbjurationSchool : WizardSchool
    {
        public AbjurationSchool(BlueprintCharacterClass class_to_use, bool archetype_used, string guid_used, bool only_first_power, bool add_spell_list, StatType stat_used, BlueprintFeatureSelection opposition_school_selection = null, BlueprintArchetype archetype_to_use = null) : base(class_to_use, archetype_used, guid_used, only_first_power, add_spell_list, stat_used, opposition_school_selection, archetype_to_use)
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


        public AbjurationSchool(): base()
        {

        }


        public override void initializeBaseFeatures()
        {
            school_used = SpellSchool.Abjuration;
            wizard_school_progression = BlueprintTools.GetBlueprint<BlueprintProgression>("c451fde0aec46454091b70384ea91989");
            second_base_feature = BlueprintTools.GetBlueprint<BlueprintFeature>("e0a0f1ec8dd1fb94d99f824c6f032c64");
            first_base_feature = BlueprintTools.GetBlueprint<BlueprintFeature>("30f20e6f850519b48aa59e8c0ff66ae9");
        }

        public override void createFirstSchoolPower()
        {

            var new_protective_ward_feature = createProtectiveWard();
            var new_abjuration_resistance_feature = createAbjurationResistance();


            first_feature = Helpers.CreateBlueprint<BlueprintFeature>(Main.HaddeqiModContext, guid_used + first_base_feature.name, bp => {
                bp.SetName(first_base_feature.m_DisplayName);
                bp.SetDescription(first_base_feature.m_Description);
                bp.m_Icon = first_base_feature.Icon;
                bp.IsClassFeature = true;
                if (add_spell_list)
                {
                    bp.AddComponent<AddSpecialSpellList>(c => {
                        c.m_CharacterClass = school_class.ToReference<BlueprintCharacterClassReference>();
                        c.m_SpellList = first_base_feature.GetComponent<AddSpecialSpellList>().m_SpellList;
                    });
                    if (use_archetype)
                    {
                        bp.GetComponent<AddSpecialSpellList>().ForArchetypeOnly = true;
                        bp.GetComponent<AddSpecialSpellList>().m_Archetype = school_archetype.ToReference<BlueprintArchetypeReference>();
                    }
                }



                bp.AddComponent<AddFacts>(a => a.m_Facts = new BlueprintUnitFactReference[] {
                    new_protective_ward_feature.ToReference<BlueprintUnitFactReference>(),
                    new_abjuration_resistance_feature.ToReference<BlueprintUnitFactReference>(),
                });
            });

            //TODO: add to ward master amulet
        }


        public BlueprintFeature createProtectiveWard()
        {
            var protective_ward_feature = BlueprintTools.GetBlueprint<BlueprintFeature>("4e63d38f5f48a474eaf1ce0521bb7e87");


            var new_resource = protective_ward_feature.GetComponent<AddAbilityResources>().Resource.CreateCopy(Main.HaddeqiModContext, guid_used + protective_ward_feature.GetComponent<AddAbilityResources>().Resource.name, bl => {
                bl.m_MaxAmount.ResourceBonusStat = stat_used;
            });



            var old_ward_effect_buff = BlueprintTools.GetBlueprint<BlueprintBuff>("c0e5382d22ca3c14593c88d4e657b0e7");
            var new_area_buff = old_ward_effect_buff.CreateCopy(Main.HaddeqiModContext, guid_used + old_ward_effect_buff.name, bp => {
                bp.ReplaceComponent<ContextRankConfig>(a => a.m_Class[0] = school_class.ToReference<BlueprintCharacterClassReference>());
            });


            var old_ability = protective_ward_feature.GetComponent<AddFacts>().Facts[0];
            var new_ability = old_ability.CreateCopy(Main.HaddeqiModContext, guid_used + old_ability.name, bl => {
                bl.ReplaceComponent<AbilityResourceLogic>(a => a.m_RequiredResource = new_resource.ToReference<BlueprintAbilityResourceReference>());
                bl.ReplaceComponent<ContextRankConfig>(a => a.m_Class[0] = school_class.ToReference<BlueprintCharacterClassReference>());
                var old_buff = (bl.GetComponent<AbilityEffectRunAction>().Actions.Actions[0] as ContextActionApplyBuff).Buff;
                var new_buff = old_buff.CreateCopy(Main.HaddeqiModContext, guid_used + old_buff.name, bl_2 => {
                    var new_area_effect = bl_2.GetComponent<AddAreaEffect>().AreaEffect.CreateCopy(Main.HaddeqiModContext, guid_used + bl_2.GetComponent<AddAreaEffect>().AreaEffect.name, bl_3 => {
                        bl_3.ReplaceComponent<AbilityAreaEffectBuff>(a => a.m_Buff = new_area_buff.ToReference<BlueprintBuffReference>());
                    });
                    bl_2.ReplaceComponent<AddAreaEffect>(a => a.m_AreaEffect = new_area_effect.ToReference<BlueprintAbilityAreaEffectReference>());
                });

                bl.ReplaceComponent<AbilityEffectRunAction>(a => (a.Actions.Actions[0] as ContextActionApplyBuff).m_Buff = new_buff.ToReference<BlueprintBuffReference>());
            });


            var feature = Helpers.CreateBlueprint<BlueprintFeature>(Main.HaddeqiModContext, guid_used + protective_ward_feature.name, bp => {
                bp.SetName(protective_ward_feature.m_DisplayName);
                bp.SetDescription(protective_ward_feature.m_Description);
                bp.m_Icon = protective_ward_feature.Icon;
                bp.IsClassFeature = true;


                bp.AddComponent<AddAbilityResources>(a => {
                    a.m_Resource = new_resource.ToReference<BlueprintAbilityResourceReference>();
                    a.RestoreAmount = true;
                });
                bp.AddComponent<ReplaceAbilitiesStat>(a => {
                    a.Stat = stat_used;
                    a.m_Ability = new BlueprintAbilityReference[] { new_ability.ToReference<BlueprintAbilityReference>() };
                });
                bp.AddComponent<AddFacts>(a => {
                    a.m_Facts = new BlueprintUnitFactReference[] { new_ability.ToReference<BlueprintUnitFactReference>() };
                });
            });




            return feature;
        }


        public BlueprintFeature createAbjurationResistance()
        {
            var abjuration_resistance_feature = BlueprintTools.GetBlueprint<BlueprintFeature>("1abe070e7a00ddd48b8a141d71f79e70");

            var damage_types = new DamageEnergyType[] {
                DamageEnergyType.Cold,
                DamageEnergyType.Electricity,
                DamageEnergyType.Fire,
                DamageEnergyType.Sonic,
                DamageEnergyType.Acid
            };

            var old_abjuration_resistance_abilities = new BlueprintAbility[] {
                BlueprintTools.GetBlueprint<BlueprintAbility>("fb1afa622509aee4db0d859e21090709"), //Cold
                BlueprintTools.GetBlueprint<BlueprintAbility>("6d29b28765958f144b00cae5e1e13e0c"), //Electricity
                BlueprintTools.GetBlueprint<BlueprintAbility>("1ecf7a23be0faa3459b8e318452a0a70"), //Fire
                BlueprintTools.GetBlueprint<BlueprintAbility>("8c123d8750aa15a4ba732666db96768e"), //Sonic
                BlueprintTools.GetBlueprint<BlueprintAbility>("9b415e09847398644ad6e57a9e3ab06a") //Acid
            };

            var old_abjuration_resistance_buffs = new BlueprintBuff[] {
                BlueprintTools.GetBlueprint<BlueprintBuff>("90ce7dac4b9001e449eb44861397a65f"), //Cold
                BlueprintTools.GetBlueprint<BlueprintBuff>("453201f110f49714fb1d713c1f7bc06d"), //Electricity
                BlueprintTools.GetBlueprint<BlueprintBuff>("b9ce1623447547946b8c58efd069f7c4"), //Fire
                BlueprintTools.GetBlueprint<BlueprintBuff>("63e2a6323c2bbff40a87fcd522778032"), //Sonic
                BlueprintTools.GetBlueprint<BlueprintBuff>("b87a0e8a961c63a44a952822fe975edb") //Acid
            };

            var new_resource = abjuration_resistance_feature.GetComponent<AddAbilityResources>().Resource.CreateCopy(Main.HaddeqiModContext, guid_used + abjuration_resistance_feature.GetComponent<AddAbilityResources>().Resource.name, bl => {
                bl.m_MaxAmount.ResourceBonusStat = stat_used;
            });

            var new_buffs = new BlueprintBuff[] { };
            for (int i = 0; i < old_abjuration_resistance_buffs.Length; i++)
            {
                var new_buff = old_abjuration_resistance_buffs[i].CreateCopy(Main.HaddeqiModContext, guid_used + old_abjuration_resistance_buffs[i].name, bp => {
                    bp.MaybeReplaceComponent<WizardAbjurationResistance>(a => a.m_Wizard = school_class.ToReference<BlueprintCharacterClassReference>());
                });
                new_buffs = new_buffs.AddToArray(new_buff);
            }

            var new_abilities = new BlueprintAbility[] { };
            for (int i = 0; i < old_abjuration_resistance_buffs.Length; i++)
            {
                var apply_buff = Helpers.Create<ContextActionApplyBuff>(a => {
                    a.IsNotDispelable = true;
                    a.DurationValue = (old_abjuration_resistance_abilities[i].GetComponent<AbilityEffectRunAction>().Actions.Actions[0] as ContextActionApplyBuff).DurationValue;
                    a.Permanent = true;
                    a.AsChild = true;
                    a.m_Buff = new_buffs[i].ToReference<BlueprintBuffReference>();
                });
                var action_list = Helpers.Create<AbilityEffectRunAction>(c => {
                    c.Actions = new ActionList()
                    {
                        Actions = new GameAction[] { apply_buff }
                    };
                });

                foreach (var buff in new_buffs)
                {
                    if (buff != new_buffs[i])
                    {
                        var remove_buff = Helpers.Create<ContextActionRemoveBuff>(a => {
                            a.m_Buff = buff.ToReference<BlueprintBuffReference>();
                        });
                        action_list.Actions.Actions = action_list.Actions.Actions.AddToArray(remove_buff);
                    }
                }

                var new_ability = Helpers.CreateBlueprint<BlueprintAbility>(Main.HaddeqiModContext, guid_used + old_abjuration_resistance_abilities[i].name, bp => {
                    bp.SetName(old_abjuration_resistance_abilities[i].m_DisplayName);
                    bp.SetDescription(old_abjuration_resistance_abilities[i].m_Description);
                    bp.m_Icon = old_abjuration_resistance_abilities[i].Icon;


                    bp.SetComponents(Helpers.Create<AbilityResourceLogic>(a => {
                        a.Amount = 1;
                        a.m_IsSpendResource = true;
                        a.m_RequiredResource = new_resource.ToReference<BlueprintAbilityResourceReference>();
                    }),
                    action_list,
                    old_abjuration_resistance_abilities[i].GetComponent<AbilitySpawnFx>()
                    );
                });
                new_abilities = new_abilities.AddToArray(new_ability);
            }

            var feature = abjuration_resistance_feature.CreateCopy(Main.HaddeqiModContext, guid_used + abjuration_resistance_feature.name, bp => {
                bp.SetComponents(
                    Helpers.Create<AddAbilityResources>(a => {
                        a.m_Resource = new_resource.ToReference<BlueprintAbilityResourceReference>();
                        a.RestoreAmount = true;
                    }),
                    Helpers.Create<AddFacts>(a => {
                        a.m_Facts = new BlueprintUnitFactReference[] {
                            new_abilities[0].ToReference<BlueprintUnitFactReference>(),
                            new_abilities[1].ToReference<BlueprintUnitFactReference>(),
                            new_abilities[2].ToReference<BlueprintUnitFactReference>(),
                            new_abilities[3].ToReference<BlueprintUnitFactReference>(),
                            new_abilities[4].ToReference<BlueprintUnitFactReference>(),
                        };
                    })
                    );
            });

            return feature;
        }


        public override void createSecondSchoolPower()
        {
            var old_resource = second_base_feature.GetComponent<AddAbilityResources>().Resource;
            var new_resource = old_resource.CreateCopy(Main.HaddeqiModContext, guid_used + old_resource.name, bl => {
                bl.m_MaxAmount.m_Class[0] = school_class.ToReference<BlueprintCharacterClassReference>();
            });

            var damage_types = new DamageEnergyType[] {
                DamageEnergyType.Cold,
                DamageEnergyType.Electricity,
                DamageEnergyType.Fire,
                DamageEnergyType.Sonic,
                DamageEnergyType.Acid,
                DamageEnergyType.PositiveEnergy,
                DamageEnergyType.NegativeEnergy
            };

            var feature = second_base_feature.CreateCopy(Main.HaddeqiModContext, guid_used + second_base_feature.name, bp => {
                bp.SetComponents(Helpers.Create<AddAbilityResources>(a =>
                {
                    a.m_Resource = new_resource.ToReference<BlueprintAbilityResourceReference>();
                    a.RestoreAmount = true;
                })
                );
                foreach (var damage in damage_types)
                {
                    bp.AddComponent(Helpers.Create<WizardEnergyAbsorption>(a => {
                        a.m_Resource = new_resource.ToReference<BlueprintAbilityResourceReference>();
                        a.Type = damage;
                        a.Value = second_base_feature.GetComponent<WizardEnergyAbsorption>().Value;
                    }));
                }
            });


            second_feature = feature;
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
                second_feature.addFeatureToProgression(new_progression, 6);
            }

            return new_progression;
        }
    }
}