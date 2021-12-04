using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkUnit : NetworkBehaviour
{
    public NetworkVariable<PromotionRule>? promotionRule;

    public NetworkList<MovementRule> movementRules;
}
