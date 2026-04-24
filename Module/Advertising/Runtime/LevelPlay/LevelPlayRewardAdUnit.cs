using System;
using VirtueSky.Core;
#if VIRTUESKY_ADS && VIRTUESKY_LEVELPLAY
using Unity.Services.LevelPlay;
#endif
using VirtueSky.Misc;
using VirtueSky.Tracking;

namespace VirtueSky.Ads
{
    [Serializable]
    public class LevelPlayRewardAdUnit : AdUnit
    {
        [NonSerialized] internal Action completedCallback;
        [NonSerialized] internal Action skippedCallback;
        [NonSerialized] internal Action receivedRewardCallback;
        public bool IsEarnRewarded { get; private set; }
        private const float FinalizeCloseDelay = 0.2f;
        private DelayHandle _finalizeCloseHandle;
        
#if VIRTUESKY_ADS && VIRTUESKY_LEVELPLAY
        LevelPlayRewardedAd rewardedAd;
#endif
        public override bool IsShowing { get; internal set; }

        public override void Init()
        {
#if VIRTUESKY_ADS && VIRTUESKY_LEVELPLAY
            if (AdStatic.IsRemoveAd) return;
            paidedCallback += AppTracking.TrackRevenue;
#endif
        }

        public override void Load()
        {
#if VIRTUESKY_ADS && VIRTUESKY_LEVELPLAY
            if (AdStatic.IsRemoveAd) return;
            var configBuilder = new LevelPlayRewardedAd.Config.Builder();
            var config = configBuilder.Build();
            rewardedAd = new LevelPlayRewardedAd(Id, config);
            rewardedAd.OnAdLoaded += OnAdLoaded;
            rewardedAd.OnAdDisplayed += RewardedVideoOnAdDisplayedEvent;
            rewardedAd.OnAdClosed += RewardedVideoOnAdClosedEvent;
            rewardedAd.OnAdDisplayFailed += RewardedVideoOnAdDisplayFailedEvent;
            rewardedAd.OnAdRewarded += RewardedVideoOnAdRewardedEvent;
            rewardedAd.OnAdClicked += RewardedVideoOnAdClickedEvent;
            rewardedAd.OnAdLoadFailed += RewardedVideoOnAdLoadFailedEvent;
            rewardedAd.LoadAd();
#endif
        }

        public override bool IsReady()
        {
#if VIRTUESKY_ADS && VIRTUESKY_LEVELPLAY
            return rewardedAd != null && rewardedAd.IsAdReady();
#else
            return false;
#endif
        }

        protected override void ShowImpl(string placement = "")
        {
#if VIRTUESKY_ADS && VIRTUESKY_LEVELPLAY
            if (rewardedAd != null) rewardedAd.ShowAd(placement);
#endif
        }

        public override AdUnit Show(string placement = "")
        {
            ResetChainCallback();
            if (!UnityEngine.Application.isMobilePlatform || !IsReady()) return this;
            ShowImpl(placement);
            return this;
        }

        public override void Destroy()
        {
            IsShowing = false;
        }
        
        private void ResetFinalizeCloseHandle()
        {
            App.CancelDelay(_finalizeCloseHandle);
            _finalizeCloseHandle = null;
        }
        
        protected override void ResetChainCallback()
        {
            base.ResetChainCallback();
            completedCallback = null;
            skippedCallback = null;
            receivedRewardCallback = null;
            IsEarnRewarded = false;
        }

#if VIRTUESKY_ADS && VIRTUESKY_LEVELPLAY

        #region Fun Callback

        internal void OnAdPaidEvent(LevelPlayImpressionData impressionData)
        {
            if (impressionData.MediationAdUnitId.Equals(Id))
            {
                paidedCallback?.Invoke((double)impressionData.Revenue, impressionData.AdNetwork,
                    impressionData.MediationAdUnitId,
                    impressionData.AdFormat, AdMediation.LevelPlay.ToString());
            }
        }

        void OnAdLoaded(LevelPlayAdInfo adInfo)
        {
            var info = new AdsInfo(adInfo);
            Common.CallActionAndClean(ref loadedCallback, info);
            OnLoadAdEvent?.Invoke(info);
        }

        private void RewardedVideoOnAdLoadFailedEvent(LevelPlayAdError ironSourceError)
        {
            var errorInfo = new AdsError(ironSourceError);
            Common.CallActionAndClean(ref failedToLoadCallback, errorInfo);
            OnFailedToLoadAdEvent?.Invoke(errorInfo);
        }

        void RewardedVideoOnAdDisplayedEvent(LevelPlayAdInfo adInfo)
        {
            AdStatic.IsShowingAd = true;
            IsShowing = true;
            var info = new AdsInfo(adInfo);
            Common.CallActionAndClean(ref displayedCallback, info);
            OnDisplayedAdEvent?.Invoke(info);
        }

        void RewardedVideoOnAdClosedEvent(LevelPlayAdInfo adInfo)
        {
            AdStatic.IsShowingAd = false;
            var info = new AdsInfo(adInfo);
            Common.CallActionAndClean(ref closedCallback, info);
            OnClosedAdEvent?.Invoke(info);
            App.CancelDelay(_finalizeCloseHandle);
            _finalizeCloseHandle = App.Delay(FinalizeCloseDelay, FinalizeClose);
        }

        void RewardedVideoOnAdDisplayFailedEvent(LevelPlayAdInfo adInfo, LevelPlayAdError ironSourceError)
        {
            var errorInfo = new AdsError(ironSourceError);
            Common.CallActionAndClean(ref failedToDisplayCallback, errorInfo);
            OnFailedToDisplayAdEvent?.Invoke(errorInfo);
        }

        void RewardedVideoOnAdRewardedEvent(LevelPlayAdInfo info, LevelPlayReward reward)
        {
            IsEarnRewarded = true;
            Common.CallActionAndClean(ref receivedRewardCallback);
        }

        void RewardedVideoOnAdClickedEvent(LevelPlayAdInfo adInfo)
        {
            var info = new AdsInfo(adInfo);
            Common.CallActionAndClean(ref clickedCallback, info);
            OnClickedAdEvent?.Invoke(info);
        }
        private void FinalizeClose()
        {
            _finalizeCloseHandle = null;
            if (!IsReady() && rewardedAd != null) rewardedAd.LoadAd();
            if (IsEarnRewarded)
            {
                Common.CallActionAndClean(ref completedCallback);
                IsEarnRewarded = false;
                ResetFinalizeCloseHandle();
                IsShowing = false;
                return;
            }

            Common.CallActionAndClean(ref skippedCallback);
            ResetFinalizeCloseHandle();
            IsShowing = false;
        }
        #endregion

#endif
    }
}