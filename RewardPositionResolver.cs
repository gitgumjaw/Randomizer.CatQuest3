using UnityEngine;

namespace Randomizer.CatQuest3
{
    public static class RewardPositionResolver
    {
        public static Vector3 PlayerOrFallback(
            Vector3 fallbackPosition)
        {
            foreach (
                GameEntity player
                in Contexts.sharedInstance.game
                    .GetGroup(
                        GameMatcher.PlayerId
                    ))
            {
                if (
                    player != null &&
                    player.hasTransform &&
                    player.transform.value != null
                )
                {
                    return player.transform
                        .value.position;
                }
            }

            return fallbackPosition;
        }
    }
}