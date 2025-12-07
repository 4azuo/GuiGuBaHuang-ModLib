using ModLib.Attributes;
using ModLib.Const;
using ModLib.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModLib.Helper
{
    /// <summary>
    /// Helper for working with world units (NPCs, player character in world map).
    /// Provides extensive utilities for unit queries, relationships, inventory, attributes, and more.
    /// </summary>
    [ActionCat("WUnit")]
    public static class WUnitHelper
    {
        public static Il2CppSystem.Collections.Generic.List<DataProps.PropsData> LastAddedItems { get; private set; }
        public static Il2CppSystem.Collections.Generic.List<DataProps.PropsData> LastDeletedItems { get; private set; }

        public static UnitDynPropertyEnum[] ALL_PHYSIC_BASISES { get; } = new UnitDynPropertyEnum[]
            {
            UnitDynPropertyEnum.BasisPalm,
            UnitDynPropertyEnum.BasisBlade,
            UnitDynPropertyEnum.BasisFist,
            UnitDynPropertyEnum.BasisFinger,
            UnitDynPropertyEnum.BasisSword,
            UnitDynPropertyEnum.BasisSpear,
            };

        public static UnitDynPropertyEnum[] ALL_MAGIC_BASISES { get; } = new UnitDynPropertyEnum[]
            {
            UnitDynPropertyEnum.BasisFroze,
            UnitDynPropertyEnum.BasisFire,
            UnitDynPropertyEnum.BasisWood,
            UnitDynPropertyEnum.BasisThunder,
            UnitDynPropertyEnum.BasisWind,
            UnitDynPropertyEnum.BasisEarth
            };

        public static UnitDynPropertyEnum[] ALL_ARTISANSHIP_BASISES { get; } = new UnitDynPropertyEnum[]
            {
            UnitDynPropertyEnum.RefineElixir,
            UnitDynPropertyEnum.RefineWeapon,
            UnitDynPropertyEnum.Symbol,
            UnitDynPropertyEnum.Geomancy,
            UnitDynPropertyEnum.Herbal,
            UnitDynPropertyEnum.Mine
            };

        public static UnitDynPropertyEnum[] ALL_BASISES { get; } = ALL_PHYSIC_BASISES.Concat(ALL_MAGIC_BASISES).ToArray();

        /// <summary>
        /// Sets unit position on map.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <param name="p">New position</param>
        public static void SetUnitPos(this WorldUnitBase wunit, Vector2Int p)
        {
            //wunit.CreateAction(new UnitActionSetPoint(p));
            wunit.data.unitData.SetPoint(p);
            SceneType.map?.world.UpdateAllUI();
        }

        /// <summary>
        /// Sets unit to random position near target point.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <param name="p">Center position</param>
        /// <param name="r">Radius</param>
        public static void SetUnitRandomPos(this WorldUnitBase wunit, Vector2Int p, int r = 8)
        {
            var x = (p.x + CommonTool.Random(-r, r)).FixValue(0, g.data.grid.mapWidth);
            var y = (p.y + CommonTool.Random(-r, r)).FixValue(0, g.data.grid.mapHeight);
            wunit.SetUnitPos(new Vector2Int(x, y));
        }

        /// <summary>
        /// Gets unit's current map position.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>Position</returns>
        public static Vector2Int GetUnitPos(this WorldUnitBase wunit)
        {
            return wunit.data.unitData.GetPoint();
        }

        /// <summary>
        /// Gets the area ID of unit's current position.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>Area ID</returns>
        public static int GetUnitPosAreaId(this WorldUnitBase wunit)
        {
            return wunit.data.unitData.pointGridData.areaBaseID;
        }

        /// <summary>
        /// Gets the map building at unit's position.
        /// </summary>
        /// <typeparam name="T">Building type</typeparam>
        /// <param name="wunit">World unit</param>
        /// <returns>Building or null</returns>
        public static T GetMapBuild<T>(this WorldUnitBase wunit) where T : MapBuildBase
        {
            return g.world.build.GetBuild<T>(wunit.GetUnitPos());
        }

        /// <summary>
        /// Gets all equipped items (artifacts, ring, mount, outfit, props).
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>List of equipped items</returns>
        public static List<DataProps.PropsData> GetEquippedItems(this WorldUnitBase wunit)
        {
            var rs = new List<DataProps.PropsData>();
            rs.AddRange(wunit.GetEquippedArtifacts());
            rs.Add(wunit.GetEquippedRing());
            rs.Add(wunit.GetEquippedMount());
            rs.Add(wunit.GetEquippedOutfit());
            rs.AddRange(wunit.GetEquippedProps());
            return rs;
        }

        /// <summary>
        /// Gets all equipped artifacts.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>Array of artifacts</returns>
        public static DataProps.PropsData[] GetEquippedArtifacts(this WorldUnitBase wunit)
        {
            return wunit.GetEquippedProps().Where(x => x?.propsItem?.IsArtifact() != null).ToArray();
        }

        /// <summary>
        /// Gets equipped ring.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>Ring or null</returns>
        public static DataProps.PropsData GetEquippedRing(this WorldUnitBase wunit)
        {
            return wunit.GetEquippedProps().FirstOrDefault(x => x?.propsItem?.IsRing() != null);
        }

        /// <summary>
        /// Gets equipped mount.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>Mount or null</returns>
        public static DataProps.PropsData GetEquippedMount(this WorldUnitBase wunit)
        {
            return wunit.GetEquippedProps().FirstOrDefault(x => x?.propsItem?.IsMount() != null);
        }

        /// <summary>
        /// Gets equipped outfit/clothing.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>Outfit or null</returns>
        public static DataProps.PropsData GetEquippedOutfit(this WorldUnitBase wunit)
        {
            return wunit.GetEquippedProps().FirstOrDefault(x => x?.propsItem?.IsOutfit() != null);
        }

        /// <summary>
        /// Gets all equipped props.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>Array of props</returns>
        public static DataProps.PropsData[] GetEquippedProps(this WorldUnitBase wunit)
        {
            if (wunit == null)
                return new DataProps.PropsData[0];
            return wunit.data.unitData.propData.GetEquipProps().ToArray();
        }

        /// <summary>
        /// Gets all unequipped props in inventory.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>Array of unequipped props</returns>
        public static DataProps.PropsData[] GetUnequippedProps(this WorldUnitBase wunit)
        {
            var equippedProps = wunit.GetEquippedProps();
            return wunit.GetUnitProps().Where(x => !equippedProps.Any(y => y.soleID == x.soleID)).ToArray();
        }

        /// <summary>
        /// Gets all action martial data (skills/abilities).
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>Dictionary of martial data</returns>
        public static Il2CppSystem.Collections.Generic.Dictionary<string, DataUnit.ActionMartialData> GetActionMartials(this WorldUnitBase wunit)
        {
            return wunit.data.unitData.allActionMartial;
        }

        /// <summary>
        /// Gets equipped left attack skill.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>Martial data or null</returns>
        public static DataProps.MartialData GetMartialAttack(this WorldUnitBase wunit)
        {
            var actMStep = wunit.data.unitData.GetActionMartial(wunit.data.unitData.skillLeft);
            var martialData = actMStep?.data?.To<DataProps.MartialData>();
            return martialData;
        }

        /// <summary>
        /// Gets equipped movement skill (step).
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>Martial data or null</returns>
        public static DataProps.MartialData GetMartialStep(this WorldUnitBase wunit)
        {
            var actMStep = wunit.data.unitData.GetActionMartial(wunit.data.unitData.step);
            var martialData = actMStep?.data?.To<DataProps.MartialData>();
            return martialData;
        }

        /// <summary>
        /// Gets equipped right special skill.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>Martial data or null</returns>
        public static DataProps.MartialData GetMartialSpecial(this WorldUnitBase wunit)
        {
            var actMStep = wunit.data.unitData.GetActionMartial(wunit.data.unitData.skillRight);
            var martialData = actMStep?.data?.To<DataProps.MartialData>();
            return martialData;
        }

        /// <summary>
        /// Gets equipped ultimate skill.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>Martial data or null</returns>
        public static DataProps.MartialData GetMartialUltimate(this WorldUnitBase wunit)
        {
            var actMStep = wunit.data.unitData.GetActionMartial(wunit.data.unitData.ultimate);
            var martialData = actMStep?.data?.To<DataProps.MartialData>();
            return martialData;
        }

        /// <summary>
        /// Gets martial by sole ID.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <param name="soleId">Martial sole ID</param>
        /// <returns>Martial data or null</returns>
        public static DataProps.MartialData GetMartial(this WorldUnitBase wunit, string soleId)
        {
            var actMStep = wunit.data.unitData.GetActionMartial(soleId);
            var martialData = actMStep?.data?.To<DataProps.MartialData>();
            return martialData;
        }

        /// <summary>
        /// Gets the martial type (slot) for a given sole ID.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <param name="soleId">Martial sole ID</param>
        /// <returns>MartialType or null</returns>
        public static MartialType? GetMartialType(this WorldUnitBase wunit, string soleId)
        {
            if (soleId == wunit.data.unitData.skillLeft)
                return MartialType.SkillLeft;
            if (soleId == wunit.data.unitData.skillRight)
                return MartialType.SkillRight;
            if (soleId == wunit.data.unitData.step)
                return MartialType.Step;
            if (soleId == wunit.data.unitData.ultimate)
                return MartialType.Ultimate;
            if (wunit.data.unitData.abilitys.Contains(soleId))
                return MartialType.Ability;
            return null;
        }

        /// <summary>
        /// Gets unit's unique ID.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>Unit ID</returns>
        public static string GetUnitId(this WorldUnitBase wunit)
        {
            return wunit?.data?.unitData?.unitID;
        }

        /// <summary>
        /// Checks if unit is the player character.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>True if player</returns>
        public static bool IsPlayer(this WorldUnitBase wunit)
        {
            return wunit.GetUnitId() == g.world.playerUnit.data.unitData.unitID;
        }

        /// <summary>
        /// Checks if unit is a hero/legend.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>True if hero</returns>
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

        /// <summary>
        /// Adds a luck/fortune to unit.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <param name="luckId">Luck ID</param>
        /// <param name="dur">Duration in months (-1 = permanent)</param>
        /// <returns>Action result</returns>
        public static int AddLuck(this WorldUnitBase wunit, int luckId, int dur = -1)
        {
            return wunit.CreateAction(new UnitActionLuckAdd(luckId, dur));
        }

        /// <summary>
        /// Removes a luck/fortune from unit.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <param name="luckId">Luck ID</param>
        /// <returns>Action result</returns>
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

        /// <summary>
        /// Gets unit property value.
        /// </summary>
        /// <typeparam name="T">Property type</typeparam>
        /// <param name="wunit">World unit</param>
        /// <param name="propType">Property type enum</param>
        /// <returns>Property value</returns>
        public static T GetProperty<T>(this WorldUnitBase wunit, UnitPropertyEnum propType)
        {
            return propType.Get<T>(wunit.data.unitData.propertyData);
        }

        /// <summary>
        /// Sets unit property value.
        /// </summary>
        /// <typeparam name="T">Property type</typeparam>
        /// <param name="wunit">World unit</param>
        /// <param name="propType">Property type enum</param>
        /// <param name="newValue">New value</param>
        public static void SetProperty<T>(this WorldUnitBase wunit, UnitPropertyEnum propType, T newValue)
        {
            propType.Set<T>(wunit.data.unitData.propertyData, newValue);
        }

        /// <summary>
        /// Adds to unit property value.
        /// </summary>
        /// <typeparam name="T">Property type</typeparam>
        /// <param name="wunit">World unit</param>
        /// <param name="propType">Property type enum</param>
        /// <param name="addValue">Value to add</param>
        public static void AddProperty<T>(this WorldUnitBase wunit, UnitPropertyEnum propType, T addValue)
        {
            SetProperty<T>(wunit, propType, MathLogicOptions.Plus.Exe(wunit.GetProperty<T>(propType).Parse<float>(), addValue.Parse<float>()).Parse<T>());
        }

        /// <summary>
        /// Gets dynamic property value.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <param name="propType">Property type enum</param>
        /// <returns>DynInt value</returns>
        public static DynInt GetDynProperty(this WorldUnitBase wunit, UnitDynPropertyEnum propType)
        {
            return propType.Get(wunit.data.dynUnitData);
        }

        //public static void SetDynProperty(this WorldUnitBase wunit, UnitDynPropertyEnum propType, DynInt newValue)
        //{
        //    propType.Set(wunit.data.dynUnitData, newValue);
        //}

        /// <summary>
        /// Gets next cultivation phase ID.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>Next phase ID</returns>
        public static int GetNextPhaseId(this WorldUnitBase wunit)
        {
            return g.conf.roleGrade.GetNextGradeItem(wunit.GetPhaseId()).id;
        }

        /// <summary>
        /// Gets next cultivation phase configuration.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>Phase config</returns>
        public static ConfRoleGradeItem GetNextPhaseConf(this WorldUnitBase wunit)
        {
            return g.conf.roleGrade.GetNextGradeItem(wunit.GetPhaseId());
        }

        /// <summary>
        /// Gets current experience points.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>Experience</returns>
        public static int GetExp(this WorldUnitBase wunit)
        {
            return wunit.GetDynProperty(UnitDynPropertyEnum.Exp).value;
        }

        /// <summary>
        /// Gets max experience for current phase.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>Max experience</returns>
        public static int GetMaxExpCurrentPhase(this WorldUnitBase wunit)
        {
            return g.conf.roleGrade.GetNextGradeItem(wunit.GetPhaseId()).exp;
        }

        /// <summary>
        /// Gets minimum experience for current phase.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>Min experience</returns>
        public static int GetMinExpCurrentPhase(this WorldUnitBase wunit)
        {
            return g.conf.roleGrade.GetItem(wunit.GetPhaseId()).exp;
        }

        /// <summary>
        /// Gets experience range for current phase.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>Experience range</returns>
        public static int GetExpCurrentPhase(this WorldUnitBase wunit)
        {
            return wunit.GetMaxExpCurrentPhase() - wunit.GetMinExpCurrentPhase();
        }

        /// <summary>
        /// Gets experience needed to level up.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>Needed experience</returns>
        public static int GetNeedExpToLevelUp(this WorldUnitBase wunit)
        {
            return wunit.GetMaxExpCurrentPhase() - wunit.GetExp();
        }

        /// <summary>
        /// Checks if unit has max experience for phase.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>True if full</returns>
        public static bool IsFullExp(this WorldUnitBase wunit)
        {
            return wunit.GetExp() >= wunit.GetMaxExpCurrentPhase();
        }

        /// <summary>
        /// Adds experience to unit (handles bottleneck luck).
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <param name="exp">Experience to add</param>
        public static void AddExp(this WorldUnitBase wunit, int exp)
        {
            var addExp = Math.Max(Math.Min(wunit.GetExp() + exp, wunit.GetMaxExpCurrentPhase()), wunit.GetMinExpCurrentPhase());
            wunit.SetProperty<int>(UnitPropertyEnum.Exp, addExp);

            var gradeEnum = GradePhaseEnum.GetEnumByVal<GradePhaseEnum>(wunit.GetPhaseId().ToString());
            if (gradeEnum != null && gradeEnum.Bottleneck != BottleneckEnum.None)
            {
                if (addExp == wunit.GetMaxExpCurrentPhase())
                {
                    wunit.AddLuck((int)gradeEnum.Bottleneck);
                }
                else
                {
                    wunit.DelLuck((int)gradeEnum.Bottleneck);
                }
            }
        }

        /// <summary>
        /// Clears all experience.
        /// </summary>
        /// <param name="wunit">World unit</param>
        public static void ClearExp(this WorldUnitBase wunit)
        {
            wunit.AddExp(int.MinValue);
        }

        /// <summary>
        /// Resets cultivation grade to level 1.
        /// </summary>
        /// <param name="wunit">World unit</param>
        public static void ResetGradeLevel(this WorldUnitBase wunit)
        {
            wunit.SetProperty<int>(UnitPropertyEnum.GradeID, 1);
        }

        /// <summary>
        /// Creates and adds a skill prop to unit.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <param name="id">Skill ID</param>
        /// <param name="grade">Grade level</param>
        /// <returns>Created skill data</returns>
        public static DataProps.PropsSkillData AddUnitSkillProp(this WorldUnitBase wunit, int id, int grade)
        {
            var skill = MartialTool.CreateSkillData(id, grade);
            wunit.AddUnitProp(skill.data);
            return skill;
        }

        /// <summary>
        /// Creates and adds a step prop to unit.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <param name="id">Step ID</param>
        /// <param name="grade">Grade level</param>
        /// <returns>Created step data</returns>
        public static DataProps.PropsStepData AddUnitStepProp(this WorldUnitBase wunit, int id, int grade)
        {
            var skill = MartialTool.CreateStepData(id, grade);
            wunit.AddUnitProp(skill.data);
            return skill;
        }

        /// <summary>
        /// Creates and adds an ability prop to unit.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <param name="id">Ability ID</param>
        /// <param name="grade">Grade level</param>
        /// <returns>Created ability data</returns>
        public static DataProps.PropsAbilityData AddUnitAbilityProp(this WorldUnitBase wunit, int id, int grade)
        {
            var skill = MartialTool.CreateAbilityData(id, grade);
            wunit.AddUnitProp(skill.data);
            return skill;
        }

        /// <summary>
        /// Creates and adds a martial prop by type.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <param name="mtype">Martial type</param>
        /// <param name="id">Martial ID</param>
        /// <param name="grade">Grade level</param>
        /// <returns>Created martial data or null</returns>
        public static DataProps.MartialData AddUnitProp(this WorldUnitBase wunit, MartialType mtype, int id, int grade)
        {
            if (mtype == MartialType.Ability)
                return wunit.AddUnitAbilityProp(id, grade);
            else if (mtype == MartialType.Step)
                return wunit.AddUnitStepProp(id, grade);
            else if (mtype == MartialType.SkillLeft || mtype == MartialType.SkillRight || mtype == MartialType.Ultimate)
                return wunit.AddUnitSkillProp(id, grade);
            return null;
        }

        /// <summary>
        /// Adds a prop to unit inventory.
        /// Result stored in LastAddedItems.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <param name="prop">Prop data</param>
        public static void AddUnitProp(this WorldUnitBase wunit, DataProps.PropsData prop)
        {
            LastAddedItems = wunit.data.unitData.propData.AddProps(prop);
        }

        /// <summary>
        /// Adds props by ID and count (handles stacking).
        /// Result stored in LastAddedItems.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <param name="propID">Prop ID</param>
        /// <param name="addCount">Count to add</param>
        public static void AddUnitProp(this WorldUnitBase wunit, int propID, int addCount)
        {
            LastAddedItems = new Il2CppSystem.Collections.Generic.List<DataProps.PropsData>();

            var curProps = wunit.GetUnitProps(propID);
            if (curProps?.Count == 0)
            {
                var conf = g.conf.itemProps.GetItem(propID);
                if (conf.isOverlay == 1)
                {
                    LastAddedItems = wunit.data.unitData.propData.AddProps(propID, addCount);
                }
                else
                {
                    for (var i = 0; i < addCount; i++)
                    {
                        LastAddedItems.AddRange(wunit.data.unitData.propData.AddProps(propID, 1));
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
                    LastAddedItems.Add(prop);
                    wunit.data.unitData.propData.allProps.Remove(prop);
                }
                return;
            }

            foreach (var prop in curProps)
            {
                if (prop?.propsItem?.isOverlay == 1)
                {
                    prop.propsCount = newCount;
                    LastAddedItems.Add(prop);
                    return;
                }
            }

            if (curCount < newCount)
            {
                while (curCount < newCount)
                {
                    LastAddedItems.AddRange(wunit.data.unitData.propData.AddProps(propID, 1));
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

        /// <summary>
        /// Removes props by ID.
        /// Result stored in LastDeletedItems.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <param name="propID">Prop ID</param>
        /// <param name="count">Count to remove</param>
        public static void RemoveUnitProp(this WorldUnitBase wunit, int propID, int count = int.MaxValue)
        {
            LastDeletedItems = new Il2CppSystem.Collections.Generic.List<DataProps.PropsData>();
            if (count == 0)
                return;
            wunit.data.unitData.propData.DelProps(propID, count);
        }

        /// <summary>
        /// Removes prop by sole ID.
        /// Result stored in LastDeletedItems.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <param name="soleID">Prop sole ID</param>
        /// <param name="count">Count to remove</param>
        public static void RemoveUnitProp(this WorldUnitBase wunit, string soleID, int count = int.MaxValue)
        {
            LastDeletedItems = new Il2CppSystem.Collections.Generic.List<DataProps.PropsData>();
            if (count == 0)
                return;
            if (count == int.MaxValue)
                LastDeletedItems.Add(wunit.data.unitData.propData.DelProps(soleID));
            else
                LastDeletedItems.Add(wunit.data.unitData.propData.DelProps(soleID, count));
        }

        /// <summary>
        /// Adds money to unit.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <param name="addCount">Amount to add</param>
        public static void AddUnitMoney(this WorldUnitBase wunit, int addCount)
        {
            //if (addCount == 0)
            //    return;
            //wunit.AddUnitProp(ModLibConst.MONEY_PROP_ID, addCount);
            //if (wunit.GetUnitMoney() <= 0)
            //    wunit.data.RewardPropMoney(int.MinValue);
            wunit.data.RewardPropMoney(addCount);
        }

        /// <summary>
        /// Sets unit money to specific amount.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <param name="setCount">Amount to set</param>
        public static void SetUnitMoney(this WorldUnitBase wunit, int setCount)
        {
            wunit.AddUnitMoney(setCount - wunit.GetUnitMoney());
        }

        /// <summary>
        /// Gets unit's current money.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>Money amount</returns>
        public static int GetUnitMoney(this WorldUnitBase wunit)
        {
            return wunit.GetUnitPropCount(ModLibConst.MONEY_PROP_ID);
        }

        /// <summary>
        /// Adds contribution points to unit.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <param name="addCount">Amount to add</param>
        public static void AddUnitContribution(this WorldUnitBase wunit, int addCount)
        {
            //if (addCount == 0)
            //    return;
            //wunit.AddUnitProp(ModLibConst.CONTRIBUTION_PROP_ID, addCount);
            if (addCount > 0)
                wunit.data.RewardPropItem(ModLibConst.CONTRIBUTION_PROP_ID, Math.Abs(addCount), false);
            else if (addCount < 0)
                wunit.data.CostPropItem(ModLibConst.CONTRIBUTION_PROP_ID, Math.Abs(addCount), false);
        }

        /// <summary>
        /// Sets unit contribution to specific amount.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <param name="setCount">Amount to set</param>
        public static void SetUnitContribution(this WorldUnitBase wunit, int setCount)
        {
            //if (setCount <= 0)
            //{
            //    wunit.RemoveUnitProp(ModLibConst.CONTRIBUTION_PROP_ID);
            //}
            //else
            //{
            wunit.AddUnitContribution(setCount - wunit.GetUnitContribution());
            //}
        }

        /// <summary>
        /// Gets unit's current contribution points.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>Contribution amount</returns>
        public static int GetUnitContribution(this WorldUnitBase wunit)
        {
            return wunit.GetUnitPropCount(ModLibConst.CONTRIBUTION_PROP_ID);
        }

        /// <summary>
        /// Adds mayor degree (town leadership points) to unit.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <param name="addCount">Amount to add</param>
        public static void AddUnitMayorDegree(this WorldUnitBase wunit, int addCount)
        {
            //if (addCount == 0)
            //    return;
            //wunit.AddUnitProp(ModLibConst.MAYOR_DEGREE_PROP_ID, addCount);
            if (addCount > 0)
                wunit.data.RewardPropItem(ModLibConst.MAYOR_DEGREE_PROP_ID, Math.Abs(addCount), false);
            else if (addCount < 0)
                wunit.data.CostPropItem(ModLibConst.MAYOR_DEGREE_PROP_ID, Math.Abs(addCount), false);
        }

        /// <summary>
        /// Sets unit mayor degree to specific amount.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <param name="setCount">Amount to set</param>
        public static void SetUnitMayorDegree(this WorldUnitBase wunit, int setCount)
        {
            //if (setCount <= 0)
            //{
            //    wunit.RemoveUnitProp(ModLibConst.MAYOR_DEGREE_PROP_ID);
            //}
            //else
            //{
            wunit.AddUnitMayorDegree(setCount - wunit.GetUnitMayorDegree());
            //}
        }

        /// <summary>
        /// Gets unit's current mayor degree.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>Mayor degree amount</returns>
        public static int GetUnitMayorDegree(this WorldUnitBase wunit)
        {
            return wunit.GetUnitPropCount(ModLibConst.MAYOR_DEGREE_PROP_ID);
        }

        /// <summary>
        /// Gets all props in unit inventory.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>List of props</returns>
        public static List<DataProps.PropsData> GetUnitProps(this WorldUnitBase wunit)
        {
            if (wunit == null)
                return new List<DataProps.PropsData>();
            return wunit.data.unitData.propData.allProps.ToList();
        }

        /// <summary>
        /// Gets all props of specific ID.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <param name="propID">Prop ID</param>
        /// <returns>List of matching props</returns>
        public static List<DataProps.PropsData> GetUnitProps(this WorldUnitBase wunit, int propID)
        {
            return wunit.GetUnitProps().Where(x => x.propsID == propID).ToList();
        }

        /// <summary>
        /// Gets prop by sole ID.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <param name="soleID">Prop sole ID</param>
        /// <returns>Prop or null</returns>
        public static DataProps.PropsData GetUnitProp(this WorldUnitBase wunit, string soleID)
        {
            return wunit.GetUnitProps().FirstOrDefault(x => x.soleID == soleID);
        }

        /// <summary>
        /// Gets copy of prop with specific count.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <param name="soleID">Prop sole ID</param>
        /// <param name="count">Count</param>
        /// <param name="customSoleID">Custom sole ID</param>
        /// <returns>Prop copy or null</returns>
        public static DataProps.PropsData GetUnitPropN(this WorldUnitBase wunit, string soleID, int count, string customSoleID = null)
        {
            var prop = wunit.GetUnitProp(soleID);
            if (prop == null && count > prop.propsCount)
                return null;
            return prop.CopyProp(count);
        }

        /// <summary>
        /// Gets total value of props by ID.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <param name="propID">Prop ID</param>
        /// <returns>Total worth</returns>
        public static int GetUnitPropValue(this WorldUnitBase wunit, int propID)
        {
            return wunit.GetUnitProps(propID).Where(x => x.propsID == propID).Sum(x => x.propsCount * x.propsInfoBase.worth);
        }

        /// <summary>
        /// Gets total count of props by ID.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <param name="propID">Prop ID</param>
        /// <returns>Total count</returns>
        public static int GetUnitPropCount(this WorldUnitBase wunit, int propID)
        {
            return wunit.GetUnitProps(propID).Where(x => x.propsID == propID).Sum(x => x.propsCount);
        }

        /// <summary>
        /// Gets sum of all physical (martial) basis values.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>Physical basis sum</returns>
        public static int GetBasisPhysicSum(this WorldUnitBase wunit)
        {
            return ALL_PHYSIC_BASISES.Sum(x => wunit.GetDynProperty(x).value);
        }

        /// <summary>
        /// Gets sum of all magic (spell) basis values.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>Magic basis sum</returns>
        public static int GetBasisMagicSum(this WorldUnitBase wunit)
        {
            return ALL_MAGIC_BASISES.Sum(x => wunit.GetDynProperty(x).value);
        }

        /// <summary>
        /// Gets sum of all artisanship basis values.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>Artisanship sum</returns>
        public static int GetArtisanshipSum(this WorldUnitBase wunit)
        {
            return ALL_ARTISANSHIP_BASISES.Sum(x => wunit.GetDynProperty(x).value);
        }

        /// <summary>
        /// Gets sum of all physical and magic basis values.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>All basis sum</returns>
        public static int GetAllBasisesSum(this WorldUnitBase wunit)
        {
            return ALL_BASISES.Sum(x => wunit.GetDynProperty(x).value);
        }

        /// <summary>
        /// Gets unit's best (highest) physical basis.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>Best physical basis</returns>
        public static UnitDynPropertyEnum GetBestPhysicBasis(this WorldUnitBase wunit)
        {
            return ALL_PHYSIC_BASISES.Select(x => new { PropEnum = x, Value = wunit.GetDynProperty(x).value }).OrderByDescending(x => x.Value).First().PropEnum;
        }

        /// <summary>
        /// Gets unit's weakness (lowest) physical basis.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>Weakest physical basis</returns>
        public static UnitDynPropertyEnum GetWeaknessPhysicBasis(this WorldUnitBase wunit)
        {
            return ALL_PHYSIC_BASISES.Select(x => new { PropEnum = x, Value = wunit.GetDynProperty(x).value }).OrderBy(x => x.Value).First().PropEnum;
        }

        /// <summary>
        /// Gets unit's best (highest) magic basis.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>Best magic basis</returns>
        public static UnitDynPropertyEnum GetBestMagicBasis(this WorldUnitBase wunit)
        {
            return ALL_MAGIC_BASISES.Select(x => new { PropEnum = x, Value = wunit.GetDynProperty(x).value }).OrderByDescending(x => x.Value).First().PropEnum;
        }

        /// <summary>
        /// Gets unit's weakness (lowest) magic basis.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>Weakest magic basis</returns>
        public static UnitDynPropertyEnum GetWeaknessMagicBasis(this WorldUnitBase wunit)
        {
            return ALL_MAGIC_BASISES.Select(x => new { PropEnum = x, Value = wunit.GetDynProperty(x).value }).OrderBy(x => x.Value).First().PropEnum;
        }

        /// <summary>
        /// Gets unit's best (highest) artisanship basis.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>Best artisanship basis</returns>
        public static UnitDynPropertyEnum GetBestArtisanshipBasis(this WorldUnitBase wunit)
        {
            return ALL_ARTISANSHIP_BASISES.Select(x => new { PropEnum = x, Value = wunit.GetDynProperty(x).value }).OrderByDescending(x => x.Value).First().PropEnum;
        }

        /// <summary>
        /// Gets unit's weakness (lowest) artisanship basis.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>Weakest artisanship basis</returns>
        public static UnitDynPropertyEnum GetWeaknessArtisanshipBasis(this WorldUnitBase wunit)
        {
            return ALL_ARTISANSHIP_BASISES.Select(x => new { PropEnum = x, Value = wunit.GetDynProperty(x).value }).OrderBy(x => x.Value).First().PropEnum;
        }

        /// <summary>
        /// Gets unit's overall best basis (physical or magic).
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>Best basis</returns>
        public static UnitDynPropertyEnum GetBestBasis(this WorldUnitBase wunit)
        {
            return ALL_BASISES.Select(x => new { PropEnum = x, Value = wunit.GetDynProperty(x).value }).OrderByDescending(x => x.Value).First().PropEnum;
        }

        /// <summary>
        /// Gets unit's overall weakest basis (physical or magic).
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>Weakest basis</returns>
        public static UnitDynPropertyEnum GetWeaknessBasis(this WorldUnitBase wunit)
        {
            return ALL_BASISES.Select(x => new { PropEnum = x, Value = wunit.GetDynProperty(x).value }).OrderBy(x => x.Value).First().PropEnum;
        }

        /// <summary>
        /// Gets unit's grade level (1-9).
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>Grade level</returns>
        public static int GetGradeLvl(this WorldUnitBase wunit)
        {
            return wunit.GetGradeConf().grade;
        }

        /// <summary>
        /// Gets unit's phase level (cultivation realm).
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>Phase level</returns>
        public static int GetPhaseLvl(this WorldUnitBase wunit)
        {
            return wunit.GetGradeConf().phase;
        }

        /// <summary>
        /// Gets unit's current phase ID.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>Phase ID</returns>
        public static int GetPhaseId(this WorldUnitBase wunit)
        {
            return wunit.GetDynProperty(UnitDynPropertyEnum.GradeID).value;
        }

        /// <summary>
        /// Gets unit's grade configuration.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>Grade config</returns>
        public static ConfRoleGradeItem GetGradeConf(this WorldUnitBase wunit)
        {
            return g.conf.roleGrade.GetItem(wunit.GetPhaseId());
        }

        /// <summary>
        /// Removes all equipped items from unit.
        /// </summary>
        /// <param name="wunit">World unit</param>
        public static void RemoveEquippedItems(this WorldUnitBase wunit)
        {
            foreach (var item in wunit.GetEquippedItems())
            {
                wunit.RemoveUnitProp(item.soleID);
            }
        }

        /// <summary>
        /// Removes all items from unit inventory.
        /// </summary>
        /// <param name="wunit">World unit</param>
        public static void RemoveAllItems(this WorldUnitBase wunit)
        {
            foreach (var item in wunit.GetUnitProps())
            {
                wunit.RemoveUnitProp(item.soleID);
            }
        }

        /// <summary>
        /// Removes all items from player storage (player only).
        /// </summary>
        /// <param name="wunit">World unit</param>
        public static void RemoveAllStorageItems(this WorldUnitBase wunit)
        {
            if (!wunit.IsPlayer())
                return;
            GameHelper.GetStorage().data.propData.allProps = new Il2CppSystem.Collections.Generic.List<DataProps.PropsData>();
        }

        /// <summary>
        /// Removes specific item from player storage (player only).
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <param name="propId">Prop ID</param>
        public static void RemoveStorageItem(this WorldUnitBase wunit, int propId)
        {
            if (!wunit.IsPlayer())
                return;
            foreach (var item in GameHelper.GetStorageItems())
            {
                if (item.propsID == propId)
                    GameHelper.GetStorage().data.propData.allProps.Remove(item);
            }
        }

        /// <summary>
        /// Gets hashed position ID.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>Position ID</returns>
        public static int GetPositionId(this WorldUnitBase wunit)
        {
            return wunit.data.unitData.pointX * wunit.data.unitData.pointY;
        }

        /// <summary>
        /// Gets all units within range of this unit.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <param name="range">Search range</param>
        /// <param name="isGetHide">Include hidden units</param>
        /// <param name="isGetPlayer">Include player</param>
        /// <returns>List of units</returns>
        public static Il2CppSystem.Collections.Generic.List<WorldUnitBase> GetUnitsAround(this WorldUnitBase wunit, int range = 16, bool isGetHide = false, bool isGetPlayer = true)
        {
            return GetUnitsAround(wunit.GetUnitPos(), range, isGetHide, isGetPlayer);
        }

        /// <summary>
        /// Gets all units within range of position.
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="range">Search range</param>
        /// <param name="isGetHide">Include hidden units</param>
        /// <param name="isGetPlayer">Include player</param>
        /// <returns>List of units</returns>
        public static Il2CppSystem.Collections.Generic.List<WorldUnitBase> GetUnitsAround(int x, int y, int range = 16, bool isGetHide = false, bool isGetPlayer = true)
        {
            return GetUnitsAround(new Vector2Int(x, y), range, isGetHide, isGetPlayer);
        }

        /// <summary>
        /// Gets all units within range of position.
        /// </summary>
        /// <param name="pos">Position</param>
        /// <param name="range">Search range</param>
        /// <param name="isGetHide">Include hidden units</param>
        /// <param name="isGetPlayer">Include player</param>
        /// <returns>List of units</returns>
        public static Il2CppSystem.Collections.Generic.List<WorldUnitBase> GetUnitsAround(Vector2Int pos, int range = 16, bool isGetHide = false, bool isGetPlayer = true)
        {
            return g.world.unit.GetUnitExact(pos, range, isGetHide, isGetPlayer);
        }

        /// <summary>
        /// Gets strongest unit (by attack) from collection.
        /// </summary>
        /// <param name="wunits">Unit collection</param>
        /// <returns>Strongest unit or null</returns>
        public static WorldUnitBase GetStrongestWUnit(this IEnumerable<WorldUnitBase> wunits)
        {
            return wunits.OrderByDescending(x => x.GetDynProperty(UnitDynPropertyEnum.Attack).value).FirstOrDefault();
        }

        /// <summary>
        /// Gets most famous unit (by reputation) from collection.
        /// </summary>
        /// <param name="wunits">Unit collection</param>
        /// <returns>Most famous unit or null</returns>
        public static WorldUnitBase GetFamousWUnit(this IEnumerable<WorldUnitBase> wunits)
        {
            return wunits.OrderByDescending(x => x.GetDynProperty(UnitDynPropertyEnum.Reputation).value).FirstOrDefault();
        }

        /// <summary>
        /// Checks if unit is righteous (good alignment).
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>True if righteous</returns>
        public static bool IsRighteous(this WorldUnitBase wunit)
        {
            return wunit.GetDynProperty(UnitDynPropertyEnum.StandUp).value > wunit.GetDynProperty(UnitDynPropertyEnum.StandDown).value;
        }

        /// <summary>
        /// Gets unit's dominant alignment value (righteousness or evil).
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>Stand value</returns>
        public static int GetStandValue(this WorldUnitBase wunit)
        {
            if (wunit.IsRighteous())
                return wunit.GetDynProperty(UnitDynPropertyEnum.StandUp).value;
            return wunit.GetDynProperty(UnitDynPropertyEnum.StandDown).value;
        }

        /// <summary>
        /// Checks if two units are from same sect/school.
        /// </summary>
        /// <param name="wunitA">First unit</param>
        /// <param name="wunitB">Second unit</param>
        /// <returns>True if same sect</returns>
        public static bool IsSameSect(this WorldUnitBase wunitA, WorldUnitBase wunitB)
        {
            return wunitA.data.school?.schoolNameID == wunitB.data.school?.schoolNameID && wunitA.data.school?.schoolNameID != null;
        }

        /// <summary>
        /// Gets unit's display name.
        /// </summary>
        /// <param name="wunit">World unit</param>
        /// <returns>Name or empty string</returns>
        public static string GetName(this WorldUnitBase wunit)
        {
            return wunit?.data?.unitData?.propertyData?.GetName() ?? string.Empty;
        }
    }
}