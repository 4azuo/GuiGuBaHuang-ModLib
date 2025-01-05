using EBattleTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.SLICE_LIFE_QUEST_EVENT)]
    public class SliceLifeQuestEvent : ModEvent
    {
        public static SliceLifeQuestEvent Instance { get; set; }

        public const int TASK_BASE_ID = 444443000;
        public const int LUCK_BASE_ID = 444441000;

        public class SliceLifeTaskInfo
        {
            public int TaskLevel { get; set; }
            public Func<bool> ActiveCondition { get; set; }
            public int TaskId { get { return TASK_BASE_ID + TaskLevel; } }
            public int LuckId { get { return LUCK_BASE_ID + TaskLevel; } }
        }

        public static IList<SliceLifeTaskInfo> TaskInfo { get; set; } = new List<SliceLifeTaskInfo>
        {
            new SliceLifeTaskInfo
            {
                TaskLevel = 1,
                ActiveCondition = () =>
                {
                    return GameHelper.GetGameYear() >= 2;
                },
            },
            new SliceLifeTaskInfo
            {
                TaskLevel = 2,
                ActiveCondition = () =>
                {
                    return GameHelper.GetGameYear() >= 10;
                },
            },
            new SliceLifeTaskInfo
            {
                TaskLevel = 3,
                ActiveCondition = () =>
                {
                    return GameHelper.GetGameYear() >= 30;
                },
            },
            new SliceLifeTaskInfo
            {
                TaskLevel = 4,
                ActiveCondition = () =>
                {
                    return GameHelper.GetGameYear() >= 70;
                },
            },
            new SliceLifeTaskInfo
            {
                TaskLevel = 5,
                ActiveCondition = () =>
                {
                    return GameHelper.GetGameYear() >= 150;
                },
            },
            new SliceLifeTaskInfo
            {
                TaskLevel = 6,
                ActiveCondition = () =>
                {
                    return GameHelper.GetGameYear() >= 400;
                },
            }
        };

        public int TaskLevel { get; set; } = 0;
        public int LastCount { get; set; } = 0;

        public override void OnLoadGame()
        {
            base.OnLoadGame();
            AddTask();
        }

        public override void OnMonthly()
        {
            base.OnMonthly();
            var curTask = AddTask();
            if (curTask == null)
            {
                LastCount = 0;
            }
            else
            {
                var task = curTask.TryCast<Task104>();
                task.taskData.curCount++;
                LastCount = task.taskData.curCount;
                if (task.taskData.curCount >= task.data.task104Item.requireNumber)
                {
                    TaskLevel++;
                    LastCount = 0;
                    AddLuck();
                    RemoveTask();
                }
            }
        }

        public override void OnBattleEndOnce(BattleEnd e)
        {
            base.OnBattleEndOnce(e);
            var curTask = AddTask();
            if (curTask == null)
            {
                LastCount = 0;
            }
            else
            {
                curTask.taskData.curCount = 0;
                LastCount = curTask.taskData.curCount;
            }
        }

        private void RemoveTask()
        {
            foreach (var tInfo in TaskInfo)
            {
                var task = g.world.playerUnit.GetTask(tInfo.TaskId).ToArray().FirstOrDefault();
                if (task != null)
                    g.world.playerUnit.DelTask(task);
            }
        }

        private TaskBase AddTask()
        {
            var taskInfo = TaskInfo.FirstOrDefault(x => x.TaskLevel == TaskLevel + 1);
            if (taskInfo == null)
            {
                return null;
            }
            var task = g.world.playerUnit.GetTask(taskInfo.TaskId).ToArray().FirstOrDefault();
            if (task == null & taskInfo.ActiveCondition())
            {
                var rs = g.world.playerUnit.CreateTask(new DataUnit.TaskData
                {
                    id = taskInfo.TaskId,
                    soleID = CommonTool.SoleID(),
                    state = TaskState.Unfinished,
                    curCount = LastCount,
                });
                return rs;
            }
            return task;
        }

        private void RemoveLuck()
        {
            foreach (var tInfo in TaskInfo)
            {
                g.world.playerUnit.DelLuck(tInfo.LuckId);
            }
        }

        private void AddLuck()
        {
            RemoveLuck();
            g.world.playerUnit.AddLuck(TaskInfo[TaskLevel - 1].LuckId);
        }
    }
}
