using ModLib.Mod;

namespace MOD_JhUKQ7
{
    public sealed class ModMain : ModMaster
    {
        public override string ModName => "MOD_JhUKQ7";

        public override string ModId => "JhUKQ7";

        protected override void OnInitConf()
        {
            base.OnInitConf();
            //skillmastery need more exp
            foreach (var item in g.conf.battleSkillMastery._allConfList)
            {
                item.grade1 = (int)(item.grade1 * 1.5f);
                item.grade2 = (int)(item.grade2 * 1.6f);
                item.grade3 = (int)(item.grade3 * 1.8f);
                item.grade4 = (int)(item.grade4 * 2.0f);
                item.grade5 = (int)(item.grade5 * 2.3f);
                item.grade6 = (int)(item.grade6 * 2.6f);
                item.grade7 = (int)(item.grade7 * 3.0f);
                item.grade8 = (int)(item.grade8 * 3.5f);
                item.grade9 = (int)(item.grade9 * 4.0f);
                item.grade10 = (int)(item.grade10 * 5.0f);
            }
        }
    }
}
