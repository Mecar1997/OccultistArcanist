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
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Stats;
using TabletopTweaks.Core.NewComponents.AbilitySpecific;
using System.Web;
using OccultistArcanist.NewComponents;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker;
using System;
using UnityEngine;
using Kingmaker.Designers.Mechanics.Recommendations;

namespace OccultistArcanist.NewContent.Classes
{
    public static class Shaman
    {
        static BlueprintCharacterClass shaman_class = BlueprintTools.GetBlueprint<BlueprintCharacterClass>("145f1d3d360a7ad48bd95d392c81b38e");
        static BlueprintCharacterClass arcanist_class = BlueprintTools.GetBlueprint<BlueprintCharacterClass>("52dbfd8505e22f84fad8d702611f60b7");

        static BlueprintAbilityResource[] spirit_magic_resources = new BlueprintAbilityResource[] { };
        static public List<AbilityResourceLogic> ability_logics = new List<AbilityResourceLogic>();

        static internal void load()
        {
            Main.HaddeqiModContext.Logger.LogHeader("Shaman");
            shaman_class.Spellbook.AddComponent<ShamanSpellbook>();

            var resist_energy = BlueprintTools.GetBlueprint<BlueprintAbility>("21ffef7791ce73f468b6fca4d9371e8b");
            var spell_list_to_copy = BlueprintTools.GetBlueprint<BlueprintSpellList>("659fbc54fc519b44dacacc78e7d46dec");
            var shaman_spirit_selection = BlueprintTools.GetBlueprint<BlueprintFeatureSelection>("00c8c566d1825dd4a871250f35285982");

            var new_spell_list = spell_list_to_copy.CreateCopy(Main.HaddeqiModContext, "ShamanSpiritMagicSpellList", bp => { });


            for (int i = 1; i < 10; i++)
            {
                var spirit_magic_spell = resist_energy.CreateCopy(Main.HaddeqiModContext, "ShamanSpiritMagicSpell" + i, bp => {
                    bp.RemoveComponents<SpellListComponent>();
                    bp.RemoveComponents<RecommendationNoFeatFromGroup>();
                    bp.GetComponent<AbilityVariants>().m_Variants = new BlueprintAbilityReference[] { };
                    bp.AddComponent<SpiritMagicAbility>();
                    bp.SetNameDescription("Spirit Magic (" + i + ")", "A shaman can spontaneously cast a limited number of spells per day beyond those she prepared ahead of time. She has one spell slot per day of each shaman spell level she can cast, not including orisons.\n" +
                        "She can choose these spells from the list of spells granted by her spirits at the time she casts them. She can enhance these spells using any metamagic feat that she knows, using up a higher-level spell slot as required by the feat and increasing the time to cast the spell");
                    bp.m_Icon = AssetLoader.LoadInternal(Main.HaddeqiModContext, folder: "Icons", file: "SpiritMagic.png"); ;
                });
                

                new_spell_list.SpellsByLevel[i].m_Spells[0] = spirit_magic_spell.ToReference<BlueprintAbilityReference>();
                spirit_magic_spell.AvailableMetamagic = 0;
            }




            var shaman_cantrips = BlueprintTools.GetBlueprint<BlueprintFeature>("1c7cc4b02e560f74796842ba31f2acda");
            shaman_cantrips.AddComponent<AddSpecialSpellList>(a => { a.m_CharacterClass = shaman_class.ToReference<BlueprintCharacterClassReference>(); a.m_SpellList = new_spell_list.ToReference<BlueprintSpellListReference>(); });
        }

    }

   
}
