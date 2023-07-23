using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Enums;
using Kingmaker.Localization.Shared;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using TabletopTweaks.Core.ModLogic;
using TabletopTweaks.Core.Utilities;

namespace OccultistArcanist {
    public static class Utils
    {
        private static void SetRequiredBlueprintFields(SimpleBlueprint blueprint)
        {
            BlueprintBuff blueprintBuff = blueprint as BlueprintBuff;
            if (blueprintBuff == null)
            {
                BlueprintFeature blueprintFeature = blueprint as BlueprintFeature;
                if (blueprintFeature != null)
                {
                    blueprintFeature.IsClassFeature = true;
                }
            }
            else
            {
                blueprintBuff.FxOnStart = new PrefabLink();
                blueprintBuff.FxOnRemove = new PrefabLink();
                blueprintBuff.IsClassFeature = true;
            }
        }

        public static void SetName(this BlueprintUnitFact feature, string name)
        {
            feature.m_DisplayName = Helpers.CreateString(Main.HaddeqiModContext, feature.name + ".Name", name);
        }

        public static void SetDescription(this BlueprintUnitFact feature, string description)
        {
            feature.m_Description = Helpers.CreateString(Main.HaddeqiModContext, feature.name + ".Description", description, Locale.enGB, shouldProcess: true);
        }


        public static T CreateBlueprint<T>([NotNull] string name, Action<T> init = null) where T : SimpleBlueprint, new()
        {
            T val = new T
            {
                name = name,
                AssetGuid = Main.HaddeqiModContext.Blueprints.GetGUID(name)
            };
            BlueprintTools.AddBlueprint(Main.HaddeqiModContext, val);
            SetRequiredBlueprintFields(val);
            init?.Invoke(val);
            Helpers.RecordMetaBlueprintInfo(val);
            return val;
        }

        //Ability Variants
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
            var a = Helpers.Create<AbilityVariants>();
            a.m_Variants = new BlueprintAbilityReference[] { };
            foreach (var v in variants)
            {
                a.m_Variants = a.m_Variants.AddToArray(v.ToReference<BlueprintAbilityReference>());
                v.Parent = parent;
            }
            return a;
        }

        //UI Determinators
        public static void AddUIDeterminator(this BlueprintProgression progression, BlueprintFeature feature)
        {
            progression.m_UIDeterminatorsGroup = progression.m_UIDeterminatorsGroup.AddToArray(feature.ToReference<BlueprintFeatureBaseReference>());
        }

        public static void AddUIDeterminator(this BlueprintProgression progression, params BlueprintFeature[] features)
        {
            foreach (var feature in features)
            {
                progression.m_UIDeterminatorsGroup = progression.m_UIDeterminatorsGroup.AddToArray(feature.ToReference<BlueprintFeatureBaseReference>());
            }
        }


        //Progressions

        public static void addFeatureToProgression(this BlueprintFeatureBase feature, BlueprintProgression progression, int level)
        {


            if (!progression.LevelEntries.Where(a => a.Level == level).Any())
            {
                progression.LevelEntries = progression.LevelEntries.AddToArray(Helpers.CreateLevelEntry(level, feature));
            }
            else
            {
                var entry = progression.LevelEntries.Where(a => a.Level == level).Concat(progression.LevelEntries).FirstOrDefault();
                entry.m_Features.Add(feature.ToReference<BlueprintFeatureBaseReference>());
            }
        }

        public static void removeFeatureFromProgression(this BlueprintFeatureBase feature, BlueprintProgression progression)
        {
            foreach (var f in progression.LevelEntries)
            {
                if (f.m_Features.Contains(feature))
                {
                    f.m_Features.Remove(feature.ToReference<BlueprintFeatureBaseReference>());
                }
            }
        }

        public static void removeFeatureFromProgression(this BlueprintFeatureBase feature, BlueprintProgression progression, int level)
        {
            if (!progression.LevelEntries.Where(a => a.Level == level).Any())
            {

            }
            else
            {
                var entry = progression.LevelEntries.Where(a => a.Level == level).Concat(progression.LevelEntries).FirstOrDefault();
                entry.m_Features.Remove(feature.ToReference<BlueprintFeatureBaseReference>());
            }
        }


        //ContextRankConfig
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