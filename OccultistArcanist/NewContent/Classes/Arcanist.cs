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
using static Kingmaker.Blueprints.Classes.BlueprintProgression;
using static Kingmaker.Blueprints.Classes.Prerequisites.Prerequisite;
using OccultistArcanist.NewComponents;

namespace OccultistArcanist.NewContent.Classes
{
    public static class Arcanist
    {
        static BlueprintCharacterClass arcanist_class = BlueprintTools.GetBlueprint<BlueprintCharacterClass>("52dbfd8505e22f84fad8d702611f60b7");


        static internal void load()
        {
            Main.HaddeqiModContext.Logger.LogHeader("Arcanist");

            //NewArchetypes.BloodArcanist.load();
            NewArchetypes.OccultistArcanist.load();
            NewArchetypes.SchoolSavant.load();

            //ArcanistExploits.ArcaneWeapon.load();
            ArcanistExploits.FeralShifting.load();
            ArcanistExploits.ArcaneDiscovery.load();
            ArcanistExploits.Metamixing.load();
            ArcanistExploits.SchoolUnderstanding.load();


            Changes.ElementalMasterArcanist.load();
            whiteMageChanges();
            unletteredArcanistBuff();
        }

        static internal void whiteMageChanges()
        {
            //if (Main.HaddeqiModContext.Fixes.Arcanist.IsDisabled("WhiteMageHealingSpells")) { return; }
            var arcane_reservoir_resource = BlueprintTools.GetBlueprint<BlueprintAbilityResource>("cac948cbbe79b55459459dd6a8fe44ce");
            var spontaneous_conversion = BlueprintTools.GetBlueprint<BlueprintFeature>("1f90a0504ae3f11408ca0ff54444b9a4");

            var cure_spells = new BlueprintAbility[] {
                CureHarmSpells.cure_light_wounds,
                CureHarmSpells.cure_moderate_wounds,
                CureHarmSpells.cure_serious_wounds,
                CureHarmSpells.cure_critical_wounds,
                CureHarmSpells.cure_light_wounds_mass,
                CureHarmSpells.cure_moderate_wounds_mass,
                CureHarmSpells.cure_serious_wounds_mass,
                CureHarmSpells.cure_critical_wounds_mass,
            };
            /*var cure_spells_white_mage = new BlueprintAbility[] {
                CureHarmSpells.cure_light_wounds_white_mage,
                CureHarmSpells.cure_moderate_wounds_white_mage,
                CureHarmSpells.cure_serious_wounds_white_mage,
                CureHarmSpells.cure_critical_wounds_white_mage,
                CureHarmSpells.cure_light_wounds_mass_white_mage,
                CureHarmSpells.cure_moderate_wounds_mass_white_mage,
                CureHarmSpells.cure_serious_wounds_mass_white_mage,
                CureHarmSpells.cure_critical_wounds_mass_white_mage,
            };*/
            int i = 0;
            foreach (var s in cure_spells)
            {
                s.AddComponent(Helpers.Create<AbilityResourceLogicIfCastFromClass>(a => {
                    a.m_RequiredResource = arcane_reservoir_resource.ToReference<BlueprintAbilityResourceReference>();
                    a.Amount = 1;
                    a.m_IsSpendResource = true;
                    a.class_to_check = arcanist_class.ToReference<BlueprintCharacterClassReference>();
                    a.fact_to_check = spontaneous_conversion.ToReference<BlueprintUnitFactReference>();
                }));

                spontaneous_conversion.AddComponent(Helpers.Create<AddSpellKnownTemporary>(a => {
                    a.OnlySpontaneous = true;
                    a.m_CharacterClass = arcanist_class.ToReference<BlueprintCharacterClassReference>();
                    a.Level = i + 1;
                    a.m_Spell = s.ToReference<BlueprintAbilityReference>();
                }));
                i++;
            }

            //old version
            /*foreach (var s in cure_spells)
            {
                cure_spells_white_mage[i].SetNameDescription(Main.WotrCContext, s.Name, s.Description);
                cure_spells_white_mage[i].SetComponents(s.Components);
                cure_spells_white_mage[i].AddComponent(Helpers.Create<AbilityResourceLogic>(a => {
                    a.m_RequiredResource = arcane_reservoir_resource.ToReference<BlueprintAbilityResourceReference>();
                    a.Amount = 1;
                    a.m_IsSpendResource = true;
                }));

                spontaneous_conversion.AddComponent(Helpers.Create<AddSpellKnownTemporary>(a => {
                    a.OnlySpontaneous = true;
                    a.m_CharacterClass = arcanist_class.ToReference<BlueprintCharacterClassReference>();
                    a.Level = i + 1;
                    a.m_Spell = cure_spells_white_mage[i].ToReference<BlueprintAbilityReference>();
                }));
                i++;
            }
            */

            spontaneous_conversion.RemoveComponents<SpontaneousSpellConversion>();
        }


        static internal void unletteredArcanistBuff()
        {
            var unlettered_arcanist = BlueprintTools.GetBlueprint<BlueprintArchetype>("44f3ba33839a87f48a66b2b9b2f7c69b");
            var witch_patron_selection = BlueprintTools.GetBlueprint<BlueprintFeatureSelection>("381cf4c890815d049a4420c6f31d063f");
            var arcanist_patrons = new BlueprintFeatureReference[] { };

            var arcanist_patron_selection = witch_patron_selection.CreateCopy(Main.HaddeqiModContext, "ArcanistPatronSelection", bp => {
                bp.SetDescription(Main.HaddeqiModContext, "At 1st level, an unlettered arcanist gains the witch's Patron feature.");
                foreach (BlueprintProgression patron in bp.m_AllFeatures)
                {
                    var arcanist_patron = patron.CreateCopy(Main.HaddeqiModContext, patron.name.Replace("Witch", "Arcanist"), bl => {
                        bl.m_Classes = new ClassWithLevel[] {
                            new ClassWithLevel {
                                m_Class = arcanist_class.ToReference<BlueprintCharacterClassReference>(),
                                AdditionalLevel = 0,
                            }
                        };
                        bl.m_Archetypes = new BlueprintProgression.ArchetypeWithLevel[0];
                        int j = 0;
                        foreach (var level_entry in bl.LevelEntries)
                        {
                            j++;
                            var arcanist_feature = level_entry.Features[0].CreateCopy(Main.HaddeqiModContext, bl.name.Replace("Progression", "SpellLevel" + j), bk => {
                                bk.GetComponent<AddKnownSpell>().m_CharacterClass = arcanist_class.ToReference<BlueprintCharacterClassReference>();
                            });
                            level_entry.m_Features[0] = arcanist_feature.ToReference<BlueprintFeatureBaseReference>();
                        }
                    });

                    arcanist_patrons = arcanist_patrons.AddToArray(arcanist_patron.ToReference<BlueprintFeatureReference>());
                }
                bp.m_AllFeatures = arcanist_patrons;
            });

            AddSecondPatron(arcanist_patron_selection);

            //if (Main.HaddeqiModContext.AddedContent.Arcanist.IsDisabled("BuffUnletteredArcanist")) { return; }
            arcanist_patron_selection.IsPrerequisiteFor = new List<BlueprintFeatureReference>();
            arcanist_patron_selection.addFeatureToArchetype(unlettered_arcanist, 1);
        }

        public static void AddSecondPatron(BlueprintFeatureSelection arcanist_patron_selection)
        {

            var WitchHexAmelioratingFeature = BlueprintTools.GetBlueprint<BlueprintFeature>("3cdd3660fb69f3e4db0160fa97dfa85d");

            var SecondPatronRequisiteFeature = Helpers.CreateBlueprint<BlueprintFeature>(Main.HaddeqiModContext, "SecondPatronRequisiteFeature", bp => {
                bp.IsClassFeature = true;
                bp.HideInUI = true;
                bp.Ranks = 1;
                bp.HideInCharacterSheetAndLevelUp = true;
                bp.SetName(Main.HaddeqiModContext, "Witch Patron (Unlettered Arcanist)");
                bp.SetDescription(Main.HaddeqiModContext, "Patron Requisite Feature");
            });
            var SecondPatronFeature = Helpers.CreateBlueprint<BlueprintFeatureSelection>(Main.HaddeqiModContext, "UnletteredArcanistSecondPatronFeature", bp => {
                bp.SetName(Main.HaddeqiModContext, "Second Patron (Unlettered Arcanist)");
                bp.SetDescription(Main.HaddeqiModContext, "You've attracted the favor of a second patron.\n" +
                    "You select a second patron, gaining all its benifits.");
                bp.m_Icon = WitchHexAmelioratingFeature.Icon;
                bp.Ranks = 1;
                bp.IsClassFeature = true;
                bp.ReapplyOnLevelUp = true;
                bp.Mode = SelectionMode.OnlyNew;
                bp.Groups = new FeatureGroup[] { FeatureGroup.MythicAbility };
                bp.AddFeatures(arcanist_patron_selection.m_AllFeatures);
                bp.AddPrerequisite<PrerequisiteNoFeature>(c => {
                    c.m_Feature = bp.ToReference<BlueprintFeatureReference>();
                });
                bp.AddPrerequisiteFeature(arcanist_patron_selection, GroupType.Any);
                bp.AddPrerequisiteFeature(SecondPatronRequisiteFeature, GroupType.Any);
            });


            //if (Main.HaddeqiModContext.AddedContent.Arcanist.IsDisabled("BuffUnletteredArcanist")) { return; }
            FeatTools.AddAsMythicAbility(SecondPatronFeature);
            arcanist_patron_selection.m_AllFeatures
                .Select(feature => feature.Get())
                .OfType<BlueprintProgression>()
                .ForEach(patron => {
                    patron.GiveFeaturesForPreviousLevels = true;
                    patron.AddComponent<AddFacts>(c => {
                        c.m_Facts = new BlueprintUnitFactReference[] { SecondPatronRequisiteFeature.ToReference<BlueprintUnitFactReference>() };
                    });
                });
        }
    }
}
