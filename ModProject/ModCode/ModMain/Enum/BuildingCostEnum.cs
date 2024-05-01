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
            /*01*/20000,
            /*02*/20000,
            /*03*/50000,
            /*04*/50000,
            /*05*/150000,
            /*06*/150000,
            /*07*/500000,
            /*08*/500000,
            /*09*/1000000,
            /*10*/1000000,
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
            /*01*/20000,
            /*02*/20000,
            /*03*/50000,
            /*04*/50000,
            /*05*/150000,
            /*06*/150000,
            /*07*/500000,
            /*08*/500000,
            /*09*/1000000,
            /*10*/1000000,
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
            /*01*/10000,
            /*02*/10000,
            /*03*/15000,
            /*04*/15000,
            /*05*/50000,
            /*06*/50000,
            /*07*/100000,
            /*08*/100000,
            /*09*/500000,
            /*10*/500000,
        });
        public static BuildingCostEnum TownFactoryBuildCost { get; } = new BuildingCostEnum(6.00f, MapBuildSubType.TownFactory, false, false, new long[]
        {
            /*01*/10000,
            /*02*/10000,
            /*03*/30000,
            /*04*/30000,
            /*05*/70000,
            /*06*/70000,
            /*07*/200000,
            /*08*/200000,
            /*09*/1000000,
            /*10*/1000000,
        });
        public static BuildingCostEnum TownMarketBookBuildCost { get; } = new BuildingCostEnum(2.00f, MapBuildSubType.TownMarketBook, false, false, new long[]
        {
            /*01*/20000,
            /*02*/20000,
            /*03*/50000,
            /*04*/50000,
            /*05*/200000,
            /*06*/200000,
            /*07*/1000000,
            /*08*/1000000,
            /*09*/5000000,
            /*10*/5000000,
        });
        public static BuildingCostEnum TownMarketDressBuildCost { get; } = new BuildingCostEnum(5.00f, MapBuildSubType.TownMarketDress, false, false, new long[]
        {
            /*01*/3000,
            /*02*/3000,
            /*03*/5000,
            /*04*/5000,
            /*05*/10000,
            /*06*/10000,
            /*07*/20000,
            /*08*/20000,
            /*09*/50000,
            /*10*/50000,
        });
        public static BuildingCostEnum TownBountyBuildCost { get; } = new BuildingCostEnum(15.00f, MapBuildSubType.TownBounty, false, false, new long[]
        {
            /*01*/1000,
            /*02*/1000,
            /*03*/3000,
            /*04*/3000,
            /*05*/5000,
            /*06*/5000,
            /*07*/20000,
            /*08*/20000,
            /*09*/60000,
            /*10*/60000,
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
