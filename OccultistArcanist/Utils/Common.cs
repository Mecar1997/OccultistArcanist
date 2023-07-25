using HarmonyLib;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using System.Collections.Generic;

namespace OccultistArcanist {
    public static class Common {

        static public string[] roman_id = { "", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X" };


        public static int PopulationCount(int i)
        {
            i = i - ((i >> 1) & 0x55555555);
            i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
            return (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
        }

        public static bool checkSpellbook(BlueprintSpellbook spellbook_blueprint, BlueprintCharacterClass blueprint_class,
                                          Spellbook spellbook, UnitDescriptor unit_descriptor)
        {
            if (spellbook_blueprint != null && spellbook_blueprint != spellbook?.Blueprint)
            {
                return false;
            }

            var class_spellbook = blueprint_class == null ? null : unit_descriptor.GetSpellbook(blueprint_class);

            if (blueprint_class != null && (class_spellbook == null || spellbook != class_spellbook))
            {
                return false;
            }
            return true;
        }

        public static List<BlueprintAbility> GetSpecialSpells(Spellbook spellbook)
        {
            var memory_list = new List<BlueprintAbility>();
            foreach (var level in spellbook.m_SpecialSpells)
            {
                foreach (var spell in level)
                {
                    if (spell != null
                        && spell.MetamagicData?.NotEmpty != true
                        && !memory_list.Contains(spell.Blueprint))
                        memory_list.Add(spell.Blueprint);
                }
            }
            return memory_list;
        }


        public static List<BlueprintAbility> GetMemorisedSpells(Spellbook spellbook)
        {
            var memory_list = new List<BlueprintAbility>();
            foreach (var level in spellbook.m_MemorizedSpells)
            {
                foreach (var slot in level)
                {
                    var spell = slot.Spell;
                    if (spell != null
                        && spell.MetamagicData?.NotEmpty != true
                        && slot.Available
                        && !memory_list.Contains(spell.Blueprint))
                        memory_list.Add(spell.Blueprint);
                }
            }
            return memory_list;
        }

        public static List<SpellSlot> GetMemorisedSlots(Spellbook spellbook)
        {
            var memory_list = new List<SpellSlot>();
            foreach (var level in spellbook.m_MemorizedSpells)
            {
                foreach (var slot in level)
                {
                    var spell = slot.Spell;
                    if (spell != null
                        && slot.SpellShell != null
                        /*&& slot.Available*/)
                        memory_list.Add(slot);
                }
            }
            return memory_list;
        }

        public static bool isDuplicateOrParent(BlueprintAbility original, BlueprintAbility duplicate)
        {
            if (original == null || duplicate == null)
            {
                return false;
            }
            return isDuplicate(original, duplicate) || isDuplicate(original, duplicate.Parent);
        }

        public static bool isDuplicate(BlueprintAbility original, BlueprintAbility duplicate)
        {
            if (original == null || duplicate == null)
            {
                return false;
            }
            return original == duplicate;
        }
    }
}