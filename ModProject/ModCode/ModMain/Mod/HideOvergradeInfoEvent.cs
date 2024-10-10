//using EGameTypeData;
//using MOD_nE7UL2.Const;
//using ModLib.Enum;
//using ModLib.Mod;
//using UnityEngine;

//namespace MOD_nE7UL2.Mod
//{
//    [Cache(ModConst.HIDE_OVERGRADE_INFO_EVENT)]
//    public class HideOvergradeInfoEvent : ModEvent
//    {
//        public override void OnOpenUIEnd(OpenUIEnd e)
//        {
//            base.OnOpenUIEnd(e);

//            var uiInfoPreview = MonoBehaviour.FindObjectOfType<UINPCInfoPreview>();
//            if (uiInfoPreview != null && g.world.playerUnit.GetDynProperty(UnitDynPropertyEnum.GradeID).value < g.world.unit.GetUnit(uiInfoPreview.unitID).GetDynProperty(UnitDynPropertyEnum.GradeID).value - 2)
//            {
//                g.ui.CloseUI(uiInfoPreview);
//            }

//            var uiInfo = MonoBehaviour.FindObjectOfType<UINPCInfo>();
//            if (uiInfo != null && g.world.playerUnit.GetDynProperty(UnitDynPropertyEnum.GradeID).value < uiInfo.unit.GetDynProperty(UnitDynPropertyEnum.GradeID).value - 2)
//            {
//                g.ui.CloseUI(uiInfo);
//            }
//        }
//    }
//}
