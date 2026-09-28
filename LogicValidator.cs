using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    public static class LogicValidator
    {
        public static bool IsCurrentLayoutValid()
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


            return IsValid(
                slots
            );
        }


        public static bool IsValid(
            List<RewardSlot> slots)
        {
            if (slots == null)
            {
                return false;
            }


            LogicState state =
                new LogicState();

            HashSet<RewardSlot> collectedSlots =
                new HashSet<RewardSlot>();


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


            // A seed is valid if the player can reach
            // Zero Dimension.
            //
            // Ship Key, Twin Castle Key, Seeker Keys,
            // and Infinity Key only matter when they
            // are necessary links in that path.
            return
                state.TentakeyCount >= 3 &&
                state.HasNorthStarEssence;
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