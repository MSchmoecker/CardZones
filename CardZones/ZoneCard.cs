namespace CardZones {
    public class ZoneCard : CardData {
        [ExtraData("zone_card_target_card_id")]
        public string targetCardId;

        public void Start() {
            NameTerm = WorldManager.instance.GameDataLoader.GetCardFromId(targetCardId).NameTerm;
            DescriptionTerm = WorldManager.instance.GameDataLoader.GetCardFromId(targetCardId).DescriptionTerm;
        }

        public override bool CanHaveCard(CardData otherCard) {
            return otherCard.Id == targetCardId;
        }

        public override bool DetermineCanHaveCardsWhenIsRoot => !MyGameCard.HasChild ? base.DetermineCanHaveCardsWhenIsRoot : MyGameCard.Child.CardData.DetermineCanHaveCardsWhenIsRoot;

        public override bool CanHaveCardsWhileHasStatus() {
            return MyGameCard.HasChild ? MyGameCard.Child.CardData.CanHaveCardsWhileHasStatus() : base.CanHaveCardsWhileHasStatus();
        }

        public override bool CanBePushedBy(CardData otherCard) => false;
    }
}
