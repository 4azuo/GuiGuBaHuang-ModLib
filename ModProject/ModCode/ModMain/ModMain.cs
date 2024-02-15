using ModLib.Mod;
using System.Linq;

namespace MOD_nE7UL2
{
    public sealed class ModMain : ModMaster
    {
        public override string ModName => "MOD_nE7UL2";

        public override string ModId => "nE7UL2";

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
            //skill need more mpCost
            foreach (var item in g.conf.battleSkillValue._allConfList.ToArray().Where(x => x.key.EndsWith("_mpCost")))
            {
                item.value1 = (item.value1.Parse<float>() * 1.5f).Parse<int>().ToString();
                item.value2 = (item.value2.Parse<float>() * 1.6f).Parse<int>().ToString();
                item.value3 = (item.value3.Parse<float>() * 1.8f).Parse<int>().ToString();
                item.value4 = (item.value4.Parse<float>() * 2.0f).Parse<int>().ToString();
                item.value5 = (item.value5.Parse<float>() * 2.3f).Parse<int>().ToString();
                item.value6 = (item.value6.Parse<float>() * 2.6f).Parse<int>().ToString();
                item.value7 = (item.value7.Parse<float>() * 3.0f).Parse<int>().ToString();
                item.value8 = (item.value8.Parse<float>() * 3.5f).Parse<int>().ToString();
                item.value9 = (item.value9.Parse<float>() * 4.0f).Parse<int>().ToString();
                item.value10 = (item.value10.Parse<float>() * 5.0f).Parse<int>().ToString();
            }
            //skill need more mpCost
            foreach (var item in g.conf.battleSkillValue._allConfList.ToArray().Where(x => x.key.EndsWith("_cd")))
            {
                item.value1 = (item.value1.Parse<float>() * 0.70f).Parse<int>().ToString();
                item.value2 = (item.value2.Parse<float>() * 0.72f).Parse<int>().ToString();
                item.value3 = (item.value3.Parse<float>() * 0.74f).Parse<int>().ToString();
                item.value4 = (item.value4.Parse<float>() * 0.76f).Parse<int>().ToString();
                item.value5 = (item.value5.Parse<float>() * 0.78f).Parse<int>().ToString();
                item.value6 = (item.value6.Parse<float>() * 0.80f).Parse<int>().ToString();
                item.value7 = (item.value7.Parse<float>() * 0.82f).Parse<int>().ToString();
                item.value8 = (item.value8.Parse<float>() * 0.84f).Parse<int>().ToString();
                item.value9 = (item.value9.Parse<float>() * 0.86f).Parse<int>().ToString();
                item.value10 = (item.value10.Parse<float>() * 0.88f).Parse<int>().ToString();
            }
        }
    }
}
