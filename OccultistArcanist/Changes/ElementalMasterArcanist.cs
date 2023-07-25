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
using OccultistArcanist.NewComponents;
using static TabletopTweaks.Core.MechanicsChanges.AdditionalActivatableAbilityGroups;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.Mechanics.Recommendations;
using Kingmaker.UnitLogic.Class.LevelUp;

namespace OccultistArcanist.Changes
{
    public static class ElementalMasterArcanist
    {
        static BlueprintCharacterClass wizard_class = BlueprintTools.GetBlueprint<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e");
        static BlueprintCharacterClass arcanist_class = BlueprintTools.GetBlueprint<BlueprintCharacterClass>("52dbfd8505e22f84fad8d702611f60b7");

        static internal void load()
        {
            var dummy_spelllist = BlueprintTools.GetBlueprint<BlueprintSpellList>("c7a55e475659a944f9229d89c4dc3a8e");
            var elemental_master_element_selection = BlueprintTools.GetBlueprint<BlueprintFeatureSelection>("1758ac999383cb1419b18cd6eb0d78e1");

            //create special spell lists
            var descriptors = new SpellDescriptor[] { SpellDescriptor.Electricity, SpellDescriptor.Acid, SpellDescriptor.Fire, SpellDescriptor.Cold };
            var opposite_descriptor = new SpellDescriptor[] { SpellDescriptor.Acid, SpellDescriptor.Electricity, SpellDescriptor.Cold, SpellDescriptor.Fire };

            int i = 0;

            foreach (var desc in descriptors)
            {
                var elemental_spelllist = Helpers.CreateBlueprint<BlueprintSpellList>(Main.HaddeqiModContext, "ElementalMasterSpellList" + desc.ToString(), bp => {
                    bp.SpellsByLevel = new SpellLevelList[] { };
                    foreach (var spell_level in arcanist_class.Spellbook.SpellList.SpellsByLevel)
                    {

                        var spell_list_to_use = new List<BlueprintAbilityReference>();

                        foreach (var spell in spell_level.Spells)
                        {
                            if (spell.SpellDescriptor.HasFlag(desc))
                            {
                                spell_list_to_use.Add(spell.ToReference<BlueprintAbilityReference>());
                            };
                        }
                        bp.SpellsByLevel = bp.SpellsByLevel.AddToArray(new SpellLevelList(spell_level.SpellLevel) { m_Spells = spell_list_to_use });
                    }


                    bp.Descriptor = new SpellDescriptorWrapper(desc);
                    bp.FilterByDescriptor = true;
                    bp.m_FilteredList = wizard_class.Spellbook.SpellList.ToReference<BlueprintSpellListReference>();
                });


                /*if (Main.HaddeqiModContext.Fixes.Arcanist.IsDisabled("FixElementalMaster"))
                {*/

                    elemental_master_element_selection.Features[i].AddComponent(Helpers.Create<AddSpecialSpellList>(a => {
                        a.m_SpellList = elemental_spelllist.ToReference<BlueprintSpellListReference>();
                        a.m_CharacterClass = arcanist_class.ToReference<BlueprintCharacterClassReference>();
                    }));
                    elemental_master_element_selection.Features[i].AddComponent(Helpers.Create<AddOppositionDescriptor>(a => {
                        a.m_Descriptor = new SpellDescriptorWrapper(opposite_descriptor[i]);
                        a.m_CharacterClass = arcanist_class.ToReference<BlueprintCharacterClassReference>();
                    }));

                //}

                i++;
            }


        }


        
    }
}
