namespace CardZones {
    public class ZoneCard : CardData {
        // public override bool CanBeDragged => false;
        public override bool CanBePushedBy(CardData otherCard) => false;

        [ExtraData("zone_card_target_card_id")]
        public string targetCardId;

        public void Start() {
            NameTerm = WorldManager.instance.GetCardFromId(targetCardId).NameTerm;
            DescriptionTerm = WorldManager.instance.GetCardFromId(targetCardId).DescriptionTerm;
        }

        public override void Update() {
            base.Update();
            MyGameCard.HighlightActive = true;
        }

        public override bool CanHaveCard(CardData otherCard) {
            return otherCard.Id == targetCardId;
        }
    }
}
