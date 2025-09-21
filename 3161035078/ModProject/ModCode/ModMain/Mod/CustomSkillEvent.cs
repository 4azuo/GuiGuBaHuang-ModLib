//using EGameTypeData;
//using MOD_nE7UL2.Const;
//using MOD_nE7UL2.Object;
//using ModLib.Enum;
//using ModLib.Mod;
//using ModLib.Object;
//using Newtonsoft.Json;
//using System.IO;
//using System.Linq;

//namespace MOD_nE7UL2.Mod
//{
//    [Cache(ModConst.CUSTOM_SKILL_EVENT)]
//    public class CustomSkillEvent : ModEvent
//    {
//        public static CustomSkillEvent Instance { get; set; }

//        private UICover<UIBattleInfo> uiCover;
//        private UIItemButton btnShow;
//        private UIItemComposite cbEfx1;
//        private UIItemComposite cbEfx2;
//        private UIItemComposite cbEfx3;
//        private UIItemButton btnOk;
//        private UIItemButton btnCancel;

//        public override void OnOpenUIEnd(OpenUIEnd e)
//        {
//            base.OnOpenUIEnd(e);
//            if (e.uiType.uiName == UIType.BattleInfo.uiName)
//            {
//                uiCover = new UICover<UIBattleInfo>(e.ui);
//                uiCover.AddPage((ui) =>
//                {
//                    btnShow = uiCover.AddButton(uiCover.FirstCol + 2, uiCover.MidRow, ShowUICustomSkill, "Custom Skill");
//                });
//                uiCover.AddPage((ui) =>
//                {
//                    var col = uiCover.FirstCol + 2;
//                    var row = uiCover.MidRow + 1;

//                    var selectEfx = 0;
//                    var efxList = JsonConvert.DeserializeObject<CustomEffect[]>(File.ReadAllText(ConfHelper.GetConfFilePath(ModMain.ModObj.ModId, "_Cus_Res_Efx_Battle_Skill.json")));

//                    cbEfx1 = uiCover.AddCompositeSelect(col, row++, "Effect",
//                        efxList.Select(x => x.Efx).ToArray(), selectEfx);
//                    btnOk = uiCover.AddButton(col, row++, Ok, "Done");
//                    btnCancel = uiCover.AddButton(col + 1, row++, Cancel, "Cancel");
//                });
//                uiCover.Pages[0].Active();
//            }
//        }

//        private void ShowUICustomSkill()
//        {
//            if (uiCover == null)
//                return;
//            uiCover.Pages[1].Active();
//        }

//        private void Ok()
//        {
//            if (uiCover == null)
//                return;
//        }

//        private void Cancel()
//        {
//            if (uiCover == null)
//                return;
//            uiCover.Pages[0].Active();
//        }
//    }
//}
