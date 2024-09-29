using MOD_nE7UL2.Mod;
using ModLib.Object;
using System;

namespace MOD_nE7UL2.Enum
{
    public class BuildingCostEnum : EnumObject
    {
        public static BuildingCostEnum SchoolTrainingHallBuildCost { get; } = new BuildingCostEnum(-1f, MapBuildSubType.SchoolTrainingHall, true, false, null);
        public static BuildingCostEnum SchoolPortalBuildCost { get; } = new BuildingCostEnum(8.00f, MapBuildSubType.SchoolTransfer, true, false, new long[]
        {
            /*01*/30000,
            /*02*/30000,
            /*03*/80000,
            /*04*/80000,
            /*05*/250000,
            /*06*/250000,
            /*07*/800000,
            /*08*/800000,
            /*09*/1500000,
            /*10*/3000000,
        });
        //public static BuildingCostEnum SchoolStorageBuildCost { get; } = new BuildingCostEnum(10.00f, MapBuildSubType.SchoolStorage, true, false, new long[]
        //{
        //    /*01*/10000,
        //    /*02*/10000,
        //    /*03*/20000,
        //    /*04*/20000,
        //    /*05*/40000,
        //    /*06*/40000,
        //    /*07*/100000,
        //    /*08*/100000,
        //    /*09*/300000,
        //    /*10*/300000,
        //});
        public static BuildingCostEnum TownPortalBuildCost { get; } = new BuildingCostEnum(8.00f, MapBuildSubType.TownTransfer, false, false, new long[]
        {
            /*01*/40000,
            /*02*/40000,
            /*03*/140000,
            /*04*/140000,
            /*05*/320000,
            /*06*/320000,
            /*07*/1000000,
            /*08*/1000000,
            /*09*/3000000,
            /*10*/5000000,
        });
        //public static BuildingCostEnum TownStorageBuildCost { get; } = new BuildingCostEnum(10.00f, MapBuildSubType.TownStorage, false, false, new long[]
        //{
        //    /*01*/10000,
        //    /*02*/10000,
        //    /*03*/20000,
        //    /*04*/20000,
        //    /*05*/40000,
        //    /*06*/40000,
        //    /*07*/100000,
        //    /*08*/100000,
        //    /*09*/300000,
        //    /*10*/300000,
        //});
        public static BuildingCostEnum TownAuctionBuildCost { get; } = new BuildingCostEnum(4.00f, MapBuildSubType.TownAuction, false, true, new long[]
        {
            /*01*/20000,
            /*02*/20000,
            /*03*/50000,
            /*04*/50000,
            /*05*/200000,
            /*06*/200000,
            /*07*/800000,
            /*08*/800000,
            /*09*/1500000,
            /*10*/2000000,
        });
        public static BuildingCostEnum TownFactoryBuildCost { get; } = new BuildingCostEnum(6.00f, MapBuildSubType.TownFactory, false, false, new long[]
        {
            /*01*/20000,
            /*02*/20000,
            /*03*/50000,
            /*04*/50000,
            /*05*/100000,
            /*06*/100000,
            /*07*/500000,
            /*08*/500000,
            /*09*/2000000,
            /*10*/3000000,
        });
        public static BuildingCostEnum TownMarketBookBuildCost { get; } = new BuildingCostEnum(2.00f, MapBuildSubType.TownMarketBook, false, false, new long[]
        {
            /*01*/30000,
            /*02*/30000,
            /*03*/80000,
            /*04*/80000,
            /*05*/300000,
            /*06*/300000,
            /*07*/1500000,
            /*08*/1500000,
            /*09*/6000000,
            /*10*/8000000,
        });
        public static BuildingCostEnum TownMarketDressBuildCost { get; } = new BuildingCostEnum(5.00f, MapBuildSubType.TownMarketDress, false, false, new long[]
        {
            /*01*/8000,
            /*02*/8000,
            /*03*/20000,
            /*04*/20000,
            /*05*/80000,
            /*06*/80000,
            /*07*/250000,
            /*08*/250000,
            /*09*/800000,
            /*10*/1000000,
        });
        public static BuildingCostEnum TownBountyBuildCost { get; } = new BuildingCostEnum(15.00f, MapBuildSubType.TownBounty, false, false, new long[]
        {
            /*01*/4000,
            /*02*/4000,
            /*03*/10000,
            /*04*/10000,
            /*05*/20000,
            /*06*/20000,
            /*07*/50000,
            /*08*/50000,
            /*09*/100000,
            /*10*/120000,
        });

        public float BuildRate { get; private set; }
        public MapBuildSubType BuildType { get; private set; }
        public bool IsSchool { get; private set; }
        public bool IsCity { get; private set; }
        public long[] BuildCosts { get; private set; }
        public Action<BuildingCostEnum, MapBuildSubBase> BuiltAfter { get; private set; }
        private BuildingCostEnum(float rate, MapBuildSubType type, bool isSchool, bool isCity, long[] costs, Action<BuildingCostEnum, MapBuildSubBase>  after = null) : base()
        {
            BuildRate = rate;
            BuildType = type;
            IsSchool = isSchool;
            IsCity = isCity;
            BuildCosts = costs;
        }

        public bool IsMatchBuildConds(MapBuildBase build)
        {
            var school = build.TryCast<MapBuildSchool>();
            var town = build.TryCast<MapBuildTown>();
            if (IsSchool)
            {
                return school != null;
            }
            else
            {
                if (town != null)
                {
                    if ((this.BuildType == MapBuildSubType.TownStorage || this.BuildType == MapBuildSubType.TownTransfer) && town.buildTownData.isMainTown)
                        return false;
                    if (IsCity)
                    {
                        return town.buildTownData.isMainTown;
                    }
                    return true;
                }
                return false;
            }
        }

        public void Build(MapBuildBase build)
        {
            var subBuild = build.AddBuildSub(BuildType);
            MapBuildPropertyEvent.AddBuildProperty(build, -BuildCosts[build.gridData.areaBaseID - 1]);
            BuiltAfter?.Invoke(this, subBuild);
        }
    }
}
