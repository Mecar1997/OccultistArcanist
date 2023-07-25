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
using OccultistArcanist.Changes;
using TabletopTweaks.Core.Utilities;
using static Kingmaker.Blueprints.Classes.BlueprintProgression;

namespace OccultistArcanist.Engine.WizardSchools {
    public class UniversalistSchool : WizardSchool
    {

        public UniversalistSchool(BlueprintCharacterClass class_to_use, bool archetype_used, string guid_used, bool only_first_power, bool add_spell_list, StatType stat_used, BlueprintFeatureSelection opposition_school_selection = null, BlueprintArchetype archetype_to_use = null) : base(class_to_use, archetype_used, guid_used, only_first_power, add_spell_list, stat_used, opposition_school_selection, archetype_to_use)
        {
            initializeBaseFeatures();

            createFirstSchoolPower();
            if (!only_first_power)
            {
                createSecondSchoolPower();
            }

        }


        public UniversalistSchool() : base()
        {

        }
        public override void initializeBaseFeatures()
        {
            school_used = SpellSchool.Universalist;
            wizard_school_progression = BlueprintTools.GetBlueprint<BlueprintProgression>("0933849149cfc9244ac05d6a5b57fd80");
            first_base_feature = BlueprintTools.GetBlueprint<BlueprintFeature>("38aab7423d96de84d8e6ab2cdbccce63");
            second_base_feature = BlueprintTools.GetBlueprint<BlueprintFeature>("541bb8d595532ec419343b7a93cdb449");
        }

        public override void createFirstSchoolPower()
        {
            var copied_feature = first_base_feature.CreateCopy(Main.HaddeqiModContext, guid_used + first_base_feature.name, bp => {

                var old_resource = bp.GetComponent<AddAbilityResources>().Resource;
                var new_resource = old_resource.CreateCopy(Main.HaddeqiModContext, guid_used + old_resource.name, bl => {
                    bl.m_MaxAmount.ResourceBonusStat = stat_used;
                });
                bp.GetComponent<AddAbilityResources>().m_Resource = new_resource.ToReference<BlueprintAbilityResourceReference>();

                var old_ability = bp.GetComponent<AddFacts>().Facts[0];
                var new_ability = old_ability.CreateCopy(Main.HaddeqiModContext, guid_used + old_ability.name, bl => {
                    bl.GetComponent<AbilityResourceLogic>().m_RequiredResource = new_resource.ToReference<BlueprintAbilityResourceReference>();
                    bl.GetComponent<AbilityDeliverProjectile>().AttackRollBonusStat = stat_used;
                });
                bp.GetComponent<ReplaceAbilitiesStat>().Stat = stat_used;
                bp.GetComponent<ReplaceAbilitiesStat>().m_Ability[0] = new_ability.ToReference<BlueprintAbilityReference>();
                bp.GetComponent<AddFacts>().m_Facts[0] = new_ability.ToReference<BlueprintUnitFactReference>();
            });

            first_feature = copied_feature;
        }


        public override void createSecondSchoolPower()
        {
            var metamagics = FeatTools.GetMetamagicFeats().Where(b => b.AssetGuid != "2f5d1e705c7967546b72ad8218ccf99c").ToArray();

            var old_resource = second_base_feature.GetComponent<AddAbilityResources>().Resource;
            var new_resource = old_resource.CreateCopy(Main.HaddeqiModContext, guid_used + old_resource.name, bl => {
                bl.m_MaxAmount.m_Class[0] = school_class.ToReference<BlueprintCharacterClassReference>();
                bl.m_MaxAmount.m_ClassDiv[0] = school_class.ToReference<BlueprintCharacterClassReference>();
            });


            var feature = Helpers.CreateBlueprint<BlueprintFeature>(Main.HaddeqiModContext, guid_used + "UniversalistMetamagicFeature", bp => {
                bp.SetName("Metamagic Mastery");
                bp.SetDescription("At 8th level, you can apply any one metamagic feat that you know to a spell you are about to cast. This does not alter the level of the spell or the casting time.\n" +
                    "You can use this ability once per day at 8th level and one additional time per day for every two wizard levels you possess beyond 8th.\n" +
                    "Any time you use this ability to apply a metamagic feat that increases the spell level by more than 1, you must use an additional daily usage for each level above 1 that the feat adds to the spell.\n" +
                    "Even though this ability does not modify the spell’s actual level, you cannot use this ability to cast a spell whose modified spell level would be above the level of the highest-level spell that you are capable of casting.");
                bp.m_Icon = wizard_school_progression.Icon;
                bp.Ranks = 1;
                bp.IsClassFeature = true;
                bp.AddComponent<AddAbilityResources>(c => {
                    c.m_Resource = new_resource.ToReference<BlueprintAbilityResourceReference>();
                    c.RestoreAmount = true;
                    c.RestoreOnLevelUp = false;
                });
            });


           MetamagicsUpdate.createMetamagicFeature(old_resource, feature, guid_used);

            second_feature = feature;



        }

        public override BlueprintProgression createProgression()
        {

            new_level_entry_progression = new LevelEntry[] { Helpers.CreateLevelEntry(1, first_feature) };

            new_progression = wizard_school_progression.CreateCopy(Main.HaddeqiModContext, guid_used + wizard_school_progression.name, bp => {
                bp.m_Classes = new ClassWithLevel[] {
                            new ClassWithLevel {
                                m_Class = school_class.ToReference<BlueprintCharacterClassReference>(),
                                AdditionalLevel = 0,
                    }
                };

                bp.LevelEntries = new_level_entry_progression;
            });

            if (!only_first_power)
            {
                second_feature.addFeatureToProgression(new_progression, 8);
            }

            return new_progression;
        }
    }
}