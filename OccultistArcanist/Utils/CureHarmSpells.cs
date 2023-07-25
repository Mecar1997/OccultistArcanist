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
    public static class CureHarmSpells
    {
        static public BlueprintAbility cure_light_wounds = BlueprintTools.GetBlueprint<BlueprintAbility>("5590652e1c2225c4ca30c4a699ab3649");
        static public BlueprintAbility cure_moderate_wounds = BlueprintTools.GetBlueprint<BlueprintAbility>("6b90c773a6543dc49b2505858ce33db5");
        static public BlueprintAbility cure_serious_wounds = BlueprintTools.GetBlueprint<BlueprintAbility>("3361c5df793b4c8448756146a88026ad");
        static public BlueprintAbility cure_critical_wounds = BlueprintTools.GetBlueprint<BlueprintAbility>("41c9016596fe1de4faf67425ed691203");

        static public BlueprintAbility cure_light_wounds_mass = BlueprintTools.GetBlueprint<BlueprintAbility>("5d3d689392e4ff740a761ef346815074");
        static public BlueprintAbility cure_moderate_wounds_mass = BlueprintTools.GetBlueprint<BlueprintAbility>("571221cc141bc21449ae96b3944652aa");
        static public BlueprintAbility cure_serious_wounds_mass = BlueprintTools.GetBlueprint<BlueprintAbility>("0cea35de4d553cc439ae80b3a8724397");
        static public BlueprintAbility cure_critical_wounds_mass = BlueprintTools.GetBlueprint<BlueprintAbility>("1f173a16120359e41a20fc75bb53d449");

        static public BlueprintAbility cure_light_wounds_white_mage = BlueprintTools.GetBlueprint<BlueprintAbility>("83d6d8f4c4d296941838086f60485fb7");
        static public BlueprintAbility cure_moderate_wounds_white_mage = BlueprintTools.GetBlueprint<BlueprintAbility>("44cf8a9f080a23f4689b4bb51e3bdb64");
        static public BlueprintAbility cure_serious_wounds_white_mage = BlueprintTools.GetBlueprint<BlueprintAbility>("1203e2dab8a593a459c0cc688f568052");
        static public BlueprintAbility cure_critical_wounds_white_mage = BlueprintTools.GetBlueprint<BlueprintAbility>("e84cb97373ca6174397bfe778a039eab");

        static public BlueprintAbility cure_light_wounds_mass_white_mage = BlueprintTools.GetBlueprint<BlueprintAbility>("d43c22896b9ef094fbd5b67689b5410e");
        static public BlueprintAbility cure_moderate_wounds_mass_white_mage = BlueprintTools.GetBlueprint<BlueprintAbility>("4d616f08e68288f438c8e6ce57672a56");
        static public BlueprintAbility cure_serious_wounds_mass_white_mage = BlueprintTools.GetBlueprint<BlueprintAbility>("586e964c75e0c6a46884a1bea3e05cdf");
        static public BlueprintAbility cure_critical_wounds_mass_white_mage = BlueprintTools.GetBlueprint<BlueprintAbility>("22a5d013be997dd479c19421343cfb00");


        static public BlueprintAbility inflict_light_wounds = BlueprintTools.GetBlueprint<BlueprintAbility>("e5af3674bb241f14b9a9f6b0c7dc3d27");
        static public BlueprintAbility inflict_moderate_wounds = BlueprintTools.GetBlueprint<BlueprintAbility>("65f0b63c45ea82a4f8b8325768a3832d");
        static public BlueprintAbility inflict_serious_wounds = BlueprintTools.GetBlueprint<BlueprintAbility>("bd5da98859cf2b3418f6d68ea66cabbe");
        static public BlueprintAbility inflict_critical_wounds = BlueprintTools.GetBlueprint<BlueprintAbility>("651110ed4f117a948b41c05c5c7624c0");

        static public BlueprintAbility inflict_light_wounds_mass = BlueprintTools.GetBlueprint<BlueprintAbility>("9da37873d79ef0a468f969e4e5116ad2");
        static public BlueprintAbility inflict_moderate_wounds_mass = BlueprintTools.GetBlueprint<BlueprintAbility>("03944622fbe04824684ec29ff2cec6a7");
        static public BlueprintAbility inflict_serious_wounds_mass = BlueprintTools.GetBlueprint<BlueprintAbility>("820170444d4d2a14abc480fcbdb49535");
        static public BlueprintAbility inflict_critical_wounds_mass = BlueprintTools.GetBlueprint<BlueprintAbility>("5ee395a2423808c4baf342a4f8395b19");


        static public BlueprintAbility cure_light_wounds_damage_mass = BlueprintTools.GetBlueprint<BlueprintAbility>("fb7e5fe8b5750f9408398d9659b0f98f");
        static public BlueprintAbility cure_moderate_wounds_damage_mass = BlueprintTools.GetBlueprint<BlueprintAbility>("638363b5afb817d4684c021d36279904");
        static public BlueprintAbility cure_serious_wounds_damage_mass = BlueprintTools.GetBlueprint<BlueprintAbility>("21d02c685b2e64b4f852b3efcb0b5ca6");
        static public BlueprintAbility cure_critical_wounds_damage_mass = BlueprintTools.GetBlueprint<BlueprintAbility>("0cce61a5e5108114092f9773572c78b8");

        static public BlueprintAbility inflict_light_wounds_damage_mass = BlueprintTools.GetBlueprint<BlueprintAbility>("b70d903464a738148a19bed630b91f8c");
        static public BlueprintAbility inflict_moderate_wounds_damage_mass = BlueprintTools.GetBlueprint<BlueprintAbility>("89ddb1b4dafc5f541a3dacafbf9ea2dd");
        static public BlueprintAbility inflict_serious_wounds_damage_mass = BlueprintTools.GetBlueprint<BlueprintAbility>("aba480ce9381684408290f5434402a32");
        static public BlueprintAbility inflict_critical_wounds_damage_mass = BlueprintTools.GetBlueprint<BlueprintAbility>("e05c263048e835043bb2784601dca339");

        static public BlueprintAbility[] cure_spells = new BlueprintAbility[] {
            cure_light_wounds,
            cure_moderate_wounds,
            cure_serious_wounds,
            cure_critical_wounds,
        };

        static public BlueprintAbility[] cure_spells_mass = new BlueprintAbility[] {
            cure_light_wounds_mass,
            cure_moderate_wounds_mass,
            cure_serious_wounds_mass,
            cure_critical_wounds_mass,
        };

        static public BlueprintAbility[] cure_spells_white_mage = new BlueprintAbility[] {
            cure_light_wounds_white_mage,
            cure_moderate_wounds_white_mage,
            cure_serious_wounds_white_mage,
            cure_critical_wounds_white_mage,
        };

        static public BlueprintAbility[] cure_spells_mass_white_mage = new BlueprintAbility[] {
            cure_light_wounds_mass_white_mage,
            cure_moderate_wounds_mass_white_mage,
            cure_serious_wounds_mass_white_mage,
            cure_critical_wounds_mass_white_mage,
        };

        static public BlueprintAbility[] inflict_spells = new BlueprintAbility[] {
            inflict_light_wounds,
            inflict_moderate_wounds,
            inflict_serious_wounds,
            inflict_critical_wounds,
        };


        static public BlueprintAbility[] inflict_spells_mass = new BlueprintAbility[] {
            inflict_light_wounds_mass,
            inflict_moderate_wounds_mass,
            inflict_serious_wounds_mass,
            inflict_critical_wounds_mass,
        };

        static public BlueprintAbility[] cure_spells_damage_mass = new BlueprintAbility[] {
            cure_light_wounds_damage_mass,
            cure_moderate_wounds_damage_mass,
            cure_serious_wounds_damage_mass,
            cure_critical_wounds_damage_mass,
        };

        static public BlueprintAbility[] inflict_spells_damage_mass = new BlueprintAbility[] {
            inflict_light_wounds_damage_mass,
            inflict_moderate_wounds_damage_mass,
            inflict_serious_wounds_damage_mass,
            inflict_critical_wounds_damage_mass,
        };
    }
}