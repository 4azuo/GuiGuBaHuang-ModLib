using Boo.Lang;
using EGameTypeData;
using MOD_nE7UL2.Const;
using MOD_nE7UL2.Object;
using ModLib.Enum;
using ModLib.Mod;
using ModLib.Object;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.CUSTOM_SKILL_EVENT)]
    public class CustomSkillEvent : ModEvent
    {
        public static CustomSkillEvent Instance { get; set; }

        private UICover<UIBattleInfo> uiCover;

        public List<CustomSkill> CustomSkills { get; set; } = new List<CustomSkill>();

        public MartialType[] EditableMartialTypes = new MartialType[]
        {
            MartialType.SkillLeft,
            MartialType.SkillRight,
            MartialType.Step,
            MartialType.Ultimate,
        };

        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.BattleInfo.uiName)
            {
                uiCover = new UICover<UIBattleInfo>(e.ui);
                uiCover.AddPage((ui) =>
                {
                    uiCover.AddButton(uiCover.FirstCol + 5, uiCover.LastRow, ShowUICustomSkill, "Custom Skill").Size(200, 40);
                });
                uiCover.AddPage((ui) =>
                {
                    var col = uiCover.FirstCol + 2;
                    var row = uiCover.MidRow;

                    var selectEfx = 0;
                    var efxList = JsonConvert.DeserializeObject<CustomEffect[]>(File.ReadAllText(ConfHelper.GetConfFilePath(ModMain.ModObj.ModId, "_Cus_Res_Efx_Battle_Skill.json")));

                    uiCover.AddCompositeSelect(col, row++, "Skill",
                        EditableMartialTypes.Select(x => x.GetMartialTypeName()).ToArray(), selectEfx);
                    uiCover.AddCompositeSelect(col, row++, "Effect",
                        efxList.Select(x => x.Efx).ToArray(), selectEfx);
                    uiCover.AddButton(col, row++, Ok, "Done");
                    uiCover.AddButton(col + 1, row++, Cancel, "Cancel");
                });
                uiCover.Pages[0].Active();
            }
        }

        private void ShowUICustomSkill()
        {
            if (uiCover == null)
                return;
            uiCover.Pages[1].Active();
        }

        private void Ok()
        {
            if (uiCover == null)
                return;
        }

        private void Cancel()
        {
            if (uiCover == null)
                return;
            uiCover.Pages[0].Active();
        }
    }
}
