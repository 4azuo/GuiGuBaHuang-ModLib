using ModLib.Mod;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    [Skill("SpecialSkill_StormSword")]
    public class StormSword : ModSkill
    {
        public const int SKILL_ID = 1000010000;
        public const string NORMAL_EFX = "Effect\\Battle\\Skill\\chuanchengzhezhen1";
        public const string CRIT_EFX = "Effect\\Battle\\Skill\\chuanchengzhezhen";
        public const float RADIUS = 3.5f;

        public override void OnCast(UnitCtrlBase cunit, SkillBase skill, StepBase step, PropItemBase prop)
        {
            base.OnCast(cunit, skill, step, prop);
            var sa = skill?.TryCast<SkillAttack>();
            if (skill != null && sa.skillData.data.propsID == SKILL_ID)
            {
                ModBattleEvent.SceneBattle.effect.Create(NORMAL_EFX, Input.mousePosition, 3f, (Il2CppSystem.Action<GameObject>)((x) =>
                {
                    x.transform.rotation = new Quaternion();
                }));
                foreach (var x in cunit.FindNearCEnemys(RADIUS))
                {
                    MartialTool.HitDanagePow(new MartialTool.HitData(cunit, null, 0, 1, cunit.data.attack.baseValue), x);
                }
            }
        }
    }
}
