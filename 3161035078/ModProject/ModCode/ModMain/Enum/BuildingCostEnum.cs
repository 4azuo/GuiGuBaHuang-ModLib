using MOD_nE7UL2.Mod;
using ModLib.Object;
using System;

namespace MOD_nE7UL2.Enum
{
    public class BuildingCostEnum : EnumObject
    {
        public static BuildingCostEnum SchoolTrainingHallBuildCost { get; } = new BuildingCostEnum(GameTool.LS("school500020000desc"), -1f, MapBuildSubType.SchoolTrainingHall, true, false, null);
        public static BuildingCostEnum SchoolPortalBuildCost { get; } = new BuildingCostEnum(GameTool.LS("school500020001desc"), 4.00f, MapBuildSubType.SchoolTransfer, true, false, new long[]
        {
            /*01*/15000,
            /*02*/30000,
            /*03*/100000,
            /*04*/100000,
            /*05*/320000,
            /*06*/320000,
            /*07*/1000000,
            /*08*/1000000,
            /*09*/3000000,
            /*10*/10000000,
        });
        //public static BuildingCostEnum SchoolStorageBuildCost { get; } = new BuildingCostEnum("School Storage", 5.00f, MapBuildSubType.SchoolStorage, true, false, new long[]
        //{
        //    /*01*/0,
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
        public static BuildingCostEnum TownPortalBuildCost { get; } = new BuildingCostEnum(GameTool.LS("town500030000desc"), 4.00f, MapBuildSubType.TownTransfer, false, false, new long[]
        {
            /*01*/20000,
            /*02*/40000,
            /*03*/150000,
            /*04*/150000,
            /*05*/400000,
            /*06*/400000,
            /*07*/2000000,
            /*08*/2000000,
            /*09*/8000000,
            /*10*/25000000,
        });
        //public static BuildingCostEnum TownStorageBuildCost { get; } = new BuildingCostEnum("Town Storage", 5.00f, MapBuildSubType.TownStorage, false, false, new long[]
        //{
        //    /*01*/0,
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
        public static BuildingCostEnum TownAuctionBuildCost { get; } = new BuildingCostEnum(GameTool.LS("town500030001desc"), 2.00f, MapBuildSubType.TownAuction, false, true, new long[]
        {
            /*01*/10000,
            /*02*/20000,
            /*03*/60000,
            /*04*/60000,
            /*05*/250000,
            /*06*/250000,
            /*07*/1000000,
            /*08*/1000000,
            /*09*/3000000,
            /*10*/10000000,
        });
        public static BuildingCostEnum TownFactoryBuildCost { get; } = new BuildingCostEnum(GameTool.LS("town500030002desc"), 3.00f, MapBuildSubType.TownFactory, false, false, new long[]
        {
            /*01*/10000,
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
        public static BuildingCostEnum TownMarketBookBuildCost { get; } = new BuildingCostEnum(GameTool.LS("town500030003desc"), 1.00f, MapBuildSubType.TownMarketBook, false, false, new long[]
        {
            /*01*/0,
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
        public static BuildingCostEnum TownMarketDressBuildCost { get; } = new BuildingCostEnum(GameTool.LS("town500030004desc"), 3.00f, MapBuildSubType.TownMarketDress, false, false, new long[]
        {
            /*01*/4000,
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
        public static BuildingCostEnum TownBountyBuildCost { get; } = new BuildingCostEnum(GameTool.LS("town500030005desc"), 8.00f, MapBuildSubType.TownBounty, false, false, new long[]
        {
            /*01*/0,
            /*02*/4000,
            /*03*/12000,
            /*04*/12000,
            /*05*/25000,
            /*06*/25000,
            /*07*/70000,
            /*08*/70000,
            /*09*/150000,
            /*10*/200000,
        });

        public string BuildingName { get; private set; }
        public float BuildRate { get; private set; }
        public MapBuildSubType BuildType { get; private set; }
        public bool IsSchool { get; private set; }
        public bool IsCity { get; private set; }
        public long[] BuildCosts { get; private set; }
        public Action<BuildingCostEnum, MapBuildSubBase> BuiltAfter { get; private set; }
        private BuildingCostEnum(string name, float rate, MapBuildSubType type, bool isSchool, bool isCity, long[] costs, Action<BuildingCostEnum, MapBuildSubBase> after = null) : base()
        {
            BuildingName = name;
            BuildRate = rate;
            BuildType = type;
            IsSchool = isSchool;
            IsCity = isCity;
            BuildCosts = costs;
            BuiltAfter = after;
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
