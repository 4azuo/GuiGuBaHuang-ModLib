using MOD_nE7UL2.Object;
using System;
using System.Collections.Generic;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Monster growth (MonstStrongerEvent, BattleModifyEvent), read from ModConf/monst_stronger_configs.json; the defaults below are the same as the file.
    /// Uses game enums (MonstType): the unit tests only check the keys of the file.
    /// </summary>
    public class MonstStrongerConfig : JsonConfig<MonstStrongerConfig>
    {
        public const string FILE = "monst_stronger_configs.json";
        public override string FileName => FILE;

        /// <summary>Share of every growth that goes to attack / defense / max HP.</summary>
        public float AtkR { get; set; } = 1.05f;
        public float DefR { get; set; } = 0.5f;
        public float MHpR { get; set; } = 1.5f;
        public float Additionnal { get; set; } = 0.02f;
        /// <summary>Growth per total grade of the rebirths.</summary>
        public float RebirthAffect { get; set; } = 0.04f;
        /// <summary>Growth a month, by monster type.</summary>
        public Dictionary<MonstType, float> GrowRate { get; set; } = new Dictionary<MonstType, float>
        {
            [MonstType.Common] = 0.004f,
            [MonstType.Elite] = 0.002f,
            [MonstType.BOSS] = 0.001f,
            [MonstType.NPC] = 0.002f,
        };
        /// <summary>Growth per monster of the type killed.</summary>
        public Dictionary<MonstType, float> KillGrowRate { get; set; } = new Dictionary<MonstType, float>
        {
            [MonstType.Common] = 0.000005f,
            [MonstType.Elite] = 0.00005f,
            [MonstType.BOSS] = 0.0001f,
            [MonstType.NPC] = 0.00005f,
        };
        /// <summary>S&amp;M MonsterPropertiesDependOnPlayerProperties: HP from the attack of the player, by monster type.</summary>
        public Dictionary<MonstType, float> PlayerAtk2Hp { get; set; } = new Dictionary<MonstType, float>
        {
            [MonstType.Common] = 1.0f,
            [MonstType.Elite] = 10.0f,
            [MonstType.BOSS] = 100.0f,
            [MonstType.NPC] = 50.0f,
        };
        /// <summary>Growth by area.</summary>
        public Dictionary<int, float> AreaBonus { get; set; } = new Dictionary<int, float>
        {
            [1] = 0.001f,
            [2] = 0.005f,
            [3] = 0.01f,
            [4] = 0.015f,
            [5] = 0.02f,
            [6] = 0.03f,
            [7] = 0.045f,
            [8] = 0.06f,
            [9] = 0.08f,
            [10] = 0.10f,
            [11] = 0.13f,
            [12] = 0.16f,
            [13] = 0.20f,
        };
        /// <summary>Summons of the player: share of the player's Atk / Def / MHp / basis (Bas).</summary>
        public Dictionary<string, double> PlayerAffectSummon { get; set; } = new Dictionary<string, double>
        {
            ["Atk"] = 0.05,
            ["Def"] = 0.01,
            ["MHp"] = 0.10,
            ["Bas"] = 0.01,
        };

        protected override void Fix(MonstStrongerConfig d)
        {
            GrowRate = GrowRate ?? d.GrowRate;
            // read as KillGrowRate[type] when GrowRate has the type
            KillGrowRate = Fill(KillGrowRate, d.KillGrowRate);
            foreach (var k in GrowRate.Keys)
                if (!KillGrowRate.ContainsKey(k)) KillGrowRate[k] = 0f;
            PlayerAtk2Hp = PlayerAtk2Hp ?? d.PlayerAtk2Hp;
            AreaBonus = AreaBonus ?? d.AreaBonus;
            // read as PlayerAffectSummon["Atk"]...
            PlayerAffectSummon = Fill(PlayerAffectSummon, d.PlayerAffectSummon);
            RebirthAffect = Math.Max(0f, RebirthAffect);
        }
    }
}
