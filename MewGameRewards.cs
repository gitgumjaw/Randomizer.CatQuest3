using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    public static class MewGameRewards
    {
        public static List<Reward> GetAll()
        {
            return new List<Reward>
            {
                // The Guiding Light
                new Reward(
                    RewardType.Equipment,
                    "b6bb6a7a321f74d3fbf158fb9e6da379"
                ),

                // North Star Bicorne
                new Reward(
                    RewardType.Equipment,
                    "9e7287eff0140485394ef479b49431aa"
                ),

                // The Guiding Wall
                new Reward(
                    RewardType.Equipment,
                    "d2955e7c2ddc742e1991c0f65cfe47ae"
                ),

                // The Guiding Blade
                new Reward(
                    RewardType.Equipment,
                    "3a3d9eb918bec4250afa91e79ab9c4d8"
                ),

                // The Guiding Star
                new Reward(
                    RewardType.Equipment,
                    "8f96b8fd5ba4e4eef88c56304c467d0f"
                ),

                // North Star Coat
                new Reward(
                    RewardType.Equipment,
                    "e80d3aaa2222c4cb8adf4e90b5294381"
                ),

                // The Guiding Claw
                new Reward(
                    RewardType.Equipment,
                    "10dc602349d724047a814545e0f23ad8"
                )
            };
        }
    }
}