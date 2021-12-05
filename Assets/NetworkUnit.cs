using Unity.Netcode;

public class NetworkUnit : NetworkBehaviour
{
    public NetworkVariable<PromotionRule> promotionRule;

    public NetworkList<int> movementRules;

    public struct PromotionRule
    {
        public int promoConditionIndex;
        public int upgradeToIndex;
    }
}
