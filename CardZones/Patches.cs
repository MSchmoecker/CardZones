using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using TMPro;

namespace CardZones {
    [HarmonyPatch]
    public static class Patches {
        private static GameObject cardContainer;

        [HarmonyPatch(typeof(GameCard), nameof(GameCard.Start)), HarmonyPostfix]
        public static void DisplayOutline(GameCard __instance) {
            if (__instance.CardData.Id == "zoneCard") {
                __instance.CardRenderer.enabled = false;
                __instance.CardData.Value = 0;
            }
        }

        [HarmonyPatch(typeof(GameCard), nameof(GameCard.Update)), HarmonyPostfix]
        public static void StopOutlineMoving(GameCard __instance) {
            if (__instance.CardData.Id == "zoneCard") {
                __instance.HighlightRectangle.DashOffset = 0;
            }
        }

        [HarmonyPatch(typeof(GameCard), nameof(GameCard.CanBePushedBy)), HarmonyPostfix]
        public static void NoPushByZoneCards(Draggable draggable, ref bool __result) {
            if (draggable is GameCard gameCard && gameCard.CardData.Id == "zoneCard") {
                __result = false;
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

        [HarmonyPatch(typeof(WorldManager), nameof(WorldManager.Awake)), HarmonyPostfix]
        public static void AddCards(WorldManager __instance) {
            cardContainer = new GameObject("CardContainer");
            cardContainer.gameObject.SetActive(false);

            ZoneCard zoneCard = CreateNewCardPrefab<ZoneCard>("zoneCard");
            __instance.idToCard.Add(zoneCard.Id, zoneCard);
        }

        [HarmonyPatch(typeof(CardTarget), nameof(CardTarget.Start)), HarmonyPostfix]
        public static void AddMakeZoneBox(CardTarget __instance) {
            if (!(__instance is SellBox)) {
                return;
            }

            Transform parent = __instance.transform.parent;
            Transform zoneMaker = parent.Find("Zone Maker");

            if (zoneMaker) {
                return;
            }

            zoneMaker = Object.Instantiate(__instance.transform.parent.Find("Basic Pack"), parent).transform;
            zoneMaker.name = "Zone Maker";
            zoneMaker.SetSiblingIndex(1);

            TextMeshPro buyText = zoneMaker.GetComponent<BuyBoosterBox>().BuyText;
            TextMeshPro nameText = zoneMaker.GetComponent<BuyBoosterBox>().NameText;

            buyText.text = "";
            nameText.text = "Make Zone";

            Object.Destroy(zoneMaker.GetComponent<BuyBoosterBox>());
            zoneMaker.gameObject.AddComponent<ZoneMaker>();

            __instance.GetComponentInParent<SetPositions>().SetPosition();
        }

        public static T CreateNewCardPrefab<T>(string uniqueId) where T : CardData {
            GameObject card = new GameObject(uniqueId);
            card.transform.SetParent(cardContainer.transform);
            T cardData = card.AddComponent<T>();

            cardData.Id = uniqueId;
            cardData.UniqueId = uniqueId;

            return cardData;
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
