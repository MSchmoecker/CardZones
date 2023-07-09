using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace CardZones {
    [HarmonyPatch]
    public static class GlueEquipmentPatches {
        [HarmonyPatch(typeof(CardData), nameof(CardData.StoppedDragging)), HarmonyPostfix]
        public static void CardDataStoppedDragging(CardData __instance) {
            if (__instance is HeavyFoundation) {
                GameCard parent = __instance.MyGameCard.Parent;
                List<CardData> glues = __instance.CardsInStackMatchingPredicate((x) => x is HeavyFoundation);

                foreach (CardData glue in glues) {
                    glue.TryEquipOnCard(parent);
                }
            }
        }

        public static void TryEquipOnCard(this CardData target, GameCard card) {
            if (target.MyGameCard.EquipmentHolder != null) {
                target.MyGameCard.EquipmentHolder.Unequip(target);
            }

            if (card && card.CardData.HasInventory && card.CardData is ZoneCard) {
                card.OpenInventory(true);
                card.CardData.EquipItem(target);
            }
        }

        public static void Unequip(this GameCard target, CardData equipable) {
            GameCard gameCard = equipable.MyGameCard;
            target.EquipmentChildren.Remove(gameCard);
            gameCard.EquipmentHolder = null;
        }

        public static void EquipItem(this CardData target, CardData equipable) {
            CardData equipped = target.GetEquipped();

            if (equipped != null) {
                target.MyGameCard.Unequip(equipped);
                equipped.MyGameCard.SendIt();
            }

            target.MyGameCard.Equip(equipable);
        }

        public static CardData GetEquipped(this CardData target) {
            return target.MyGameCard.EquipmentChildren.FirstOrDefault()?.CardData;
        }

        public static void Equip(this GameCard target, CardData equipable) {
            GameCard gameCard = equipable.MyGameCard;
            target.EquipmentChildren.Add(gameCard);
            gameCard.EquipmentHolder = target;
            gameCard.RemoveFromStack();
        }

        [HarmonyPatch(typeof(GameCard), nameof(GameCard.GetMyEquipmentStackPosition)), HarmonyPrefix]
        public static bool GetMyEquipmentStackPosition(GameCard __instance, ref Transform __result) {
            if (__instance.CardData is not HeavyFoundation) {
                return true;
            }

            __result = __instance.EquipmentHolder.HandEquipmentPosition.transform;
            return false;
        }

        [HarmonyPatch(typeof(CardData), nameof(CardData.HasEquipableOfEquipableType)), HarmonyPrefix]
        public static bool CardDataHasEquipableOfEquipableType(CardData __instance, ref bool __result, EquipableType type) {
            if (type == EquipableType.Weapon && __instance is ZoneCard) {
                __result = __instance.MyGameCard.EquipmentChildren.Exists((x) => x.CardData is HeavyFoundation);
                return false;
            }

            return true;
        }
    }
}
