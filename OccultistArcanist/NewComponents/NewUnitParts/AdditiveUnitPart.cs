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
using Kingmaker.UnitLogic;
using Newtonsoft.Json;
using System;

namespace OccultistArcanist.NewComponents
{
    public class AdditiveUnitPart : UnitPart
    {
        [JsonProperty]
        protected List<UnitFact> buffs = new List<UnitFact>();

        public virtual void addBuff(UnitFact buff)
        {
            if (!buffs.Contains(buff))
            {
                buffs.Add(buff);
            }
        }

        public virtual void removeBuff(UnitFact buff)
        {
            buffs.Remove(buff);
            if (buffs.Empty())
            {
                Aux.removePart(this);
            }
        }
    }

    static public class Aux
    {
        static public void removePart(UnitPart part)
        {

            var owner = part.Owner;
            Type part_type = part.GetType();

            var remove_method = owner.GetType().GetMethod(nameof(UnitDescriptor.Remove));
            remove_method.MakeGenericMethod(part_type).Invoke(owner, null);
        }
    }

}
