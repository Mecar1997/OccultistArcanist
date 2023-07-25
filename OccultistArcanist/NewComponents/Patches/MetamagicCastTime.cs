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
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace OccultistArcanist.NewComponents
{
    public static class MetamagicCastTime
    {
        public static bool checkFullRound(AbilityData __instance)
        {
            if (!__instance.IsArcanist && __instance.ConvertedFrom == null)
                return false;

            if (__instance.HasMetamagic(Metamagic.Quicken))
                return false;

            if (__instance.ConvertedFrom != null)
            {
                return __instance.ConvertedFrom.m_CachedName?.StartsWith("(Spontaneous)") == true || __instance.m_CachedName?.StartsWith("(Spontaneous)") == true;
            }
            else
            {
                return __instance.m_CachedName?.StartsWith("(Spontaneous)") == true;
            }
        }

        /*[HarmonyPatch(typeof(AbilityData), nameof(AbilityData.GetDefaultActionType))]
        private static class AbilityData_GetDefaultActionType_Patch
        {
            private static void Postfix(AbilityData __instance, ref CommandType __result)
            {
                if (checkFullRound(__instance))
                    __result = CommandType.Standard;
            }
        }

        [HarmonyPatch(typeof(AbilityData), nameof(AbilityData.RequireFullRoundAction), MethodType.Getter)]
            private static class AbilityData_RequireFullRoundAction_Patch {
                private static void Postfix(AbilityData __instance, ref bool __result) {

                if (checkFullRound(__instance))
                        __result = true;

                    if (!__instance.Blueprint.IsFullRoundAction && __result) {
                        if (__instance.MetamagicData.MetamagicMask == 0) {
                            return;
                        }

                        var fast_metamagic = __instance.Caster.Unit.Get<UnitPartSpontaneousMetamagic>();
                        if (fast_metamagic == null) {
                            return;
                        }

                        if (!__instance.Blueprint.IsSpell) {
                            return;
                        }

                        __result = !fast_metamagic.canBeUsedOnAbility(__instance);
                    }
                }
            }*/
    }
}
