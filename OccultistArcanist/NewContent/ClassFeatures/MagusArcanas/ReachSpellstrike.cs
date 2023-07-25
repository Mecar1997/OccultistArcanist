
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
using Kingmaker.UnitLogic.Parts;
using UnityEngine;
using System;
using OccultistArcanist.NewComponents;
using Kingmaker.UnitLogic;

namespace OccultistArcanist.NewContent.MagusArcanas
{
    public static class ReachSpellstrike
    {
        static BlueprintCharacterClass magus_class = BlueprintTools.GetBlueprint<BlueprintCharacterClass>("45a4607686d96a1498891b3286121780");
        public static BlueprintFeature arcana;

        static string guid_name = "ReachSpellStrikeMagusArcana";
        static string description = "The magus can deliver spells with a range of touch with ranged spellstrike.";
        static string name = "Reach Spellstrike";

        static Sprite icon = BlueprintTools.GetBlueprint<BlueprintAbility>("3e9d1119d43d07c4c8ba9ebfd1671952").Icon; //Hurricane Bow


        static internal void load()
        {

            var eldritch_archer = BlueprintTools.GetBlueprint<BlueprintArchetype>("44388c01eb4a29d4d90a25cc0574320d");
            var arcane_accuracy = BlueprintTools.GetBlueprint<BlueprintFeature>("2eacbdbf1c4f4134aa7fea99ab8763dc");


            arcana = arcane_accuracy.CreateCopy(Main.HaddeqiModContext, guid_name + "Feature", bp => {
                bp.RemoveComponents<AddFacts>();
                bp.SetNameDescription(name, description);
                bp.m_Icon = icon;
                bp.AddComponent<ReachSpellStrikeComponent>(r => r.Metamagic = Metamagic.Reach);
                bp.AddComponent(Helpers.Create<PrerequisiteClassLevel>(c => {
                    c.m_CharacterClass = magus_class.ToReference<BlueprintCharacterClassReference>();
                    c.Level = 9;
                }));
                bp.AddComponent(Helpers.Create<PrerequisiteFeature>(c => {
                    c.m_Feature = BlueprintTools.GetBlueprint<BlueprintFeature>("6aa84ca8918ac604685a3d39a13faecc").ToReference<BlueprintFeatureReference>(); //ranged spell strike
                }));
            });



            //if (Main.HaddeqiModContext.AddedContent.Magus.IsDisabled("EnableReachSpellstrikeArcana")) { return; }
            FeatTools.AddAsMagusArcana(arcana);
        }

    }

    [HarmonyPatch(typeof(UnitPartMagus), nameof(UnitPartMagus.IsSuitableForEldritchArcherSpellStrike), new Type[] {
            typeof(AbilityData)
        })]
    static class AutoMetamagic_ShouldApplyTo_Patch
    {
        static void Postfix(UnitPartMagus __instance, AbilityData spell, ref bool __result)
        {
            //if (Main.HaddeqiModContext.AddedContent.Magus.IsDisabled("EnableReachSpellstrikeArcana")) { return; }
            if (__result == true)
            {
                return;
            }

            if (spell.Blueprint.GetComponent<AbilityDeliverTouch>() != null
                && (spell.HasMetamagic(Metamagic.Reach) || ((spell.Blueprint.AvailableMetamagic & Metamagic.Reach) != 0) && __instance.Owner.HasFact(ReachSpellstrike.arcana))
                )
            {
                __result = __instance.IsSpellFromMagusSpellList(spell);
            }
        }
    }
}
