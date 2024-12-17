﻿using MOD_nE7UL2.Const;
using ModLib.Enum;
using ModLib.Mod;
using ModLib.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UIHelper;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.TRAINER_EVENT)]
    public class TrainerEvent : ModEvent
    {
        public const string TITLE = "Mini Trainer";
        public const float TRAINER_BTN_WIDTH = 200;
        public const float TRAINER_BTN_HEIGHT = 36;
        public const float TELE_BTN_WIDTH = 250;
        public const float TELE_BTN_HEIGHT = 28;
        public const int MAX_NUMBER = 100000000;

        private UIHelper.UICustom1 uiTrainer;
        private UIHelper.UICustom1 uiTele;

        private void ClearTempVar()
        {
            uiTrainer = null;
            uiTele = null;
        }

        public override void OnMonoUpdate()
        {
            base.OnMonoUpdate();
            var smConfigs = EventHelper.GetEvent<SMLocalConfigsEvent>(ModConst.SM_LOCAL_CONFIGS_EVENT);
            if (smConfigs.Configs.EnableTrainer)
            {
                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Alpha1))
                {
                    OpenTrainer();
                }
                else
                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Alpha2))
                {
                    OpenTeleport();
                }
            }
        }

        private void OpenTrainer()
        {
            var player = g.world.playerUnit;

            uiTrainer = new UIHelper.UICustom1(TITLE, (uiTrainer) =>
            {
                uiTrainer.UI.isFastClose = true;

                int col, row;
                uiTrainer.AddText(uiTrainer.MidCol, uiTrainer.FirstRow, "Cheating will destroy your experience.").Format(Color.red, 17);

                col = 2; row = 2;
                uiTrainer.AddText(col - 1, row - 1, "(HP/MP/SP/Mood/Stanima/Health)").Format(null, 13).Align(TextAnchor.MiddleLeft);
                FormatButton1(uiTrainer.AddButton(col, row, Recover, "Recover ALL"));
                FormatButton1(uiTrainer.AddButton(col, row += 2, AddMaxHP, "+100000 Max HP"));
                FormatButton1(uiTrainer.AddButton(col, row += 2, ReduceMaxHP, "-100000 Max HP"));
                FormatButton1(uiTrainer.AddButton(col, row += 2, AddMaxMP, "+10000 Max MP"));
                FormatButton1(uiTrainer.AddButton(col, row += 2, ReduceMaxMP, "-10000 Max MP"));
                FormatButton1(uiTrainer.AddButton(col, row += 2, AddMaxSP, "+1000 Max SP"));
                FormatButton1(uiTrainer.AddButton(col, row += 2, ReduceMaxSP, "-1000 Max SP"));
                FormatButton1(uiTrainer.AddButton(col, row += 2, AddBasis, "+100 ALL Basises"));
                FormatButton1(uiTrainer.AddButton(col, row += 2, ReduceBasis, "-100 ALL Basises"));
                FormatButton1(uiTrainer.AddButton(col, row += 2, AddAtk, "+10000 Attack"));
                FormatButton1(uiTrainer.AddButton(col, row += 2, ReduceAtk, "-10000 Attack"));
                FormatButton1(uiTrainer.AddButton(col, row += 2, AddDef, "+1000 Defence"));
                FormatButton1(uiTrainer.AddButton(col, row += 2, ReduceDef, "-1000 Defence"));

                col = 8; row = 2;
                FormatButton1(uiTrainer.AddButton(col, row, OpenTeleport, "Teleport"));
                FormatButton1(uiTrainer.AddButton(col, row += 2, AddMoney, "+1000000 Money"));
                FormatButton1(uiTrainer.AddButton(col, row += 2, ReduceMoney, "-1000000 Money"));
                FormatButton1(uiTrainer.AddButton(col, row += 2, AddDegree, "+1000 Degree"));
                FormatButton1(uiTrainer.AddButton(col, row += 2, ReduceDegree, "-1000 Degree"));
                FormatButton1(uiTrainer.AddButton(col, row += 2, AddContribution, "+100000 Contribution"));
                FormatButton1(uiTrainer.AddButton(col, row += 2, ReduceContribution, "-100000 Contribution"));
                FormatButton1(uiTrainer.AddButton(col, row += 2, AddAbilityExp, "+100000 Ability Point"));
                FormatButton1(uiTrainer.AddButton(col, row += 2, ReduceAbilityExp, "-100000 Ability Point"));

                col = 14; row = 2;
                FormatButton1((UIItemBase.UIItemButton)uiTrainer.AddButton(col, row, StopGame, "{0}").SetWork(new UIItemBase.UIItemWork
                {
                    Formatter = (x) => new object[] { Time.timeScale == 0 ? "Resume Game" : "Stop Game" },
                }));
                FormatButton1(uiTrainer.AddButton(col, row += 2, () => SpeedGame(1), "Game Speed x1"));
                FormatButton1(uiTrainer.AddButton(col, row += 2, () => SpeedGame(2), "Game Speed x2"));
                FormatButton1(uiTrainer.AddButton(col, row += 2, () => SpeedGame(3), "Game Speed x3"));
                FormatButton1(uiTrainer.AddButton(col, row += 2, Leveldown, "Leveldown"));
                FormatButton1(uiTrainer.AddButton(col, row += 2, AddExp, "+10000 Exp"));
                FormatButton1(uiTrainer.AddButton(col, row += 2, AddLife, "+100 Yearlfie"));
                FormatButton1(uiTrainer.AddButton(col, row += 2, ReduceLife, "-100 Yearlfie"));
                FormatButton1(uiTrainer.AddButton(col, row += 2, AddFootspeed, "+1000 Travel speed"));
                FormatButton1(uiTrainer.AddButton(col, row += 2, ReduceFootspeed, "-1000 Travel speed"));
                FormatButton1(uiTrainer.AddButton(col, row += 2, AddViewRange, "+10 View-range"));
                FormatButton1(uiTrainer.AddButton(col, row += 2, ReduceViewRange, "-10 View-range"));

                col = 24; row = 2;
                uiTrainer.AddText(col - 1, row++, "General Properties:").Format(null, 17, FontStyle.Italic).Align(TextAnchor.MiddleCenter);
                uiTrainer.AddText(col, row, "HP: {0}/{1}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Hp).value, player.GetDynProperty(UnitDynPropertyEnum.HpMax).value },
                });
                uiTrainer.AddText(col - 3, row++, "{0} :Money").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetUnitMoney() },
                });
                uiTrainer.AddText(col, row, "MP: {0}/{1}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Mp).value, player.GetDynProperty(UnitDynPropertyEnum.MpMax).value },
                });
                uiTrainer.AddText(col - 3, row++, "{0} :Degree").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetUnitMayorDegree() },
                });
                uiTrainer.AddText(col, row, "SP: {0}/{1}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Sp).value, player.GetDynProperty(UnitDynPropertyEnum.SpMax).value },
                });
                uiTrainer.AddText(col - 3, row++, "{0} :Contribution").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetUnitContribution() },
                });
                uiTrainer.AddText(col, row, "Atk: {0}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Attack).value },
                });
                uiTrainer.AddText(col - 3, row++, "{0} :Def").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Defense).value },
                });
                uiTrainer.AddText(col, row, "Mood: {0}/{1}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Mood).value, player.GetDynProperty(UnitDynPropertyEnum.MoodMax).value },
                });
                uiTrainer.AddText(col - 3, row++, "{0}/{1} :Stanima").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Energy).value, player.GetDynProperty(UnitDynPropertyEnum.EnergyMax).value },
                });
                uiTrainer.AddText(col, row, "Heath: {0}/{1}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Health).value, player.GetDynProperty(UnitDynPropertyEnum.HealthMax).value },
                });
                uiTrainer.AddText(col - 3, row++, "{0}/{1} :Life").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Age).value, player.GetDynProperty(UnitDynPropertyEnum.Life).value },
                });
                uiTrainer.AddText(col, row, "Travel Speed: {0}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.FootSpeed).value },
                });
                uiTrainer.AddText(col - 3, row++, "{0} :View Range").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.PlayerView).value },
                });
                uiTrainer.AddText(col, row, "Battle Speed: {0}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.MoveSpeed).value },
                });
                uiTrainer.AddText(col - 3, row++, "Ability: {0} ({1})").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
                {
                    Formatter = (x) => new object[]
                    {
                        player.GetDynProperty(UnitDynPropertyEnum.AbilityExp).value,
                        player.GetDynProperty(UnitDynPropertyEnum.AbilityPoint).value
                    },
                });
                uiTrainer.AddText(col, row, "Luck: {0}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Luck).value },
                });
                uiTrainer.AddText(col - 3, row++, "{0} :Insight").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Talent).value },
                });
                uiTrainer.AddText(col - 1, row++, "Grade: {0} {1} ― {2}").Align(TextAnchor.MiddleCenter).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
                {
                    Formatter = (x) =>
                    {
                        var grade = player.GetGradeConf();
                        return new object[]
                        {
                    GameTool.LS(g.conf.roleGrade.GetItem(grade.id).gradeName),
                    GameTool.LS(g.conf.roleGrade.GetItem(grade.id).phaseName),
                    GameTool.LS(g.conf.roleGrade.GetItem(grade.id).qualityName)
                        };
                    },
                });
                uiTrainer.AddText(col - 1, row++, "Exp: {0}/{1}").Align(TextAnchor.MiddleCenter).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Exp).value, player.GetMaxExpCurrentPhase() },
                });

                row++;
                uiTrainer.AddText(col - 1, row++, "Martial/Spiritual:").Format(null, 17, FontStyle.Italic).Align(TextAnchor.MiddleCenter);
                uiTrainer.AddText(col, row, "Fire: {0}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.BasisFire).value },
                });
                uiTrainer.AddText(col - 3, row++, "{0} :Blade").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.BasisBlade).value },
                });
                uiTrainer.AddText(col, row, "Water: {0}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.BasisFroze).value },
                });
                uiTrainer.AddText(col - 3, row++, "{0} :Spear").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.BasisSpear).value },
                });
                uiTrainer.AddText(col, row, "Lightning: {0}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.BasisThunder).value },
                });
                uiTrainer.AddText(col - 3, row++, "{0} :Sword").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.BasisSword).value },
                });
                uiTrainer.AddText(col, row, "Wind: {0}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.BasisWind).value },
                });
                uiTrainer.AddText(col - 3, row++, "{0} :Fist").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.BasisFist).value },
                });
                uiTrainer.AddText(col, row, "Earth: {0}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.BasisEarth).value },
                });
                uiTrainer.AddText(col - 3, row++, "{0} :Palm").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.BasisPalm).value },
                });
                uiTrainer.AddText(col, row, "Wood: {0}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.BasisWood).value },
                });
                uiTrainer.AddText(col - 3, row++, "{0} :Finger").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.BasisFinger).value },
                });

                row++;
                uiTrainer.AddText(col - 1, row++, "Artisanship:").Format(null, 17, FontStyle.Italic).Align(TextAnchor.MiddleCenter);
                uiTrainer.AddText(col, row, "Alchemy: {0}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.RefineElixir).value },
                });
                uiTrainer.AddText(col - 3, row++, "{0} :Forge").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.RefineWeapon).value },
                });
                uiTrainer.AddText(col, row, "Feng Shui: {0}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Geomancy).value },
                });
                uiTrainer.AddText(col - 3, row++, "{0} :Talismans").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Symbol).value },
                });
                uiTrainer.AddText(col, row, "Herbology: {0}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Herbal).value },
                });
                uiTrainer.AddText(col - 3, row++, "{0} :Mining").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Mine).value },
                });
            }, ClearTempVar);
        }

        private void FormatButton1(UIItemBase.UIItemButton btn)
        {
            btn.Format(Color.black, 15).Size(TRAINER_BTN_WIDTH, TRAINER_BTN_HEIGHT);
        }

        public override void OnTimeUpdate()
        {
            base.OnTimeUpdate();
            if (uiTrainer != null)
            {
                uiTrainer.UpdateUI();
            }
        }

        private void Recover()
        {
            var player = g.world.playerUnit;
            player.SetProperty<int>(UnitPropertyEnum.Hp, int.MaxValue);
            player.SetProperty<int>(UnitPropertyEnum.Mp, int.MaxValue);
            player.SetProperty<int>(UnitPropertyEnum.Sp, int.MaxValue);
            player.SetProperty<int>(UnitPropertyEnum.Mood, int.MaxValue);
            player.SetProperty<int>(UnitPropertyEnum.Energy, int.MaxValue);
            player.SetProperty<int>(UnitPropertyEnum.Health, int.MaxValue);
        }

        private static readonly Dictionary<int, MultiValue> AREA_COL = new Dictionary<int, MultiValue>
        {
            [1] = MultiValue.Create(0, new Func<MapBuildBase, bool>((x) => x.IsSchool() || x.IsTown())),
            [2] = MultiValue.Create(0, new Func<MapBuildBase, bool>((x) => x.IsSchool() || x.IsTown())),
            [3] = MultiValue.Create(0, new Func<MapBuildBase, bool>((x) => true)),
            [4] = MultiValue.Create(1, new Func<MapBuildBase, bool>((x) => x.IsSchool() || x.IsTown())),
            [5] = MultiValue.Create(1, new Func<MapBuildBase, bool>((x) => true)),
            [6] = MultiValue.Create(2, new Func<MapBuildBase, bool>((x) => x.IsSchool() || x.IsTown())),
            [7] = MultiValue.Create(2, new Func<MapBuildBase, bool>((x) => true)),
            [8] = MultiValue.Create(3, new Func<MapBuildBase, bool>((x) => x.IsSchool() || x.IsTown())),
            [9] = MultiValue.Create(3, new Func<MapBuildBase, bool>((x) => true)),
            [10] = MultiValue.Create(4, new Func<MapBuildBase, bool>((x) => x.IsSchool() || x.IsTown())),
            [11] = MultiValue.Create(4, new Func<MapBuildBase, bool>((x) => true)),
            [12] = MultiValue.Create(4, new Func<MapBuildBase, bool>((x) => true)),
            [13] = MultiValue.Create(4, new Func<MapBuildBase, bool>((x) => true)),
        };
        private void OpenTeleport()
        {
            if (!g.ui.HasUI(UIType.MapMain))
            {
                var uiConfirm = g.ui.OpenUI<UICheckPopup>(UIType.CheckPopup);
                uiConfirm.InitData("Teleport", "You have to on map!", 1);
                return;
            }
            uiTele = new UIHelper.UICustom1(TITLE, (uiTele) =>
            {
                uiTele.UI.isFastClose = true;

                var row = new Dictionary<int, int>
                {
                    [0] = 0,
                    [1] = 0,
                    [2] = 0,
                    [3] = 0,
                    [4] = 0,
                };
                int col = 0, areaId = -1;
                foreach (var build in g.world.build.GetBuilds().ToArray().Where(x => !string.IsNullOrEmpty(x.name) && AREA_COL[x.gridData.areaBaseID].Value0.Parse<int>() >= 0 && ((Func<MapBuildBase, bool>)AREA_COL[x.gridData.areaBaseID].Value1).Invoke(x))
                    .OrderBy(x => AREA_COL[x.gridData.areaBaseID].Value0.Parse<int>()).ThenBy(x => x.gridData.areaBaseID).ThenBy(x => x.name))
                {
                    if (areaId != build.gridData.areaBaseID)
                    {
                        areaId = build.gridData.areaBaseID;
                        col = AREA_COL[areaId].Value0.Parse<int>() * 6 + 3;
                        uiTele.AddText(col, row[AREA_COL[areaId].Value0.Parse<int>()]++, $"Area {areaId}:").Format(null, 16, FontStyle.Italic).Align(TextAnchor.MiddleCenter);
                    }
                    FormatButton2(uiTele.AddButton(col, row[AREA_COL[areaId].Value0.Parse<int>()]++, () => Tele(build.GetOpenBuildPoints()[0]), build.name));
                }
            }, ClearTempVar);
        }

        private void FormatButton2(UIItemBase.UIItemButton btn)
        {
            btn.Format(Color.black, 13).Size(TELE_BTN_WIDTH, TELE_BTN_HEIGHT);
        }

        private void Tele(Vector2Int p)
        {
            g.world.playerUnit.SetUnitPos(p);
        }

        private float oldSpeed = 1;
        private void StopGame()
        {
            if (Time.timeScale == 0)
            {
                Time.timeScale = oldSpeed;
            }
            else
            {
                oldSpeed = Time.timeScale;
                Time.timeScale = 0;
            }
        }

        private void SpeedGame(float multiplier)
        {
            Time.timeScale = multiplier;
        }

        private void AddLife()
        {
            var player = g.world.playerUnit;
            if (player.GetProperty<int>(UnitPropertyEnum.Life) < MAX_NUMBER)
                player.AddProperty<int>(UnitPropertyEnum.Life, 1200); //100 * 12month
        }

        private void ReduceLife()
        {
            var player = g.world.playerUnit;
            if (player.GetProperty<int>(UnitPropertyEnum.Life) > 1200)
                player.AddProperty<int>(UnitPropertyEnum.Life, -1200); //100 * 12month
        }

        private void AddMaxHP()
        {
            var player = g.world.playerUnit;
            if (player.GetProperty<int>(UnitPropertyEnum.HpMax) < MAX_NUMBER)
                player.AddProperty<int>(UnitPropertyEnum.HpMax, 100000);
        }

        private void ReduceMaxHP()
        {
            var player = g.world.playerUnit;
            if (player.GetProperty<int>(UnitPropertyEnum.HpMax) > 100000)
                player.AddProperty<int>(UnitPropertyEnum.HpMax, -100000);
        }

        private void AddMaxMP()
        {
            var player = g.world.playerUnit;
            if (player.GetProperty<int>(UnitPropertyEnum.MpMax) < MAX_NUMBER)
                player.AddProperty<int>(UnitPropertyEnum.MpMax, 10000);
        }

        private void ReduceMaxMP()
        {
            var player = g.world.playerUnit;
            if (player.GetProperty<int>(UnitPropertyEnum.MpMax) > 10000)
                player.AddProperty<int>(UnitPropertyEnum.MpMax, -10000);
        }

        private void AddMaxSP()
        {
            var player = g.world.playerUnit;
            if (player.GetProperty<int>(UnitPropertyEnum.SpMax) < MAX_NUMBER)
                player.AddProperty<int>(UnitPropertyEnum.SpMax, 1000);
        }

        private void ReduceMaxSP()
        {
            var player = g.world.playerUnit;
            if (player.GetProperty<int>(UnitPropertyEnum.SpMax) > 1000)
                player.AddProperty<int>(UnitPropertyEnum.SpMax, -1000);
        }

        private void AddMoney()
        {
            var player = g.world.playerUnit;
            player.AddUnitMoney(1000000);
        }

        private void ReduceMoney()
        {
            var player = g.world.playerUnit;
            player.AddUnitMoney(-1000000);
        }

        private void AddDegree()
        {
            var player = g.world.playerUnit;
            player.AddUnitMayorDegree(1000);
        }

        private void ReduceDegree()
        {
            var player = g.world.playerUnit;
            player.AddUnitMayorDegree(-1000);
        }

        private void AddContribution()
        {
            var player = g.world.playerUnit;
            player.AddUnitContribution(100000);
        }

        private void ReduceContribution()
        {
            var player = g.world.playerUnit;
            player.AddUnitContribution(-100000);
        }

        private void AddAbilityExp()
        {
            var player = g.world.playerUnit;
            player.AddProperty<int>(UnitPropertyEnum.AbilityExp, 100000);
        }

        private void ReduceAbilityExp()
        {
            var player = g.world.playerUnit;
            player.AddProperty<int>(UnitPropertyEnum.AbilityExp, -100000);
        }

        private void AddFootspeed()
        {
            var player = g.world.playerUnit;
            player.AddProperty<int>(UnitPropertyEnum.FootSpeed, 1000);
        }

        private void ReduceFootspeed()
        {
            var player = g.world.playerUnit;
            player.AddProperty<int>(UnitPropertyEnum.FootSpeed, -1000);
        }

        private void AddViewRange()
        {
            var player = g.world.playerUnit;
            player.GetDynProperty(UnitDynPropertyEnum.PlayerView).baseValue += 10;
        }

        private void ReduceViewRange()
        {
            var player = g.world.playerUnit;
            player.GetDynProperty(UnitDynPropertyEnum.PlayerView).baseValue -= 10;
        }

        private void AddBasis()
        {
            var v = 100;
            var player = g.world.playerUnit;
            player.AddProperty<int>(UnitPropertyEnum.BasisBlade, v);
            player.AddProperty<int>(UnitPropertyEnum.BasisEarth, v);
            player.AddProperty<int>(UnitPropertyEnum.BasisFinger, v);
            player.AddProperty<int>(UnitPropertyEnum.BasisFire, v);
            player.AddProperty<int>(UnitPropertyEnum.BasisFist, v);
            player.AddProperty<int>(UnitPropertyEnum.BasisFroze, v);
            player.AddProperty<int>(UnitPropertyEnum.BasisPalm, v);
            player.AddProperty<int>(UnitPropertyEnum.BasisSpear, v);
            player.AddProperty<int>(UnitPropertyEnum.BasisSword, v);
            player.AddProperty<int>(UnitPropertyEnum.BasisThunder, v);
            player.AddProperty<int>(UnitPropertyEnum.BasisWind, v);
            player.AddProperty<int>(UnitPropertyEnum.BasisWood, v);
            player.AddProperty<int>(UnitPropertyEnum.RefineElixir, v);
            player.AddProperty<int>(UnitPropertyEnum.RefineWeapon, v);
            player.AddProperty<int>(UnitPropertyEnum.Symbol, v);
            player.AddProperty<int>(UnitPropertyEnum.Geomancy, v);
            player.AddProperty<int>(UnitPropertyEnum.Herbal, v);
            player.AddProperty<int>(UnitPropertyEnum.Mine, v);
        }

        private void ReduceBasis()
        {
            var v = -100;
            var player = g.world.playerUnit;
            player.AddProperty<int>(UnitPropertyEnum.BasisBlade, v);
            player.AddProperty<int>(UnitPropertyEnum.BasisEarth, v);
            player.AddProperty<int>(UnitPropertyEnum.BasisFinger, v);
            player.AddProperty<int>(UnitPropertyEnum.BasisFire, v);
            player.AddProperty<int>(UnitPropertyEnum.BasisFist, v);
            player.AddProperty<int>(UnitPropertyEnum.BasisFroze, v);
            player.AddProperty<int>(UnitPropertyEnum.BasisPalm, v);
            player.AddProperty<int>(UnitPropertyEnum.BasisSpear, v);
            player.AddProperty<int>(UnitPropertyEnum.BasisSword, v);
            player.AddProperty<int>(UnitPropertyEnum.BasisThunder, v);
            player.AddProperty<int>(UnitPropertyEnum.BasisWind, v);
            player.AddProperty<int>(UnitPropertyEnum.BasisWood, v);
            player.AddProperty<int>(UnitPropertyEnum.RefineElixir, v);
            player.AddProperty<int>(UnitPropertyEnum.RefineWeapon, v);
            player.AddProperty<int>(UnitPropertyEnum.Symbol, v);
            player.AddProperty<int>(UnitPropertyEnum.Geomancy, v);
            player.AddProperty<int>(UnitPropertyEnum.Herbal, v);
            player.AddProperty<int>(UnitPropertyEnum.Mine, v);
        }

        private void Levelup()
        {
            var player = g.world.playerUnit;
            if (player.GetProperty<int>(UnitPropertyEnum.GradeID) < g.conf.roleGrade._allConfList.ToArray().Max(x => x.id))
                player.AddProperty<int>(UnitPropertyEnum.GradeID, 1);
        }

        private void Leveldown()
        {
            var player = g.world.playerUnit;
            if (player.GetProperty<int>(UnitPropertyEnum.GradeID) > 1)
                player.AddProperty<int>(UnitPropertyEnum.GradeID, -1);
        }

        private void AddAtk()
        {
            var player = g.world.playerUnit;
            if (player.GetProperty<int>(UnitPropertyEnum.Attack) < MAX_NUMBER)
                player.AddProperty<int>(UnitPropertyEnum.Attack, 10000);
        }

        private void ReduceAtk()
        {
            var player = g.world.playerUnit;
            if (player.GetProperty<int>(UnitPropertyEnum.Attack) > 10000)
                player.AddProperty<int>(UnitPropertyEnum.Attack, -10000);
        }

        private void AddDef()
        {
            var player = g.world.playerUnit;
            if (player.GetProperty<int>(UnitPropertyEnum.Defense) < MAX_NUMBER)
                player.AddProperty<int>(UnitPropertyEnum.Defense, 1000);
        }

        private void ReduceDef()
        {
            var player = g.world.playerUnit;
            if (player.GetProperty<int>(UnitPropertyEnum.Defense) > 1000)
                player.AddProperty<int>(UnitPropertyEnum.Defense, -1000);
        }

        private void AddExp()
        {
            var player = g.world.playerUnit;
            player.AddExp(10000);
        }
    }
}