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
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using OccultistArcanist.NewComponents;

namespace OccultistArcanist.Changes
{
    public static class MetamagicsUpdate
    {
        //Sorcerers' Arcane Bloodline update
        //Universalist wizards' Metamagic Mastery

        static internal void load()
        {
            Main.HaddeqiModContext.Logger.LogHeader("Updating Arcane Bloodline & Metamagic Mastery");


            updateArcaneBloodline();
            updateMetamagicMastery();
        }



        static internal void updateArcaneBloodline()
        {
            //if (Main.HaddeqiModContext.AddedContent.Sorcerer.IsDisabled("ArcaneBloodlineChanges")) { return; }
            var sorcerer_progression = BlueprintTools.GetBlueprint<BlueprintProgression>("997665565ca80a649aedd72455c4df1f");

            var metamagic_adept_resource = Helpers.CreateBlueprint<BlueprintAbilityResource>(Main.HaddeqiModContext, "MetamagicAdeptResource", bp => {
                bp.m_MaxAmount = new BlueprintAbilityResource.Amount()
                {
                    m_Class = new BlueprintCharacterClassReference[0],
                    m_ClassDiv = new BlueprintCharacterClassReference[0],
                    m_Archetypes = new BlueprintArchetypeReference[0],
                    m_ArchetypesDiv = new BlueprintArchetypeReference[0],
                    BaseValue = 1,
                    LevelIncrease = 0,
                    IncreasedByLevel = false,
                    IncreasedByStat = false
                };
            });

            var combat_casting_adept = BlueprintTools.GetBlueprint<BlueprintFeature>("7aa83ee3526a946419561d8d1aa09e75");

            var progressions = new BlueprintProgression[] {
                BlueprintTools.GetBlueprint<BlueprintProgression>("4d491cf9631f7e9429444f4aed629791"),
                BlueprintTools.GetBlueprint<BlueprintProgression>("7d990675841a7354c957689a6707c6c2"),
                BlueprintTools.GetBlueprint<BlueprintProgression>("9c6dcb965432426d9625b6ba271c8f0a")
            };

            combat_casting_adept.m_Icon = AssetLoader.LoadInternal(Main.HaddeqiModContext, folder: "Icons", file: "Metamixing.png");
            combat_casting_adept.SetNameDescription("Metamagic Adept",
                                                   "At 3rd level, you can apply any one metamagic feat you know to a spell you are about to cast without increasing the casting time. You must still expend a higher-level spell slot to cast this spell.\n" +
                                                   "You can use this ability once per day at 3rd level and one additional time per day for every four sorcerer levels you possess beyond 3rd, up to five times per day at 19th level.\n" +
                                                   "At 20th level, this ability is replaced by arcane apotheosis.");

            var buff = Helpers.CreateBlueprint<BlueprintBuff>(Main.HaddeqiModContext, "MetamagicAdept" + "Buff", bp => {
                bp.m_Icon = combat_casting_adept.m_Icon;
                bp.SetName(combat_casting_adept.Name);
                bp.SetDescription(combat_casting_adept.Description);
                bp.AddComponent<MetamagicAdept>(a => { a.resource = metamagic_adept_resource; });
            });

            var ability = Helpers.CreateBlueprint<BlueprintActivatableAbility>(Main.HaddeqiModContext, "MetamagicAdept" + "ToggleAbility", bp => {
                bp.m_Icon = combat_casting_adept.m_Icon;
                bp.SetName(combat_casting_adept.Name);
                bp.SetDescription(combat_casting_adept.Description);
                bp.m_Buff = buff.ToReference<BlueprintBuffReference>();
                bp.IsOnByDefault = false;
                bp.DoNotTurnOffOnRest = true;
                bp.DeactivateImmediately = true;
                bp.ActivationType = AbilityActivationType.Immediately;
                bp.m_ActivateWithUnitCommand = Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Free;
                bp.AddComponent(Utils.CreateActivatableResourceLogic(metamagic_adept_resource, ResourceSpendType.Never));
            });
            ability.DeactivateImmediately = true;

            combat_casting_adept.SetComponents(
                Helpers.Create<AddFacts>(c => {
                    c.m_Facts = new BlueprintUnitFactReference[] {
                        ability.ToReference<BlueprintUnitFactReference>(),
                    };
                }),
                Helpers.Create<AddAbilityResources>(a => { a.m_Resource = metamagic_adept_resource.ToReference<BlueprintAbilityResourceReference>(); a.RestoreAmount = true; })
            );

            var arcane_apotheosis = BlueprintTools.GetBlueprint<BlueprintFeature>("2086d8c0d40e35b40b86d47e47fb17e4");
            arcane_apotheosis.SetDescription("At 20th level, your body surges with arcane power. You can add any metamagic feats that you know to your spells without increasing their casting time, although you must still expend higher-level spell slots.");
            arcane_apotheosis.RemoveComponents<ConcentrationBonus>();
            arcane_apotheosis.AddComponents(Helpers.Create<SpontaneousMetamagicMasteryComponent>(),
                                            Helpers.Create<RemoveFeatureOnApply>(c => {
                                                c.m_Feature = combat_casting_adept.ToReference<BlueprintUnitFactReference>();
                                            }));


            var combat_casting_adept_2 = BlueprintTools.GetBlueprint<BlueprintFeature>("3d7b19c8a1d03464aafeb306342be000");
            combat_casting_adept_2.SetComponents(Helpers.Create<IncreaseResourceAmount>(a => { a.Value = 1; a.m_Resource = metamagic_adept_resource.ToReference<BlueprintAbilityResourceReference>(); }));
            combat_casting_adept_2.HideInUI = false;
            combat_casting_adept_2.m_Icon = combat_casting_adept.Icon;
            combat_casting_adept_2.SetNameDescription(combat_casting_adept.Name + " - Extra Use",
                                                   "At 3rd level, you can apply any one metamagic feat you know to a spell you are about to cast without increasing the casting time. You must still expend a higher-level spell slot to cast this spell. You can use this ability once per day at 3rd level and one additional time per day for every four sorcerer levels you possess beyond 3rd, up to five times per day at 19th level. At 20th level, this ability is replaced by arcane apotheosis.");
            combat_casting_adept_2.Ranks = 10;





            foreach (var f in progressions)
            {
                combat_casting_adept_2.removeFeatureFromProgression(f);
                combat_casting_adept_2.addFeatureToProgression(f, 7);
                combat_casting_adept_2.addFeatureToProgression(f, 11);
                combat_casting_adept_2.addFeatureToProgression(f, 15);
                combat_casting_adept_2.addFeatureToProgression(f, 19);
            }
        }


        static internal void updateMetamagicMastery()
        {
            var universalist_progression = BlueprintTools.GetBlueprint<BlueprintProgression>("0933849149cfc9244ac05d6a5b57fd80");
            var universalist_extend_reach_feature = BlueprintTools.GetBlueprint<BlueprintFeature>("541bb8d595532ec419343b7a93cdb449");


            var resource = BlueprintTools.GetBlueprint<BlueprintAbilityResource>("42fd5b455f986f94293b15b13f38d6a5");

            var feature = Helpers.CreateBlueprint<BlueprintFeature>(Main.HaddeqiModContext, "UniversalistMetamagicFeature", bp => {
                bp.SetName("Metamagic Mastery");
                bp.SetDescription("At 8th level, you can apply any one metamagic feat that you know to a spell you are about to cast. This does not alter the level of the spell or the casting time.\n" +
                    "You can use this ability once per day at 8th level and one additional time per day for every two wizard levels you possess beyond 8th.\n" +
                    "Any time you use this ability to apply a metamagic feat that increases the spell level by more than 1, you must use an additional daily usage for each level above 1 that the feat adds to the spell.\n" +
                    "Even though this ability does not modify the spell’s actual level, you cannot use this ability to cast a spell whose modified spell level would be above the level of the highest-level spell that you are capable of casting.");
                bp.m_Icon = universalist_progression.Icon;
                bp.Ranks = 1;
                bp.IsClassFeature = true;
                bp.AddComponent<AddAbilityResources>(c => {
                    c.m_Resource = resource.ToReference<BlueprintAbilityResourceReference>();
                    c.RestoreAmount = true;
                    c.RestoreOnLevelUp = false;
                });
            });

            createMetamagicFeature(resource, feature, "UniversalistMetamagicMastery");


            universalist_progression.LevelEntries = universalist_progression.LevelEntries.Where(le => le.Level < 8).ToArray();
            universalist_progression.LevelEntries = universalist_progression.LevelEntries.AddToArray(Helpers.CreateLevelEntry(8, feature));
            universalist_progression.SetDescription("Wizards who do not specialize (known as as universalists) have the most diversity of all arcane spellcasters.\nHand of the Apprentice: You cause your melee weapon to fly from your grasp and strike a foe before instantly returning to you. As a standard action, you can make a single attack using a melee weapon at a range of 30 feet. This attack is treated as a ranged attack with a thrown weapon, except that you add your Intelligence modifier on the attack roll instead of your Dexterity modifier (damage still relies on Strength). This ability cannot be used to perform a combat maneuver. You can use this ability a number of times per day equal to 3 + your Intelligence modifier.\nMetamagic Mastery: " + feature.Description);

        }

        public static void createMetamagicFeature(BlueprintAbilityResource resource, BlueprintFeature feature, string guid)
        {
            var metamagics = FeatTools.GetMetamagicFeats().Where(b => b.AssetGuid != "2f5d1e705c7967546b72ad8218ccf99c").ToArray();
            foreach (var mf in metamagics)
            {
                var metamagic_enum = mf.GetComponent<AddMetamagicFeat>().Metamagic;
                var cost = metamagic_enum.DefaultCost();


                var buff = Helpers.CreateBlueprint<BlueprintBuff>(Main.HaddeqiModContext, mf.name + guid + "Buff", bp => {
                    bp.SetName(feature.Name + " - " + mf.Name);
                    bp.SetDescription(feature.Description + "\n" + mf.Name + ": " + mf.Description);
                    bp.m_Icon = mf.Icon;
                    bp.AddComponent(Helpers.Create<MetamagicOnSpell>(mm => {
                        mm.Metamagic = metamagic_enum;
                        mm.limit_spell_level = true;
                        mm.resource = resource;
                        mm.amount = cost;
                    }));


                });

                var toggle = Utils.buffToToggle(buff, UnitCommand.CommandType.Free, true,
                                                 resource.CreateActivatableResourceLogic(ActivatableAbilityResourceLogic.ResourceSpendType.Never),
                                                 Helpers.Create<RestrictionHasEnoughResource>(r => { r.resource = resource; r.amount = cost; }));
                //toggle.Group = (ActivatableAbilityGroup)ExtentedActivatableAbilityGroup.MetamagicMastery;
                var add_toggle = Utils.ActivatableAbilityToFeature(toggle);

                feature.AddComponent<AddFeatureIfHasFact>(a => { a.m_CheckedFact = mf.ToReference<BlueprintUnitFactReference>(); a.m_Feature = add_toggle.ToReference<BlueprintUnitFactReference>(); });
            }

        }
    }
}
