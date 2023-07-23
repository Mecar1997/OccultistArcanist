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

namespace OccultistArcanist.NewContent.NewArchetypes
{
    public static class OccultistArcanist
    {
        static BlueprintCharacterClass archetype_class = BlueprintTools.GetBlueprint<BlueprintCharacterClass>("52dbfd8505e22f84fad8d702611f60b7");
        public static BlueprintArchetype archetype;

        private const string archetype_guid_name = "ArcanistOccultistArchetype";
        private const string archetype_name = "Occultist";
        private const string archetype_description = "Not all arcanists peer inward to discern the deepest secrets of magic. Some look outward, connecting with extraplanar creatures and bartering for secrets, power, and favor.";



        static public BlueprintFeature perfect_summoner;
        static public BlueprintFeature[] occultist_summon_monster = new BlueprintFeature[9];
        static public BlueprintSummonPool occultist_summon_pool;

        static internal void load()
        {
            createArchetype();
            createProgression();
            archetype_class.m_Archetypes = archetype_class.m_Archetypes.AddToArray(archetype.ToReference<BlueprintArchetypeReference>());
        }



        static internal void createArchetype()
        {
            archetype = Helpers.CreateBlueprint<BlueprintArchetype>(Main.HaddeqiModContext, archetype_guid_name + "Archetype", bp => {
                bp.LocalizedName = Helpers.CreateString(Main.HaddeqiModContext, $"{bp.name}.Name", archetype_name);
                bp.LocalizedDescription = Helpers.CreateString(Main.HaddeqiModContext, $"{bp.name}.Description", archetype_description);
                bp.LocalizedDescriptionShort = Helpers.CreateString(Main.HaddeqiModContext, $"{bp.name}.Description", archetype_description);
                bp.m_ParentClass = archetype_class.ToReference<BlueprintCharacterClassReference>();
            });
        }

        static internal void createProgression()
        {
            var arcane_exploits = BlueprintTools.GetBlueprint<BlueprintFeatureSelection>("b8bf3d5023f2d8c428fdf6438cecaea7");
            var arcanist_capstone = archetype_class.Progression.LevelEntries.Where(a => a.Level == 20).First().Features[0];

            createOccultistSummoning();

            archetype.RemoveFeatures = new LevelEntry[] {
                Helpers.CreateLevelEntry(1, arcane_exploits),
                Helpers.CreateLevelEntry(7, arcane_exploits),
                Helpers.CreateLevelEntry(20, arcanist_capstone)
            };

            archetype.AddFeatures = new LevelEntry[] { Helpers.CreateLevelEntry(1, occultist_summon_monster[0]),
                                                        Helpers.CreateLevelEntry(3, occultist_summon_monster[1]),
                                                        Helpers.CreateLevelEntry(5, occultist_summon_monster[2]),
                                                        Helpers.CreateLevelEntry(7, occultist_summon_monster[3]),
                                                        Helpers.CreateLevelEntry(9, occultist_summon_monster[4]),
                                                        Helpers.CreateLevelEntry(11, occultist_summon_monster[5]),
                                                        Helpers.CreateLevelEntry(13, occultist_summon_monster[6]),
                                                        Helpers.CreateLevelEntry(15, occultist_summon_monster[7]),
                                                        Helpers.CreateLevelEntry(17, occultist_summon_monster[8]),
                                                        Helpers.CreateLevelEntry(20, perfect_summoner)
                                                    };


            archetype_class.Progression.UIGroups = archetype_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(occultist_summon_monster.AddToArray(perfect_summoner)));

        }


        static internal void createOccultistSummoning()
        {
            var arcane_reservoir_resource = BlueprintTools.GetBlueprint<BlueprintAbilityResource>("cac948cbbe79b55459459dd6a8fe44ce");
            var augment_summoning = BlueprintTools.GetBlueprint<BlueprintFeature>("38155ca9e4055bb48a89240a2055dcc3");

            perfect_summoner = Helpers.CreateBlueprint<BlueprintFeature>(Main.HaddeqiModContext, "OccultistPerfectSummonerFeature", bp => {
                bp.SetName(Main.HaddeqiModContext, "Perfect Summoner");
                bp.SetDescription(Main.HaddeqiModContext, "At 20th level, an occultist can use her conjurer’s focus without spending points from her arcane reservoir, and the creatures summoned last one day or until dismissed.");
                bp.m_Icon = augment_summoning.m_Icon;
                bp.IsClassFeature = true;
                bp.Groups = new FeatureGroup[] {
                        FeatureGroup.None
                    };
            });


            var monster_tactician_summon_pool = BlueprintTools.GetBlueprint<BlueprintSummonPool>("490248a826bbf904e852f5e3afa6d138");
            var monster_tactician_feats = new BlueprintFeature[]
            {
                BlueprintTools.GetBlueprint<BlueprintFeature>("c19f6f34ed0bc364cbdec88b49a54f67"),
                BlueprintTools.GetBlueprint<BlueprintFeature>("45e466127a8961d40bb3030816ed245b"),
                BlueprintTools.GetBlueprint<BlueprintFeature>("ea26b3a3acb98074fa34f80fcc4e497d"),
                BlueprintTools.GetBlueprint<BlueprintFeature>("03168f4f13ff26f429d912085e88baba"),
                BlueprintTools.GetBlueprint<BlueprintFeature>("00fda605a917fcc4e89612dd31683bdd"),
                BlueprintTools.GetBlueprint<BlueprintFeature>("9b14b05456142914888a48354a0eec17"),
                BlueprintTools.GetBlueprint<BlueprintFeature>("667fd017406abd548b89292edd7dbfb7"),
                BlueprintTools.GetBlueprint<BlueprintFeature>("20d72612311ba914aaba5cc8a4cf312c"),
                BlueprintTools.GetBlueprint<BlueprintFeature>("f63d23b4e41b3264fa6aa2be8079d28d")
            };

            var description = "An occultist can spend 1 point from her arcane reservoir to cast summon monster I. She can cast this spell as a standard action and the summoned creatures remain for 1 minute per level (instead of 1 round per level). At 3rd level and every 2 levels thereafter, the power of this ability increases by one spell level, allowing her to summon more powerful creatures (to a maximum of summon monster IX at 17th level), at the cost of an additional point from her arcane spell reserve per spell level. An occultist cannot have more than one summon monster spell active in this way at one time. If this ability is used again, any existing summon monster immediately ends.";
            occultist_summon_pool = monster_tactician_summon_pool.CreateCopy(Main.HaddeqiModContext, "OccultistSummonPool", bp => { });



            for (int i = 0; i < monster_tactician_feats.Length; i++)
            {
                List<BlueprintAbility> summon_spells = new List<BlueprintAbility>();
                foreach (BlueprintAbility f in monster_tactician_feats[i].GetComponent<AddFacts>().Facts)
                {

                    var ability = f.CreateCopy(Main.HaddeqiModContext, f.name.Replace("MonsterTactician", "Occultist"), bp => {

                        bp.GetComponent<AbilityResourceLogic>().Amount = i + 1;
                        bp.GetComponent<AbilityResourceLogic>().m_RequiredResource = arcane_reservoir_resource.ToReference<BlueprintAbilityResourceReference>();
                        //bp.GetComponent<AbilityResourceLogic>().CostIsCustom = true;

                        for (int j = 0; j < i + 1; j++)
                        {
                            bp.GetComponent<AbilityResourceLogic>().ResourceCostDecreasingFacts.Add(perfect_summoner.ToReference<BlueprintUnitFactReference>());
                        }

                        foreach (var c in bp.GetComponents<ContextRankConfig>())
                        {
                            if (c.IsBasedOnClassLevel)
                            {
                                c.m_Class[0] = archetype_class.ToReference<BlueprintCharacterClassReference>();
                            }
                        }

                        var action_clear_summon_pool = bp.GetComponent<AbilityEffectRunAction>().Actions.Actions[0] as ContextActionClearSummonPool;
                        action_clear_summon_pool.m_SummonPool = occultist_summon_pool.ToReference<BlueprintSummonPoolReference>();


                        var action_spawn_monster = bp.GetComponent<AbilityEffectRunAction>().Actions.Actions[1];
                        if (action_spawn_monster is Conditional)
                        {
                            var conditional = action_spawn_monster as Conditional;
                            action_spawn_monster = conditional.IfTrue.Actions[0];
                            (action_spawn_monster as ContextActionSpawnMonster).m_SummonPool = occultist_summon_pool.ToReference<BlueprintSummonPoolReference>();
                            (action_spawn_monster as ContextActionSpawnMonster).DurationValue = Utils.CreateContextDuration(Utils.CreateContextValue(AbilityRankType.DamageDiceAlternative), DurationRate.Minutes, DiceType.One, (action_spawn_monster as ContextActionSpawnMonster).DurationValue.BonusValue);
                            action_spawn_monster = conditional.IfFalse.Actions[0];
                        }
                        (action_spawn_monster as ContextActionSpawnMonster).m_SummonPool = occultist_summon_pool.ToReference<BlueprintSummonPoolReference>();
                        (action_spawn_monster as ContextActionSpawnMonster).DurationValue = Utils.CreateContextDuration(Utils.CreateContextValue(AbilityRankType.DamageDiceAlternative), DurationRate.Minutes, DiceType.One, (action_spawn_monster as ContextActionSpawnMonster).DurationValue.BonusValue);


                        bp.AddComponent(Helpers.CreateContextRankConfig(ContextRankBaseValueType.FeatureRank, ContextRankProgression.MultiplyByModifier, AbilityRankType.DamageDiceAlternative,
                                                                             feature: perfect_summoner, stepLevel: 60 * 24 - 20)
                                                                             );
                        summon_spells.Add(bp);
                    });

                }

                BlueprintAbility summon_base = null;
                if (summon_spells.Count == 1)
                {
                    summon_base = summon_spells[0];
                }
                else
                {
                    summon_base = Utils.createVariantWrapper($"OccultistSummon{i + 1}Base", "", summon_spells.ToArray());
                    summon_base.SetNameDescription(Main.HaddeqiModContext, "Summon Monster " + Common.roman_id[i + 1], description);
                }


                occultist_summon_monster[i] = Helpers.CreateBlueprint<BlueprintFeature>(Main.HaddeqiModContext, $"OccultistSummonMonster{i + 1}Feature", bp => {
                    bp.SetName(Main.HaddeqiModContext, "Conjurer's Focus: Summon Monster " + Common.roman_id[i + 1]);
                    bp.SetDescription(Main.HaddeqiModContext, description);
                    bp.m_Icon = summon_spells[0].Icon;
                    bp.IsClassFeature = true;
                    bp.Groups = new FeatureGroup[] {
                        FeatureGroup.None
                    };
                });

                occultist_summon_monster[i].AddComponent<AddFacts>(a => { a.m_Facts = new BlueprintUnitFactReference[] { summon_base.ToReference<BlueprintUnitFactReference>() }; });
            }


        }








    }
}
