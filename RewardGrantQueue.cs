using Gentlebros;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    public static class RewardGrantQueue
    {
        private const float RewardDelaySeconds =
            0.25f;

        private class QueuedReward
        {
            public int SlotIndex;
            public Reward Reward;
            public int VanillaAwardLevel;
            public Vector3 Position;
            public Action CompletionCallback;
        }

        private class LocationQueue
        {
            public List<QueuedReward> Rewards =
                new List<QueuedReward>();

            public bool IsProcessing;
        }

        private static readonly Dictionary<string, LocationQueue>
            queues =
                new Dictionary<string, LocationQueue>();


        public static void Enqueue(
            RewardLocation location,
            int slotIndex,
            Reward reward,
            int vanillaAwardLevel,
            Vector3 position,
            Action completionCallback = null)
        {
            if (location == null ||
                reward == null)
            {
                completionCallback?.Invoke();
                return;
            }

            if (!queues.TryGetValue(
                    location.Key,
                    out LocationQueue queue))
            {
                queue =
                    new LocationQueue();

                queues[location.Key] =
                    queue;
            }

            queue.Rewards.Add(
                new QueuedReward
                {
                    SlotIndex = slotIndex,
                    Reward = reward,
                    VanillaAwardLevel =
                        vanillaAwardLevel,
                    Position = position,
                    CompletionCallback =
                        completionCallback
                }
            );

            if (queue.IsProcessing)
            {
                return;
            }

            queue.IsProcessing = true;

            SingletonMonoBehaviour<SaveGameManager>
                .Instance
                .StartCoroutine(
                    ProcessQueue(
                        location.Key,
                        queue
                    )
                );
        }

        private static IEnumerator ProcessQueue(
            string locationKey,
            LocationQueue queue)
        {
            // Give every PlayMaker action in this
            // location one frame to submit its rewards.
            yield return null;

            while (true)
            {
                if (queue.Rewards.Count == 0)
                {
                    // Give any callbacks/FSM actions a chance
                    // to enqueue another reward.
                    yield return null;

                    if (queue.Rewards.Count == 0)
                    {
                        break;
                    }
                }

                queue.Rewards.Sort(
                    (a, b) =>
                        a.SlotIndex.CompareTo(
                            b.SlotIndex
                        )
                );

                QueuedReward queued =
                    queue.Rewards[0];

                queue.Rewards.RemoveAt(0);

                bool finished =
                    false;

                RewardGranter.Grant(
                    queued.Reward,
                    queued.VanillaAwardLevel,
                    queued.Position,
                    delegate
                    {
                        queued.CompletionCallback
                            ?.Invoke();

                        finished = true;
                    }
                );

                yield return new WaitUntil(
                    () => finished
                );

                // Award callbacks can occur before the UI
                // panel has fully completed its outro.
                //
                // Give the previous reward a small amount
                // of real time to finish cleaning up before
                // displaying the next reward.
                yield return new WaitForSecondsRealtime(
                    RewardDelaySeconds
                );
            }

            queues.Remove(
                locationKey
            );
        }
    }
}