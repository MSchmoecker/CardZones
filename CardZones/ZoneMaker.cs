using UnityEngine;

namespace CardZones {
    public class ZoneMaker : CardTarget {
        public override void CardDropped(GameCard card) {
            base.CardDropped(card);

            if (card.CardData is ZoneCard) {
                return;
            }

            ZoneCard zoneCard = (ZoneCard)WorldManager.instance.CreateCard(transform.position, "zoneCard", true, false);
            zoneCard.targetCardId = card.CardData.Id;
            zoneCard.NameTerm = card.CardData.NameTerm;
            zoneCard.MyGameCard.Velocity = new Vector3(0f, 8f, -4.5f);
        }

        public override string GetTooltipText() {
            return "Drop a card to create a zone card of this type";
        }
    }
}
