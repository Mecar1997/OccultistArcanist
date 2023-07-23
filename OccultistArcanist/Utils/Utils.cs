using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Mechanics;
using TabletopTweaks.Core.Utilities;

namespace OccultistArcanist {
    public static class Utils
    {

        public static BlueprintAbility createVariantWrapper(string name, string guid, params BlueprintAbility[] variants)
        {
            var wrapper = variants[0].CreateCopy(Main.HaddeqiModContext, name, bp => {
                List<BlueprintComponent> components = new List<BlueprintComponent>();
                components.Add(CreateAbilityVariants(bp, variants));
                bp.ComponentsArray = components.ToArray();
            });


            return wrapper;
        }

        public static AbilityVariants CreateAbilityVariants(this BlueprintAbility parent, IEnumerable<BlueprintAbility> variants) => CreateAbilityVariants(parent, variants.ToArray());

        public static AbilityVariants CreateAbilityVariants(this BlueprintAbility parent, params BlueprintAbility[] variants)
        {
            var a = TabletopTweaks.Core.Utilities.Helpers.Create<AbilityVariants>();
            a.m_Variants = new BlueprintAbilityReference[] { };
            foreach (var v in variants)
            {
                a.m_Variants = a.m_Variants.AddToArray(v.ToReference<BlueprintAbilityReference>());
                v.Parent = parent;
            }
            return a;
        }




        public static ContextDurationValue CreateContextDuration(ContextValue bonus = null, DurationRate rate = DurationRate.Rounds, DiceType diceType = DiceType.Zero, ContextValue diceCount = null)
        {
            return new ContextDurationValue()
            {
                BonusValue = bonus ?? CreateContextValueRank(),
                Rate = rate,
                DiceCountValue = diceCount ?? 0,
                DiceType = diceType
            };
        }

        public static ContextValue CreateContextValueRank(AbilityRankType value = AbilityRankType.Default)
        {
            return value.CreateContextValue();
        }

        public static ContextValue CreateContextValue(this AbilityRankType value)
        {
            return new ContextValue
            {
                ValueType = ContextValueType.Rank,
                ValueRank = value
            };
        }

        public static ContextValue CreateContextValue(this AbilitySharedValue value)
        {
            return new ContextValue
            {
                ValueType = ContextValueType.Shared,
                ValueShared = value
            };
        }

        public static ContextDiceValue CreateContextDiceValue(this DiceType dice, ContextValue diceCount = null, ContextValue bonus = null)
        {
            return new ContextDiceValue
            {
                DiceType = dice,
                DiceCountValue = (diceCount ?? CreateContextValueRank()),
                BonusValue = (bonus ?? ((ContextValue)0))
            };
        }
    }
}