using Harmony;
using ModLib.Enum;
using System;
using System.Collections.Generic;
using System.Linq;

public static class UnitHelper
{
    public static string GetUnitId(this WorldUnitBase wunit)
    {
        return wunit?.data?.unitData?.unitID;
    }

    public static bool IsPlayer(this WorldUnitBase wunit)
    {
        return wunit.GetUnitId() == g.world.playerUnit.data.unitData.unitID;
    }

    public static bool IsHero(this WorldUnitBase wunit)
    {
        return wunit.data.unitData.heart.IsHeroes();
    }

    //public static DataUnit.LuckData GetNatureLuck(this WorldUnitBase wunit, int luckId)
    //{
    //    return wunit.data.unitData.propertyData.bornLuck.FirstOrDefault(x => x.id == luckId);
    //}

    //public static DataUnit.LuckData GetNurtureLuck(this WorldUnitBase wunit, int luckId)
    //{
    //    foreach (var item in wunit.data.unitData.propertyData.addLuck)
    //    {
    //        if (item.id == luckId)
    //            return item;
    //    }
    //    return null;
    //}

    //public static DataUnit.LuckData GetLuck(this WorldUnitBase wunit, int luckId)
    //{
    //    return wunit.GetNatureLuck(luckId) ?? wunit.GetNurtureLuck(luckId);
    //}

    public static int AddLuck(this WorldUnitBase wunit, int luckId, int dur = -1)
    {
        return wunit.CreateAction(new UnitActionLuckAdd(luckId, dur));
    }

    public static int DelLuck(this WorldUnitBase wunit, int luckId)
    {
        return wunit.CreateAction(new UnitActionLuckDel(luckId));
    }

    //public static void AddNatureLuck(this WorldUnitBase wunit, int luckId, AddLuckOptions option = AddLuckOptions.Dup)
    //{
    //    var old = wunit.GetNatureLuck(luckId);
    //    if (old != null && option == AddLuckOptions.IgnoreDup)
    //        return;
    //    if (old != null && option == AddLuckOptions.AddTime)
    //        return;
    //    wunit.data.unitData.propertyData.bornLuck.AddItem(new DataUnit.LuckData
    //    {
    //        id = luckId,
    //        duration = -1,
    //        createTime = g.game.world.run.roundMonth,
    //    });
    //}

    //public static void AddNurtureLuck(this WorldUnitBase wunit, int luckId, int dur = -1, AddLuckOptions option = AddLuckOptions.Dup)
    //{
    //    var old = wunit.GetNurtureLuck(luckId);
    //    if (old != null && option == AddLuckOptions.IgnoreDup)
    //        return;
    //    if (old != null && option == AddLuckOptions.AddTime)
    //    {
    //        if (old.duration == -1 || dur == -1)
    //        {
    //            old.duration = -1;
    //            return;
    //        }
    //        else
    //        {
    //            old.duration += dur;
    //            return;
    //        }
    //    }
    //    wunit.data.unitData.propertyData.addLuck.Add(new DataUnit.LuckData
    //    {
    //        id = luckId,
    //        duration = dur,
    //        createTime = g.game.world.run.roundMonth,
    //    });
    //}

    public static T GetProperty<T>(this WorldUnitBase wunit, UnitPropertyEnum propType)
    {
        return propType.Get<T>(wunit.data.unitData.propertyData);
    }

    public static void SetProperty<T>(this WorldUnitBase wunit, UnitPropertyEnum propType, T newValue)
    {
        propType.Set<T>(wunit.data.unitData.propertyData, newValue);
    }

    public static void AddProperty<T>(this WorldUnitBase wunit, UnitPropertyEnum propType, T addValue)
    {
        SetProperty<T>(wunit, propType, MathLogicOptions.Plus.Exe(wunit.GetProperty<T>(propType).Parse<float>(), addValue.Parse<float>()).Parse<T>());
    }

    public static DynInt GetDynProperty(this WorldUnitBase wunit, UnitDynPropertyEnum propType)
    {
        return propType.Get(wunit.data.dynUnitData);
    }

    public static void SetDynProperty(this WorldUnitBase wunit, UnitDynPropertyEnum propType, DynInt newValue)
    {
        propType.Set(wunit.data.dynUnitData, newValue);
    }

    public static int GetMaxExpCurrentGrade(this WorldUnitBase wunit)
    {
        return g.conf.roleGrade.GetNextGradeItem(wunit.GetProperty<int>(UnitPropertyEnum.GradeID)).exp;
    }

    public static int GetMinExpCurrentGrade(this WorldUnitBase wunit)
    {
        return g.conf.roleGrade.GetItem(wunit.GetProperty<int>(UnitPropertyEnum.GradeID)).exp;
    }

    public static int GetNeedExpToLevelUp(this WorldUnitBase wunit)
    {
        return wunit.GetProperty<int>(UnitPropertyEnum.Exp) - g.conf.roleGrade.GetItem(wunit.GetProperty<int>(UnitPropertyEnum.GradeID)).exp;
    }

    public static void AddExp(this WorldUnitBase wunit, int exp)
    {
        var curExp = wunit.GetProperty<int>(UnitPropertyEnum.Exp);
        var addExp = Math.Max(Math.Min(curExp + exp, wunit.GetMaxExpCurrentGrade()), wunit.GetMinExpCurrentGrade());
        wunit.SetProperty<int>(UnitPropertyEnum.Exp, addExp);

        var gradeEnum = GradePhaseEnum.GetEnumByVal<GradePhaseEnum>(wunit.GetProperty<int>(UnitPropertyEnum.GradeID).ToString());
        if (gradeEnum != null && gradeEnum.Bottleneck != BottleneckEnum.None)
        {
            if (addExp == wunit.GetMaxExpCurrentGrade())
            {
                wunit.AddLuck((int)gradeEnum.Bottleneck);
            }
            else
            {
                wunit.DelLuck((int)gradeEnum.Bottleneck);
            }
        }
    }

    public static void ClearExp(this WorldUnitBase wunit)
    {
        wunit.AddExp(int.MinValue);
    }

    public static void AddUnitProp(this WorldUnitBase wunit, DataProps.PropsData prop)
    {
        wunit.data.unitData.propData.AddProps(prop);
    }

    public static void AddUnitProp(this WorldUnitBase wunit, int propID, int addCount)
    {
        var curProps = wunit.GetUnitProps(propID);
        if (curProps?.Count == 0)
        {
            var conf = g.conf.itemProps.GetItem(propID);
            if (conf.isOverlay == 1)
            {
                wunit.data.unitData.propData.AddProps(propID, addCount);
            }
            else
            {
                for (var i = 0; i < addCount; i++)
                {
                    wunit.data.unitData.propData.AddProps(propID, 1);
                }
            }
            return;
        }

        var curCount = curProps.Sum(x => x.propsCount);
        var newCount = curCount + addCount;
        if (newCount <= 0)
        {
            foreach (var prop in curProps)
            {
                wunit.data.unitData.propData.allProps.Remove(prop);
            }
            return;
        }

        foreach (var prop in curProps)
        {
            if (prop.propsItem.isOverlay == 1)
            {
                prop.propsCount = newCount;
                return;
            }
        }

        if (curCount < newCount)
        {
            while (curCount < newCount)
            {
                wunit.data.unitData.propData.AddProps(propID, 1);
                curCount++;
            }
        }
        else
        {
            while (curCount > newCount)
            {
                wunit.data.unitData.propData.allProps.Remove(curProps[curCount - 1]);
                curCount--;
            }
        }
    }

    public static void RemoveUnitProp(this WorldUnitBase wunit, int propID, int count = int.MaxValue)
    {
        wunit.data.unitData.propData.DelProps(propID, count);
    }

    public static void RemoveUnitProp(this WorldUnitBase wunit, string soleID, int count = int.MaxValue)
    {
        if (count == int.MaxValue)
            wunit.data.unitData.propData.DelProps(soleID);
        else
            wunit.data.unitData.propData.DelProps(soleID, count);
    }

    public static void AddUnitMoney(this WorldUnitBase wunit, int addCount)
    {
        wunit.AddUnitProp(10001, addCount);
        if (wunit.GetUnitMoney() <= 0)
            g.world.playerUnit.data.RewardPropMoney(int.MinValue);
    }

    public static void SetUnitMoney(this WorldUnitBase wunit, int setCount)
    {
        wunit.AddUnitMoney(setCount - wunit.GetUnitMoney());
    }

    public static int GetUnitMoney(this WorldUnitBase wunit)
    {
        return wunit.GetUnitPropCount(10001);
    }

    public static void AddUnitContribution(this WorldUnitBase wunit, int addCount)
    {
        wunit.AddUnitProp(10011, addCount);
    }

    public static void SetUnitContribution(this WorldUnitBase wunit, int setCount)
    {
        wunit.AddUnitContribution(setCount - wunit.GetUnitContribution());
    }

    public static int GetUnitContribution(this WorldUnitBase wunit)
    {
        return wunit.GetUnitPropCount(10011);
    }

    public static void AddUnitMayorDegree(this WorldUnitBase wunit, int addCount)
    {
        wunit.AddUnitProp(10041, addCount);
    }

    public static void SetUnitMayorDegree(this WorldUnitBase wunit, int setCount)
    {
        wunit.AddUnitMayorDegree(setCount - wunit.GetUnitMayorDegree());
    }

    public static int GetUnitMayorDegree(this WorldUnitBase wunit)
    {
        return wunit.GetUnitPropCount(10041);
    }

    public static List<DataProps.PropsData> GetUnitProps(this WorldUnitBase wunit)
    {
        return wunit.data.unitData.propData.allProps.ToArray().ToList();
    }

    public static List<DataProps.PropsData> GetUnitProps(this WorldUnitBase wunit, int propID)
    {
        return wunit.GetUnitProps().Where(x => x.propsID == propID).ToList();
    }

    public static int GetUnitPropValue(this WorldUnitBase wunit, int propID)
    {
        return wunit.GetUnitProps(propID).Where(x => x.propsID == propID).Sum(x => x.propsCount * x.propsInfoBase.worth);
    }

    public static int GetUnitPropCount(this WorldUnitBase wunit, int propID)
    {
        return wunit.GetUnitProps(propID).Where(x => x.propsID == propID).Sum(x => x.propsCount);
    }

    public static int GetBasisPhysicSum(this WorldUnitBase wunit)
    {
        return wunit.GetProperty<int>(UnitPropertyEnum.BasisPalm) +
            wunit.GetProperty<int>(UnitPropertyEnum.BasisBlade) +
            wunit.GetProperty<int>(UnitPropertyEnum.BasisFist) +
            wunit.GetProperty<int>(UnitPropertyEnum.BasisFinger) +
            wunit.GetProperty<int>(UnitPropertyEnum.BasisSword) +
            wunit.GetProperty<int>(UnitPropertyEnum.BasisSpear);
    }

    public static int GetBasisMagicSum(this WorldUnitBase wunit)
    {
        return wunit.GetProperty<int>(UnitPropertyEnum.BasisFroze) +
            wunit.GetProperty<int>(UnitPropertyEnum.BasisFire) +
            wunit.GetProperty<int>(UnitPropertyEnum.BasisWood) +
            wunit.GetProperty<int>(UnitPropertyEnum.BasisThunder) +
            wunit.GetProperty<int>(UnitPropertyEnum.BasisWind) +
            wunit.GetProperty<int>(UnitPropertyEnum.BasisEarth);
    }

    public static UnitPropertyEnum GetBestBasis(this WorldUnitBase wunit)
    {
        var basises = new Dictionary<UnitPropertyEnum, int>
        {
            [UnitPropertyEnum.BasisPalm] = wunit.GetProperty<int>(UnitPropertyEnum.BasisPalm),
            [UnitPropertyEnum.BasisBlade] = wunit.GetProperty < int >(UnitPropertyEnum.BasisBlade),
            [UnitPropertyEnum.BasisFist] = wunit.GetProperty<int>(UnitPropertyEnum.BasisFist),
            [UnitPropertyEnum.BasisFinger] = wunit.GetProperty < int >(UnitPropertyEnum.BasisFinger),
            [UnitPropertyEnum.BasisSword] = wunit.GetProperty<int>(UnitPropertyEnum.BasisSword),
            [UnitPropertyEnum.BasisSpear] = wunit.GetProperty<int>(UnitPropertyEnum.BasisSpear),
            [UnitPropertyEnum.BasisFroze] = wunit.GetProperty<int>(UnitPropertyEnum.BasisFroze),
            [UnitPropertyEnum.BasisFire] = wunit.GetProperty<int>(UnitPropertyEnum.BasisFire),
            [UnitPropertyEnum.BasisWood] = wunit.GetProperty<int>(UnitPropertyEnum.BasisWood),
            [UnitPropertyEnum.BasisThunder] = wunit.GetProperty<int>(UnitPropertyEnum.BasisThunder),
            [UnitPropertyEnum.BasisWind] = wunit.GetProperty<int>(UnitPropertyEnum.BasisWind),
            [UnitPropertyEnum.BasisEarth] = wunit.GetProperty<int>(UnitPropertyEnum.BasisEarth)
        };
        var max = basises.Max(x => x.Value);
        return basises.FirstOrDefault(x => x.Value == max).Key;
    }

    public static UnitPropertyEnum GetWeaknessBasis(this WorldUnitBase wunit)
    {
        var basises = new Dictionary<UnitPropertyEnum, int>
        {
            [UnitPropertyEnum.BasisPalm] = wunit.GetProperty<int>(UnitPropertyEnum.BasisPalm),
            [UnitPropertyEnum.BasisBlade] = wunit.GetProperty<int>(UnitPropertyEnum.BasisBlade),
            [UnitPropertyEnum.BasisFist] = wunit.GetProperty<int>(UnitPropertyEnum.BasisFist),
            [UnitPropertyEnum.BasisFinger] = wunit.GetProperty<int>(UnitPropertyEnum.BasisFinger),
            [UnitPropertyEnum.BasisSword] = wunit.GetProperty<int>(UnitPropertyEnum.BasisSword),
            [UnitPropertyEnum.BasisSpear] = wunit.GetProperty<int>(UnitPropertyEnum.BasisSpear),
            [UnitPropertyEnum.BasisFroze] = wunit.GetProperty<int>(UnitPropertyEnum.BasisFroze),
            [UnitPropertyEnum.BasisFire] = wunit.GetProperty<int>(UnitPropertyEnum.BasisFire),
            [UnitPropertyEnum.BasisWood] = wunit.GetProperty<int>(UnitPropertyEnum.BasisWood),
            [UnitPropertyEnum.BasisThunder] = wunit.GetProperty<int>(UnitPropertyEnum.BasisThunder),
            [UnitPropertyEnum.BasisWind] = wunit.GetProperty<int>(UnitPropertyEnum.BasisWind),
            [UnitPropertyEnum.BasisEarth] = wunit.GetProperty<int>(UnitPropertyEnum.BasisEarth)
        };
        var max = basises.Min(x => x.Value);
        return basises.FirstOrDefault(x => x.Value == max).Key;
    }

    public static int GetGradeLvl(this WorldUnitBase wunit)
    {
        return (int)GradePhaseEnum.GetEnumByVal<GradePhaseEnum>(wunit.GetProperty<int>(UnitPropertyEnum.GradeID).ToString()).Grade;
    }
}