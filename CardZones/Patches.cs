using System;
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
                __instance.HighlightRectangle.enabled = true;
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
            __instance.GameDataLoader.idToCard.Add(zoneCard.Id, zoneCard);
        }

        [HarmonyPatch(typeof(CreatePackLine), nameof(CreatePackLine.Create)), HarmonyPostfix]
        public static void AddMakeZoneBox(CreatePackLine __instance) {
            Transform zoneMaker = Object.Instantiate(PrefabManager.instance.BoosterBoxPrefab, __instance.transform).transform;
            zoneMaker.name = "Zone Maker";
            zoneMaker.SetSiblingIndex(1);

            BuyBoosterBox buyBoosterBox = zoneMaker.GetComponent<BuyBoosterBox>();

            TextMeshPro buyText = buyBoosterBox.BuyText;
            TextMeshPro nameText = buyBoosterBox.NameText;

            buyText.text = "";
            nameText.text = "Make Zone";

            ZoneMaker maker = zoneMaker.gameObject.AddComponent<ZoneMaker>();
            maker.HighlightRectangle = buyBoosterBox.HighlightRectangle;
            Object.Destroy(buyBoosterBox);

            __instance.SetPositions();
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
        ///     WorldManager.GetCardCount() has a generic method as an overload, so we have to get a bit creative to only patch the no-generic one
        /// </summary>
        [HarmonyPatch]
        public class ZoneCardNotInCardCount {
            public static IEnumerable<MethodBase> TargetMethods() {
                Type[] types = Assembly.GetAssembly(typeof(WorldManager)).GetTypes();
                List<MethodInfo> patches = types
                                           .SelectMany(type => type.GetMethods())
                                           .Where(method => method.ReturnType == typeof(int) &&
                                                            method.Name == "GetCardCount" &&
                                                            !method.IsGenericMethod &&
                                                            method.GetParameters().Length == 0)
                                           .ToList();

                if (patches.Count > 1) {
                    Log.LogWarning("More than one WorldManager.GetCardCount() method found, this can cause unexpected behavior");
                }

                return patches;
            }

            public static void Postfix(WorldManager __instance, ref int __result) {
                __result -= WorldManager.instance.GetCardCount("zoneCard");
            }
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
