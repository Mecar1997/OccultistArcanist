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
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.Blueprints.Root;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker;
using System;
using static Kingmaker.Armies.TacticalCombat.Grid.TacticalCombatGrid;

namespace OccultistArcanist.NewComponents
{
    [AllowedOn(typeof(BlueprintAbility), false)]

    public class AbilityResourceLogicIfCastFromClass : AbilityResourceLogic
    {


        public override void Spend(AbilityData ability)
        {
            UnitEntityData unit = ability.Caster.Unit;
            if (unit == null)
            {
                return;
            }
            if (!unit.HasFact(fact_to_check))
            {
                return;
            }

            if (ability.Spellbook.Blueprint.CharacterClass != class_to_check)
            {
                return;
            }

            if (!this.IsAbilityRestrictionPassed(ability))
            {
                return;
            }
            if (this.IsSpendResource)
            {
                unit.Descriptor.Resources.Spend(ability.OverrideRequiredResource.Or(null) ?? this.RequiredResource, this.CalculateCost(ability));
            }
        }

        public override int CalculateCost(AbilityData ability)
        {
            UnitEntityData unit = ability.Caster.Unit;
            if (unit == null)
            {
                return 0;
            }

            if (!unit.HasFact(fact_to_check) || ability.Spellbook.Blueprint.CharacterClass != class_to_check)
            {
                return 0;
            }

            if (CostIsCustom)
            {
                IAbilityResourceCostCalculator component = ability.Blueprint.GetComponent<IAbilityResourceCostCalculator>();
                if (component == null)
                {
                    PFLog.Default.Error(ability.Blueprint, $"Custom resource cost calculator is missing: {ability.Blueprint}");
                    return 1;
                }

                return component.Calculate(ability);
            }

            int num = Amount;
            for (int i = 0; i < ResourceCostIncreasingFacts.Count; i++)
            {
                BlueprintUnitFactReference blueprintUnitFactReference = ResourceCostIncreasingFacts[i];
                if (ability.Caster.HasFact(blueprintUnitFactReference.Get()))
                {
                    num++;
                }
            }

            for (int j = 0; j < ResourceCostDecreasingFacts.Count; j++)
            {
                BlueprintUnitFactReference blueprintUnitFactReference2 = ResourceCostDecreasingFacts[j];
                if (ability.Caster.HasFact(blueprintUnitFactReference2.Get()))
                {
                    num--;
                }
            }

            return num;
        }

        public BlueprintCharacterClass class_to_check;
        public BlueprintUnitFact fact_to_check;

    }
}
