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

namespace OccultistArcanist.NewComponents
{
    [AllowedOn(typeof(BlueprintUnitFact))]
    public class ArcanistMetamixing : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCastSpell>, IRulebookHandler<RuleCastSpell>, ISubscriber, IInitiatorRulebookSubscriber, INoSpontaneousMetamagicTimeIncrease
    {
        public BlueprintCharacterClass class_to_use;
        public BlueprintAbilityResource resource;
        public int amount = 1;
        public int max_metamagics = 1;

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
            if (ability == null || ability?.Blueprint == null)
            {
                return false;
            }

            if (!ability.Blueprint.IsSpell)
            {
                return false;
            }

            if (ability.Spellbook.Blueprint.CharacterClass != class_to_use)
            {
                return false;
            }

            if (ability.MetamagicData == null)
            {
                return false;
            }


            if (!this.Owner.Resources.HasEnoughResource(resource, amount))
            {
                return false;
            }

            if (ability.ConvertedFrom != null)
            {
                if (ability.ConvertedFrom.m_CachedName.Contains("Metamixing"))
                {
                    return true;
                }
            }
            else if (ability.m_CachedName.Contains("Metamixing"))
            {
                return true;
            }



            if (ability.ConvertedFrom != null)
            {
                if (!ability.ConvertedFrom.m_CachedName.Contains("(Spontaneous)"))
                {
                    return false;
                }
            }
            else if (!ability.m_CachedName.Contains("(Spontaneous)"))
            {
                return false;
            }


            int metamagic_count = Common.PopulationCount((int)(ability.MetamagicData.MetamagicMask));
            if (metamagic_count > 1)
            {
                return false;
            }

            return true;
        }

        public void OnEventAboutToTrigger(RuleCastSpell evt)
        {

        }

        public void OnEventDidTrigger(RuleCastSpell evt)
        {
            var spellbook_blueprint = evt.Spell?.Spellbook?.Blueprint;
            if (spellbook_blueprint == null)
            {
                return;
            }

            if (!canUseOnAbility(evt.Spell))
            {
                return;
            }

            if (evt.Spell.StickyTouch != null)
            {
                return;
            }


            if (evt.Spell.MetamagicData == null || (evt.Spell.MetamagicData.MetamagicMask != 0 && (evt.Spell.MetamagicData.MetamagicMask & Metamagic.Quicken) != 0))
            {
                return;
            }



            if (this.Owner.Resources.GetResourceAmount(resource) >= amount)
            {
                this.Owner.Resources.Spend((BlueprintScriptableObject)this.resource, amount);
            }


        }

    }
}
