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
using Kingmaker.UI.MVVM._VM.ActionBar;
using Kingmaker.EntitySystem.Entities;
using Steamworks;
using Kingmaker.PubSubSystem;
using Kingmaker;
using System.Web;
using static Kingmaker.Armies.TacticalCombat.Grid.TacticalCombatGrid;

namespace OccultistArcanist.NewComponents
{
    /*[HarmonyPatch(typeof(Spellbook), nameof(Spellbook.Memorize), new Type[] { typeof(AbilityData), typeof(SpellSlot) })]
    static class Spellbook_Memorize_Patch_SpiritMagic
    {
        static void Postfix(Spellbook __instance, AbilityData data, SpellSlot slot, ref bool __result)
        {
            if (slot.Type == SpellSlotType.Favorite || slot.Type == SpellSlotType.Domain)
            {
                if (data.m_CachedName == null) {
                    data.m_CachedName = "(Domain)" + data.Name;
                } else
                {
                    data.m_CachedName += "(Domain)";
                }
            }
            return;
        }
    }
    */


    [HarmonyPatch(typeof(ActionBarSpellbookHelper), nameof(ActionBarSpellbookHelper.IsEquals), new Type[] { typeof(SpellSlot), typeof(SpellSlot) })]
    static class ActionBarSpellbookHelper_IsEquals_Patch
    {
        static void Postfix(ref bool __result, SpellSlot s1, SpellSlot s2)
        {

            __result = s1.SpellShell.Blueprint == s2.SpellShell.Blueprint && s1.SpellLevel == s2.SpellLevel && s1.SpellShell.MetamagicData == s2.SpellShell.MetamagicData && (s1.SpellShell.Spellbook == s2.SpellShell.Spellbook);

            Main.HaddeqiModContext.Logger.LogHeader(__result.ToString());
        }
    }


    [HarmonyPatch(typeof(Spellbook), nameof(Spellbook.GetAvailableForCastSpellCount), new Type[] { typeof(AbilityData) })]
    static class Spellbook_GetAvailableForCastSpellCount_Patch
    {
        static void Postfix(ref int __result, Spellbook __instance, AbilityData spell)
        {
            int spellLevel = __instance.GetSpellLevel(spell);
            if (spellLevel < 0)
            {
                return;
            }
            if (spellLevel == 0)
            {
                return;
            }
            int num = 0;
            if (__instance.Blueprint.MemorizeSpells)
            {
                List<SpellSlot> list = __instance.m_MemorizedSpells[spellLevel];
                for (int i = 0; i < list.Count; i++)
                {
                    SpellSlot spellSlot = list[i];
                    if (spellSlot != null && spellSlot.Available && spellSlot.SpellShell != null)
                    {
                        if (MetamagicForSpontaneousConversions.IsEquals(spellSlot.SpellShell, spell))
                        {
                            num++;
                        }
                    }
                }
                if (__instance.OppositionSchools.HasItem(spell.Blueprint.School))
                {
                    num /= 2;
                }
                if (__instance.OppositionDescriptors.HasAnyFlag(spell.Blueprint.SpellDescriptor))
                {
                    num /= 2;
                }
            }

            if (!__instance.Blueprint.Spontaneous)
            {
                __result = num;
                return;
            }
            if (__instance.Blueprint.MemorizeSpells && num <= 0)
            {
                __result = 0;
                return;
            }

            __result = __instance.GetSpontaneousSlots(spellLevel);


        }
    }
}
