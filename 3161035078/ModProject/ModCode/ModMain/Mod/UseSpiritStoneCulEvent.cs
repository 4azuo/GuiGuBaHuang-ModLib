using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Attributes;
using ModLib.Enum;
using ModLib.Mod;
using ModLib.Object;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.USE_SPIRIT_STONE_CUL_EVENT)]
    public class UseSpiritStoneCulEvent : ModEvent
    {
        public static UseSpiritStoneCulEvent Instance { get; set; }

        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.FateFeature.uiName)
            {
                var player = g.world.playerUnit;
                if (!player.IsFullExp())
                {
                    var ui = new UICover<UIFateFeature>(e.ui);
                    {
                        ui.AddToolTipButton(ui.MidCol - 4, ui.MidRow + 4, GameTool.LS("other500020073"));
                        ui.AddButton(ui.MidCol, ui.MidRow + 4, () =>
                        {
                            var playerGradeLvl = player.GetGradeLvl();
                            var requiringMoney = 10000 * playerGradeLvl;
                            if (player.GetUnitMoney() > requiringMoney)
                            {
                                var insightRate = player.GetDynProperty(UnitDynPropertyEnum.Talent).value / 100f;
                                var basisRate = player.GetAllBasisesSum() / 100f / WUnitHelper.ALL_BASISES.Length;
                                var exp = (100 * playerGradeLvl * insightRate * basisRate * CommonTool.Random(0.8f, 1.2f)).Parse<int>();
                                player.AddUnitMoney(-requiringMoney);
                                player.AddExp(exp);
                                ui.UI.UpdateUI();
                                g.ui.MsgBox(string.Empty, string.Format(GameTool.LS("other500020070"), requiringMoney, exp));
                            }
                            else
                            {
                                g.ui.MsgBox(GameTool.LS("other500020022"), string.Format(GameTool.LS("other500020069"), requiringMoney));
                            }
                        }, GameTool.LS("other500020068")).Size(200f, 40f);
                    }
                }
            }
        }
    }
}
