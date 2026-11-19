using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MOD_nE7UL2.Mod;

namespace UnitTests.Taoist
{
    /// <summary>
    /// Icons of the texts: every sprite name used ("[icon:NAME]" in the localTexts, I("NAME") / icon arrays in the code)
    /// is a sprite of the game's TextMeshPro sprite asset; an unknown name shows the raw tag in the game.
    /// </summary>
    [TestClass]
    public class IconTests
    {
        /// <summary>Sprites of the game's TextMeshPro sprite asset (read from resources.assets, game 2026-11).</summary>
        public static readonly HashSet<string> SPRITES = new HashSet<string>(
            ("chixushijian citiaoqianzhui_2 dahuo dandaoshuliang dandaosudu daodian daofa daoli diejiacengshu fangyu fanwei " +
             "fenglinggen fengshui gailv gongji gongsu gongsuD huafu hudun huifutili huixin huixinbeilv hunli huolinggen huxin " +
             "huxinbeilv jianfa jiankang jianyidu jiaoli jineng jinenglengque jingli jituijuli jituiqiangdu kongbai kuangcai " +
             "leilinggen liandan lianqi lingli momian mubiaogeshu mulinggen naijiu nianli qiangfa qianyin quanfa shanbiD " +
             "shanghaijianmian shanghaiweili shengwang shenmizuzhi1001 shenmizuzhi1002 shifafangshi shouming shuilinggen tili " +
             "tulinggen wumian wuxing xiaohuo xingyun xinqing yaocai yisu zhandouxiaoguo zhangfa zhaohuanwu zhifa zhijieshanghai " +
             "zm_andingdu zm_fanrongdu zm_kuangshi zm_lingdi zm_lingshi zm_mingyu zm_renshu zm_yaocai zm_zhongchengdu").Split(' '));

        private static string ModConf => TestSetup.Repo("3161035078", "ModProject", "ModConf");
        private static string ModCode => TestSetup.Repo("3161035078", "ModProject", "ModCode", "ModMain");

        [TestMethod, Description("Every [icon:NAME] of the Taoist localTexts (all languages) is a sprite of the game")]
        public void LocalTextIcons_Exist()
        {
            var bad = new List<string>();
            var n = 0;
            foreach (var f in Directory.GetFiles(ModConf, "*localText.json", SearchOption.AllDirectories))
                foreach (Match m in Regex.Matches(File.ReadAllText(f), @"\[icon:([A-Za-z0-9_]+)\]"))
                {
                    n++;
                    if (!SPRITES.Contains(m.Groups[1].Value)) bad.Add($"{Path.GetFileName(Path.GetDirectoryName(f))}/{Path.GetFileName(f)}: {m.Groups[1].Value}");
                }
            Check.None(bad.Distinct().ToList(), "unknown sprites in localTexts", n);
        }

        [TestMethod, Description("Every sprite of the code (I(\"NAME\"), [icon:NAME], the icon arrays) is a sprite of the game")]
        public void CodeIcons_Exist()
        {
            var names = new List<string>();
            foreach (var f in Directory.GetFiles(Path.Combine(ModCode, "Mod"), "*.cs"))
            {
                var s = File.ReadAllText(f);
                names.AddRange(Regex.Matches(s, @"\bI\(""([A-Za-z0-9_]+)""\)").Cast<Match>().Select(m => m.Groups[1].Value));
                names.AddRange(Regex.Matches(s, @"\[icon:([A-Za-z0-9_]+)\]").Cast<Match>().Select(m => m.Groups[1].Value));
            }
            names.AddRange(DamageInfo.BASIS_ICONS);
            names.AddRange(SMScoreEvent.TIER_ICONS);
            foreach (var key in new[] { "log_tax", "log_loan", "log_family", "log_story", "log_wanted", "log_score", "x" })
                names.Add(TaoistLogEvent.Icon(key));
            Check.None(names.Where(x => x != "NAME" && !SPRITES.Contains(x)).Distinct().ToList(), "unknown sprites in the code", names.Count);
        }
    }
}
