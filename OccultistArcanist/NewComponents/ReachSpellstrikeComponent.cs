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
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;

namespace OccultistArcanist.NewComponents
{
    [AllowedOn(typeof(BlueprintUnitFact), false)]
    [AllowMultipleComponents]
    public class ReachSpellStrikeComponent : AutoMetamagicExtender, IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IRulebookHandler<RuleCalculateAbilityParams>, IInitiatorRulebookSubscriber
    {

        static BlueprintBuff spell_strike_buff = BlueprintTools.GetBlueprint<BlueprintBuff>("06e0c9887eb1724409977dac7168bfd7");
        public override bool CanBeUsedOn(BlueprintAbility ability, [CanBeNull] AbilityData data)
        {
            bool is_metamagic_not_available = ability == null || data?.Spellbook == null || ability.Type != AbilityType.Spell
                                          || ((ability.AvailableMetamagic & this.Metamagic) == 0);



            if (is_metamagic_not_available)
            {
                return false;
            }

            var caster = data?.Caster;

            if (caster == null)
            {
                return false;
            }
            if (data.Blueprint.StickyTouch == null && data.Blueprint.GetComponent<AbilityDeliverTouch>() == null)
            {
                return false;
            }

            var unit_part_magus = caster.Get<UnitPartMagus>();

            if (unit_part_magus == null)
            {
                return false;
            }

            if (caster.HasFact(spell_strike_buff))
            {
                return true;
            }

            return false;
        }


        public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            //Main.WotrCContext.Logger.LogHeader("Starting metamagic check for " + evt.Spell.Name);
            if (CanBeUsedOn(evt.Spell, evt.AbilityData))
            {
                //Main.WotrCContext.Logger.LogHeader("Metamagic applied");
                evt.AddMetamagic(this.Metamagic);
            }
        }

        public void OnEventDidTrigger(RuleCalculateAbilityParams evt)
        {
        }
    }
}
