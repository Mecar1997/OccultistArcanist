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

namespace OccultistArcanist.NewComponents
{
    [AllowedOn(typeof(BlueprintSpellbook))]
    public class ShamanSpellbook : BlueprintComponent
    {
    }

    [AllowedOn(typeof(BlueprintAbility))]
    public class SpiritMagicAbility : BlueprintComponent
    {
    }


    public static class MetamagicForSpontaneousConversions
    {
        public static bool IsEquals(SpellSlot s1, SpellSlot s2)
        {
            return s1.SpellShell.Blueprint == s2.SpellShell.Blueprint && s1.SpellLevel == s2.SpellLevel && s1.SpellShell.MetamagicData == s2.SpellShell.MetamagicData && s1.SpellShell.Spellbook == s2.SpellShell.Spellbook;
        }

        public static bool IsEquals(AbilityData a1, AbilityData a2)
        {
            return a1.Blueprint == a2.Blueprint && a1.SpellLevel == a2.SpellLevel && a1.MetamagicData == a2.MetamagicData && a1.Spellbook == a2.Spellbook;

        }


        [HarmonyPatch(typeof(MechanicActionBarSlotSpontaneusConvertedSpell), nameof(MechanicActionBarSlotSpontaneusConvertedSpell.GetDecorationSprite))]
        public static class MechanicActionBarSlotSpontaneusConvertedSpell_GetDecorationSprite_Patch
        {
            private static void Postfix(MechanicActionBarSlotSpontaneusConvertedSpell __instance, ref Sprite __result)
            {
                if (__instance.Spell?.ConvertedFrom != null)
                {
                    __result = UIUtility.GetDecorationBorderByIndex(__instance.Spell.DecorationBorderNumber);
                }

            }
        }


        [HarmonyPatch(typeof(MechanicActionBarSlotSpontaneusConvertedSpell), nameof(MechanicActionBarSlotSpontaneusConvertedSpell.GetDecorationColor))]
        public static class MechanicActionBarSlotSpontaneusConvertedSpell_GetDecorationColor_Patch
        {
            private static void Postfix(MechanicActionBarSlotSpontaneusConvertedSpell __instance, ref Color __result)
            {
                if (__instance.Spell?.ConvertedFrom != null)
                {
                    __result = UIUtility.GetDecorationColorByIndex(__instance.Spell.DecorationColorNumber);
                }

            }
        }

        static void addAbilityOrVariants(List<AbilityData> list, AbilityData instance, BlueprintAbility blueprintAbility)
        {
            var variants = blueprintAbility.GetComponent<AbilityVariants>()?.Variants;
            if (variants == null)
            {
                AbilityData.AddAbilityUnique(ref list, new AbilityData(instance, blueprintAbility)
                {
                    //m_ConvertedFrom = instance,
                    MetamagicData = new MetamagicData(),
                    //m_CachedName = "(Spontaneous)" + blueprintAbility.Name
                });
            }
            else
            {
                foreach (var variant in variants)
                {
                    addAbilityOrVariants(list, instance, variant);
                }
            }
        }

        //-----------Prepared casters
        [HarmonyPatch(typeof(AbilityData), nameof(AbilityData.GetConversions))]
        public static class AbilityData_GetConversions_Patch
        {
            private static void Postfix(AbilityData __instance, ref IEnumerable<AbilityData> __result)
            {
                if (__instance.Spellbook == null || __instance.SpellSlot == null)
                    return;

                //Magus
                UnitPartMagus unitPartMagus = __instance.Spellbook.Owner.Get<UnitPartMagus>();
                var list = __result as List<AbilityData> ?? __result.ToList();


                /*if (__instance.Blueprint.GetComponents<CloseRangeEquivalent>().Any() && __instance.Spellbook.Owner.HasFact(CloseRangeArcana.arcana) && unitPartMagus.IsSpellFromMagusSpellList(__instance))
                {
                    createCloseRangeVariation(__instance, list);
                }*/


                //Shamans
                
                //bool domain_slot = (__instance.m_CachedName != null ? __instance.m_CachedName.Contains("Domain") : false) ;
                if (__instance.Spellbook.Blueprint.GetComponents<ShamanSpellbook>().Any() && __instance.Blueprint.GetComponents<SpiritMagicAbility>().Any())
                {
                    var all_spirit_magics = new List<BlueprintAbility>();
                    for (int i = 0; i < 9; i++)
                    {
                        foreach (var spell in __instance.Spellbook.GetSpecialSpells(i))
                        {
                            all_spirit_magics.Add(spell.Blueprint);
                        }
                    }

                    if (__instance.MetamagicData != null)
                    {
                        __instance.m_CachedName = "(Spontaneous)" + __instance.Blueprint.Name;
                    }

                    var spirit_magic_lists = __instance.Spellbook.GetSpecialSpells(__instance.SpellLevel);

                    foreach (AbilityData blueprintAbility in spirit_magic_lists)
                    {
                        if (__instance.Blueprint != blueprintAbility.Blueprint)
                        {
                            addAbilityOrVariants(list, __instance, blueprintAbility.Blueprint);
                            var metamagics = __instance.Spellbook.GetCustomSpells(__instance.SpellLevel).Where(w => all_spirit_magics.Contains(w.Blueprint) && !IsEquals(w, __instance)).ToList();
                            addMetamagicVersions(metamagics, __instance, list);
                        }
                    }
                }

                var conversion_lists = __instance.Spellbook.m_SpellConversionLists;

                foreach (var conlist in conversion_lists)
                {
                    try
                    {
                        if (conlist.Key.ToLower().Contains("preferredspell"))
                        {
                            var targetspell = conlist.Value.Last();
                            list.FirstOrDefault(f => f.Blueprint == targetspell)?.MetamagicData?.Clear();
                            for (int i = 1; i <= __instance.SpellLevel; i++)
                            {
                                var metamagics = __instance.Spellbook.GetCustomSpells(i).Where(w => Common.isDuplicateOrParent(w.Blueprint, targetspell)).ToList();
                                addMetamagicVersions(metamagics, __instance, list);
                            }
                        }
                        else
                        {
                            foreach (var targetspell in conlist.Value)
                            {
                                list.FirstOrDefault(f => f.Blueprint == targetspell)?.MetamagicData?.Clear();
                                var metamagics = __instance.Spellbook.GetCustomSpells(__instance.SpellLevel).Where(w => Common.isDuplicateOrParent(w.Blueprint, targetspell)).ToList();
                                addMetamagicVersions(metamagics, __instance, list);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                    }
                }

                __result = list;
            }
        }
        public static void addMetamagicVersions(List<AbilityData> metamagics, AbilityData __instance, List<AbilityData> list)
        {
            foreach (var metamagic_ability in metamagics)
            {
                var variants = metamagic_ability.Blueprint.GetComponent<AbilityVariants>()?.Variants;
                if (variants == null)
                {
                    AbilityData.AddAbilityUnique(ref list, new AbilityData(metamagic_ability.Blueprint, __instance.Caster)
                    {
                        m_ConvertedFrom = __instance,
                        MetamagicData = metamagic_ability.MetamagicData?.Clone(),
                        DecorationBorderNumber = metamagic_ability.DecorationBorderNumber,
                        DecorationColorNumber = metamagic_ability.DecorationColorNumber,
                        m_CachedName = "(Spontaneous)" + (metamagic_ability.m_CachedName != null ? metamagic_ability.m_CachedName : metamagic_ability.Blueprint.Name),
                    });
                }
                else
                {
                    foreach (var variant in variants)
                    {
                        AbilityData.AddAbilityUnique(ref list, new AbilityData(variant, __instance.Caster)
                        {
                            m_ConvertedFrom = __instance,
                            MetamagicData = metamagic_ability.MetamagicData?.Clone(),
                            DecorationBorderNumber = metamagic_ability.DecorationBorderNumber,
                            DecorationColorNumber = metamagic_ability.DecorationColorNumber,
                            m_CachedName = "(Spontaneous)" + (metamagic_ability.m_CachedName != null ? metamagic_ability.m_CachedName : metamagic_ability.Blueprint.Name) + variant.Name,
                        });
                    }
                }
            }
            
        }


        public static void createCloseRangeVariation(AbilityData __instance, List<AbilityData> list)
        {
            /*var copied_ability = new AbilityData(__instance.Blueprint.GetComponent<CloseRangeEquivalent>().ability_to_copy, __instance.Caster)
            {
                m_ConvertedFrom = __instance,
                MetamagicData = __instance.MetamagicData?.Clone(),
                DecorationBorderNumber = __instance.DecorationBorderNumber,
                DecorationColorNumber = __instance.DecorationColorNumber,
                m_CachedName = "(CloseRange)" + __instance.Blueprint.GetComponent<CloseRangeEquivalent>().ability_to_copy.Name,
            };

            if (copied_ability.HasMetamagic(Metamagic.Reach))
            {
                copied_ability.MetamagicData.MetamagicMask = copied_ability.MetamagicData.MetamagicMask & ~Metamagic.Reach;
            }

            list.Add(copied_ability);*/

            
        }


        


    }
}
