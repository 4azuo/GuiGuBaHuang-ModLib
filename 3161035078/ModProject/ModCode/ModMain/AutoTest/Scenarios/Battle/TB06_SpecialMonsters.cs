using ModLib.Helper;
using ModLib.Mod;
using MOD_nE7UL2.Mod;
using System.Collections.Generic;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Battle
{
    /// <summary>
    /// TB-06 (SpecialMonsterEvent/MonstStrongerEvent, started by code): in a monster wave with the S&amp;M special
    /// monster rate raised for the test, common/elite monsters get a shield and split when killed; kills raise the
    /// "monsters stronger" kill counter, and a monster spawned after the growth counter rose has more HP than the
    /// same monster before. Replaces the manual TM-26 and TM-35.
    /// </summary>
    public class TB06_SpecialMonsters : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TB-06";

        private static bool IsSpecialCandidate(UnitCtrlBase x)
        {
            var m = x?.data?.TryCast<UnitDataMonst>();
            return m != null && !x.isDie && !x.IsPotmon() && !x.IsSummoned() && (m.monstType == MonstType.Common || m.monstType == MonstType.Elite);
        }

        // ShieldUp adds the main effect, whose shield effect (EFX id) is the one listed on the unit
        private static bool HasShield(UnitCtrlBase x) =>
            x.GetEffectInID(BattleManashieldEvent.MANASHIELD_EFFECT_MAIN_ID).Count > 0 || x.GetEffectInID(BattleManashieldEvent.MANASHIELD_EFFECT_EFX_ID).Count > 0;

        // every stat changed by MonstStrongerEvent.OnBattleUnitInto: the test applies the buff again on a live unit,
        // then puts all of them back (a unit left with x25 basis and a big move speed froze the game)
        private static DynInt[] Stats(UnitDataMonst d) => new[] { d.maxHP, d.attack, d.defense, d.moveSpeed,
            d.basisBlade, d.basisEarth, d.basisFinger, d.basisFire, d.basisFist, d.basisFroze, d.basisPalm, d.basisSpear, d.basisSword, d.basisThunder, d.basisWind, d.basisWood };
        private static int[] Snap(UnitDataMonst d) => Stats(d).Select(x => x.baseValue).ToArray();
        private static void Restore(UnitDataMonst d, int[] v, int hp)
        {
            var s = Stats(d);
            for (var i = 0; i < s.Length; i++) s[i].baseValue = v[i];
            d.hp = hp;
        }

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            var specialRate = 0f;
            var counter = 0;
            UnitCtrlBase spawned = null;
            return new Scenario { Id = "TB-06", Feature = "SpecialMonsterEvent/MonstStrongerEvent", Title = "Special monsters (shield, split) and monsters getting stronger", Overlay = false }
                .Step("travel", c => TravelTo(c, FindTown(t => t != null)))
                .Step("join wave", c =>
                {
                    specialRate = SMLocalConfigsEvent.Instance.Configs.AddSpecialMonsterRate;
                    SMLocalConfigsEvent.Instance.Configs.AddSpecialMonsterRate = 1000f;
                    c.Info($"special monster rate {specialRate} -> 1000 for the test");
                    MapBuildBattleEvent.JoinMonstWave(BagTown(c), MapBuildBattleEvent.TOWN_MONST_WAVE_DUNGEON_BASE_ID);
                })
                .Wait("battle loaded", c =>
                {
                    // shields are checked before "Fight": nobody can have broken one yet (a shield absorbs the damage
                    // before the HP, so a full-HP monster may already have lost it once the battle runs)
                    if (InBattle && ModBattleEvent.BattleMonsters.Count > 0 && !c.Bag.ContainsKey("atLoad"))
                    {
                        var mons = ModBattleEvent.BattleMonsters.Where(IsSpecialCandidate).ToList();
                        c.Bag["atLoad"] = mons.Count;
                        c.Bag["atLoadShielded"] = mons.Count(HasShield);
                        c.Bag["atLoadStarted"] = BattleStarted;
                    }
                    if (c.Bag.ContainsKey("atLoad") || !InBattle) StartBattle();
                    return InBattle && ModBattleEvent.BattleMonsters.Count > 0 && c.Bag.ContainsKey("atLoad");
                }, 90000, 1000)
                .Wait("battle started", c => StartBattle(), 60000, 1000)
                .Step("long wave", c =>
                {
                    // each kill takes one monster from the wave: keep enough so the wave does not end during the test
                    MapBuildBattleEvent.AddTeamCount(MapBuildBattleEvent.TeamSideEnum.TeamB, 100);
                    c.Info($"monsters left in the wave: {MapBuildBattleEvent.TeamBUnitCount}");
                    // the x1000 rate is only for the spawned monsters (shields) and for the 3 kills below: left on, every
                    // monster killed by the guards splits and its replacement splits again -> exponential monster count, game frozen
                    c.Bag["spawned"] = ModBattleEvent.BattleMonsters.Where(IsSpecialCandidate).ToList();
                    SMLocalConfigsEvent.Instance.Configs.AddSpecialMonsterRate = specialRate;
                }, 500)
                .Step("shields", c =>
                {
                    if (!InBattle) { c.Assert(false, "still in the battle (the wave ended too early)"); return; }
                    // monsters spawned while the rate was x1000 (replacements spawned later use the normal rate)
                    var mons = ((System.Collections.Generic.List<UnitCtrlBase>)c.Bag["spawned"]).Where(IsSpecialCandidate).ToList();
                    if (!c.Assert(mons.Count > 0, $"{mons.Count} common/elite monsters")) return;
                    var shielded = mons.Count(HasShield);
                    var m0 = mons[0];
                    var md = m0.data.TryCast<UnitDataMonst>();
                    var gameLvl = GameHelper.GetGameLevel().Parse<int>();
                    c.Info($"game level {GameHelper.GetGameLevel()} -> {gameLvl}, monster grade {md.grade.value}, shield chance {SMLocalConfigsEvent.Instance.Calculate(SpecialMonsterEvent.MONST_SHIELD_CHANCE * md.grade.value * gameLvl, SMLocalConfigsEvent.Instance.Configs.AddSpecialMonsterRate):0.##}%");
                    c.Info($"effects of monster #1: {string.Join(", ", m0.effects.ToArray().Select(x => x?.data?.effectItem?.id.ToString() ?? "?"))}");
                    c.Info($"shields counted at load, battle already started: {c.Bag["atLoadStarted"]}");
                    c.Assert(shielded > 0, $"monsters spawned with a shield: {shielded}/{mons.Count}");
                    c.AssertEqual((int)c.Bag["atLoad"], (int)c.Bag["atLoadShielded"], "every common/elite monster spawned with a shield (special rate x1000, checked before the fight)");
                    if (shielded == 0)
                    {
                        SpecialMonsterEvent.Instance.OnBattleUnitInto(m0);
                        c.Info($"after calling SpecialMonsterEvent.OnBattleUnitInto again: shield {HasShield(m0)}, effects {string.Join(", ", m0.effects.ToArray().Select(x => x?.data?.effectItem?.id.ToString() ?? "?"))}");
                    }
                }, 2000)
                .Step("stronger", c =>
                {
                    if (!InBattle) { c.Assert(false, "still in the battle (the wave ended too early)"); return; }
                    var m = ModBattleEvent.BattleMonsters.FirstOrDefault(IsSpecialCandidate);
                    if (m == null) { c.Skip("no monster"); return; }
                    var data = m.data.TryCast<UnitDataMonst>();
                    counter = MonstStrongerEvent.Instance.Counter;
                    var snap = Snap(data); var hpNow = data.hp; var hp0 = data.maxHP.baseValue;
                    MonstStrongerEvent.Instance.OnBattleUnitInto(m);
                    var gainNormal = data.maxHP.baseValue - hp0;
                    Restore(data, snap, hpNow);
                    MonstStrongerEvent.Instance.Counter = counter + 100;
                    try { MonstStrongerEvent.Instance.OnBattleUnitInto(m); }
                    finally { MonstStrongerEvent.Instance.Counter = counter; }
                    var gainRaised = data.maxHP.baseValue - hp0;
                    Restore(data, snap, hpNow);
                    c.Info($"monster {data.unitAttrItem.id} ({data.monstType}) base max HP {hp0}; growth counter {counter}");
                    c.Assert(gainRaised > gainNormal, $"HP buff with counter {counter}: +{gainNormal}, with {counter + 100}: +{gainRaised}");
                    c.Assert(gainNormal >= 0 && gainRaised >= 0, "buffs are not negative");
                    c.AssertEqual(hp0, data.maxHP.baseValue, "unit stats restored after the measure");
                }, 1000)
                .Step("huge counters", c =>
                {
                    // after a very long game the growth/kill counters are huge: the buff must stay at the int max,
                    // not wrap to a negative value or fall back to 0 (Parse<int> returns 0 on overflow)
                    if (!InBattle) { c.Assert(false, "still in the battle (the wave ended too early)"); return; }
                    var m = ModBattleEvent.BattleMonsters.FirstOrDefault(IsSpecialCandidate);
                    if (m == null) { c.Skip("no monster"); return; }
                    var d = m.data.TryCast<UnitDataMonst>();
                    int hp0 = d.maxHP.baseValue, atk0 = d.attack.baseValue, def0 = d.defense.baseValue, bas0 = d.basisBlade.baseValue;
                    var snap = Snap(d); var hpNow = d.hp;
                    var kc = MonstStrongerEvent.Instance.KillCounter.ToDictionary(x => x.Key, x => x.Value);
                    counter = MonstStrongerEvent.Instance.Counter;
                    try
                    {
                        MonstStrongerEvent.Instance.Counter = 10000000;
                        foreach (var k in kc.Keys) MonstStrongerEvent.Instance.KillCounter[k] = 100000000;
                        MonstStrongerEvent.Instance.OnBattleUnitInto(m);
                        c.Info($"buff with huge counters: HP {hp0} -> {d.maxHP.baseValue}, Atk {atk0} -> {d.attack.baseValue}, Def {def0} -> {d.defense.baseValue}, blade basis {bas0} -> {d.basisBlade.baseValue}");
                        c.Assert(d.maxHP.baseValue >= hp0 && d.maxHP.baseValue > 0, "max HP did not wrap/fall back");
                        c.Assert(d.attack.baseValue >= atk0 && d.attack.baseValue > 0, "attack did not wrap/fall back");
                        c.Assert(d.defense.baseValue >= def0, "defense did not wrap/fall back");
                        c.Assert(d.basisBlade.baseValue >= bas0, "basis did not wrap/fall back");
                    }
                    catch (System.Exception ex) { c.Assert(false, $"buff with huge counters threw {ex.GetType().Name}: {ex.Message}"); }
                    finally
                    {
                        MonstStrongerEvent.Instance.Counter = counter;
                        foreach (var k in kc) MonstStrongerEvent.Instance.KillCounter[k.Key] = k.Value;
                        Restore(d, snap, hpNow);
                    }
                }, 1000)
                .Step("huge hit", c =>
                {
                    // the Taoist damage pipeline multiplies the damage (time, crit x2..x4): a huge hit must not
                    // wrap to a negative damage that heals the target
                    if (!InBattle) { c.Assert(false, "still in the battle (the wave ended too early)"); return; }
                    var m = ModBattleEvent.BattleMonsters.FirstOrDefault(IsSpecialCandidate);
                    if (m == null) { c.Skip("no monster"); return; }
                    var d = m.data.TryCast<UnitDataMonst>();
                    var hp0 = d.maxHP.baseValue;
                    d.maxHP.baseValue = int.MaxValue / 2;
                    d.hp = int.MaxValue / 2;
                    var before = d.hp;
                    try
                    {
                        // 10 hits: the pipeline adds time/basis/MP damage and may crit (x2..x4): no hit may wrap to a heal
                        var healed = 0;
                        for (var i = 0; i < 10 && !m.isDie; i++)
                        {
                            var hp = d.hp;
                            PlayerHit(m, int.MaxValue / 3);
                            if (!m.isDie && d.hp > hp) healed++;
                        }
                        c.Assert(healed == 0, $"10 hits of {int.MaxValue / 3}: hp {before} -> {(m.isDie ? 0 : d.hp)}, hits that healed: {healed}");
                    }
                    catch (System.Exception ex) { c.Assert(false, $"huge hit threw {ex.GetType().Name}: {ex.Message}"); }
                    finally { if (!m.isDie) { d.maxHP.baseValue = hp0; d.hp = hp0; } }
                }, 1000)
                .Step("kill: split", c =>
                {
                    if (!InBattle) { c.Assert(false, "still in the battle (the wave ended too early)"); return; }
                    var kc = MonstStrongerEvent.Instance.KillCounter.ToDictionary(x => x.Key, x => x.Value);
                    var split = SpecialMonsterEvent.Instance.MultipliedUnits.Count;
                    var targets = ModBattleEvent.BattleMonsters.Where(IsSpecialCandidate).Where(x => x != spawned).Take(3).ToList();
                    var types = targets.Select(x => x.data.TryCast<UnitDataMonst>().monstType).ToList();
                    SMLocalConfigsEvent.Instance.Configs.AddSpecialMonsterRate = 1000f;
                    try { foreach (var t in targets) PlayerKill(t); }
                    finally { SMLocalConfigsEvent.Instance.Configs.AddSpecialMonsterRate = specialRate; }
                    c.Assert(targets.All(x => x.isDie), $"{targets.Count} monsters killed");
                    foreach (var t in types.Distinct())
                        c.AssertEqual(kc[t] + types.Count(x => x == t), MonstStrongerEvent.Instance.KillCounter[t], $"{t} kill counter");
                    c.Assert(SpecialMonsterEvent.Instance.MultipliedUnits.Count > split, $"killed monsters split: {split} -> {SpecialMonsterEvent.Instance.MultipliedUnits.Count} new monsters");
                }, 1500)
                .Step("win", c => { if (InBattle) EndBattle(true); }, 1500)
                .Wait("back to the map", c => LeaveBattle(), 90000, 2000)
                .Step("after battle", c =>
                {
                    SMLocalConfigsEvent.Instance.Configs.AddSpecialMonsterRate = specialRate;
                    c.Assert(OnMap, "player back on the world map");
                    c.AssertEqual(0, SpecialMonsterEvent.Instance.MultipliedUnits.Count, "split list cleared at battle end");
                }, 2000);
        }
    }
}
