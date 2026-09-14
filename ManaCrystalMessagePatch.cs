using System;
using HarmonyLib;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(typeof(MessagePanel), "Show")]
    public static class ManaCrystalMessagePatch
    {
        private const string AeliusVisit1Location =
            "Interior_ZeroDimension|SQ_Aelius_ZeroDimension_01|FSM|give reward";

        private const string AeliusVisit2Location =
            "Interior_ZeroDimension|SQ_Aelius_ZeroDimension_02|FSM|give reward 2";

        private const string AeliusVisit3Location =
            "Interior_ZeroDimension|SQ_Aelius_ZeroDimension_03|FSM|give reward 2";

        private const string MeowgusLocation =
            "MainOverworld|WorldQuest_PuzzleStone_Meowgus|FSM|Reward with mana";

        private const string AeliusVisit1Message =
            "SideQuest/SQ_Aelius_ZeroDimension_01_S14_Message_1481353870";

        private const string AeliusVisit2Message =
            "SideQuest/SQ_Aelius_ZeroDimension_02_S12_Message_1745303584";

        private const string AeliusVisit3Message =
            "SideQuest/SQ_Aelius_ZeroDimension_03_S12_Message_1464440685";

        private const string MeowgusMessage =
            "SideQuest/WorldQuest_PuzzleStone_Meowgus_S2_Message_1330124423";

        public static bool AllowRandomizerMessage { get; set; }

        public static bool Prefix(
            MessageUIInfo displayData,
            Action completionCallback)
        {
            if (AllowRandomizerMessage)
            {
                return true;
            }

            if (displayData?.contentMessage == null)
            {
                return true;
            }

            string locationKey;

            switch (displayData.contentMessage.translateID)
            {
                case AeliusVisit1Message:
                    locationKey =
                        AeliusVisit1Location;
                    break;

                case AeliusVisit2Message:
                    locationKey =
                        AeliusVisit2Location;
                    break;

                case AeliusVisit3Message:
                    locationKey =
                        AeliusVisit3Location;
                    break;

                case MeowgusMessage:
                    locationKey =
                        MeowgusLocation;
                    break;

                default:
                    return true;
            }

            RewardLocation location =
                RewardCatalog.Get(
                    locationKey
                );

            if (location == null)
            {
                return true;
            }

            int rewardIndex =
                location.FindVanillaRewardIndex(
                    RewardType.ManaCrystal,
                    ""
                );

            if (rewardIndex < 0)
            {
                return true;
            }

            Reward randomizedReward =
                location.RandomizedRewards[rewardIndex];

            if (randomizedReward == null)
            {
                return true;
            }

            Plugin.Log.LogInfo(
                $"Suppressing vanilla mana crystal message | " +
                $"Location:{location.Label} | " +
                $"Randomized:{randomizedReward.Type}:" +
                $"{randomizedReward.Id}"
            );

            completionCallback?.Invoke();

            return false;
        }
    }
}