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
    public class ArcaneCrafter : WizardSchool
    {
        WizardSchool main_school;

        public ArcaneCrafter(WizardSchool main_school, string guid_used): base (guid_used)
        {
            this.main_school = main_school;
            SetAsSubschool(main_school);
            createFirstSchoolPower();

        }


        public override void createFirstSchoolPower()
        {
            var metacharge = Helpers.CreateBlueprint<BlueprintFeatureSelection>(Main.HaddeqiModContext, guid_used + "ArcaneCrafterMetachargeFeatureSelection", bp => {
                bp.m_Icon = wizard_school_progression.Icon;
                bp.SetName("Metacharge");
                bp.SetDescription("As an Arcane crafter, you gain a bonus metamagic feat at 3rd level. You must still meet all prerequisites for this bonus feat, including caster level minimums.");
                
            });

            metacharge.AddFeatures(FeatTools.GetMetamagicFeats());

            first_feature = metacharge;
        }


        public override BlueprintProgression createProgression()
        {
            new_level_entry_progression = new LevelEntry[] { Helpers.CreateLevelEntry(3, first_feature) };

            new_progression = wizard_school_progression.CreateCopy(Main.HaddeqiModContext, guid_used + "SpecialistionArcaneCrafterProgression", bp => {
                bp.m_Classes = new ClassWithLevel[] {
                            new ClassWithLevel {
                                m_Class = school_class.ToReference<BlueprintCharacterClassReference>(),
                                AdditionalLevel = 0,
                    }
                };

                bp.LevelEntries = new_level_entry_progression;
            });

            new_progression.SetNameDescription("Arcane Crafter",
                            "Wizards who do not specialize (known as as universalists) have the most diversity of all arcane spellcasters.\n"
                            + first_feature.Name + ": " + first_feature.Description + "\n"
                            + second_feature.Name + ": " + second_feature.Description);


            if (!only_first_power)
            {
                second_feature.addFeatureToProgression(new_progression, 8);
            }

            return new_progression;
        }
    }
}