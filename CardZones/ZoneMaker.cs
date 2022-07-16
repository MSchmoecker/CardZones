using Shapes;
using UnityEngine;

namespace CardZones {
    public class ZoneMaker : CardTarget {
        public Rectangle HighlightRectangle;

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

        public override void Update() {
            if (WorldManager.instance.CurrentBoard) {
                HighlightRectangle.Color = WorldManager.instance.CurrentBoard.CardHighlightColor;
            }

            HighlightRectangle.enabled = WorldManager.instance.DraggingCard && WorldManager.instance.DraggingCard && WorldManager.instance.DraggingCard.CardData.Id != "zoneCard";
            HighlightRectangle.DashOffset += Time.deltaTime;
            if (HighlightRectangle.DashOffset >= 1.0) {
                HighlightRectangle.DashOffset--;
            }
        }

        public override string GetTooltipText() {
            return "Drop a card to create a zone card of this type";
        }
    }
}
