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


        public static void LogRequiredQuestItems()
        {
            List<RewardSlot> slots = GetCurrentSlots();

            int tested = 0;
            int required = 0;

            foreach (RewardSlot slot in slots)
            {
                Reward reward = slot.RandomizedReward;

                if (reward == null ||
                    reward.Type != RewardType.QuestItem)
                {
                    continue;
                }

                tested++;

                LogicState state;
                HashSet<RewardSlot> collectedSlots;

                bool canStillWin = Evaluate(
                    slots,
                    out state,
                    out collectedSlots,
                    slot
                );

                if (canStillWin)
                {
                    continue;
                }

                required++;

                Plugin.Log.LogInfo(
                    "HINT IMPORTANCE | REQUIRED | " +
                    RewardNameResolver.GetName(reward) +
                    " | Location: " +
                    (slot.Location.Label ?? slot.Location.Key) +
                    " | Slot: " + slot.RewardIndex
                );
            }

            Plugin.Log.LogInfo(
                "HINT IMPORTANCE | " +
                "Quest items tested: " + tested +
                " | Required: " + required
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
            out HashSet<RewardSlot> collectedSlots,
            RewardSlot suppressedRewardSlot = null)
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


                    if (state.AddCompletedLocation(
                            slot.Location.Key))
                    {
                        madeProgress = true;
                    }

                    if (slot != suppressedRewardSlot &&
                        ApplyReward(
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
                $"LovepurrBooks:{state.LovepurrBookCount}/3 | " +
                $"InfinityKey:{state.HasInfinityKey} | " +
                $"VoodooDoll:{state.HasVoodooDoll} | " +
                $"SmithyHammer:{state.HasSmithyHammer} | " +
                $"JugOfMilk:{state.HasJugOfMilk} | " +
                $"CathulhuFang:{state.HasCathulhuFang} | " +
                $"ConcertTicket:{state.HasConcertTicket} | " +
                $"Necropawmicon:{state.HasNecropawmicon} | " +
                $"Dragonbone:{state.HasDragonbone} | " +
                $"Catfish:{state.CatfishCount}/4 | " +
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


                HashSet<RewardSlot> currentPath =
                    new HashSet<RewardSlot>();


                currentPath.Add(
                    slot
                );


                LogRequirementChain(
                    slots,
                    collectedSlots,
                    state,
                    slot.Location.LogicRequirements,
                    1,
                    currentPath
                );
            }
        }


        private static void LogRequirementChain(
            List<RewardSlot> slots,
            HashSet<RewardSlot> collectedSlots,
            LogicState state,
            LogicRequirements requirements,
            int depth,
            HashSet<RewardSlot> currentPath)
        {
            if (
                slots == null ||
                collectedSlots == null ||
                state == null ||
                requirements == null
            )
            {
                return;
            }


            if (
                requirements.RequiresShipKey &&
                !state.HasShipKey
            )
            {
                TraceBlocker(
                    slots,
                    collectedSlots,
                    state,
                    "Ship Key",
                    RewardType.QuestItem,
                    depth,
                    currentPath,
                    SpecialRewards.ShipKeyGuid
                );
            }


            if (
                requirements.RequiresTwinCastleKey &&
                !state.HasTwinCastleKey
            )
            {
                TraceBlocker(
                    slots,
                    collectedSlots,
                    state,
                    "Twin Castle Key",
                    RewardType.QuestItem,
                    depth,
                    currentPath,
                    SpecialRewards.TwinCastleKeyGuid
                );
            }


            if (
                state.TentakeyCount <
                requirements.RequiredTentakeys
            )
            {
                TraceBlocker(
                    slots,
                    collectedSlots,
                    state,
                    "Tentakey",
                    RewardType.QuestItem,
                    depth,
                    currentPath,
                    SpecialRewards.Tentakey1Guid,
                    SpecialRewards.Tentakey2Guid,
                    SpecialRewards.Tentakey3Guid
                );
            }


            if (
                state.SeekerKeyCount <
                requirements.RequiredSeekerKeys
            )
            {
                TraceBlocker(
                    slots,
                    collectedSlots,
                    state,
                    "Seeker Key",
                    RewardType.QuestItem,
                    depth,
                    currentPath,
                    SpecialRewards.SeekerKeyAntaresGuid,
                    SpecialRewards.SeekerKeyCentauriGuid,
                    SpecialRewards.SeekerKeyOrionGuid
                );
            }


            if (
                state.LovepurrBookCount <
                requirements.RequiredLovepurrBooks
            )
            {
                TraceBlocker(
                    slots,
                    collectedSlots,
                    state,
                    "Lovepurr Book",
                    RewardType.QuestItem,
                    depth,
                    currentPath,
                    SpecialRewards.LovepurrBook1Guid,
                    SpecialRewards.LovepurrBook2Guid,
                    SpecialRewards.LovepurrBook3Guid
                );
            }


            if (
                requirements.RequiresInfinityKey &&
                !state.HasInfinityKey
            )
            {
                TraceBlocker(
                    slots,
                    collectedSlots,
                    state,
                    "Infinity Key",
                    RewardType.QuestItem,
                    depth,
                    currentPath,
                    SpecialRewards.InfinityKeyGuid
                );
            }


            if (
                requirements.RequiresNorthStarEssence &&
                !state.HasNorthStarEssence
            )
            {
                TraceBlocker(
                    slots,
                    collectedSlots,
                    state,
                    "North Star Essence",
                    RewardType.QuestItem,
                    depth,
                    currentPath,
                    SpecialRewards.NorthStarEssenceGuid
                );
            }


            if (
                requirements.RequiresVoodooDoll &&
                !state.HasVoodooDoll
            )
            {
                TraceBlocker(
                    slots,
                    collectedSlots,
                    state,
                    "Voodoo Doll",
                    RewardType.QuestItem,
                    depth,
                    currentPath,
                    SpecialRewards.VoodooDollGuid
                );
            }


            if (
                requirements.RequiresSmithyHammer &&
                !state.HasSmithyHammer
            )
            {
                TraceBlocker(
                    slots,
                    collectedSlots,
                    state,
                    "Smithy Hammer",
                    RewardType.QuestItem,
                    depth,
                    currentPath,
                    SpecialRewards.SmithyHammerGuid
                );
            }


            if (
                requirements.RequiresJugOfMilk &&
                !state.HasJugOfMilk
            )
            {
                TraceBlocker(
                    slots,
                    collectedSlots,
                    state,
                    "Jug o' Milk",
                    RewardType.QuestItem,
                    depth,
                    currentPath,
                    SpecialRewards.JugOfMilkGuid
                );
            }


            if (
                requirements.RequiresCathulhuFang &&
                !state.HasCathulhuFang
            )
            {
                TraceBlocker(
                    slots,
                    collectedSlots,
                    state,
                    "Fang of Cathulhu",
                    RewardType.QuestItem,
                    depth,
                    currentPath,
                    SpecialRewards.CathulhuFangGuid
                );
            }


            if (
                requirements.RequiresConcertTicket &&
                !state.HasConcertTicket
            )
            {
                TraceBlocker(
                    slots,
                    collectedSlots,
                    state,
                    "Concert Ticket",
                    RewardType.QuestItem,
                    depth,
                    currentPath,
                    SpecialRewards.ConcertTicketGuid
                );
            }


            if (
                requirements.RequiresNecropawmicon &&
                !state.HasNecropawmicon
            )
            {
                TraceBlocker(
                    slots,
                    collectedSlots,
                    state,
                    "Necropawmicon",
                    RewardType.Equipment,
                    depth,
                    currentPath,
                    SpecialRewards.NecropawmiconGuid
                );
            }


            if (
                requirements.RequiresDragonbone &&
                !state.HasDragonbone
            )
            {
                TraceBlocker(
                    slots,
                    collectedSlots,
                    state,
                    "Dragonbone",
                    RewardType.Equipment,
                    depth,
                    currentPath,
                    SpecialRewards.DragonboneGuid
                );
            }


            if (
                state.CatfishCount <
                requirements.RequiredCatfish
            )
            {
                TraceBlocker(
                    slots,
                    collectedSlots,
                    state,
                    "Catfish",
                    RewardType.QuestItem,
                    depth,
                    currentPath,
                    SpecialRewards.SunsetCatfishGuid,
                    SpecialRewards.GhostCatfishGuid,
                    SpecialRewards.RockCatfishGuid,
                    SpecialRewards.LoverCatfishGuid
                );
            }


            foreach (
                string spellGuid
                in requirements.RequiredSpells)
            {
                if (state.HasSpell(
                        spellGuid))
                {
                    continue;
                }


                TraceSpellBlocker(
                    slots,
                    collectedSlots,
                    state,
                    spellGuid,
                    depth,
                    currentPath
                );
            }


            foreach (
                string locationKey
                in requirements.RequiredLocations)
            {
                if (state.HasCompletedLocation(
                        locationKey))
                {
                    continue;
                }


                TraceLocationBlocker(
                    slots,
                    collectedSlots,
                    state,
                    locationKey,
                    depth,
                    currentPath
                );
            }
        }


        private static void TraceSpellBlocker(
            List<RewardSlot> slots,
            HashSet<RewardSlot> collectedSlots,
            LogicState state,
            string spellGuid,
            int depth,
            HashSet<RewardSlot> currentPath)
        {
            bool foundBlocker =
                false;


            foreach (
                RewardSlot slot
                in slots)
            {
                if (
                    slot == null ||
                    collectedSlots.Contains(
                        slot)
                )
                {
                    continue;
                }


                Reward reward =
                    slot.RandomizedReward;


                if (
                    reward == null ||
                    reward.Type != RewardType.Spell ||
                    reward.Id != spellGuid
                )
                {
                    continue;
                }


                foundBlocker =
                    true;


                string spellName =
                    RewardNameResolver.GetName(
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


                if (currentPath.Contains(
                        slot))
                {
                    Plugin.Log.LogInfo(
                        "LOGIC BLOCKER CHAIN | " +
                        $"Depth:{depth} | " +
                        $"Blocker:{spellName} | " +
                        $"Location:{locationName} | " +
                        $"BlockedBy:{blockedBy} | " +
                        "Self-Lock:True"
                    );

                    continue;
                }


                Plugin.Log.LogInfo(
                    "LOGIC BLOCKER CHAIN | " +
                    $"Depth:{depth} | " +
                    $"Blocker:{spellName} | " +
                    $"Location:{locationName} | " +
                    $"BlockedBy:{blockedBy}"
                );


                currentPath.Add(
                    slot
                );


                LogRequirementChain(
                    slots,
                    collectedSlots,
                    state,
                    slot.Location.LogicRequirements,
                    depth + 1,
                    currentPath
                );


                currentPath.Remove(
                    slot
                );
            }


            if (!foundBlocker)
            {
                Reward spellReward =
                    new Reward(
                        RewardType.Spell,
                        spellGuid
                    );


                Plugin.Log.LogInfo(
                    "LOGIC BLOCKER CHAIN | " +
                    $"Depth:{depth} | " +
                    $"Blocker:{RewardNameResolver.GetName(spellReward)} | " +
                    "Location:<not found in reward slots> | " +
                    "BlockedBy:Unknown"
                );
            }
        }


        private static void TraceLocationBlocker(
            List<RewardSlot> slots,
            HashSet<RewardSlot> collectedSlots,
            LogicState state,
            string locationKey,
            int depth,
            HashSet<RewardSlot> currentPath)
        {
            RewardSlot targetSlot =
                null;


            foreach (
                RewardSlot slot
                in slots)
            {
                if (
                    slot == null ||
                    slot.Location == null ||
                    slot.Location.Key != locationKey
                )
                {
                    continue;
                }


                targetSlot =
                    slot;

                break;
            }


            if (targetSlot == null)
            {
                Plugin.Log.LogInfo(
                    "LOGIC BLOCKER CHAIN | " +
                    $"Depth:{depth} | " +
                    $"Blocker:Location:{locationKey} | " +
                    "Location:<not found in reward slots> | " +
                    "BlockedBy:Unknown"
                );

                return;
            }


            string locationName =
                targetSlot.Location.Label ??
                targetSlot.Location.Key;

            string blockedBy =
                GetUnmetRequirements(
                    targetSlot.Location.LogicRequirements,
                    state
                );


            if (currentPath.Contains(
                    targetSlot))
            {
                Plugin.Log.LogInfo(
                    "LOGIC BLOCKER CHAIN | " +
                    $"Depth:{depth} | " +
                    $"Blocker:{locationName} | " +
                    $"Location:{locationName} | " +
                    $"BlockedBy:{blockedBy} | " +
                    "Self-Lock:True"
                );

                return;
            }


            Plugin.Log.LogInfo(
                "LOGIC BLOCKER CHAIN | " +
                $"Depth:{depth} | " +
                $"Blocker:{locationName} | " +
                $"Location:{locationName} | " +
                $"BlockedBy:{blockedBy}"
            );


            currentPath.Add(
                targetSlot
            );


            LogRequirementChain(
                slots,
                collectedSlots,
                state,
                targetSlot.Location.LogicRequirements,
                depth + 1,
                currentPath
            );


            currentPath.Remove(
                targetSlot
            );
        }


        private static void TraceBlocker(
            List<RewardSlot> slots,
            HashSet<RewardSlot> collectedSlots,
            LogicState state,
            string fallbackBlockerName,
            RewardType rewardType,
            int depth,
            HashSet<RewardSlot> currentPath,
            params string[] rewardIds)
        {
            bool foundBlocker =
                false;


            foreach (
                RewardSlot slot
                in slots)
            {
                if (
                    slot == null ||
                    collectedSlots.Contains(
                        slot)
                )
                {
                    continue;
                }


                Reward reward =
                    slot.RandomizedReward;


                if (
                    reward == null ||
                    reward.Type != rewardType ||
                    !ContainsId(
                        rewardIds,
                        reward.Id
                    )
                )
                {
                    continue;
                }


                foundBlocker =
                    true;


                string blockerName =
                    GetDetailedLogicRewardName(
                        reward,
                        fallbackBlockerName
                    );

                string locationName =
                    slot.Location.Label ??
                    slot.Location.Key;

                string blockedBy =
                    GetUnmetRequirements(
                        slot.Location.LogicRequirements,
                        state
                    );


                if (currentPath.Contains(
                        slot))
                {
                    Plugin.Log.LogInfo(
                        "LOGIC BLOCKER CHAIN | " +
                        $"Depth:{depth} | " +
                        $"Blocker:{blockerName} | " +
                        $"Location:{locationName} | " +
                        $"BlockedBy:{blockedBy} | " +
                        "Self-Lock:True"
                    );

                    continue;
                }


                Plugin.Log.LogInfo(
                    "LOGIC BLOCKER CHAIN | " +
                    $"Depth:{depth} | " +
                    $"Blocker:{blockerName} | " +
                    $"Location:{locationName} | " +
                    $"BlockedBy:{blockedBy}"
                );


                currentPath.Add(
                    slot
                );


                LogRequirementChain(
                    slots,
                    collectedSlots,
                    state,
                    slot.Location.LogicRequirements,
                    depth + 1,
                    currentPath
                );


                currentPath.Remove(
                    slot
                );
            }


            if (!foundBlocker)
            {
                Plugin.Log.LogInfo(
                    "LOGIC BLOCKER CHAIN | " +
                    $"Depth:{depth} | " +
                    $"Blocker:{fallbackBlockerName} | " +
                    "Location:<not found in reward slots> | " +
                    "BlockedBy:Unknown"
                );
            }
        }


        private static bool ContainsId(
            string[] ids,
            string id)
        {
            if (
                ids == null ||
                id == null
            )
            {
                return false;
            }


            for (
                int i = 0;
                i < ids.Length;
                i++)
            {
                if (ids[i] == id)
                {
                    return true;
                }
            }


            return false;
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


            if (IsLovepurrBook(
                    reward.Id))
            {
                return "Lovepurr Book";
            }


            if (reward.Id ==
                SpecialRewards.InfinityKeyGuid)
            {
                return "Infinity Key";
            }


            if (reward.Id ==
                SpecialRewards.VoodooDollGuid)
            {
                return "Voodoo Doll";
            }


            if (reward.Id ==
                SpecialRewards.SmithyHammerGuid)
            {
                return "Smithy Hammer";
            }


            if (reward.Id ==
                SpecialRewards.JugOfMilkGuid)
            {
                return "Jug o' Milk";
            }


            if (reward.Id ==
                SpecialRewards.CathulhuFangGuid)
            {
                return "Fang of Cathulhu";
            }


            if (reward.Id ==
                SpecialRewards.ConcertTicketGuid)
            {
                return "Meowtallika Concert Ticket";
            }


            if (reward.Id ==
                SpecialRewards.NecropawmiconGuid)
            {
                return "The Necropawmicon";
            }


            if (reward.Id ==
                SpecialRewards.DragonboneGuid)
            {
                return "The Dragonbone";
            }


            if (IsCatfish(
                    reward.Id))
            {
                return "Catfish";
            }


            return reward.Id;
        }


        private static string GetDetailedLogicRewardName(
            Reward reward,
            string fallbackName)
        {
            if (reward == null)
            {
                return fallbackName;
            }


            if (reward.Id ==
                SpecialRewards.SeekerKeyAntaresGuid)
            {
                return "Key of Antares";
            }


            if (reward.Id ==
                SpecialRewards.SeekerKeyCentauriGuid)
            {
                return "Key of Centauri";
            }


            if (reward.Id ==
                SpecialRewards.SeekerKeyOrionGuid)
            {
                return "Key of Orion";
            }


            if (reward.Id ==
                SpecialRewards.LovepurrBook1Guid)
            {
                return "Lovepurr Book One";
            }


            if (reward.Id ==
                SpecialRewards.LovepurrBook2Guid)
            {
                return "Lovepurr Book Two";
            }


            if (reward.Id ==
                SpecialRewards.LovepurrBook3Guid)
            {
                return "Lovepurr Book Three";
            }


            if (reward.Id ==
                SpecialRewards.SunsetCatfishGuid)
            {
                return "Sunset Catfish";
            }


            if (reward.Id ==
                SpecialRewards.GhostCatfishGuid)
            {
                return "Ghost Catfish";
            }


            if (reward.Id ==
                SpecialRewards.RockCatfishGuid)
            {
                return "Rock Catfish";
            }


            if (reward.Id ==
                SpecialRewards.LoverCatfishGuid)
            {
                return "Lover Catfish";
            }


            return GetLogicRewardName(
                reward
            );
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
                state.LovepurrBookCount <
                requirements.RequiredLovepurrBooks
            )
            {
                unmet.Add(
                    $"Lovepurr Books " +
                    $"{state.LovepurrBookCount}/" +
                    $"{requirements.RequiredLovepurrBooks}"
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


            if (
                requirements.RequiresVoodooDoll &&
                !state.HasVoodooDoll
            )
            {
                unmet.Add(
                    "Voodoo Doll"
                );
            }


            if (
                requirements.RequiresSmithyHammer &&
                !state.HasSmithyHammer
            )
            {
                unmet.Add(
                    "Smithy Hammer"
                );
            }


            if (
                requirements.RequiresJugOfMilk &&
                !state.HasJugOfMilk
            )
            {
                unmet.Add(
                    "Jug o' Milk"
                );
            }


            if (
                requirements.RequiresCathulhuFang &&
                !state.HasCathulhuFang
            )
            {
                unmet.Add(
                    "Fang of Cathulhu"
                );
            }


            if (
                requirements.RequiresConcertTicket &&
                !state.HasConcertTicket
            )
            {
                unmet.Add(
                    "Concert Ticket"
                );
            }


            if (
                requirements.RequiresNecropawmicon &&
                !state.HasNecropawmicon
            )
            {
                unmet.Add(
                    "Necropawmicon"
                );
            }


            if (
                requirements.RequiresDragonbone &&
                !state.HasDragonbone
            )
            {
                unmet.Add(
                    "Dragonbone"
                );
            }


            if (
                state.CatfishCount <
                requirements.RequiredCatfish
            )
            {
                unmet.Add(
                    $"Catfish " +
                    $"{state.CatfishCount}/" +
                    $"{requirements.RequiredCatfish}"
                );
            }


            foreach (
                string spellGuid
                in requirements.RequiredSpells)
            {
                if (state.HasSpell(
                        spellGuid))
                {
                    continue;
                }


                Reward reward =
                    new Reward(
                        RewardType.Spell,
                        spellGuid
                    );


                unmet.Add(
                    RewardNameResolver.GetName(
                        reward
                    )
                );
            }


            foreach (
                string locationKey
                in requirements.RequiredLocations)
            {
                if (state.HasCompletedLocation(
                        locationKey))
                {
                    continue;
                }


                RewardLocation location =
                    RewardCatalog.Get(
                        locationKey
                    );


                unmet.Add(
                    location != null &&
                    !string.IsNullOrEmpty(
                        location.Label)
                        ? location.Label
                        : locationKey
                );
            }


            if (unmet.Count == 0)
            {
                return "None";
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


            if (
                reward.Type ==
                RewardType.Spell
            )
            {
                return state.AddSpell(
                    reward.Id
                );
            }


            if (
                reward.Type ==
                RewardType.Equipment
            )
            {
                if (
                    reward.Id ==
                    SpecialRewards.NecropawmiconGuid
                )
                {
                    if (state.HasNecropawmicon)
                    {
                        return false;
                    }

                    state.HasNecropawmicon = true;

                    return true;
                }


                if (
                    reward.Id ==
                    SpecialRewards.DragonboneGuid
                )
                {
                    if (state.HasDragonbone)
                    {
                        return false;
                    }

                    state.HasDragonbone = true;

                    return true;
                }


                return false;
            }


            if (
                reward.Type !=
                RewardType.QuestItem
            )
            {
                return false;
            }

            if (reward.Id ==
               SpecialRewards.ShellPhoneGuid)
            {
                if (state.HasShellPhone)
                {
                    return false;
                }

                state.HasShellPhone = true;
                return true;
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


            if (IsLovepurrBook(
                    reward.Id))
            {
                state.LovepurrBookCount++;

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


            if (reward.Id ==
                SpecialRewards.VoodooDollGuid)
            {
                if (state.HasVoodooDoll)
                {
                    return false;
                }

                state.HasVoodooDoll = true;

                return true;
            }


            if (reward.Id ==
                SpecialRewards.SmithyHammerGuid)
            {
                if (state.HasSmithyHammer)
                {
                    return false;
                }

                state.HasSmithyHammer = true;

                return true;
            }


            if (reward.Id ==
                SpecialRewards.JugOfMilkGuid)
            {
                if (state.HasJugOfMilk)
                {
                    return false;
                }

                state.HasJugOfMilk = true;

                return true;
            }


            if (reward.Id ==
                SpecialRewards.CathulhuFangGuid)
            {
                if (state.HasCathulhuFang)
                {
                    return false;
                }

                state.HasCathulhuFang = true;

                return true;
            }


            if (reward.Id ==
                SpecialRewards.ConcertTicketGuid)
            {
                if (state.HasConcertTicket)
                {
                    return false;
                }

                state.HasConcertTicket = true;

                return true;
            }


            if (IsCatfish(
                    reward.Id))
            {
                state.CatfishCount++;

                return true;
            }


            return false;
        }


        public static bool IsLogicReward(
            Reward reward)
        {
            if (reward == null)
            {
                return false;
            }


            if (
                reward.Type ==
                RewardType.Equipment
            )
            {
                return
                    reward.Id ==
                        SpecialRewards.NecropawmiconGuid ||

                    reward.Id ==
                        SpecialRewards.DragonboneGuid;
            }


            if (
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

                IsLovepurrBook(
                    reward.Id
                ) ||

                reward.Id ==
                    SpecialRewards.InfinityKeyGuid ||

                reward.Id ==
                    SpecialRewards.NorthStarEssenceGuid ||

                reward.Id ==
                    SpecialRewards.VoodooDollGuid ||

                reward.Id ==
                    SpecialRewards.SmithyHammerGuid ||

                reward.Id ==
                    SpecialRewards.JugOfMilkGuid ||

                reward.Id ==
                    SpecialRewards.CathulhuFangGuid ||

                reward.Id ==
                    SpecialRewards.ConcertTicketGuid ||

                reward.Id ==
                    SpecialRewards.ShellPhoneGuid ||

                IsCatfish(
                    reward.Id
                );
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


        private static bool IsLovepurrBook(
            string id)
        {
            return
                id ==
                    SpecialRewards.LovepurrBook1Guid ||

                id ==
                    SpecialRewards.LovepurrBook2Guid ||

                id ==
                    SpecialRewards.LovepurrBook3Guid;
        }


        private static bool IsCatfish(
            string id)
        {
            return
                id ==
                    SpecialRewards.SunsetCatfishGuid ||

                id ==
                    SpecialRewards.GhostCatfishGuid ||

                id ==
                    SpecialRewards.RockCatfishGuid ||

                id ==
                    SpecialRewards.LoverCatfishGuid;
        }
    }
}