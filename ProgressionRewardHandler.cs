using System;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    public static class ProgressionRewardHandler
    {
        public static void HandleQuestItem(
            QuestItem questItem,
            Vector3 position,
            Action callback)
        {
            // QuestItem progression preservation is
            // intentionally paused for now.

            callback?.Invoke();
        }
    }
}