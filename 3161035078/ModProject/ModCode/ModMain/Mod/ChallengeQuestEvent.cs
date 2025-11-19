using EBattleTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.CHALLENGE_QUEST_EVENT)]
    public class ChallengeQuestEvent : ModEvent
    {
        public static ChallengeQuestEvent Instance { get; set; }

        public const int TASK_BASE_ID = 444444000;
        public const int LUCK_BASE_ID = 444442000;

        public class ChallengeTaskInfo
        {
            public int TaskLevel { get; set; }
            public Func<bool> ActiveCondition { get; set; }
            public int RequiredWins { get; set; }
            public int MinLevelDifference { get; set; }
            public int TaskId { get { return TASK_BASE_ID + TaskLevel; } }
            public int LuckId { get { return LUCK_BASE_ID + TaskLevel; } }
        }

        public static IList<ChallengeTaskInfo> TaskInfo { get; set; } = new List<ChallengeTaskInfo>
        {
            new ChallengeTaskInfo
            {
                TaskLevel = 1,
                RequiredWins = 1,
                MinLevelDifference = 1,
                ActiveCondition = () => true,
            },
            new ChallengeTaskInfo
            {
                TaskLevel = 2,
                RequiredWins = 2,
                MinLevelDifference = 1,
                ActiveCondition = () => true,
            },
            new ChallengeTaskInfo
            {
                TaskLevel = 3,
                RequiredWins = 2,
                MinLevelDifference = 2,
                ActiveCondition = () => true,
            },
            new ChallengeTaskInfo
            {
                TaskLevel = 4,
                RequiredWins = 4,
                MinLevelDifference = 2,
                ActiveCondition = () => true,
            },
            new ChallengeTaskInfo
            {
                TaskLevel = 5,
                RequiredWins = 3,
                MinLevelDifference = 3,
                ActiveCondition = () => true,
            },
            new ChallengeTaskInfo
            {
                TaskLevel = 6,
                RequiredWins = 9,
                MinLevelDifference = 3,
                ActiveCondition = () => true,
            },
            new ChallengeTaskInfo
            {
                TaskLevel = 7,
                RequiredWins = 5,
                MinLevelDifference = 4,
                ActiveCondition = () => true,
            },
            new ChallengeTaskInfo
            {
                TaskLevel = 8,
                RequiredWins = 20,
                MinLevelDifference = 4,
                ActiveCondition = () => true,
            },
            new ChallengeTaskInfo
            {
                TaskLevel = 9,
                RequiredWins = 3,
                MinLevelDifference = 5,
                ActiveCondition = () => true,
            },
            new ChallengeTaskInfo
            {
                TaskLevel = 10,
                RequiredWins = 3,
                MinLevelDifference = 6,
                ActiveCondition = () => true,
            }
        };

        public int TaskLevel { get; set; } = 0;
        public int CurrentWins { get; set; } = 0;
        public List<string> DefeatedOpponents { get; set; } = new List<string>();

        public override void OnLoadGame()
        {
            base.OnLoadGame();
            AddTask();
        }

        public override void OnMonthly()
        {
            base.OnMonthly();
            AddTask();
        }

        public override void OnBattleUnitDie(UnitDie e)
        {
            base.OnBattleUnitDie(e);

            var dieUnit = e?.unit;
            var dieUnitWUnit = dieUnit?.GetWorldUnit();
            if (dieUnit == null || dieUnitWUnit == null)
                return;

            var killer = e?.hitData?.attackUnit;
            var killerWUnit = killer?.GetWorldUnit();

            // Chỉ tính khi player giết địch
            if (killerWUnit == null || !killerWUnit.IsPlayer())
                return;

            var currentTaskInfo = TaskInfo.FirstOrDefault(x => x.TaskLevel == TaskLevel + 1);
            if (currentTaskInfo == null)
                return;

            // Kiểm tra level difference
            var playerLevel = g.world.playerUnit.GetGradeLvl();
            var opponentLevel = dieUnitWUnit.GetGradeLvl();
            var levelDifference = opponentLevel - playerLevel;

            if (levelDifference >= currentTaskInfo.MinLevelDifference)
            {
                var opponentId = dieUnitWUnit.GetUnitId();
                
                // Tránh đánh cùng một đối thủ nhiều lần
                if (!DefeatedOpponents.Contains(opponentId))
                {
                    CurrentWins++;
                    DefeatedOpponents.Add(opponentId);

                    var currentTask = GetCurrentTask();
                    if (currentTask != null)
                    {
                        var task = currentTask.TryCast<Task104>();
                        if (task != null)
                        {
                            task.taskData.curCount = CurrentWins;
                        }
                    }

                    // Kiểm tra hoàn thành nhiệm vụ
                    if (CurrentWins >= currentTaskInfo.RequiredWins)
                    {
                        CompleteTask();
                    }
                }
            }
        }

        private void CompleteTask()
        {
            TaskLevel++;
            CurrentWins = 0;
            DefeatedOpponents.Clear();
            
            AddLuck();
            RemoveTask();
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

        private TaskBase GetCurrentTask()
        {
            var taskInfo = TaskInfo.FirstOrDefault(x => x.TaskLevel == TaskLevel + 1);
            if (taskInfo == null)
                return null;

            return g.world.playerUnit.GetTask(taskInfo.TaskId).ToArray().FirstOrDefault();
        }

        private TaskBase AddTask()
        {
            var taskInfo = TaskInfo.FirstOrDefault(x => x.TaskLevel == TaskLevel + 1);
            if (taskInfo == null)
            {
                return null;
            }

            var existingTask = g.world.playerUnit.GetTask(taskInfo.TaskId).ToArray().FirstOrDefault();
            if (existingTask == null && taskInfo.ActiveCondition())
            {
                var rs = g.world.playerUnit.CreateTask(new DataUnit.TaskData
                {
                    id = taskInfo.TaskId,
                    soleID = CommonTool.SoleID(),
                    state = TaskState.Unfinished,
                    curCount = CurrentWins,
                });
                return rs;
            }
            return existingTask;
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
            if (TaskLevel > 0 && TaskLevel <= TaskInfo.Count)
            {
                g.world.playerUnit.AddLuck(TaskInfo[TaskLevel - 1].LuckId);
            }
        }

        // Reset progress khi bắt đầu game mới
        public override void OnLoadNewGame()
        {
            base.OnLoadNewGame();
            TaskLevel = 0;
            CurrentWins = 0;
            DefeatedOpponents.Clear();
        }
    }
}