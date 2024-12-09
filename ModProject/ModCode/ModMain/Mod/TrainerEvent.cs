using MOD_nE7UL2.Const;
using MOD_nE7UL2.Enum;
using ModLib.Enum;
using ModLib.Mod;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UIHelper;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.TRAINER_EVENT)]
    public class TrainerEvent : ModEvent
    {
        public const string TITLE = "Trainer";

        public override void OnMonoUpdate()
        {
            base.OnMonoUpdate();
            if (Input.GetKeyDown(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Alpha0))
            {
                OpenTrainer();
            }
        }

        private void OpenTrainer()
        {
            var uiCustom = UIHelper.UICustom1.Create(TITLE, () => { });
            uiCustom.AddText(15, 0, "Cheating will destroy your experience.").Format(Color.red, 17);
            int col, row;

            col = 2; row = 1;
            uiCustom.AddText(col, row += 2, "Player:").Format(null, 17, FontStyle.Italic).Align(TextAnchor.MiddleRight);
            uiCustom.AddButton(col, row += 2, Recover, "Recover ALL").Item.Size(280, 70);
            uiCustom.AddText(col - 2, row + 1, "(HP/MP/SP/Mood/Stanima/Health)").Format(null, 13).Align(TextAnchor.MiddleLeft);
            uiCustom.AddButton(col, row += 2, AddLife, "+100 Yearlfie").Item.Size(280, 70);
            uiCustom.AddButton(col, row += 2, AddMaxHP, "+100000 Max HP").Item.Size(280, 70);
            uiCustom.AddButton(col, row += 2, AddMaxHP, "+10000 Max MP").Item.Size(280, 70);
            uiCustom.AddButton(col, row += 2, AddMaxHP, "+1000 Max SP").Item.Size(280, 70);
            uiCustom.AddButton(col, row += 2, AddMaxHP, "+1000000 Money").Item.Size(280, 70);
            uiCustom.AddButton(col, row += 2, AddMaxHP, "+1000 Degree").Item.Size(280, 70);
            uiCustom.AddButton(col, row += 2, AddMaxHP, "+100000 Contribution").Item.Size(280, 70);
            uiCustom.AddButton(col, row += 2, AddMaxHP, "+1000 Footspeed").Item.Size(280, 70);
            uiCustom.AddButton(col, row += 2, AddMaxHP, "+10 View-range").Item.Size(280, 70);
            uiCustom.AddButton(col, row += 2, AddMaxHP, "+100 ALL Basises").Item.Size(280, 70);

            col = 10; row = 4;
            uiCustom.AddButton(col, row += 2, AddLife, "-100 Yearlfie").Item.Size(280, 70);
            uiCustom.AddButton(col, row += 2, AddMaxHP, "-100000 Max HP").Item.Size(280, 70);
            uiCustom.AddButton(col, row += 2, AddMaxHP, "-10000 Max MP").Item.Size(280, 70);
            uiCustom.AddButton(col, row += 2, AddMaxHP, "-1000 Max SP").Item.Size(280, 70);
            uiCustom.AddButton(col, row += 2, AddMaxHP, "-1000000 Money").Item.Size(280, 70);
            uiCustom.AddButton(col, row += 2, AddMaxHP, "-1000 Degree").Item.Size(280, 70);
            uiCustom.AddButton(col, row += 2, AddMaxHP, "-100000 Contribution").Item.Size(280, 70);
            uiCustom.AddButton(col, row += 2, AddMaxHP, "-1000 Footspeed").Item.Size(280, 70);
            uiCustom.AddButton(col, row += 2, AddMaxHP, "-10 View-range").Item.Size(280, 70);
            uiCustom.AddButton(col, row += 2, AddMaxHP, "-100 ALL Basises").Item.Size(280, 70);
        }

        private void Recover()
        {

        }

        private void AddLife()
        {

        }

        private void ReduceLife()
        {

        }

        private void AddMaxHP()
        {

        }

        private void ReduceMaxHP()
        {

        }
    }
}
