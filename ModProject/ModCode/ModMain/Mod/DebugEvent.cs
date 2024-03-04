//using EGameTypeData;
//using ModLib.Mod;
//using System.Linq;
//using TMPro;
//using UnityEngine;
//using UnityEngine.EventSystems;
//using UnityEngine.UI;

//namespace MOD_nE7UL2.Mod
//{
//    [Cache("DEBUG", IsGlobal = true)]
//    public class DebugEvent : ModEvent
//    {
//        //public override void OnLoadGame()
//        //{
//        //    g.world.playerUnit.AddProperty<int>(UnitPropertyEnum.Attack, 1000000);
//        //    g.world.playerUnit.AddProperty<int>(UnitPropertyEnum.Defense, 1000000);
//        //    g.world.playerUnit.AddProperty<int>(UnitPropertyEnum.HpMax, 1000000);
//        //    g.world.playerUnit.AddProperty<int>(UnitPropertyEnum.MpMax, 1000000);
//        //    g.world.playerUnit.AddProperty<int>(UnitPropertyEnum.SpMax, 1000000);
//        //    g.world.playerUnit.data.RewardPropMoney(1000000);

//        //    g.world.playerUnit.data.unitData.propertyData.footSpeed = 10000;
//        //    g.world.playerUnit.data.dynUnitData.playerView.baseValue = 20;
//        //}

//        //public override void OnOpenUIStart(OpenUIStart e)
//        //{
//        //    DebugHelper.WriteLine($"UI: {e.uiType.uiName}");
//        //    foreach (var item in MonoBehaviour.FindObjectsOfType<UIBehaviour>().Where(x => x?.GetComponentInParent<UIBase>()?.name == e.uiType.uiName))
//        //    {
//        //        var className = item.GetScriptClassName();
//        //        DebugHelper.Write($"　{item.name}({className}): ");
//        //        switch (className)
//        //        {
//        //            case "Text":
//        //                DebugHelper.Write($"text:{item.TryCast<Text>()?.text}", false);
//        //                break;
//        //            case "TextMeshProUGUI":
//        //                DebugHelper.Write($"text:{item.TryCast<TextMeshProUGUI>()?.text}", false);
//        //                break;
//        //            default:
//        //                break;
//        //        }
//        //        DebugHelper.WriteLine();
//        //    }
//        //    DebugHelper.Save();
//        //}
//    }
//}
