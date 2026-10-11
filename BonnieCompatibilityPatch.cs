using HarmonyLib;
using HutongGames.PlayMaker;

namespace Randomizer.CatQuest3
{
    public static class BonnieCompatibilityPatch
    {
        private const string
            BonnieObjectName =
                "Interactable_Mage";

        private const string
            FurryShieldCheckState =
                "Check if have Spell";

        private const string
            NecropawmiconCheckState =
                "Check if Player has got the Necropawmicon";

        private const string
            NecropawmiconGuid =
                "11f1df91d1233e347b28fd752ea56bef";


        private static bool IsBonnieState(
            FsmStateAction action,
            string stateName)
        {
            return
                action != null &&
                action.Fsm != null &&
                action.Fsm.GameObject != null &&
                action.Fsm.GameObject.name ==
                    BonnieObjectName &&
                action.State != null &&
                action.State.Name ==
                    stateName;
        }


        [HarmonyPatch(
            typeof(CheckIfSpellObtained),
            "OnEnter"
        )]
        private static class
            FurryShieldCheckPatch
        {
            public struct EventState
            {
                public bool Modified;

                public FsmEvent
                    OriginalObtainedEvent;
            }


            public static void Prefix(
                CheckIfSpellObtained __instance,
                out EventState __state)
            {
                __state =
                    new EventState();


                if (!RandomizerRuntime.IsEnabled ||
                    RandomizerState.Settings == null ||
                    !RandomizerState.Settings
                        .RandomizeSpells ||
                    !IsBonnieState(
                        __instance,
                        FurryShieldCheckState
                    ))
                {
                    return;
                }


                __state.Modified =
                    true;

                __state.OriginalObtainedEvent =
                    __instance.onSpellObtained;


                // Whether Furry Shield is owned or not,
                // Bonnie should enter the P2 Spell reward
                // branch while spells are randomized.
                __instance.onSpellObtained =
                    __instance.onSpellNotObtained;


                Plugin.Log.LogInfo(
                    "BONNIE COMPATIBILITY | " +
                    "P2 Spell check forced available."
                );
            }


            public static void Postfix(
                CheckIfSpellObtained __instance,
                EventState __state)
            {
                if (!__state.Modified)
                {
                    return;
                }


                __instance.onSpellObtained =
                    __state.OriginalObtainedEvent;
            }
        }


        [HarmonyPatch(
            typeof(CheckIfKeysObtained),
            "OnEnter"
        )]
        private static class
            NecropawmiconCheckPatch
        {
            public struct EventState
            {
                public bool Modified;

                public FsmEvent
                    OriginalNoKeysEvent;

                public FsmEvent
                    OriginalSomeKeysEvent;

                public FsmEvent
                    OriginalAllKeysEvent;
            }


            public static void Prefix(
                CheckIfKeysObtained __instance,
                out EventState __state)
            {
                __state =
                    new EventState();


                if (!RandomizerRuntime.IsEnabled ||
                    !IsBonnieState(
                        __instance,
                        NecropawmiconCheckState
                    ))
                {
                    return;
                }


                __state.Modified =
                    true;

                __state.OriginalNoKeysEvent =
                    __instance.onNoKeysObtained;

                __state.OriginalSomeKeysEvent =
                    __instance.onSomeKeysObtained;

                __state.OriginalAllKeysEvent =
                    __instance.onAllKeysObtained;


                bool hasNecropawmicon =
                    Contexts.sharedInstance.game
                        .GetEquipment(
                            NecropawmiconGuid
                        ) != null;


                if (hasNecropawmicon)
                {
                    // Regardless of the vanilla key result,
                    // take Bonnie's successful branch.
                    __instance.onNoKeysObtained =
                        __state.OriginalAllKeysEvent;

                    __instance.onSomeKeysObtained =
                        __state.OriginalAllKeysEvent;

                    __instance.onAllKeysObtained =
                        __state.OriginalAllKeysEvent;
                }
                else
                {
                    // Regardless of the vanilla key result,
                    // take Bonnie's reminder branch.
                    __instance.onNoKeysObtained =
                        __state.OriginalNoKeysEvent;

                    __instance.onSomeKeysObtained =
                        __state.OriginalNoKeysEvent;

                    __instance.onAllKeysObtained =
                        __state.OriginalNoKeysEvent;
                }


                Plugin.Log.LogInfo(
                    "BONNIE COMPATIBILITY | " +
                    "Necropawmicon ownership:" +
                    hasNecropawmicon
                );
            }


            public static void Postfix(
                CheckIfKeysObtained __instance,
                EventState __state)
            {
                if (!__state.Modified)
                {
                    return;
                }


                __instance.onNoKeysObtained =
                    __state.OriginalNoKeysEvent;

                __instance.onSomeKeysObtained =
                    __state.OriginalSomeKeysEvent;

                __instance.onAllKeysObtained =
                    __state.OriginalAllKeysEvent;
            }
        }
    }
}