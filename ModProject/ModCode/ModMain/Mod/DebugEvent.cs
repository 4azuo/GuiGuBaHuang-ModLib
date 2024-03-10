//using EGameTypeData;
//using ModLib.Enum;
//using ModLib.Mod;
//using System.Linq;
//using System.Reflection;
//using TMPro;
//using UnityEngine;
//using UnityEngine.Events;
//using UnityEngine.EventSystems;
//using UnityEngine.UI;

//namespace MOD_nE7UL2.Mod
//{
//    [Cache("DEBUG", IsGlobal = true)]
//    public class DebugEvent : ModEvent
//    {
//        /* === In game debug === */
//        //public override void OnLoadGame()
//        //{
//        //    g.world.playerUnit.AddProperty<int>(UnitPropertyEnum.Attack, 1000000);
//        //    g.world.playerUnit.AddProperty<int>(UnitPropertyEnum.Defense, 1000000);
//        //    g.world.playerUnit.AddProperty<int>(UnitPropertyEnum.HpMax, 1000000);
//        //    g.world.playerUnit.AddProperty<int>(UnitPropertyEnum.MpMax, 1000000);
//        //    g.world.playerUnit.AddProperty<int>(UnitPropertyEnum.SpMax, 1000000);
//        //    g.world.playerUnit.data.RewardPropMoney(1000000);

//        //    g.world.playerUnit.data.unitData.propertyData.footSpeed = 10000;
//        //    g.world.playerUnit.data.dynUnitData.playerView.baseValue = 1000;
//        //}

//        /* === Ui component list === */
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

//        /* === Button insert action === */
//        //bool test = true;
//        //public override void OnOpenUIEnd(OpenUIEnd e)
//        //{
//        //    var ui = MonoBehaviour.FindObjectOfType<UIStartGameTip>();
//        //    if (ui != null)
//        //    {
//        //        if (test)
//        //        {
//        //            ui.btnOK.onClick.m_Calls.m_RuntimeCalls.Insert(0, new InvokableCall((UnityAction)xxx));
//        //            test = false;
//        //        }
//        //        DebugHelper.WriteLine($"{ui.btnOK.onClick.m_Calls.m_RuntimeCalls.Count}");
//        //        DebugHelper.Save();
//        //    }
//        //}
//        //private void xxx()
//        //{
//        //    DebugHelper.WriteLine($"TEST");
//        //}
//    }
//}
