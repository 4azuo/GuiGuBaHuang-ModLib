using MOD_nE7UL2.Const;
using ModLib.Enum;
using ModLib.Mod;
using ModLib.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.TRAINER_EVENT)]
    public class TrainerEvent : ModEvent
    {
        public static TrainerEvent Instance { get; set; }

        public const string TITLE = "Trainer";
        public const float PAGE1_BTN_WIDTH = 200;
        public const float PAGE1_BTN_HEIGHT = 38;
        public const int PAGE1_FONT_SIZE = 15;
        public const float PAGE2_BTN_WIDTH = 250;
        public const float PAGE2_BTN_HEIGHT = 28;
        public const int PAGE2_FONT_SIZE = 13;
        public const float PAGE3_BTN_WIDTH = 210;
        public const float PAGE3_BTN_HEIGHT = 50;
        public const int PAGE3_FONT_SIZE = 13;
        public const int MAX_NUMBER = 100000000;

        private readonly Dictionary<int, MultiValue> TELE_AREA_COL = new Dictionary<int, MultiValue>
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

        private static readonly List<Tuple<string, GameAnimaWeapon, Action>> GodWeaponSelections = new List<Tuple<string, GameAnimaWeapon, Action>>
        {
            new Tuple<string, GameAnimaWeapon, Action>(GameTool.LS("trainer055"), GameAnimaWeapon.None, GodArtifactHelper.DisableAnimaWeapon),
            new Tuple<string, GameAnimaWeapon, Action>(GameTool.LS("trainer058"), GameAnimaWeapon.PiscesPendant, GodArtifactHelper.EnableOnlyPiscesPendant),
            new Tuple<string, GameAnimaWeapon, Action>(GameTool.LS("trainer056"), GameAnimaWeapon.DevilDemon, GodArtifactHelper.EnableOnlyDevilDemon),
            new Tuple<string, GameAnimaWeapon, Action>(GameTool.LS("trainer057"), GameAnimaWeapon.HootinEye, GodArtifactHelper.EnableOnlyGodEye)
        };

        private UICustom1 uiTrainer;

        public override bool OnCacheHandler()
        {
            return SMLocalConfigsEvent.Instance.Configs.EnableTrainer;
        }

        public override void OnMonoUpdate()
        {
            base.OnMonoUpdate();
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Alpha1))
            {
                if (uiTrainer == null)
                    OpenTrainer(0);
                else
                    uiTrainer.Pages[0].Active();
            }
            else
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Alpha2))
            {
                if (uiTrainer == null)
                    OpenTrainer(1);
                else
                    uiTrainer.Pages[1].Active();
            }
            else
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Alpha3))
            {
                if (uiTrainer == null)
                    OpenTrainer(2);
                else
                    uiTrainer.Pages[2].Active();
            }
        }

        public void OpenTrainer(int defPage)
        {
            if (!g.ui.HasUI(UIType.MapMain))
            {
                g.ui.MsgBox("Trainer", GameTool.LS("trainer044"));
                return;
            }

            var player = g.world.playerUnit;

            uiTrainer = new UICustom1(TITLE, okAct: () => uiTrainer = null);
            uiTrainer.IsShowNavigationButtons = true;
            uiTrainer.UI.isFastClose = true;

            //page 1
            uiTrainer.AddPage((ui) =>
            {
                int col, row;
                uiTrainer.AddText(uiTrainer.MidCol, uiTrainer.FirstRow, GameTool.LS("trainer000")).Format(Color.red, 17);

                col = 2; row = 2;
                uiTrainer.AddText(col - 1, row - 1, GameTool.LS("trainer001")).Format(null, 13).Align(TextAnchor.MiddleLeft);
                FormatButton1(uiTrainer.AddButton(col, row, Recover, GameTool.LS("trainer002")));
                FormatButton1(uiTrainer.AddButton(col, row += 2, AddMaxHP, GameTool.LS("trainer003")));
                FormatButton1(uiTrainer.AddButton(col, row += 2, ReduceMaxHP, GameTool.LS("trainer004")));
                FormatButton1(uiTrainer.AddButton(col, row += 2, AddMaxMP, GameTool.LS("trainer005")));
                FormatButton1(uiTrainer.AddButton(col, row += 2, ReduceMaxMP, GameTool.LS("trainer006")));
                FormatButton1(uiTrainer.AddButton(col, row += 2, AddMaxSP, GameTool.LS("trainer007")));
                FormatButton1(uiTrainer.AddButton(col, row += 2, ReduceMaxSP, GameTool.LS("trainer008")));
                FormatButton1(uiTrainer.AddButton(col, row += 2, AddBasis, GameTool.LS("trainer009")));
                FormatButton1(uiTrainer.AddButton(col, row += 2, ReduceBasis, GameTool.LS("trainer010")));
                FormatButton1(uiTrainer.AddButton(col, row += 2, AddAtk, GameTool.LS("trainer011")));
                FormatButton1(uiTrainer.AddButton(col, row += 2, ReduceAtk, GameTool.LS("trainer012")));
                FormatButton1(uiTrainer.AddButton(col, row += 2, AddDef, GameTool.LS("trainer013")));
                FormatButton1(uiTrainer.AddButton(col, row += 2, ReduceDef, GameTool.LS("trainer014")));

                col = 8; row = 2;
                FormatButton1(uiTrainer.AddButton(col, row, () => uiTrainer.Pages[1].Active(), GameTool.LS("trainer015")));
                FormatButton1(uiTrainer.AddButton(col, row += 2, AddMoney, GameTool.LS("trainer016")));
                FormatButton1(uiTrainer.AddButton(col, row += 2, ReduceMoney, GameTool.LS("trainer017")));
                FormatButton1(uiTrainer.AddButton(col, row += 2, AddDegree, GameTool.LS("trainer018")));
                FormatButton1(uiTrainer.AddButton(col, row += 2, ReduceDegree, GameTool.LS("trainer019")));
                FormatButton1(uiTrainer.AddButton(col, row += 2, AddContribution, GameTool.LS("trainer020")));
                FormatButton1(uiTrainer.AddButton(col, row += 2, ReduceContribution, GameTool.LS("trainer021")));
                FormatButton1(uiTrainer.AddButton(col, row += 2, AddAbilityExp, GameTool.LS("trainer022")));
                FormatButton1(uiTrainer.AddButton(col, row += 2, ReduceAbilityExp, GameTool.LS("trainer023")));
                FormatButton1(uiTrainer.AddButton(col, row += 2, AddReputation, GameTool.LS("trainer042")));
                FormatButton1(uiTrainer.AddButton(col, row += 2, ReduceReputation, GameTool.LS("trainer043")));
                FormatButton1(uiTrainer.AddButton(col, row += 2, BecomeTownMaster, GameTool.LS("trainer047")));
                FormatButton1(uiTrainer.AddSelect(col, row += 2, GodWeaponSelections.Select(x => x.Item1).ToArray(), GodWeaponSelections.IndexOf(x => x.Item2 == GodArtifactHelper.GetAnimaWeapon()[0]).FixValue(0, GodWeaponSelections.Count - 1))
                    .SetWork(new UIItemWork
                    {
                        ChangeAct = (comp, selectedIndex) =>
                        {
                            GodWeaponSelections[(comp as UIItemSelect).SelectedIndex].Item3.Invoke();
                        }
                    }) as UIItemSelect);

                col = 14; row = 2;
                FormatButton1((UIItemButton)uiTrainer.AddButton(col, row, GameHelper.ChangeGameSpeed, "{0}").SetWork(new UIItemWork
                {
                    Formatter = (x) => new object[] { Time.timeScale == 0 ? GameTool.LS("trainer024") : GameTool.LS("trainer025") },
                }));
                FormatButton1(uiTrainer.AddButton(col, row += 2, () => GameHelper.SpeedGame(1), GameTool.LS("trainer026")));
                FormatButton1(uiTrainer.AddButton(col, row += 2, () => GameHelper.SpeedGame(2), GameTool.LS("trainer027")));
                FormatButton1(uiTrainer.AddButton(col, row += 2, () => GameHelper.SpeedGame(3), GameTool.LS("trainer028")));
                FormatButton1(uiTrainer.AddButton(col, row += 2, Levelup, GameTool.LS("trainer029")));
                FormatButton1(uiTrainer.AddButton(col, row += 2, Leveldown, GameTool.LS("trainer030")));
                FormatButton1(uiTrainer.AddButton(col, row += 2, AddExp, GameTool.LS("trainer031")));
                FormatButton1(uiTrainer.AddButton(col, row += 2, AddLife, GameTool.LS("trainer032")));
                FormatButton1(uiTrainer.AddButton(col, row += 2, ReduceLife, GameTool.LS("trainer033")));
                FormatButton1(uiTrainer.AddButton(col, row += 2, AddFootspeed, GameTool.LS("trainer034")));
                FormatButton1(uiTrainer.AddButton(col, row += 2, ReduceFootspeed, GameTool.LS("trainer035")));
                FormatButton1(uiTrainer.AddButton(col, row += 2, AddViewRange, GameTool.LS("trainer036")));
                FormatButton1(uiTrainer.AddButton(col, row += 2, ReduceViewRange, GameTool.LS("trainer037")));

                col = 23; row = 2;
                uiTrainer.AddText(col - 1, row++, GameTool.LS("trainer038")).Format(null, 17, FontStyle.Italic).Align(TextAnchor.MiddleCenter);
                uiTrainer.AddText(col, row, "HP: {0}/{1}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Hp).value.ToString(ModConst.FORMAT_NUMBER), player.GetDynProperty(UnitDynPropertyEnum.HpMax).value.ToString(ModConst.FORMAT_NUMBER) },
                });
                uiTrainer.AddText(col - 3, row++, "{0} :Money").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetUnitMoney().ToString(ModConst.FORMAT_NUMBER) },
                });
                uiTrainer.AddText(col, row, "MP: {0}/{1}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Mp).value.ToString(ModConst.FORMAT_NUMBER), player.GetDynProperty(UnitDynPropertyEnum.MpMax).value.ToString(ModConst.FORMAT_NUMBER) },
                });
                uiTrainer.AddText(col - 3, row++, "{0} :Degree").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetUnitMayorDegree().ToString(ModConst.FORMAT_NUMBER) },
                });
                uiTrainer.AddText(col, row, "SP: {0}/{1}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Sp).value.ToString(ModConst.FORMAT_NUMBER), player.GetDynProperty(UnitDynPropertyEnum.SpMax).value.ToString(ModConst.FORMAT_NUMBER) },
                });
                uiTrainer.AddText(col - 3, row++, "{0} :Contribution").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetUnitContribution().ToString(ModConst.FORMAT_NUMBER) },
                });
                uiTrainer.AddText(col, row, "Atk: {0}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Attack).value.ToString(ModConst.FORMAT_NUMBER) },
                });
                uiTrainer.AddText(col - 3, row++, "{0} :Def").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Defense).value.ToString(ModConst.FORMAT_NUMBER) },
                });
                uiTrainer.AddText(col, row, "Mood: {0}/{1}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Mood).value, player.GetDynProperty(UnitDynPropertyEnum.MoodMax).value },
                });
                uiTrainer.AddText(col - 3, row++, "{0}/{1} :Stanima").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Energy).value, player.GetDynProperty(UnitDynPropertyEnum.EnergyMax).value },
                });
                uiTrainer.AddText(col, row, "Heath: {0}/{1}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Health).value, player.GetDynProperty(UnitDynPropertyEnum.HealthMax).value },
                });
                uiTrainer.AddText(col - 3, row++, "{0}/{1} :Life").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Age).value.ToString(ModConst.FORMAT_NUMBER), player.GetDynProperty(UnitDynPropertyEnum.Life).value.ToString(ModConst.FORMAT_NUMBER) },
                });
                uiTrainer.AddText(col, row, "Travel Speed: {0}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.FootSpeed).value },
                });
                uiTrainer.AddText(col - 3, row++, "{0} :View Range").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.PlayerView).value },
                });
                uiTrainer.AddText(col, row, "Battle Speed: {0}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.MoveSpeed).value },
                });
                uiTrainer.AddText(col - 3, row++, "({1}) {0} :Ability").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.AbilityExp).value.ToString(ModConst.FORMAT_NUMBER), player.GetDynProperty(UnitDynPropertyEnum.AbilityPoint).value.ToString(ModConst.FORMAT_NUMBER) },
                });
                uiTrainer.AddText(col, row, "Luck: {0}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Luck).value },
                });
                uiTrainer.AddText(col - 3, row++, "{0} :Insight").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Talent).value },
                });
                uiTrainer.AddText(col, row, "Exp: {0}/{1}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetExp().ToString(ModConst.FORMAT_NUMBER), player.GetMaxExpCurrentPhase().ToString(ModConst.FORMAT_NUMBER) },
                });
                uiTrainer.AddText(col - 3, row++, "{0} :Reputation").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Reputation).value.ToString(ModConst.FORMAT_NUMBER) },
                });
                uiTrainer.AddText(col - 1, row++, "Grade: {0} {1} ― {2}").Align(TextAnchor.MiddleCenter).Format(Color.black, 15).SetWork(new UIItemWork
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

                row++;
                uiTrainer.AddText(col - 1, row++, GameTool.LS("trainer039")).Format(null, 17, FontStyle.Italic).Align(TextAnchor.MiddleCenter);
                uiTrainer.AddText(col, row, "Fire: {0}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.BasisFire).value },
                });
                uiTrainer.AddText(col - 3, row++, "{0} :Blade").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.BasisBlade).value },
                });
                uiTrainer.AddText(col, row, "Water: {0}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.BasisFroze).value },
                });
                uiTrainer.AddText(col - 3, row++, "{0} :Spear").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.BasisSpear).value },
                });
                uiTrainer.AddText(col, row, "Lightning: {0}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.BasisThunder).value },
                });
                uiTrainer.AddText(col - 3, row++, "{0} :Sword").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.BasisSword).value },
                });
                uiTrainer.AddText(col, row, "Wind: {0}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.BasisWind).value },
                });
                uiTrainer.AddText(col - 3, row++, "{0} :Fist").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.BasisFist).value },
                });
                uiTrainer.AddText(col, row, "Earth: {0}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.BasisEarth).value },
                });
                uiTrainer.AddText(col - 3, row++, "{0} :Palm").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.BasisPalm).value },
                });
                uiTrainer.AddText(col, row, "Wood: {0}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.BasisWood).value },
                });
                uiTrainer.AddText(col - 3, row++, "{0} :Finger").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.BasisFinger).value },
                });

                row++;
                uiTrainer.AddText(col - 1, row++, GameTool.LS("trainer040")).Format(null, 17, FontStyle.Italic).Align(TextAnchor.MiddleCenter);
                uiTrainer.AddText(col, row, GameTool.LS("trainer049")).Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.RefineElixir).value },
                });
                uiTrainer.AddText(col - 3, row++, GameTool.LS("trainer050")).Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.RefineWeapon).value },
                });
                uiTrainer.AddText(col, row, GameTool.LS("trainer051")).Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Geomancy).value },
                });
                uiTrainer.AddText(col - 3, row++, GameTool.LS("trainer052")).Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Symbol).value },
                });
                uiTrainer.AddText(col, row, GameTool.LS("trainer053")).Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Herbal).value },
                });
                uiTrainer.AddText(col - 3, row++, GameTool.LS("trainer054")).Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemWork
                {
                    Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Mine).value },
                });
            });

            //page 2
            uiTrainer.AddPage((ui) =>
            {
                uiTrainer.AddText(uiTrainer.MidCol, uiTrainer.FirstRow, GameTool.LS("trainer041")).Format(Color.red, 17);

                var row = new Dictionary<int, int>
                {
                    [0] = 0,
                    [1] = 0,
                    [2] = 0,
                    [3] = 0,
                    [4] = 0,
                };
                int col = 0, areaId = -1;
                foreach (var build in ModMaster.ModObj.Buildings.Where(x => !string.IsNullOrEmpty(x.name) && TELE_AREA_COL[x.gridData.areaBaseID].Value0.Parse<int>() >= 0 && ((Func<MapBuildBase, bool>)TELE_AREA_COL[x.gridData.areaBaseID].Value1).Invoke(x))
                    .OrderBy(x => TELE_AREA_COL[x.gridData.areaBaseID].Value0.Parse<int>()).ThenBy(x => x.gridData.areaBaseID).ThenBy(x => x.name))
                {
                    if (areaId != build.gridData.areaBaseID)
                    {
                        areaId = build.gridData.areaBaseID;
                        col = TELE_AREA_COL[areaId].Value0.Parse<int>() * 6 + 2;
                        uiTrainer.AddText(col, 1 + row[TELE_AREA_COL[areaId].Value0.Parse<int>()]++, $"Area {areaId}:").Format(null, 16, FontStyle.Italic).Align(TextAnchor.MiddleCenter);
                    }
                    FormatButton2(uiTrainer.AddButton(col, 1 + row[TELE_AREA_COL[areaId].Value0.Parse<int>()]++, () => Tele(build.GetOpenBuildPoints()[0]), build.name));
                }
            });

            //page 3
            uiTrainer.AddPage((ui) =>
            {
                uiTrainer.AddText(uiTrainer.MidCol, uiTrainer.FirstRow, GameTool.LS("trainer045")).Format(Color.red, 17);

                int col = 3, row = 2, c = 0;
                foreach (var conf in g.conf.roleGrade._allConfList)
                {
                    if (conf.itemShowA != "0" || conf.itemShowB != "0")
                    {
                        FormatButton3(uiTrainer.AddButton(col + 6 * (c / 13), row + 2 * (c % 13), () => AddBreakthroughItems(conf), $"{GameTool.LS(conf.gradeName)} - {GameTool.LS(conf.phaseName)}\n{GameTool.LS(conf.qualityName)}"));
                        c++;
                    }
                }
            });

            //page 4
            uiTrainer.AddPage((ui) =>
            {
                int col, row;

                col = 2; row = 2;
                uiTrainer.AddText(col, row++, string.Format(GameTool.LS("trainer048"), g.world.unit.allUnits.Count)).Align(TextAnchor.MiddleLeft).Format(Color.black, 15);
            });

            uiTrainer.Pages[defPage].Active();
        }

        private void FormatButton1(UIItemButton btn)
        {
            btn.Format(Color.black, PAGE1_FONT_SIZE).Size(PAGE1_BTN_WIDTH, PAGE1_BTN_HEIGHT);
        }

        private void FormatButton1(UIItemSelect slt)
        {
            slt.Format(Color.black, PAGE1_FONT_SIZE).Size(PAGE1_BTN_WIDTH, PAGE1_BTN_HEIGHT);
        }

        private void FormatButton2(UIItemButton btn)
        {
            btn.Format(Color.black, PAGE2_FONT_SIZE).Size(PAGE2_BTN_WIDTH, PAGE2_BTN_HEIGHT);
        }

        private void FormatButton3(UIItemButton btn)
        {
            btn.Format(Color.black, PAGE3_FONT_SIZE).Size(PAGE3_BTN_WIDTH, PAGE3_BTN_HEIGHT);
        }

        public void AddBreakthroughItems(ConfRoleGradeItem conf)
        {
            if (conf.itemShowA != "0")
            {
                foreach (var itemId in conf.itemShowA.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    g.world.playerUnit.AddUnitProp(itemId.Parse<int>(), 1);
                }
            }
            if (conf.itemShowB != "0")
            {
                foreach (var itemId in conf.itemShowB.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    g.world.playerUnit.AddUnitProp(itemId.Parse<int>(), 1);
                }
            }
            g.ui.MsgBox("Info", GameTool.LS("trainer046"));
        }

        public void Recover()
        {
            var player = g.world.playerUnit;
            player.SetProperty<int>(UnitPropertyEnum.Hp, int.MaxValue);
            player.SetProperty<int>(UnitPropertyEnum.Mp, int.MaxValue);
            player.SetProperty<int>(UnitPropertyEnum.Sp, int.MaxValue);
            player.SetProperty<int>(UnitPropertyEnum.Mood, int.MaxValue);
            player.SetProperty<int>(UnitPropertyEnum.Energy, int.MaxValue);
            player.SetProperty<int>(UnitPropertyEnum.Health, int.MaxValue);
        }

        public void Tele(Vector2Int p)
        {
            g.world.playerUnit.SetUnitPos(p);
        }

        public void AddLife()
        {
            var player = g.world.playerUnit;
            if (player.GetProperty<int>(UnitPropertyEnum.Life) < MAX_NUMBER)
                player.AddProperty<int>(UnitPropertyEnum.Life, 1200); //100 * 12month
        }

        public void ReduceLife()
        {
            var player = g.world.playerUnit;
            if (player.GetProperty<int>(UnitPropertyEnum.Life) > 1200)
                player.AddProperty<int>(UnitPropertyEnum.Life, -1200); //100 * 12month
        }

        public void AddMaxHP()
        {
            var player = g.world.playerUnit;
            if (player.GetProperty<int>(UnitPropertyEnum.HpMax) < MAX_NUMBER)
                player.AddProperty<int>(UnitPropertyEnum.HpMax, 100000);
        }

        public void ReduceMaxHP()
        {
            var player = g.world.playerUnit;
            if (player.GetProperty<int>(UnitPropertyEnum.HpMax) > 100000)
                player.AddProperty<int>(UnitPropertyEnum.HpMax, -100000);
        }

        public void AddMaxMP()
        {
            var player = g.world.playerUnit;
            if (player.GetProperty<int>(UnitPropertyEnum.MpMax) < MAX_NUMBER)
                player.AddProperty<int>(UnitPropertyEnum.MpMax, 10000);
        }

        public void ReduceMaxMP()
        {
            var player = g.world.playerUnit;
            if (player.GetProperty<int>(UnitPropertyEnum.MpMax) > 10000)
                player.AddProperty<int>(UnitPropertyEnum.MpMax, -10000);
        }

        public void AddMaxSP()
        {
            var player = g.world.playerUnit;
            if (player.GetProperty<int>(UnitPropertyEnum.SpMax) < MAX_NUMBER)
                player.AddProperty<int>(UnitPropertyEnum.SpMax, 1000);
        }

        public void ReduceMaxSP()
        {
            var player = g.world.playerUnit;
            if (player.GetProperty<int>(UnitPropertyEnum.SpMax) > 1000)
                player.AddProperty<int>(UnitPropertyEnum.SpMax, -1000);
        }

        public void AddMoney()
        {
            var player = g.world.playerUnit;
            player.AddUnitMoney(1000000);
        }

        public void ReduceMoney()
        {
            var player = g.world.playerUnit;
            player.AddUnitMoney(-1000000);
        }

        public void AddDegree()
        {
            var player = g.world.playerUnit;
            player.AddUnitMayorDegree(1000);
        }

        public void ReduceDegree()
        {
            var player = g.world.playerUnit;
            player.AddUnitMayorDegree(-1000);
        }

        public void AddContribution()
        {
            var player = g.world.playerUnit;
            player.AddUnitContribution(100000);
        }

        public void ReduceContribution()
        {
            var player = g.world.playerUnit;
            player.AddUnitContribution(-100000);
        }

        public void AddAbilityExp()
        {
            var player = g.world.playerUnit;
            player.AddProperty<int>(UnitPropertyEnum.AbilityExp, 100000);
        }

        public void ReduceAbilityExp()
        {
            var player = g.world.playerUnit;
            player.AddProperty<int>(UnitPropertyEnum.AbilityExp, -100000);
        }

        public void AddReputation()
        {
            var player = g.world.playerUnit;
            player.AddProperty<int>(UnitPropertyEnum.Reputation, 10000);
        }

        public void ReduceReputation()
        {
            var player = g.world.playerUnit;
            player.AddProperty<int>(UnitPropertyEnum.Reputation, -10000);
        }

        public void AddFootspeed()
        {
            var player = g.world.playerUnit;
            player.AddProperty<int>(UnitPropertyEnum.FootSpeed, 1000);
        }

        public void ReduceFootspeed()
        {
            var player = g.world.playerUnit;
            player.AddProperty<int>(UnitPropertyEnum.FootSpeed, -1000);
        }

        public void AddViewRange()
        {
            var player = g.world.playerUnit;
            player.GetDynProperty(UnitDynPropertyEnum.PlayerView).baseValue += 10;
        }

        public void ReduceViewRange()
        {
            var player = g.world.playerUnit;
            player.GetDynProperty(UnitDynPropertyEnum.PlayerView).baseValue -= 10;
        }

        public void AddBasis()
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

        public void ReduceBasis()
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

        public void Levelup()
        {
            var player = g.world.playerUnit;
            if (player.GetProperty<int>(UnitPropertyEnum.GradeID) < g.conf.roleGrade._allConfList.ToArray().Max(x => x.id))
            {
                player.AddProperty<int>(UnitPropertyEnum.GradeID, 1);
                player.ClearExp();
            }
        }

        public void Leveldown()
        {
            var player = g.world.playerUnit;
            if (player.GetProperty<int>(UnitPropertyEnum.GradeID) > 1)
            {
                player.AddProperty<int>(UnitPropertyEnum.GradeID, -1);
                player.ClearExp();
            }
        }

        public void AddAtk()
        {
            var player = g.world.playerUnit;
            if (player.GetProperty<int>(UnitPropertyEnum.Attack) < MAX_NUMBER)
                player.AddProperty<int>(UnitPropertyEnum.Attack, 10000);
        }

        public void ReduceAtk()
        {
            var player = g.world.playerUnit;
            if (player.GetProperty<int>(UnitPropertyEnum.Attack) > 10000)
                player.AddProperty<int>(UnitPropertyEnum.Attack, -10000);
        }

        public void AddDef()
        {
            var player = g.world.playerUnit;
            if (player.GetProperty<int>(UnitPropertyEnum.Defense) < MAX_NUMBER)
                player.AddProperty<int>(UnitPropertyEnum.Defense, 1000);
        }

        public void ReduceDef()
        {
            var player = g.world.playerUnit;
            if (player.GetProperty<int>(UnitPropertyEnum.Defense) > 1000)
                player.AddProperty<int>(UnitPropertyEnum.Defense, -1000);
        }

        public void AddExp()
        {
            var player = g.world.playerUnit;
            player.AddExp(10000);
        }

        public void BecomeTownMaster()
        {
            var player = g.world.playerUnit;
            var town = player.GetMapBuild<MapBuildTown>();
            if (town != null)
            {
                foreach (var guardian in MapBuildPropertyEvent.GetTownGuardians(town))
                {
                    MapBuildPropertyEvent.RemoveFromTownGuardians(guardian.GetUnitId(), guardian);
                }
                player.AddLuck(MapBuildPropertyEvent.TOWN_MASTER_LUCK_ID);
                MapBuildPropertyEvent.Instance.TownMasters[town.buildData.id] = new List<string> { player.GetUnitId() };
                g.ui.MsgBox(GameTool.LS("other500020011"), GameTool.LS("townmaster420041113drama"));
            }
            else
            {
                g.ui.MsgBox(GameTool.LS("other500020022"), GameTool.LS("other500020065"));
            }
        }
    }
}
