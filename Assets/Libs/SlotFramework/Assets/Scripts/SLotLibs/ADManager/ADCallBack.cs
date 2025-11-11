using Classic;
using Libs;
using Plugins;

namespace Ads
{
    public class ADCallBack : IAcbAdsCallbackHandler
    {
        //获得全屏广告时调用
        public void OnReceivedInterstitialAd(string message)
        {
        }

        //展示全屏广告时被调用
        public void OnShowInterstitialAd(string message)
        {
            PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.ShowVideo, 1);
        }

        //展示激励视频错误时被调用
        public void OnShowInterstitialAdError(string message)
        {
        }

        //点击全屏广告时被调用
        public void OnClickInterstitialAd(string message)
        {
        }

        //关闭全屏广告时被调用
        public void OnCloseInterstitialAd(string message)
        {
        }

        //获得激励视频广告时调用
        public void OnReceivedRewardVideo(string message)
        {
        }

        //激励视频加载完成时调用
        public void OnLoadRewardVideoComplete(string message)
        {
        }

        //激励视频加载失败时调用
        public void OnLoadRewardVideoError(string message)
        {
        }

        //展示激励视频广告时被调用
        public void OnShowRewardVideo(string message)
        {
            PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.ShowVideo, 0);
        }

        //点击激励视频时被调用
        public void OnClickRewardVideo(string message)
        {
        }

        //关闭激励视频时被调用
        public void OnCloseRewardVideo(string message)
        {
        }

        //获得视频奖励时调用，根据观看视屏的入口给予不同类型的奖励
        public void OnVideoReward(string message)
        {
            switch (message)
            {
                case ADEntrances.REWARD_VIDEO_ENTRANCE_LUCKYCASH:
                    //点击 luckyCash按钮给出的金钱奖励翻倍
                    Messenger.Broadcast<int>(ADConstants.PlayLuckyCashAD, 0);
                    break;
                case ADEntrances.Interstitial_Entrance_CLOSELUCKYCASH:
                    //关闭luckyCash时播放的全屏广告
                    Messenger.Broadcast<int>(ADConstants.PlayLuckyCashAD, 1);
                    break;
                case ADEntrances.Interstitial_Entrance_CLOSEFREESPINSTART:
                    //关闭freeSpin时播放的全屏广告
                    Messenger.Broadcast<int>(ADConstants.PlayFreeSpinAD, 1);
                    break;
                case ADEntrances.REWARD_VIDEO_ENTRANCE_MORECASH:
                    //Getmorecash弹板播放广告
                    Messenger.Broadcast<int>(ADConstants.PlayGetMoreCashAD, 0);
                    break;
                case ADEntrances.REWARD_VIDEO_ENTRANCE_SPINWIN:
                    //SpinWin弹板播放广告
                    Messenger.Broadcast<int>(ADConstants.PlaySpinWinAD, 0);
                    break;
                case ADEntrances.Interstitial_Entrance_CLOSESPINWIN:
                    //SpinWin弹板播放广告
                    Messenger.Broadcast<int>(ADConstants.PlaySpinWinAD, 1);
                    break;
                case ADEntrances.REWARD_VIDEO_ENTRANCE_FREESPINSTART:
                    Messenger.Broadcast<int>(ADConstants.PlayFreeSpinAD, 0);
                    break;
                case ADEntrances.REWARD_VIDEO_ENTRANCE_FREESPINEND:
                    Messenger.Broadcast<int>(ADConstants.PlayFreeSpinEndAD, 0);
                    break;
                case ADEntrances.Interstitial_Entrance_CLOSEFREESPINEND:
                    Messenger.Broadcast<int>(ADConstants.PlayFreeSpinEndAD, 1);
                    break;
                case ADEntrances.REWARD_VIDEO_ENTRANCE_FREESPINEXTA:
                    break;
                case ADEntrances.REWARD_VIDEO_ENTRANCE_JACKPOT:
                    //SpinWin弹板播放广告
                    Messenger.Broadcast<int>(ADConstants.PlayJackPotGameAD, 0);
                    break;
                case ADEntrances.Interstitial_Entrance_JACKPOTEND:
                    //SpinWin弹板播放广告
                    Messenger.Broadcast<int>(ADConstants.PlayJackPotGameAD, 1);
                    break;
                case ADEntrances.Interstitial_Entrance_JACKPOTSTART:
                    //SpinWin弹板播放广告
                    Messenger.Broadcast<int>(ADConstants.PlayJackPotGameAD, 1);
                    break;
                case ADEntrances.Interstitial_Entrance_CARDLOTTERY:
                    //SpinWin弹板播放广告
                    Messenger.Broadcast<int>(ADConstants.PlayCardLotteryAD, 1);
                    break;
                case ADEntrances.REWARD_VIDEO_ENTRANCE_CARDLOTTERY:
                    //点击 luckyCash按钮给出的金钱奖励翻倍
                    Messenger.Broadcast<int>(ADConstants.PlayCardLotteryAD, 2);
                    break;
                case ADEntrances.REWARD_VIDEO_CONTINUE_SPIN:
                    //点击 小屋按钮给钱
                    Messenger.Broadcast<int>(ADConstants.PlayContinueSpinAD, 0);
                    break;
                case ADEntrances.Interstitial_Entrance_CLOSECONTINUESPIN:
                    //点击 小屋按钮给钱
                    Messenger.Broadcast<int>(ADConstants.PlayContinueSpinAD, 1);
                    break;
                case ADEntrances.REWARD_VIDEO_ROTATION_BUFF:
                    //点击 小屋按钮给钱
                    Messenger.Broadcast<int>(ADConstants.PlayRotationBuffAD, 0);
                    break;
                case ADEntrances.Interstitial_Entrance_CLOSEROTATIONBUFF:
                    //点击 小屋按钮给钱
                    Messenger.Broadcast<int>(ADConstants.PlayRotationBuffAD, 1);
                    break;
                case ADEntrances.REWARD_VIDEO_WHEELLUCK_SPIN:
                    //兑换商店播广告
                    Messenger.Broadcast<int>(ADConstants.PlayWheelLuckAD, 0);
                    break;
                case ADEntrances.Interstitial_Entrance_WHEELLUCKBubble:
                    //兑换商店播广告
                    Messenger.Broadcast<int>(ADConstants.PlayWheelLuckAD, 1);
                    break;
                case ADEntrances.REWARD_VIDEO_FLOATING_REWARD:
                    //浮动奖励广告
                    Messenger.Broadcast<int>(ADConstants.PlayFloatingRewardAD, 0);
                    break;
                case ADEntrances.Interstitial_Entrance_LUCKYGIFT_ACTIVITY:
                    //LuckyCashActivity插屏广告
                    Messenger.Broadcast<int>(ADConstants.PlayLuckyGiftActivityAD, 1);
                    break;
                case ADEntrances.Interstitial_Entrance_WITHDRAWCLOSE:
                    //提现商店关闭广告
                    Messenger.Broadcast<int>(ADConstants.PlayWithDrawCloseAD, 1);
                    break;
                default:
                    break;
            }
        }

        //给奖励错误
        public void OnVideoRewardError(string message)
        {
            switch (message)
            {
                case ADEntrances.REWARD_VIDEO_ENTRANCE_LUCKYCASH:
                    //点击 luckyCash按钮给出的金钱奖励翻倍
                    Messenger.Broadcast<int>(ADConstants.PlayLuckyCashADFailed, 0);
                    break;
                case ADEntrances.Interstitial_Entrance_CLOSELUCKYCASH:
                    //关闭luckyCash时播放的全屏广告
                    Messenger.Broadcast<int>(ADConstants.PlayLuckyCashADFailed, 1);
                    break;
                case ADEntrances.Interstitial_Entrance_CLOSEFREESPINSTART:
                    //关闭freeSpin时播放的全屏广告
                    Messenger.Broadcast<int>(ADConstants.PlayFreeSpinADFailed, 1);
                    break;
                case ADEntrances.REWARD_VIDEO_ENTRANCE_MORECASH:
                    //Getmorecash弹板播放广告
                    Messenger.Broadcast<int>(ADConstants.PlayGetMoreCashADFailed, 0);
                    break;
                case ADEntrances.REWARD_VIDEO_ENTRANCE_SPINWIN:
                    //SpinWin弹板播放广告
                    Messenger.Broadcast<int>(ADConstants.PlaySpinWinADFailed, 0);
                    break;
                case ADEntrances.Interstitial_Entrance_CLOSESPINWIN:
                    //SpinWin弹板播放广告
                    Messenger.Broadcast<int>(ADConstants.PlaySpinWinADFailed, 1);
                    break;
                case ADEntrances.REWARD_VIDEO_ENTRANCE_FREESPINSTART:
                    Messenger.Broadcast<int>(ADConstants.PlayFreeSpinADFailed, 0);
                    break;
                case ADEntrances.REWARD_VIDEO_ENTRANCE_FREESPINEND:
                    Messenger.Broadcast<int>(ADConstants.PlayFreeSpinEndADFailed, 0);
                    break;
                case ADEntrances.Interstitial_Entrance_CLOSEFREESPINEND:
                    Messenger.Broadcast<int>(ADConstants.PlayFreeSpinEndADFailed, 1);
                    break;
                case ADEntrances.REWARD_VIDEO_ENTRANCE_FREESPINEXTA:
                    break;
                case ADEntrances.REWARD_VIDEO_ENTRANCE_JACKPOT:
                    //SpinWin弹板播放广告
                    Messenger.Broadcast<int>(ADConstants.PlayJackPotGameADFailed, 0);
                    break;
                case ADEntrances.Interstitial_Entrance_JACKPOTEND:
                    //SpinWin弹板播放广告
                    Messenger.Broadcast<int>(ADConstants.PlayJackPotGameADFailed, 1);
                    break;
                case ADEntrances.Interstitial_Entrance_JACKPOTSTART:
                    //SpinWin弹板播放广告
                    Messenger.Broadcast<int>(ADConstants.PlayJackPotGameADFailed, 1);
                    break;
                case ADEntrances.Interstitial_Entrance_CARDLOTTERY:
                    //SpinWin弹板播放广告
                    Messenger.Broadcast<int>(ADConstants.PlayCardLotteryADFailed, 1);
                    break;
                case ADEntrances.REWARD_VIDEO_ENTRANCE_CARDLOTTERY:
                    //点击cardLottery按钮给出的金钱奖励翻倍
                    Messenger.Broadcast<int>(ADConstants.PlayCardLotteryADFailed, 2);
                    break;
                case ADEntrances.REWARD_VIDEO_CONTINUE_SPIN:
                    //点击 小屋按钮给钱
                    Messenger.Broadcast<int>(ADConstants.PlayContinueSpinADFailed, 0);
                    break;
                case ADEntrances.Interstitial_Entrance_CLOSECONTINUESPIN:
                    //点击 小屋按钮给钱
                    Messenger.Broadcast<int>(ADConstants.PlayContinueSpinADFailed, 1);
                    break;
                case ADEntrances.REWARD_VIDEO_ROTATION_BUFF:
                    //点击RotationBuff
                    Messenger.Broadcast<int>(ADConstants.PlayRotationBuffADFailed, 0);
                    break;
                case ADEntrances.Interstitial_Entrance_CLOSEROTATIONBUFF:
                    //点击 小屋按钮给钱
                    Messenger.Broadcast<int>(ADConstants.PlayRotationBuffADFailed, 1);
                    break;
                case ADEntrances.REWARD_VIDEO_WHEELLUCK_SPIN:
                    //兑换商店播广告
                    Messenger.Broadcast<int>(ADConstants.PlayWheelLuckADFailed, 0);
                    break;
                case ADEntrances.Interstitial_Entrance_WHEELLUCKBubble:
                    //兑换商店播广告
                    Messenger.Broadcast<int>(ADConstants.PlayWheelLuckADFailed, 1);
                    break;
                case ADEntrances.REWARD_VIDEO_FLOATING_REWARD:
                    //浮动奖励广告失败
                    Messenger.Broadcast<int>(ADConstants.PlayFloatingRewardADFailed, 0);
                    break;
                case ADEntrances.Interstitial_Entrance_LUCKYGIFT_ACTIVITY:
                    //LuckyCashActivity插屏广告失败
                    Messenger.Broadcast<int>(ADConstants.PlayLuckyGiftActivityADFailed, 1);
                    break;
                case ADEntrances.Interstitial_Entrance_WITHDRAWCLOSE:
                    //提现商店关闭广告
                    Messenger.Broadcast<int>(ADConstants.PlayWithDrawCloseADFailed, 1);
                    break;
                default:
                    break;
            }
        }

        //展示激励视频错误时被调用
        public void OnShowRewardVideoError(string message)
        {
        }

        private void Rewarded()
        {
        }
    }
}