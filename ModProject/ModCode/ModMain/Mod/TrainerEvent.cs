using MOD_nE7UL2.Const;
using ModLib.Enum;
using ModLib.Mod;
using System.Linq;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.TRAINER_EVENT)]
    public class TrainerEvent : ModEvent
    {
        public const string TITLE = "Mini Trainer";
        public const float BTN_WIDTH = 200;
        public const float BTN_HEIGHT = 36;
        public const int MAX_NUMBER = 100000000;

        private UIHelper.UICustom1 uiCustom;

        public override void OnMonoUpdate()
        {
            base.OnMonoUpdate();
            var smConfigs = EventHelper.GetEvent<SMLocalConfigsEvent>(ModConst.SM_LOCAL_CONFIGS_EVENT);
            if (smConfigs.Configs.EnableTrainer)
            {
                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Q))
                {
                    if (uiCustom == null)
                        OpenTrainer();
                }
            }
        }

        private void OpenTrainer()
        {
            var player = g.world.playerUnit;

            uiCustom = UIHelper.UICustom1.Create(TITLE, () => { uiCustom = null; });
            uiCustom.AddText(15, 0, "Cheating will destroy your experience.").Format(Color.red, 17);
            int col, row;

            col = 2; row = 2;
            uiCustom.AddText(col - 1, row - 1, "(HP/MP/SP/Mood/Stanima/Health)").Format(null, 13).Align(TextAnchor.MiddleLeft);
            FormatButton(uiCustom.AddButton(col, row, Recover, "Recover ALL"));
            FormatButton(uiCustom.AddButton(col, row += 2, AddMaxHP, "+100000 Max HP"));
            FormatButton(uiCustom.AddButton(col, row += 2, ReduceMaxHP, "-100000 Max HP"));
            FormatButton(uiCustom.AddButton(col, row += 2, AddMaxMP, "+10000 Max MP"));
            FormatButton(uiCustom.AddButton(col, row += 2, ReduceMaxMP, "-10000 Max MP"));
            FormatButton(uiCustom.AddButton(col, row += 2, AddMaxSP, "+1000 Max SP"));
            FormatButton(uiCustom.AddButton(col, row += 2, ReduceMaxSP, "-1000 Max SP"));
            FormatButton(uiCustom.AddButton(col, row += 2, AddBasis, "+100 ALL Basises"));
            FormatButton(uiCustom.AddButton(col, row += 2, ReduceBasis, "-100 ALL Basises"));
            FormatButton(uiCustom.AddButton(col, row += 2, AddAtk, "+10000 Attack"));
            FormatButton(uiCustom.AddButton(col, row += 2, ReduceAtk, "-10000 Attack"));
            FormatButton(uiCustom.AddButton(col, row += 2, AddDef, "+1000 Defence"));
            FormatButton(uiCustom.AddButton(col, row += 2, ReduceDef, "-1000 Defence"));

            col = 8; row = 2;
            //FormatButton(uiCustom.AddButton(col, row, Teleport, "Teleport"));
            FormatButton(uiCustom.AddButton(col, row += 2, AddMoney, "+1000000 Money"));
            FormatButton(uiCustom.AddButton(col, row += 2, ReduceMoney, "-1000000 Money"));
            FormatButton(uiCustom.AddButton(col, row += 2, AddDegree, "+1000 Degree"));
            FormatButton(uiCustom.AddButton(col, row += 2, ReduceDegree, "-1000 Degree"));
            FormatButton(uiCustom.AddButton(col, row += 2, AddContribution, "+100000 Contribution"));
            FormatButton(uiCustom.AddButton(col, row += 2, ReduceContribution, "-100000 Contribution"));

            col = 14; row = 2;
            FormatButton(uiCustom.AddButton(col, row, StopGame, "Stop Game"));
            FormatButton(uiCustom.AddButton(col, row += 2, Levelup, "Levelup"));
            FormatButton(uiCustom.AddButton(col, row += 2, Leveldown, "Leveldown"));
            FormatButton(uiCustom.AddButton(col, row += 2, AddExp, "+10000 Exp"));
            FormatButton(uiCustom.AddButton(col, row += 2, AddLife, "+100 Yearlfie"));
            FormatButton(uiCustom.AddButton(col, row += 2, ReduceLife, "-100 Yearlfie"));
            FormatButton(uiCustom.AddButton(col, row += 2, AddFootspeed, "+1000 Travel speed"));
            FormatButton(uiCustom.AddButton(col, row += 2, ReduceFootspeed, "-1000 Travel speed"));
            FormatButton(uiCustom.AddButton(col, row += 2, AddViewRange, "+10 View-range"));
            FormatButton(uiCustom.AddButton(col, row += 2, ReduceViewRange, "-10 View-range"));

            col = 23; row = 2;
            uiCustom.AddText(col - 1, row++, "General Properties:").Format(null, 17, FontStyle.Italic).Align(TextAnchor.MiddleCenter);
            uiCustom.AddText(col, row, "HP: {0}/{1}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
            {
                Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Hp).value, player.GetDynProperty(UnitDynPropertyEnum.HpMax).value },
            });
            uiCustom.AddText(col - 3, row++, "{0} :Money").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
            {
                Formatter = (x) => new object[] { player.GetUnitMoney() },
            });
            uiCustom.AddText(col, row, "MP: {0}/{1}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
            {
                Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Mp).value, player.GetDynProperty(UnitDynPropertyEnum.MpMax).value },
            });
            uiCustom.AddText(col - 3, row++, "{0} :Degree").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
            {
                Formatter = (x) => new object[] { player.GetUnitMayorDegree() },
            });
            uiCustom.AddText(col, row, "SP: {0}/{1}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
            {
                Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Sp).value, player.GetDynProperty(UnitDynPropertyEnum.SpMax).value },
            });
            uiCustom.AddText(col - 3, row++, "{0} :Contribution").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
            {
                Formatter = (x) => new object[] { player.GetUnitContribution() },
            });
            uiCustom.AddText(col, row, "Atk: {0}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
            {
                Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Attack).value },
            });
            uiCustom.AddText(col - 3, row++, "{0} :Def").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
            {
                Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Defense).value },
            });
            uiCustom.AddText(col, row, "Mood: {0}/{1}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
            {
                Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Mood).value, player.GetDynProperty(UnitDynPropertyEnum.MoodMax).value },
            });
            uiCustom.AddText(col - 3, row++, "{0}/{1} :Stanima").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
            {
                Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Energy).value, player.GetDynProperty(UnitDynPropertyEnum.EnergyMax).value },
            });
            uiCustom.AddText(col, row, "Heath: {0}/{1}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
            {
                Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Health).value, player.GetDynProperty(UnitDynPropertyEnum.HealthMax).value },
            });
            uiCustom.AddText(col - 3, row++, "{0}/{1} :Life").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
            {
                Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Age).value, player.GetDynProperty(UnitDynPropertyEnum.Life).value },
            });
            uiCustom.AddText(col, row, "Travel Speed: {0}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
            {
                Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.FootSpeed).value },
            });
            uiCustom.AddText(col - 3, row++, "{0} :View Range").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
            {
                Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.PlayerView).value },
            });

            row++;
            uiCustom.AddText(col - 1, row++, "Martial/Spiritual:").Format(null, 17, FontStyle.Italic).Align(TextAnchor.MiddleCenter);
            uiCustom.AddText(col, row, "Fire: {0}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
            {
                Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.BasisFire).value },
            });
            uiCustom.AddText(col - 3, row++, "{0} :Blade").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
            {
                Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.BasisBlade).value },
            });
            uiCustom.AddText(col, row, "Water: {0}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
            {
                Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.BasisFroze).value },
            });
            uiCustom.AddText(col - 3, row++, "{0} :Spear").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
            {
                Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.BasisSpear).value },
            });
            uiCustom.AddText(col, row, "Lightning: {0}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
            {
                Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.BasisThunder).value },
            });
            uiCustom.AddText(col - 3, row++, "{0} :Sword").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
            {
                Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.BasisSword).value },
            });
            uiCustom.AddText(col, row, "Wind: {0}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
            {
                Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.BasisWind).value },
            });
            uiCustom.AddText(col - 3, row++, "{0} :Fist").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
            {
                Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.BasisFist).value },
            });
            uiCustom.AddText(col, row, "Earth: {0}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
            {
                Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.BasisEarth).value },
            });
            uiCustom.AddText(col - 3, row++, "{0} :Palm").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
            {
                Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.BasisPalm).value },
            });
            uiCustom.AddText(col, row, "Wood: {0}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
            {
                Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.BasisWood).value },
            });
            uiCustom.AddText(col - 3, row++, "{0} :Finger").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
            {
                Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.BasisFinger).value },
            });

            row++;
            uiCustom.AddText(col - 1, row++, "Artisanship:").Format(null, 17, FontStyle.Italic).Align(TextAnchor.MiddleCenter);
            uiCustom.AddText(col, row, "Alchemy: {0}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
            {
                Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.RefineElixir).value },
            });
            uiCustom.AddText(col - 3, row++, "{0} :Forge").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
            {
                Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.RefineWeapon).value },
            });
            uiCustom.AddText(col, row, "Feng Shui: {0}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
            {
                Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Geomancy).value },
            });
            uiCustom.AddText(col - 3, row++, "{0} :Talismans").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
            {
                Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Symbol).value },
            });
            uiCustom.AddText(col, row, "Herbology: {0}").Align(TextAnchor.MiddleLeft).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
            {
                Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Herbal).value },
            });
            uiCustom.AddText(col - 3, row++, "{0} :Mining").Align(TextAnchor.MiddleRight).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
            {
                Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Mine).value },
            });

            row++;
            uiCustom.AddText(col - 1, row++, "Grade: {0} {1} ― {2}").Align(TextAnchor.MiddleCenter).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
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
            uiCustom.AddText(col - 1, row++, "Exp").Align(TextAnchor.MiddleCenter).Format(Color.black, 15).SetWork(new UIItemBase.UIItemWork
            {
                Formatter = (x) => new object[] { player.GetDynProperty(UnitDynPropertyEnum.Exp).value },
            });
        }

        private void FormatButton(UIItemBase.UIItemButton btn)
        {
            btn.Item.Format(Color.black, 15).Size(BTN_WIDTH, BTN_HEIGHT);
        }

        public override void OnTimeUpdate()
        {
            base.OnTimeUpdate();
            if (uiCustom != null)
            {
                uiCustom.UpdateUI();
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

        private void Teleport()
        {
        }

        private void StopGame()
        {
            if (Time.timeScale == 0)
                Time.timeScale = 1;
            else
                Time.timeScale = 0;
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
            player.AddProperty<int>(UnitPropertyEnum.Exp, 10000);
        }
    }
}
