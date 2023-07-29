namespace CardZones {
    public class ZoneCard : CardData {
        [ExtraData("zone_card_target_card_id")]
        public string targetCardId;

        public void Start() {
            NameTerm = WorldManager.instance.GameDataLoader.GetCardFromId(targetCardId).NameTerm;
            DescriptionTerm = WorldManager.instance.GameDataLoader.GetCardFromId(targetCardId).DescriptionTerm;
        }

        public override bool CanHaveCard(CardData otherCard) {
            return otherCard.Id == targetCardId && otherCard.CanHaveCard(otherCard);
        }

        public override bool CanBePushedBy(CardData otherCard) => false;
    }
}
