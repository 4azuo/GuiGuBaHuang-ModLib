using MOD_nE7UL2.Const;
using ModLib.Mod;
using ModLib.Object;
using System;
using System.Collections.Generic;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.SLICE_LIFE_QUEST_EVENT)]
    public class SliceLifeQuestEvent : ModEvent
    {
        public class SliceLifeQuestInfo
        {
            public int QuestLevel { get; set; }
            public Func<bool> ActiveCondition { get; set; }
            public int LuckId { get; set; }
            public int FinishCondition_MonthCount { get; set; }
        }

        public static IList<SliceLifeQuestInfo> QuestInfo { get; set; } = new List<SliceLifeQuestInfo>
        {
            new SliceLifeQuestInfo
            {
                QuestLevel = 1,
                ActiveCondition = () =>
                {
                    return GameHelper.GetGameYear() >= 5;
                },
                LuckId = 0,
                FinishCondition_MonthCount = 36,
            },
            new SliceLifeQuestInfo
            {
                QuestLevel = 2,
                ActiveCondition = () =>
                {
                    return GameHelper.GetGameYear() >= 20;
                },
                LuckId = 0,
                FinishCondition_MonthCount = 60,
            },
            new SliceLifeQuestInfo
            {
                QuestLevel = 3,
                ActiveCondition = () =>
                {
                    return GameHelper.GetGameYear() >= 35;
                },
                LuckId = 0,
                FinishCondition_MonthCount = 96,
            },
            new SliceLifeQuestInfo
            {
                QuestLevel = 4,
                ActiveCondition = () =>
                {
                    return GameHelper.GetGameYear() >= 60;
                },
                LuckId = 0,
                FinishCondition_MonthCount = 144,
            },
            new SliceLifeQuestInfo
            {
                QuestLevel = 5,
                ActiveCondition = () =>
                {
                    return GameHelper.GetGameYear() >= 100;
                },
                LuckId = 0,
                FinishCondition_MonthCount = 240,
            }
        };

        public int QuestLevel { get; set; } = 0;
        public int MonthCount { get; set; } = 0;
        public long MoneyOut { get; set; } = 0;
        public long MoneyIn { get; set; } = 0;

        public override void OnYearly()
        {
            base.OnYearly();


        }
    }
}
