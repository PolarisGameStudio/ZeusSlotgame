using System.Collections.Generic;
using UnityEngine;
using Ads;
namespace CardSystem
{
    public class ADCondition:BaseWeightCondition
    {
        public override void Execute()
        {
            Debug.Log("ADCondition Execute");
            Messenger.Broadcast<string>(ADConstants.PlayAdByEntrance,ADEntrances.REWARD_VIDEO_ENTRANCE_CARDLOTTERY);
        }
    }
}