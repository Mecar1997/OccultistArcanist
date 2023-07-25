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
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic;
using System;
using Kingmaker.UI.Common;
using Kingmaker.UI.UnitSettings;
using UnityEngine;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Classes.Spells;
using TabletopTweaks.Core.Config;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;
using TinyJson;

namespace OccultistArcanist.NewComponents
{
    public static class ArcanistMetamagic
    {



        [HarmonyPatch(typeof(Spellbook), nameof(Spellbook.GetMemorizedSpells), new Type[] { typeof(int) })]
        private static class Spellbook_GetMemorizedSpells_Patch
        {
            private static void Postfix(Spellbook __instance, ref IEnumerable<SpellSlot> __result, int spellLevel)
            {
                if (!__instance.Blueprint.IsArcanist)
                    return;
                var memory_spells_list = Common.GetMemorisedSpells(__instance);
                var spell_slots_list = __result.ToList();


                var temporary_spells_list = new List<BlueprintAbility>();
                var temporary_spells_unit_part = __instance.Owner.Unit.Get<UnitPartTemporarySpellsKnown>();
                if (temporary_spells_unit_part != null)
                {
                    foreach (var entry in temporary_spells_unit_part.m_Entries)
                    {
                        foreach (var spell in entry.Spells)
                        {
                            temporary_spells_list.Add(spell.Blueprint);
                            if (spell.SpellLevel != spellLevel)
                                continue;
                            if (spell.Spellbook != __instance)
                                continue;
                            if (spell_slots_list.Any(a => a.Spell.Blueprint == spell.Blueprint))
                                continue;

                            spell.m_CachedName = "(Temporary)" + spell.Name;
                            var slot = new SpellSlot(spellLevel, SpellSlotType.Common, -1, __instance)
                            {
                                Spell = spell,
                                Available = true
                            };
                            spell_slots_list.Add(slot);
                        }
                    }
                }

                foreach (var custom_spell in __instance.GetCustomSpells(spellLevel))
                {
                    if (custom_spell.MetamagicData?.NotEmpty != true)
                    {
                        continue;
                    }

                    if (!memory_spells_list.Contains(custom_spell.Blueprint) && !temporary_spells_list.Contains(custom_spell.Blueprint))
                        continue;

                    if (__instance.m_MemorizedSpells[spellLevel].Any(a => a.Spell?.Equals(custom_spell) == true))
                        continue;

                    bool metamixing = false;
                    int metamagic_count_spell = Common.PopulationCount((int)(custom_spell.MetamagicData.MetamagicMask));

                    for (int i = 1; i <= spellLevel; i++)
                    {
                        foreach (var memorized_slot in __instance.m_MemorizedSpells[i])
                        {
                            if (memorized_slot.Spell == null || memorized_slot.Spell?.MetamagicData?.NotEmpty != true || !(memorized_slot.Available))
                            {
                                continue;
                            }

                            if (memorized_slot.Spell?.Blueprint == custom_spell.Blueprint && (Common.PopulationCount((int)(custom_spell.MetamagicData?.MetamagicMask & ~memorized_slot.Spell?.MetamagicData?.MetamagicMask)) == 1))
                            {
                                metamixing = true;
                                break;
                            }
                        }
                        if (metamixing)
                        {
                            break;
                        }
                    }
                    custom_spell.SpellLevelInSpellbook ??= __instance.GetMinSpellLevel(custom_spell.Blueprint);


                    var spell = new AbilityData(custom_spell.Blueprint, __instance, custom_spell.SpellLevelInSpellbook.Value);
                    spell.MetamagicData = custom_spell.MetamagicData?.Clone();
                    spell.DecorationBorderNumber = custom_spell.DecorationBorderNumber;
                    spell.DecorationColorNumber = custom_spell.DecorationColorNumber;
                    spell.m_CachedName = "(Arcanist)(Spontaneous)" + spell.Name;

                    if (metamixing)
                    {
                        spell.m_CachedName += "(Metamixing)";
                    }

                    var slot = new SpellSlot(spellLevel, SpellSlotType.Common, -1, __instance)
                    {
                        Spell = spell,
                        Available = true
                    };

                    spell_slots_list.Add(slot);
                }

                __result = spell_slots_list;

            }
        }

        [HarmonyPatch(typeof(Spellbook), nameof(Spellbook.GetAvailableForCastSpellCount), new Type[] { typeof(AbilityData) })]
            private static class Spellbook_GetAvailableForCastSpellCount_Patch
            {
                private static void Postfix(Spellbook __instance, ref int __result, AbilityData spell)
                {
                    if (__result == 0 && __instance.Blueprint.IsArcanist)
                    {
                        if (spell.m_CachedName?.Contains("(Spontaneous)") == true
                            || spell.m_CachedName?.Contains("(Temporary)") == true)
                        {
                            __result = __instance.GetSpontaneousSlots(__instance.GetSpellLevel(spell));
                        }
                    }
                }
            }

        [HarmonyPatch(typeof(AbilityData), nameof(AbilityData.GetDefaultActionType))]
        private static class AbilityData_GetDefaultActionType_Patch
        {
            private static void Postfix(AbilityData __instance, ref CommandType __result)
            {
                if (checkFullRound(__instance))
                    __result = CommandType.Standard;
            }
        }

        [HarmonyPatch(typeof(AbilityData), nameof(AbilityData.RequireFullRoundAction), MethodType.Getter)]
        private static class AbilityData_RequireFullRoundAction_Patch
        {
            private static void Postfix(AbilityData __instance, ref bool __result)
            {

                if (checkFullRound(__instance))
                    __result = true;

                if (!__instance.Blueprint.IsFullRoundAction && __result)
                {
                    if (__instance.MetamagicData.MetamagicMask == 0)
                    {
                        return;
                    }

                    var fast_metamagic = __instance.Caster.Unit.Get<UnitPartSpontaneousMetamagic>();
                    if (fast_metamagic == null)
                    {
                        return;
                    }

                    if (!__instance.Blueprint.IsSpell)
                    {
                        return;
                    }

                    __result = !fast_metamagic.canBeUsedOnAbility(__instance);
                }
            }


        }

        public static bool checkFullRound(AbilityData __instance)
        {
            if (__instance.Spellbook == null)
            {
                return false;
            }

            if (!(__instance.Spellbook.Blueprint.IsArcanist || __instance.Spellbook.Blueprint.GetComponents<ShamanSpellbook>().Any()) && __instance.ConvertedFrom == null)
                return false;

            if (__instance.HasMetamagic(Metamagic.Quicken) || __instance.MetamagicData == null)
                return false;


            if (__instance.ConvertedFrom != null)
            {
                return __instance.ConvertedFrom.m_CachedName?.StartsWith("(Arcanist)(Spontaneous)") == true || __instance.m_CachedName?.StartsWith("(Spontaneous)") == true;
            }
            else
            {
                return __instance.m_CachedName?.StartsWith("(Spontaneous)") == true;
            }


        }

    }
}
