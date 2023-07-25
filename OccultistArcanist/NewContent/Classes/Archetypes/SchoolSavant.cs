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
using OccultistArcanist.Engine.WizardSchools;

namespace OccultistArcanist.NewContent.NewArchetypes
{
    public static class SchoolSavant
    {
        static BlueprintCharacterClass archetype_class = BlueprintTools.GetBlueprint<BlueprintCharacterClass>("52dbfd8505e22f84fad8d702611f60b7");
        public static BlueprintArchetype archetype;

        private const string archetype_guid_name = "ArcanistSchoolSavant";
        private const string archetype_name = "School Savant";
        private const string archetype_description = "Some arcanists specialize in a school of magic and trade flexibility for focus. School savants are able to prepare more spells per day than typical arcanists, but their selection is more limited.";


        static internal void load()
        {
            createArchetype();
            createProgression();

            //Remove the duplicate from homebrew archetypes
            if (Main.IsHomebrewArchetypesEnabled())
            {
                var homebrew_archetype = archetype_class.Archetypes.Where(a => a.Name.Contains(archetype_name)).First();
                Main.HaddeqiModContext.Logger.LogHeader("Homebrew Archetype detected - Removing duplicate archetype: " + homebrew_archetype.Name);
                archetype_class.m_Archetypes = archetype_class.m_Archetypes.RemoveFromArray(homebrew_archetype.ToReference<BlueprintArchetypeReference>());
            }
            var archetypes = archetype_class.m_Archetypes.AppendToArray(archetype.ToReference<BlueprintArchetypeReference>());
            archetype_class.m_Archetypes = archetypes;
        }



        static internal void createArchetype()
        {
            archetype = Helpers.CreateBlueprint<BlueprintArchetype>(Main.HaddeqiModContext, archetype_guid_name + "Archetype", bp => {
                bp.LocalizedName = Helpers.CreateString(Main.HaddeqiModContext, $"{bp.name}.Name", archetype_name);
                bp.LocalizedDescription = Helpers.CreateString(Main.HaddeqiModContext, $"{bp.name}.Description", archetype_description);
                bp.LocalizedDescriptionShort = Helpers.CreateString(Main.HaddeqiModContext, $"{bp.name}.Description", archetype_description);
                bp.m_ParentClass = archetype_class.ToReference<BlueprintCharacterClassReference>();
            });
        }

        static internal void createProgression()
        {
            var arcane_exploits = BlueprintTools.GetBlueprint<BlueprintFeatureSelection>("b8bf3d5023f2d8c428fdf6438cecaea7");

            var school_focus = createSchoolFocus();

            archetype.RemoveFeatures = new LevelEntry[] {
                Helpers.CreateLevelEntry(1, arcane_exploits),
                Helpers.CreateLevelEntry(3, arcane_exploits),
                Helpers.CreateLevelEntry(7, arcane_exploits)
            };

            archetype.AddFeatures = new LevelEntry[] {
                    Helpers.CreateLevelEntry(1, school_focus)
                };


            archetype_class.Progression.AddUIDeterminator(school_focus);

        }


        static internal BlueprintFeatureSelection createSchoolFocus()
        {
            var wizard_school_selection = BlueprintTools.GetBlueprint<BlueprintFeatureSelection>("5f838049069f1ac4d804ce0862ab5110");

            var arcanist_school_selection = wizard_school_selection.CreateCopy(Main.HaddeqiModContext, archetype_guid_name + "SchoolSelection", bp => {
                bp.SetDescription(Main.HaddeqiModContext, "At 1st level, a school savant chooses a school of magic. The arcanist gains the abilities granted by that school, as the arcane school class feature of the wizard, treating her arcanist level as her wizard level for these abilities. She can also further specialize by selecting a subschool.\n" +
                    "In addition, the arcanist can prepare one additional spell per day of each level she can cast, but this spell must be chosen from the selected school.\n" +
                    "Finally, the arcanist must select two additional schools of magic as her opposition schools. Whenever she prepares spells from one of her opposition schools, the spell takes up two of her prepared spell slots. ");
            });

            var wizard_opposite_school_selection = BlueprintTools.GetBlueprint<BlueprintFeatureSelection>("6c29030e9fea36949877c43a6f94ff31");
            var arcanist_opposite_school_selection = wizard_opposite_school_selection.CreateCopy(Main.HaddeqiModContext, archetype_guid_name + "OppositionSchoolSelection", bp => { });
            arcanist_opposite_school_selection.m_AllFeatures = new BlueprintFeatureReference[] { };

            foreach (var opposite_school_feature in wizard_opposite_school_selection.AllFeatures)
            {
                var school_to_use = opposite_school_feature.GetComponent<AddOppositionSchool>().School;
                var arcanist_opposite_school_feature = opposite_school_feature.CreateCopy(Main.HaddeqiModContext, archetype_guid_name + opposite_school_feature.name, bp => {
                    bp.GetComponent<AddOppositionSchool>().m_CharacterClass = archetype_class.ToReference<BlueprintCharacterClassReference>();
                });

                arcanist_opposite_school_selection.AddFeatures(opposite_school_feature);
            }



            var abjuration = new AbjurationSchool(archetype_class, true, "SchoolSavant", false, true, archetype_class.Spellbook.CastingAttribute, arcanist_opposite_school_selection, archetype);
            var conjuration = new ConjurationSchool(archetype_class, true, "SchoolSavant", false, true, archetype_class.Spellbook.CastingAttribute, arcanist_opposite_school_selection, archetype);
            var divination = new DivinationSchool(archetype_class, true, "SchoolSavant", false, true, archetype_class.Spellbook.CastingAttribute, arcanist_opposite_school_selection, archetype);
            var enchantment = new EnchantmentSchool(archetype_class, true, "SchoolSavant", false, true, archetype_class.Spellbook.CastingAttribute, arcanist_opposite_school_selection, archetype);
            var evocation = new EvocationSchool(archetype_class, true, "SchoolSavant", false, true, archetype_class.Spellbook.CastingAttribute, arcanist_opposite_school_selection, archetype);
            var illusion = new IllusionSchool(archetype_class, true, "SchoolSavant", false, true, archetype_class.Spellbook.CastingAttribute, arcanist_opposite_school_selection, archetype);
            var necromancy = new NecromancySchool(archetype_class, true, "SchoolSavant", false, true, archetype_class.Spellbook.CastingAttribute, arcanist_opposite_school_selection, archetype);
            var transmutation = new TransmutationSchool(archetype_class, true, "SchoolSavant", false, true, archetype_class.Spellbook.CastingAttribute, arcanist_opposite_school_selection, archetype);
            var universalist = new UniversalistSchool(archetype_class, true, "SchoolSavant", false, false, archetype_class.Spellbook.CastingAttribute, arcanist_opposite_school_selection, archetype);
            var arcane_crafter = new ArcaneCrafter(universalist, "SchoolSavant");


            arcanist_school_selection.m_AllFeatures = new BlueprintFeatureReference[] {
                abjuration.createProgression().ToReference<BlueprintFeatureReference>(),
                conjuration.createProgression().ToReference<BlueprintFeatureReference>(),
                divination.createProgression().ToReference<BlueprintFeatureReference>(),
                enchantment.createProgression().ToReference<BlueprintFeatureReference>(),
                evocation.createProgression().ToReference<BlueprintFeatureReference>(),
                illusion.createProgression().ToReference<BlueprintFeatureReference>(),
                necromancy.createProgression().ToReference<BlueprintFeatureReference>(),
                transmutation.createProgression().ToReference<BlueprintFeatureReference>(),
                universalist.createProgression().ToReference<BlueprintFeatureReference>(),
                arcane_crafter.createProgression().ToReference<BlueprintFeatureReference>(),
            };

            return arcanist_school_selection;
        }

    }
}
