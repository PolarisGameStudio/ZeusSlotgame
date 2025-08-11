using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ads
{
	/// <summary>
	/// 定义封装后的对全屏广告库的调用的接口
	/// </summary>
	public interface IAcbAdsCallbackHandler
	{
		// 全屏广告被展示时调用
		void OnShowInterstitialAd(string message);

		// 全屏广告展示失败时调用
		void OnShowInterstitialAdError(string message);

		// 全屏广告被点击时调用
		void OnClickInterstitialAd(string message);

		// 全屏广告被关闭时调用
		void OnCloseInterstitialAd(string message);

		// 获取到视频广告时被调用
		void OnReceivedRewardVideo(string message);

		// 视频广告加载完成时被调用
		void OnLoadRewardVideoComplete(string message);

		// 视频广告加载失败时调用
		void OnLoadRewardVideoError(string message);

		// 视频广告被展示时调用
		void OnShowRewardVideo(string message);

		// 视频广告展示失败时调用
		void OnShowRewardVideoError(string message);

		// 视频广告被点击时调用
		void OnClickRewardVideo(string message);

		// 视频广告被关闭时调用
		void OnCloseRewardVideo(string message);

		// 获得视频奖励时调用
		void OnVideoReward(string message);
		// 视频奖励错误时调用
		void OnVideoRewardError(string message);
	}
}

