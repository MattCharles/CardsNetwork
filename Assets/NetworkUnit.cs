using Unity.Netcode;
using intPair = System.Tuple<int, int>;

public class NetworkUnit : NetworkBehaviour
{
    public NetworkVariable<bool> royal;

    public NetworkVariable<PromotionRule> promotionRule;

    public NetworkVariable<int> movementRule;
    public NetworkVariable<ushort> internalId;
    public intPair boardPosition;

    public struct PromotionRule
    {
        public int promoConditionIndex;
        public int upgradeToIndex;
    }
}
