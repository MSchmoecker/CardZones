﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using TMPro;
using Object = UnityEngine.Object;

namespace CardZones {
    [HarmonyPatch]
    public static class Patches {
        [HarmonyPatch(typeof(GameCard), nameof(GameCard.Start)), HarmonyPostfix]
        public static void DisplayOutline(GameCard __instance) {
            if (__instance.CardData.Id == "zoneCard") {
                __instance.CardRenderer.enabled = false;
                __instance.CardData.Value = 0;
                __instance.CardData.CitiesValue = 0;
            }
        }

        [HarmonyPatch(typeof(GameCard), nameof(GameCard.SetColors)), HarmonyPostfix]
        public static void SetColor(GameCard __instance) {
            if (__instance.CardData.Id == "zoneCard") {
                __instance.CardNameText.color = __instance.HighlightRectangle.Color;
            }
        }

        [HarmonyPatch(typeof(GameCard), nameof(GameCard.Update)), HarmonyPostfix]
        public static void StopOutlineMoving(GameCard __instance) {
            if (__instance.CardData.Id == "zoneCard") {
                __instance.HighlightRectangle.DashOffset = 0;
                __instance.HighlightRectangle.enabled = true;
            }
        }

        [HarmonyPatch(typeof(GameCard), nameof(GameCard.Update)), HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> HideCoinIcon(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
            FieldInfo gameCard_CardData = AccessTools.Field(typeof(GameCard), nameof(GameCard.CardData));
            MethodInfo cardData_Value = AccessTools.Method(typeof(CardData), nameof(CardData.GetValue));

            return new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(i => i.LoadsField(gameCard_CardData)),
                    new CodeMatch(i => i.Calls(cardData_Value)),
                    new CodeMatch(OpCodes.Stloc_S),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldc_I4_M1),
                    new CodeMatch(i => i.opcode == OpCodes.Beq_S || i.opcode == OpCodes.Beq)
                )
                .GetOperand(out object jumpLabel)
                .Advance(1)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(CodeInstruction.LoadField(typeof(GameCard), nameof(GameCard.CardData))),
                    new CodeInstruction(CodeInstruction.Call(typeof(Patches), nameof(IsZoneCard))),
                    new CodeInstruction(OpCodes.Brtrue, jumpLabel))
                .InstructionEnumeration();
        }

        public static bool IsZoneCard(CardData cardData) {
            return cardData.Id == "zoneCard";
        }

        [HarmonyPatch(typeof(Draggable), nameof(Draggable.PushAwayFromOthers)), HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Sticky(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
            return new CodeMatcher(instructions)
                .MatchForward(true, new CodeMatch(OpCodes.Isinst, typeof(HeavyFoundation)))
                .SetInstruction(new CodeInstruction(CodeInstruction.Call(typeof(Patches), nameof(IsHeavy))))
                .MatchForward(true, new CodeMatch(OpCodes.Isinst, typeof(HeavyFoundation)))
                .SetInstruction(new CodeInstruction(CodeInstruction.Call(typeof(Patches), nameof(IsHeavy))))
                .InstructionEnumeration();
        }

        private static bool IsHeavy(CardData cardData) {
            if (cardData is HeavyFoundation) {
                return true;
            }

            if (CardZonesMod.stickyZones.Value && cardData is ZoneCard) {
                return true;
            }

            return false;
        }

        [HarmonyPatch(typeof(GameCard), nameof(GameCard.Mass), MethodType.Getter), HarmonyPostfix]
        public static void Sticky(GameCard __instance, ref float __result) {
            if (CardZonesMod.stickyZones.Value && __instance.CardData is ZoneCard) {
                __result += 1000f;
            }
        }

        [HarmonyPatch(typeof(CardData), nameof(CardData.CanHaveCardOnTop)), HarmonyPrefix]
        public static bool NoStacking(CardData otherCard, ref bool __result) {
            if (otherCard.Id != "zoneCard") {
                return true;
            }

            __result = false;
            return false;
        }

        [HarmonyPatch(typeof(CreatePackLine), nameof(CreatePackLine.CreateBoosterBoxes)), HarmonyPostfix]
        public static void AddMakeZoneBox(CreatePackLine __instance) {
            Transform zoneMaker = Object.Instantiate(PrefabManager.instance.BoosterBoxPrefab, __instance.transform).transform;
            zoneMaker.name = "Zone Maker";
            zoneMaker.SetSiblingIndex(1);

            BuyBoosterBox buyBoosterBox = zoneMaker.GetComponent<BuyBoosterBox>();
            buyBoosterBox.NewLabel.SetActive(false);

            TextMeshPro buyText = buyBoosterBox.BuyText;
            TextMeshPro nameText = buyBoosterBox.NameText;

            buyText.text = "";
            nameText.text = "Make Zone";

            ZoneMaker maker = zoneMaker.gameObject.AddComponent<ZoneMaker>();
            maker.HighlightRectangle = buyBoosterBox.HighlightRectangle;
            Object.Destroy(buyBoosterBox);

            __instance.SetPositions();
        }

        [HarmonyPatch(typeof(WorldManager), nameof(WorldManager.CountsTowardCardCount)), HarmonyPrefix]
        public static bool DontCountZoneCard(GameCard card, ref bool __result) {
            if (card.CardData.Id == "zoneCard") {
                __result = false;
                return false;
            }

            return true;
        }

        /// <summary>
        ///     When a card is produced, it only stacks to cards with the same Id but zone cards have a different Id.
        ///     This changes inside the big if statement's `allCard.CardData.Id == myCard.CardData.Id` to allow for the needed stacking.
        /// </summary>
        /// <param name="instructions"></param>
        /// <param name="generator"></param>
        /// <returns></returns>
        [HarmonyPatch(typeof(WorldManager), nameof(WorldManager.StackSend)), HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> StackSendAllowToZone(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
            FieldInfo gameCardCardData = AccessTools.Field(typeof(GameCard), nameof(GameCard.CardData));
            FieldInfo cardDataId = AccessTools.Field(typeof(CardData), nameof(CardData.Id));
            Label label = generator.DefineLabel();

            return new CodeMatcher(instructions, generator)
                   .MatchForward(true, new[] {
                       new CodeMatch(i => i.LoadsField(gameCardCardData)),
                       new CodeMatch(i => i.LoadsField(cardDataId)),
                       new CodeMatch(OpCodes.Ldarg_1),
                       new CodeMatch(i => i.LoadsField(gameCardCardData)),
                       new CodeMatch(i => i.LoadsField(cardDataId))
                   })
                   .Advance(3)
                   .AddLabels(new[] { label })
                   .Advance(-1)
                   .Insert(new CodeInstruction(OpCodes.Brtrue, label),
                           new CodeInstruction(OpCodes.Ldarg_1),
                           new CodeInstruction(OpCodes.Ldloc_S, 4),
                           new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Patches), nameof(CanStackCardOnZone))))
                   .InstructionEnumeration();
        }

        public static bool CanStackCardOnZone(GameCard myCard, GameCard otherCard) {
            return otherCard.CardData.Id == "zoneCard" && otherCard.CardData is ZoneCard zone && zone.targetCardId == myCard.CardData.Id;
        }
    }
}
