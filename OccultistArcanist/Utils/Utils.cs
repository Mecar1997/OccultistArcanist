using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Enums;
using Kingmaker.Localization.Shared;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using TabletopTweaks.Core.ModLogic;
using TabletopTweaks.Core.Utilities;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

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

        public static void SetNameDescription(this BlueprintUnitFact feature, string name, string description)
        {
            feature.SetName(name);
            feature.SetDescription(description);
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

        //ActivatableAbilities
        public static ActivatableAbilityResourceLogic CreateActivatableResourceLogic(this BlueprintAbilityResource resource,
            ResourceSpendType spendType)
        {
            var logic = Helpers.Create<ActivatableAbilityResourceLogic>();
            logic.m_RequiredResource = resource.ToReference<BlueprintAbilityResourceReference>();
            logic.SpendType = spendType;
            return logic;
        }

        static public BlueprintActivatableAbility buffToToggle(BlueprintBuff buff, CommandType command, bool deactivate_immediately, params BlueprintComponent[] components)
        {

            var toggle = Helpers.CreateBlueprint<BlueprintActivatableAbility>(Main.HaddeqiModContext, buff.name + "ToggleAbility", bp => {
                bp.SetName(buff.m_DisplayName);
                bp.SetDescription(buff.m_Description);
                bp.m_Buff = buff.ToReference<BlueprintBuffReference>();
                bp.m_Icon = buff.Icon;
                bp.m_ActivateWithUnitCommand = command;
                bp.ActivationType = command == CommandType.Free ? AbilityActivationType.Immediately : AbilityActivationType.WithUnitCommand;
                bp.DeactivateImmediately = true;
                bp.SetComponents(components);
            });

            toggle.DeactivateImmediately = deactivate_immediately;
            return toggle;
        }

        static public BlueprintFeature ActivatableAbilityToFeature(BlueprintActivatableAbility ability, bool hide = true, string guid = "")
        {
            var feature = Helpers.CreateBlueprint<BlueprintFeature>(Main.HaddeqiModContext, ability.name + "Feature", bp => {
                bp.SetName(ability.m_DisplayName);
                bp.SetDescription(ability.m_Description);
                bp.m_Icon = ability.Icon;
                bp.Groups = new FeatureGroup[] { FeatureGroup.None };
                bp.AddComponent<AddFeatureIfHasFact>(c => {
                    c.m_Feature = ability.ToReference<BlueprintUnitFactReference>();
                    c.m_CheckedFact = ability.ToReference<BlueprintUnitFactReference>();
                    c.Not = true;
                });
            });

            if (hide)
            {
                feature.HideInCharacterSheetAndLevelUp = true;
                feature.HideInUI = true;
            }
            return feature;
        }

        //Archetypes
        public static void addFeatureToArchetype(this BlueprintFeatureBase feature, BlueprintArchetype archetype, int level)
        {
            if (!archetype.AddFeatures.Where(a => a.Level == level).Any())
            {
                archetype.AddFeatures = archetype.AddFeatures.AddToArray(Helpers.CreateLevelEntry(level, feature));
            }
            else
            {
                var entry = archetype.AddFeatures.Where(a => a.Level == level).Concat(archetype.AddFeatures).FirstOrDefault();
                entry.m_Features.Add(feature.ToReference<BlueprintFeatureBaseReference>());
            }
        }

        public static void removeFeatureFromArchetype(this BlueprintFeatureBase feature, BlueprintArchetype archetype, int level)
        {

            if (!archetype.RemoveFeatures.Where(a => a.Level == level).Any())
            {
                archetype.RemoveFeatures = archetype.RemoveFeatures.AddToArray(Helpers.CreateLevelEntry(level, feature));
            }
            else
            {
                var entry = archetype.RemoveFeatures.Where(a => a.Level == level).Concat(archetype.RemoveFeatures).FirstOrDefault();
                entry.m_Features.Add(feature.ToReference<BlueprintFeatureBaseReference>());
            }
        }

        public static void deleteFeatureFromArchetype(this BlueprintFeatureBase feature, BlueprintArchetype archetype, bool remove = false)
        {
            if (remove && archetype.RemoveFeatures.Where(a => a.m_Features.Contains(feature)).Any())
            {
                foreach (var f in archetype.RemoveFeatures.Where(a => a.m_Features.Contains(feature)))
                {
                    f.m_Features.Remove(feature.ToReference<BlueprintFeatureBaseReference>());
                }
            }
            else if (archetype.AddFeatures.Where(a => a.m_Features.Contains(feature)).Any())
            {
                foreach (var f in archetype.AddFeatures.Where(a => a.m_Features.Contains(feature)))
                {
                    f.m_Features.Remove(feature.ToReference<BlueprintFeatureBaseReference>());
                }
            }
        }
    }
}