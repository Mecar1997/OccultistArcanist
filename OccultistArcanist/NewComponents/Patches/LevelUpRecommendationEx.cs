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
using JetBrains.Annotations;
using System;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.Mechanics.Recommendations;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic;

namespace OccultistArcanist.NewComponents
{
    public class LevelUpRecommendation_Patch
    {
        //This is so spells with the correct descriptors have a green thumb-up in the leveling recommendations
        [HarmonyPatch(typeof(LevelUpRecommendationEx), nameof(LevelUpRecommendationEx.SpellPriority), new Type[] { typeof(BlueprintScriptableObject), typeof(LevelUpState) })]
        private static class LevelUpRecommendationEx_SpellPriority_Patch
        {
            private static void Postfix(ref RecommendationPriority __result, BlueprintScriptableObject blueprint, LevelUpState levelUpState)
            {
                //if (Main.HaddeqiModContext.Fixes.Arcanist.IsDisabled("FixElementalMaster")) { return; }

                BlueprintAbility blueprintAbility = blueprint as BlueprintAbility;
                if (blueprintAbility == null || blueprintAbility.Type != AbilityType.Spell)
                {
                    __result = RecommendationPriority.Same;
                    return;
                }
                ClassData classData = levelUpState.Unit.Progression.GetClassData(levelUpState.SelectedClass);
                BlueprintSpellbook blueprintSpellbook = (classData != null) ? classData.Spellbook : null;
                if (blueprintSpellbook == null)
                {
                    __result = RecommendationPriority.Same;
                    return;
                }
                Spellbook spellbook = levelUpState.Unit.DemandSpellbook(blueprintSpellbook);

                if (IsSpecialistDescriptor(blueprintAbility.SpellDescriptor, spellbook))
                {
                    __result = RecommendationPriority.Good;
                }

            }
        }

        public static bool IsSpecialistDescriptor(SpellDescriptor descriptor, Spellbook spellbook)
        {
            return spellbook.m_SpecialLists.Exists((BlueprintSpellList p) => p.Descriptor.HasAnyFlag(descriptor));
        }
    }

    

}
