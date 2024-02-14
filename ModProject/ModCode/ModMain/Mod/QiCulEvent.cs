using MOD_JhUKQ7.Const;
using MOD_JhUKQ7.Enum;
using ModLib.Enum;
using ModLib.Mod;
using System.Linq;
using UnityEngine;

namespace MOD_JhUKQ7.Mod
{
    [Cache(ModConst.QI_CUL_EVENT_KEY)]
    public sealed class QiCulEvent : ModEvent
    {
        private const int LEAST_ANIMA = 100;

        public Vector2Int CurPoint { get; set; }
        public int CurAnima { get; set; } = 0;

        public override void OnMonthly()
        {
            DebugHelper.WriteLine($"Start: QiCulEvent: CurPoint=({CurPoint.x}, {CurPoint.y}), CurAnima={CurAnima}");

            var player = g.world.playerUnit;
            var playerData = player.data.unitData;
            var playerPoint = playerData.GetPoint();
            if (CurPoint.x == playerPoint.x && CurPoint.y == playerPoint.y)
            {
                var curMp = player.GetProperty<int>(UnitPropertyEnum.Mp);
                if (CurAnima >= LEAST_ANIMA)
                {
                    CurAnima += curMp / 10;
                    player.AddProperty(UnitPropertyEnum.Mp, -(curMp / 2));
                    playerData.pointGridData.SetAnimaValue(CurAnima);

                    UpProp();

                    UpGrade();
                }
            }
            else
            {
                CurPoint = playerData.GetPoint();
                CurAnima = playerData.pointGridData.anima;
                //playerData.pointGridData.animaMax = int.MaxValue;
            }

            DebugHelper.WriteLine($"End: QiCulEvent: CurPoint=({CurPoint.x}, {CurPoint.y}), CurAnima={CurAnima}");
        }

        public void UpProp()
        {
            var stt = AnimaIncPropEnum.GetAllEnums<AnimaIncPropEnum>().OrderByDescending(x => x.Value).FirstOrDefault(x => CurAnima > int.Parse(x.Value));
            stt?.Cal(g.world.playerUnit);
        }

        public void UpGrade()
        {
            var player = g.world.playerUnit;
            var curExp = player.GetProperty<int>(UnitPropertyEnum.Exp);
            var needExp = g.conf.roleGrade.GetNextGradeItem(player.GetProperty<int>(UnitPropertyEnum.GradeID)).exp - g.conf.roleGrade.GetGradeItemInExp(curExp).exp;
            var settings = AnimaUpGradeEnum.GetAllEnums<AnimaUpGradeEnum>().FirstOrDefault(x => x.Grade.Value.Parse<int>() == g.world.playerUnit.data.unitData.propertyData.gradeID);
            if (settings != null && curExp >= needExp && CurAnima > settings.MinAnima)
            {
                var r = CommonTool.Random(0.00f, 100.00f);
                if (r <= (((CurAnima - settings.MinAnima) / 100) * settings.RatioPer100Anima))
                {
                    if (DramaTool.OpenDrama(480010100))
                    {
                        var playerData = player.data.unitData;

                        player.SetProperty(UnitPropertyEnum.GradeID, settings.NextGrade.Value.Parse<int>());
                        player.SetProperty<int>(UnitPropertyEnum.Exp, 0);
                        var playerUnitType = EventHelper.GetEvent<UnitTypeEvent>(ModConst.UNIT_TYPE_EVENT_KEY);
                        for (int i = settings.MinAnima, j = 1; i < CurAnima; i += 100 * j++)
                        {
                            playerUnitType.AddProp(player);
                        }

                        CurAnima = 0;
                        player.SetProperty(UnitPropertyEnum.Mp, 0);
                        playerData.pointGridData.SetAnimaValue(CurAnima);
                    }
                }
            }
        }
    }
}