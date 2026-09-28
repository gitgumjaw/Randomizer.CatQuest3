using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    public static class LogicValidator
    {
        public static bool IsCurrentLayoutValid()
        {
            List<RewardSlot> slots =
                GetCurrentSlots();


            return IsValid(
                slots
            );
        }


        public static void LogCurrentLayoutFailureDiagnostics()
        {
            List<RewardSlot> slots =
                GetCurrentSlots();

            LogicState state;

            HashSet<RewardSlot> collectedSlots;


            Evaluate(
                slots,
                out state,
                out collectedSlots
            );


            LogFailureDiagnostics(
                slots,
                collectedSlots,
                state
            );
        }


        public static bool IsValid(
            List<RewardSlot> slots)
        {
            if (slots == null)
            {
                return false;
            }


            LogicState state;

            HashSet<RewardSlot> collectedSlots;


            return Evaluate(
                slots,
                out state,
                out collectedSlots
            );
        }


        private static List<RewardSlot> GetCurrentSlots()
        {
            List<RewardSlot> slots =
                new List<RewardSlot>();


            foreach (
                RewardLocation location
                in RewardCatalog.GetAll())
            {
                for (
                    int i = 0;
                    i < location.RandomizedRewards.Count;
                    i++)
                {
                    slots.Add(
                        new RewardSlot(
                            location,
                            i,
                            location.AllowCollectiblesBySlot[i]
                        )
                    );
                }
            }


            return slots;
        }


        private static bool Evaluate(
            List<RewardSlot> slots,
            out LogicState state,
            out HashSet<RewardSlot> collectedSlots)
        {
            state =
                new LogicState();

            collectedSlots =
                new HashSet<RewardSlot>();


            if (slots == null)
            {
                return false;
            }


            bool madeProgress;


            do
            {
                madeProgress = false;


                foreach (
                    RewardSlot slot
                    in slots)
                {
                    if (slot == null)
                    {
                        continue;
                    }


                    if (collectedSlots.Contains(
                        slot))
                    {
                        continue;
                    }


                    if (!slot.Location
                        .LogicRequirements
                        .IsSatisfiedBy(state))
                    {
                        continue;
                    }


                    collectedSlots.Add(
                        slot
                    );


                    if (ApplyReward(
                        slot.RandomizedReward,
                        state))
                    {
                        madeProgress = true;
                    }
                }
            }
            while (madeProgress);


            return
                state.TentakeyCount >= 3 &&
                state.HasNorthStarEssence;
        }


        private static void LogFailureDiagnostics(
            List<RewardSlot> slots,
            HashSet<RewardSlot> collectedSlots,
            LogicState state)
        {
            Plugin.Log.LogInfo(
                "LOGIC FAILURE STATE | " +
                "Reachable progression | " +
                $"ShipKey:{state.HasShipKey} | " +
                $"TwinCastleKey:{state.HasTwinCastleKey} | " +
                $"Tentakeys:{state.TentakeyCount}/3 | " +
                $"SeekerKeys:{state.SeekerKeyCount}/3 | " +
                $"InfinityKey:{state.HasInfinityKey} | " +
                $"NorthStarEssence:{state.HasNorthStarEssence}"
            );


            foreach (
                RewardSlot slot
                in slots)
            {
                if (slot == null)
                {
                    continue;
                }


                if (collectedSlots.Contains(
                    slot))
                {
                    continue;
                }


                Reward reward =
                    slot.RandomizedReward;


                if (!IsRequiredForVictory(
                    reward))
                {
                    continue;
                }


                string rewardName =
                    GetLogicRewardName(
                        reward
                    );

                string locationName =
                    slot.Location.Label ??
                    slot.Location.Key;

                string blockedBy =
                    GetUnmetRequirements(
                        slot.Location.LogicRequirements,
                        state
                    );


                Plugin.Log.LogInfo(
                    "LOGIC FAILURE STATE | " +
                    $"Unreachable:{rewardName} | " +
                    $"Location:{locationName} | " +
                    $"BlockedBy:{blockedBy}"
                );
            }
        }


        private static bool IsRequiredForVictory(
            Reward reward)
        {
            if (
                reward == null ||
                reward.Type !=
                    RewardType.QuestItem
            )
            {
                return false;
            }


            return
                IsTentakey(
                    reward.Id
                ) ||

                reward.Id ==
                    SpecialRewards.NorthStarEssenceGuid;
        }


        private static string GetLogicRewardName(
            Reward reward)
        {
            if (reward == null)
            {
                return "Unknown";
            }


            if (IsTentakey(
                reward.Id))
            {
                return "Tentakey";
            }


            if (reward.Id ==
                SpecialRewards.NorthStarEssenceGuid)
            {
                return "North Star Essence";
            }


            if (reward.Id ==
                SpecialRewards.ShipKeyGuid)
            {
                return "Ship Key";
            }


            if (reward.Id ==
                SpecialRewards.TwinCastleKeyGuid)
            {
                return "Twin Castle Key";
            }


            if (IsSeekerKey(
                reward.Id))
            {
                return "Seeker Key";
            }


            if (reward.Id ==
                SpecialRewards.InfinityKeyGuid)
            {
                return "Infinity Key";
            }


            return reward.Id;
        }


        private static string GetUnmetRequirements(
            LogicRequirements requirements,
            LogicState state)
        {
            if (
                requirements == null ||
                state == null
            )
            {
                return "Unknown";
            }


            List<string> unmet =
                new List<string>();


            if (
                requirements.RequiresShipKey &&
                !state.HasShipKey
            )
            {
                unmet.Add(
                    "Ship Key"
                );
            }


            if (
                requirements.RequiresTwinCastleKey &&
                !state.HasTwinCastleKey
            )
            {
                unmet.Add(
                    "Twin Castle Key"
                );
            }


            if (
                state.TentakeyCount <
                requirements.RequiredTentakeys
            )
            {
                unmet.Add(
                    $"Tentakeys " +
                    $"{state.TentakeyCount}/" +
                    $"{requirements.RequiredTentakeys}"
                );
            }


            if (
                state.SeekerKeyCount <
                requirements.RequiredSeekerKeys
            )
            {
                unmet.Add(
                    $"Seeker Keys " +
                    $"{state.SeekerKeyCount}/" +
                    $"{requirements.RequiredSeekerKeys}"
                );
            }


            if (
                requirements.RequiresInfinityKey &&
                !state.HasInfinityKey
            )
            {
                unmet.Add(
                    "Infinity Key"
                );
            }


            if (
                requirements.RequiresNorthStarEssence &&
                !state.HasNorthStarEssence
            )
            {
                unmet.Add(
                    "North Star Essence"
                );
            }


            if (unmet.Count == 0)
            {
                return "Unknown";
            }


            return string.Join(
                ", ",
                unmet
            );
        }


        public static bool ApplyReward(
            Reward reward,
            LogicState state)
        {
            if (
                reward == null ||
                state == null
            )
            {
                return false;
            }


            if (reward.Type !=
                RewardType.QuestItem)
            {
                return false;
            }


            if (reward.Id ==
                SpecialRewards.ShipKeyGuid)
            {
                if (state.HasShipKey)
                {
                    return false;
                }

                state.HasShipKey = true;

                return true;
            }


            if (reward.Id ==
                SpecialRewards.TwinCastleKeyGuid)
            {
                if (state.HasTwinCastleKey)
                {
                    return false;
                }

                state.HasTwinCastleKey = true;

                return true;
            }


            if (IsTentakey(
                reward.Id))
            {
                state.TentakeyCount++;

                return true;
            }


            if (IsSeekerKey(
                reward.Id))
            {
                state.SeekerKeyCount++;

                return true;
            }


            if (reward.Id ==
                SpecialRewards.InfinityKeyGuid)
            {
                if (state.HasInfinityKey)
                {
                    return false;
                }

                state.HasInfinityKey = true;

                return true;
            }


            if (reward.Id ==
                SpecialRewards.NorthStarEssenceGuid)
            {
                if (state.HasNorthStarEssence)
                {
                    return false;
                }

                state.HasNorthStarEssence = true;

                return true;
            }


            return false;
        }


        public static bool IsLogicReward(
            Reward reward)
        {
            if (
                reward == null ||
                reward.Type !=
                    RewardType.QuestItem
            )
            {
                return false;
            }


            return
                reward.Id ==
                    SpecialRewards.ShipKeyGuid ||

                reward.Id ==
                    SpecialRewards.TwinCastleKeyGuid ||

                IsTentakey(
                    reward.Id
                ) ||

                IsSeekerKey(
                    reward.Id
                ) ||

                reward.Id ==
                    SpecialRewards.InfinityKeyGuid ||

                reward.Id ==
                    SpecialRewards.NorthStarEssenceGuid;
        }


        private static bool IsTentakey(
            string id)
        {
            return
                id ==
                    SpecialRewards.Tentakey1Guid ||

                id ==
                    SpecialRewards.Tentakey2Guid ||

                id ==
                    SpecialRewards.Tentakey3Guid;
        }


        private static bool IsSeekerKey(
            string id)
        {
            return
                id ==
                    SpecialRewards.SeekerKeyAntaresGuid ||

                id ==
                    SpecialRewards.SeekerKeyCentauriGuid ||

                id ==
                    SpecialRewards.SeekerKeyOrionGuid;
        }
    }
}