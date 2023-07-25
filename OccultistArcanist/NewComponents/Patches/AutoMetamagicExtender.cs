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
using JetBrains.Annotations;
using System;

namespace OccultistArcanist.NewComponents
{
    public class AutoMetamagicExtender : AutoMetamagic
    {

        virtual public bool CanBeUsedOn(BlueprintAbility ability, [CanBeNull] AbilityData data)
        {
            return true;
        }
    }

    [HarmonyPatch(typeof(AutoMetamagic), nameof(AutoMetamagic.ShouldApplyTo), new Type[] { typeof(AutoMetamagic), typeof(BlueprintAbility), typeof(AbilityData)})]
    static class AutoMetamagic_ShouldApplyTo_Patch
    {
        static void Postfix(AutoMetamagic c, BlueprintAbility ability, AbilityData data, ref bool __result)
        {
            if (c is AutoMetamagicExtender)
            {
                __result = ((AutoMetamagicExtender)c).CanBeUsedOn(ability, data);
            }

        }
    }

}
