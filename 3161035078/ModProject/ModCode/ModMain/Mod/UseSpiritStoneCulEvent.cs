using EGameTypeData;
using MOD_nE7UL2.Const;
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
                        ui.AddButton(ui.MidCol, ui.MidRow + 4, () =>
                        {
                            var playerGradeLvl = player.GetGradeLvl();
                            var requiringMoney = 10000 * playerGradeLvl;
                            if (player.GetUnitMoney() > requiringMoney)
                            {
                                var exp = (100 * playerGradeLvl * CommonTool.Random(0.8f, 1.2f)).Parse<int>();
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
