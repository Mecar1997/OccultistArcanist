﻿using Epic.OnlineServices;
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
using Kingmaker.UnitLogic;

namespace OccultistArcanist.NewComponents
{
    [AllowedOn(typeof(BlueprintParametrizedFeature))]
    public class SpontaneousMetamagicMasteryComponent : UnitFactComponentDelegate, INoSpontaneousMetamagicTimeIncrease
    {

        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartSpontaneousMetamagic>().addBuff(this.Fact);
        }

        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartSpontaneousMetamagic>().removeBuff(this.Fact);
        }

        public bool canUseOnAbility(AbilityData ability)
        {

            return true;
        }
    }
}
