using Unity.Netcode;

public class NetworkUnit : NetworkBehaviour
{
    public NetworkVariable<bool> royal;

    public NetworkVariable<PromotionRule> promotionRule;

    public NetworkList<int> movementRules;
    public ushort internalId;

    public struct PromotionRule
    {
        public int promoConditionIndex;
        public int upgradeToIndex;
    }
}
