//using EGameTypeData;
//using Il2CppSystem.Net;
//using MOD_nE7UL2.Const;
//using MOD_nE7UL2.Enum;
//using ModLib.Enum;
//using ModLib.Mod;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Numerics;
//using System.Reflection;
//using TMPro;
//using UnityEngine;
//using UnityEngine.Events;
//using UnityEngine.EventSystems;
//using UnityEngine.UI;
//using static SpecialBattle83;

//namespace MOD_nE7UL2.Mod
//{
//    [Cache(ModConst.DEBUG_EVENT)]
//    public class DebugEvent : ModEvent
//    {
//        public static DebugEvent Instance { get; set; }

//        private int showindex = 0;
//        private string[] defres = File.ReadAllLines(ConfHelper.GetConfFilePath(ModMain.ModObj.ModId, "_Def_Res_Efx_Battle_Unit.txt"));

//        public override void OnMonoUpdate()
//        {
//            base.OnMonoUpdate();
//            if (GameHelper.IsInBattlle())
//            {
//                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z))
//                {
//                    showindex--;
//                    if (showindex < 0)
//                        showindex = 0;
//                    ModBattleEvent.SceneBattle.effect.Create(defres[showindex], ModBattleEvent.PlayerUnit.transform.position, 3f);
//                    DebugHelper.WriteLine($"DefRes: {defres[showindex]}");
//                }
//                else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.X))
//                {
//                    showindex++;
//                    if (showindex >= g.conf.battleEffect._allConfList.Count)
//                        showindex = g.conf.battleEffect._allConfList.Count - 1;
//                    ModBattleEvent.SceneBattle.effect.Create(defres[showindex], ModBattleEvent.PlayerUnit.transform.position, 3f);
//                    DebugHelper.WriteLine($"DefRes: {defres[showindex]}");
//                }
//                else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.C))
//                {
//                    ModBattleEvent.SceneBattle.effect.Create(defres[showindex], ModBattleEvent.PlayerUnit.transform.position, 3f);
//                    DebugHelper.WriteLine($"DefRes: {defres[showindex]}");
//                }
//                else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S))
//                {
//                    DebugHelper.Save();
//                }
//            }
//        }

//        /* === Test attributes === */
//        //public override void OnSave(ETypeData e)
//        //{
//        //    base.OnSave(e);
//        //    DebugHelper.WriteLine($"HpMax: {g.world.playerUnit.GetProperty<int>(UnitPropertyEnum.HpMax)}");
//        //    DebugHelper.WriteLine($"HpMax(Dyn): {g.world.playerUnit.GetDynProperty(UnitDynPropertyEnum.HpMax).value}");
//        //    DebugHelper.WriteLine($"MpMax: {g.world.playerUnit.GetProperty<int>(UnitPropertyEnum.MpMax)}");
//        //    DebugHelper.WriteLine($"MpMax(Dyn): {g.world.playerUnit.GetDynProperty(UnitDynPropertyEnum.MpMax).value}");
//        //    DebugHelper.WriteLine($"RefineElixir: {g.world.playerUnit.GetProperty<int>(UnitPropertyEnum.RefineElixir)}");
//        //    DebugHelper.WriteLine($"RefineWeapon: {g.world.playerUnit.GetProperty<int>(UnitPropertyEnum.RefineWeapon)}");
//        //    DebugHelper.WriteLine($"Symbol: {g.world.playerUnit.GetProperty<int>(UnitPropertyEnum.Symbol)}");
//        //    DebugHelper.WriteLine($"Geomancy: {g.world.playerUnit.GetProperty<int>(UnitPropertyEnum.Geomancy)}");
//        //    DebugHelper.WriteLine($"Herbal: {g.world.playerUnit.GetProperty<int>(UnitPropertyEnum.Herbal)}");
//        //    DebugHelper.WriteLine($"Mine: {g.world.playerUnit.GetProperty<int>(UnitPropertyEnum.Mine)}");
//        //}

//        //public override void OnOpenUIEnd(OpenUIEnd e)
//        //{
//        //    base.OnOpenUIEnd(e);
//        //    if (g.res.allRes != null)
//        //    {
//        //        foreach (var res in g.res.allRes)
//        //        {
//        //            DebugHelper.WriteLine($"{res?.key}: {res?.value?.name}: {res?.value?.GetType()?.FullName}");
//        //        }
//        //    }
//        //}

//        /* === In game debug === */
//        //public override void OnLoadGame()
//        //{
//        //    //g.world.playerUnit.SetProperty<int>(UnitPropertyEnum.Attack, 1000000);
//        //    //g.world.playerUnit.SetProperty<int>(UnitPropertyEnum.Defense, 1000000);
//        //    //g.world.playerUnit.SetProperty<int>(UnitPropertyEnum.HpMax, 1000000);
//        //    //g.world.playerUnit.SetProperty<int>(UnitPropertyEnum.MpMax, 1000000);
//        //    //g.world.playerUnit.SetProperty<int>(UnitPropertyEnum.SpMax, 1000000);

//        //    //g.world.playerUnit.SetProperty<int>(UnitPropertyEnum.BasisBlade, 10000);
//        //    //g.world.playerUnit.SetProperty<int>(UnitPropertyEnum.BasisEarth, 10000);
//        //    //g.world.playerUnit.SetProperty<int>(UnitPropertyEnum.BasisFinger, 10000);
//        //    //g.world.playerUnit.SetProperty<int>(UnitPropertyEnum.BasisFire, 10000);
//        //    //g.world.playerUnit.SetProperty<int>(UnitPropertyEnum.BasisFist, 10000);
//        //    //g.world.playerUnit.SetProperty<int>(UnitPropertyEnum.BasisFroze, 10000);
//        //    //g.world.playerUnit.SetProperty<int>(UnitPropertyEnum.BasisPalm, 10000);
//        //    //g.world.playerUnit.SetProperty<int>(UnitPropertyEnum.BasisSpear, 10000);
//        //    //g.world.playerUnit.SetProperty<int>(UnitPropertyEnum.BasisSword, 10000);
//        //    //g.world.playerUnit.SetProperty<int>(UnitPropertyEnum.BasisThunder, 10000);
//        //    //g.world.playerUnit.SetProperty<int>(UnitPropertyEnum.BasisWind, 10000);
//        //    //g.world.playerUnit.SetProperty<int>(UnitPropertyEnum.BasisWood, 10000);

//        //    g.world.playerUnit.SetUnitMoney(1000000000);
//        //    g.world.playerUnit.SetUnitMayorDegree(1000000000);
//        //    g.world.playerUnit.SetUnitContribution(1000000000);
//        //    g.world.playerUnit.SetProperty<int>(UnitPropertyEnum.GradeID, 44);
//        //    g.world.playerUnit.data.unitData.propertyData.footSpeed = 10000;
//        //    g.world.playerUnit.data.dynUnitData.playerView.baseValue = 111;

//        //    foreach (var build in g.world.build.GetBuilds())
//        //    {
//        //        MapBuildPropertyEvent.AddBuildProperty(build, 1000000000);
//        //    }
//        //}

//        /* === Ui component list === */
//        //public override void OnOpenUIEnd(OpenUIEnd e)
//        //{
//        //    DebugHelper.WriteLine($"UI: {e.uiType.uiName}");
//        //    foreach (var item in MonoBehaviour.FindObjectsOfType<UIBehaviour>().Where(x => x?.GetComponentInParent<UIBase>()?.name == e.uiType.uiName))
//        //    {
//        //        var className = item.GetScriptClassName();
//        //        var uiParent = item.GetComponentInParent<UIBase>();
//        //        var uiComp1 = item.GetComponentInParent<UIBehaviour>();
//        //        var uiComp2 = uiComp1.GetComponentInParent<UIBehaviour>();
//        //        DebugHelper.Write($"　{item.name}({className} extends {uiParent.uiType.uiName}.{uiComp1.name}, {uiComp2.name}): ");
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

//        /* === Ui component list - details === */
//        //public override void OnOpenUIEnd(OpenUIEnd e)
//        //{
//        //    var ui = MonoBehaviour.FindObjectOfType<UITownStorageProps>();
//        //    if (ui != null)
//        //    {
//        //        DebugHelper.WriteLine($"UI: {ui.uiType.uiName}");
//        //        PrintChildrenComponents(1, ui, new List<MonoBehaviour>());
//        //        DebugHelper.Save();
//        //    }
//        //}
//        //private void PrintChildrenComponents(int level, MonoBehaviour parent, List<MonoBehaviour> ignored)
//        //{
//        //    foreach (var child in parent.GetComponentsInChildren<MonoBehaviour>())
//        //    {
//        //        if (ignored.Contains(child))
//        //            continue;

//        //        ignored.Add(child);
//        //        var className = child.GetScriptClassName();
//        //        DebugHelper.Write($"{new string('\t', level)}{child.name}({className}): ");
//        //        switch (className)
//        //        {
//        //            case "Text":
//        //                DebugHelper.Write($"text:{child.TryCast<Text>()?.text}", false);
//        //                break;
//        //            case "TextMeshProUGUI":
//        //                DebugHelper.Write($"text:{child.TryCast<TextMeshProUGUI>()?.text}", false);
//        //                break;
//        //            default:
//        //                break;
//        //        }
//        //        DebugHelper.WriteLine();

//        //        PrintChildrenComponents(level + 1, child, ignored);
//        //    }
//        //}

//        /* === Ui button list === */
//        //public override void OnOpenUIEnd(OpenUIEnd e)
//        //{
//        //    DebugHelper.WriteLine($"UI: {e.uiType.uiName}");
//        //    foreach (var btn in MonoBehaviour.FindObjectsOfType<Button>())
//        //    {
//        //        var btnText = btn.GetComponentInChildren<Text>();
//        //        var uiParent = btn.GetComponentInParent<UIBase>();
//        //        var className = btn.GetScriptClassName();
//        //        DebugHelper.WriteLine($"　{btn.name}({className} extends {uiParent.uiType.uiName}): {btnText?.text}");
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
