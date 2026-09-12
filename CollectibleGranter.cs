using DG.Tweening;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    public static class CollectibleGranter
    {
        public static void Spawn(
            Reward reward,
            Vector3 position,
            float spread)
        {
            if (reward == null ||
                reward.Type != RewardType.Collectible ||
                reward.CollectibleData == null)
            {
                Plugin.Log.LogError(
                    "CollectibleGranter received an invalid reward."
                );

                return;
            }

            CollectibleRewardData data =
                reward.CollectibleData;

            LootDropConfig lootConfig =
                AddressableSingletonScriptableObject<GameConfig>
                    .Instance
                    .lootDropConfig;

            int quantity =
                data.Quantity;

            if (data.RandomQuantity)
            {
                quantity =
                    Random.Range(
                        data.QuantityMin,
                        data.QuantityMax + 1
                    );
            }

            quantity *=
                data.QuantityMultiplier;

            for (int i = 0; i < quantity; i++)
            {
                int value =
                    data.Value;

                if (data.RandomValue)
                {
                    value =
                        Random.Range(
                            data.ValueMin,
                            data.ValueMax + 1
                        );
                }

                if (!data.IgnoreCollectibleMultiplier)
                {
                    value =
                        Mathf.CeilToInt(
                            value *
                            data.ValueMultiplier
                        );
                }

                value =
                    Mathf.CeilToInt(
                        (float)value /
                        quantity
                    );

                GameObject collectible =
                    CollectibleFactory.CreateCollectible(
                        data.PrefabName,
                        position,
                        value
                    );

                Collider collider =
                    collectible.GetComponent<Collider>();

                collider.enabled = false;

                Vector2 direction =
                    Random.insideUnitCircle *
                    spread;

                Vector3 firstBounce =
                    position +
                    new Vector3(
                        direction.x *
                        lootConfig.tweenFirstBounceOffset,
                        0f,
                        direction.y *
                        lootConfig.tweenFirstBounceOffset
                    );

                Vector3 finalPosition =
                    position +
                    new Vector3(
                        direction.x,
                        0f,
                        direction.y
                    );

                float duration =
                    Random.Range(
                        lootConfig.baseTweenDurationMin,
                        lootConfig.baseTweenDurationMax
                    );

                Sequence sequence =
                    DOTween.Sequence();

                sequence.Append(
                    collectible.transform.DOJump(
                        firstBounce,
                        lootConfig.tweenFirstBounceHeight,
                        1,
                        duration *
                        lootConfig.tweenFirstBounceDurationWeight
                    )
                );

                sequence.Append(
                    collectible.transform.DOJump(
                        finalPosition,
                        lootConfig.tweenSecondBounceHeight,
                        1,
                        duration *
                        (1f -
                         lootConfig.tweenFirstBounceDurationWeight)
                    )
                );

                sequence.SetEase(
                    lootConfig.dropTweenEase
                );

                sequence.Play()
                    .OnComplete(
                        delegate
                        {
                            collider.enabled = true;
                        }
                    );

                AddLifetime(
                    collectible,
                    lootConfig
                );
            }
        }

        private static void AddLifetime(
            GameObject collectible,
            LootDropConfig lootConfig)
        {
            GameEntity existing =
                Contexts.sharedInstance.game
                    .GetEntityWithLifetimeTarget(
                        collectible
                    );

            if (existing != null)
            {
                existing.isDestroyed = true;
                existing.Destroy();
            }

            GameEntity lifetime =
                Contexts.sharedInstance.game
                    .CreateEntity();

            lifetime.AddLifetime(
                lootConfig.lootCollectibleLifetime,
                0f
            );

            lifetime.AddLifetimeTarget(
                collectible,
                0f,
                lootConfig
                    .lootLifetimeExpireFlickerInitialInterval
            );

            lifetime.AddTimer(0f);
        }
    }
}