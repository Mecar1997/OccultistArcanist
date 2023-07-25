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

namespace OccultistArcanist.NewComponents
{
    [AllowedOn(typeof(BlueprintBuff))]
    public class MetamagicOnSpell : AutoMetamagicExtender, IInitiatorRulebookHandler<RuleCastSpell>, IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IRulebookHandler<RuleCalculateAbilityParams>, IInitiatorRulebookSubscriber
    {
        public SpellDescriptorWrapper spell_descriptor;
        public BlueprintAbilityResource resource = null;
        public int amount;
        public BlueprintUnitFact[] cost_reducing_facts = new BlueprintUnitFact[0];
        private int cost_to_pay;
        public BlueprintSpellbook spellbook = null;
        public BlueprintCharacterClass specific_class = null;
        public bool limit_spell_level;

        private int calculate_cost(UnitEntityData caster)
        {
            var cost = amount;
            foreach (var f in cost_reducing_facts)
            {
                if (caster.Buffs.HasFact(f))
                {
                    cost--;
                }
            }
            return cost < 0 ? 0 : cost;
        }


        public override bool CanBeUsedOn(BlueprintAbility ability, [CanBeNull] AbilityData data)
        {
            bool is_metamagic_not_available = ability == null || data?.Spellbook == null || ability.Type != AbilityType.Spell
                                          || ((ability.AvailableMetamagic & Metamagic) == 0);

            if (is_metamagic_not_available)
            {
                return false;
            }

            if (!Common.checkSpellbook(spellbook, specific_class, data?.Spellbook, this.Owner))
            {
                return false;
            }

            if (this.Abilities != null && !this.Abilities.Empty() && !this.Abilities.Contains(ability))
            {
                return false;
            }

            if (!spell_descriptor.HasAnyFlag(data.Blueprint.SpellDescriptor) && spell_descriptor != SpellDescriptor.None)
            {
                return false;
            }

            int cost = calculate_cost(this.Owner);
            if (resource != null && this.Owner.Resources.GetResourceAmount((BlueprintScriptableObject)this.resource) < cost)
            {
                return false;
            }


            if (limit_spell_level && data.Spellbook.MaxSpellLevel < data.SpellLevel + Metamagic.DefaultCost())
            {
                return false;
            }

            return true;
        }


        public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            cost_to_pay = 0;
            if (!CanBeUsedOn(evt.Spell, evt.AbilityData))
            {
                return;
            }
            cost_to_pay = calculate_cost(this.Owner);
            evt.AddMetamagic(Metamagic);
        }

        public void OnEventDidTrigger(RuleCalculateAbilityParams evt)
        {
        }

        public void OnEventAboutToTrigger(RuleCastSpell evt)
        {
            if (cost_to_pay == 0 || evt.Spell.SourceItem != null)
            {
                cost_to_pay = 0;
                return;
            }
        }

        public void OnEventDidTrigger(RuleCastSpell evt)
        {

            if (cost_to_pay == 0)
            {
                return;
            }
            this.Owner.Resources.Spend(resource, cost_to_pay);
            cost_to_pay = 0;
        }
    }
}
