using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    public static class BagOnlyRewards
    {
        public static List<Reward> GetAll()
        {
            return new List<Reward>
            {
                // Bomb Satchel
                new Reward(
                    RewardType.Equipment,
                    "2a9ad2fe7abf0124a8ca3d72e91fe6a4"
                ),

                // Electric Circuit
                new Reward(
                    RewardType.Equipment,
                    "b39ec733282179049a39f0ce28245c73"
                ),

                // Flame Crystal
                new Reward(
                    RewardType.Equipment,
                    "4af61a79efc84e94385da5cda75eef83"
                ),

                // Ice Crystal
                new Reward(
                    RewardType.Equipment,
                    "e08d84b88c3622a439496c2c4cacae0f"
                ),

                // Weird Mushroom
                new Reward(
                    RewardType.Equipment,
                    "5a2435316df0c480ab7b6eb569761ec4"
                ),

                // Bird Poop
                new Reward(
                    RewardType.Equipment,
                    "321ebd701eff1264eb7f7296d033831c"
                )
            };
        }
    }
}