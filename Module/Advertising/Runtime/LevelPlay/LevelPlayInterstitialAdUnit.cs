using System;
#if VIRTUESKY_ADS && VIRTUESKY_LEVELPLAY
using Unity.Services.LevelPlay;
#endif
using UnityEngine;
using VirtueSky.Misc;

namespace VirtueSky.Ads
{
    [Serializable]
    public class LevelPlayInterstitialAdUnit : AdUnit
    {
        [NonSerialized] internal Action completedCallback;
#if VIRTUESKY_ADS && VIRTUESKY_LEVELPLAY
        private LevelPlayInterstitialAd interstitialAd;
#endif
        public override bool IsShowing { get; internal set; }

        public override void Init()
        {
#if VIRTUESKY_ADS && VIRTUESKY_LEVELPLAY
            if (AdStatic.IsRemoveAd) return;
#endif
        }

        public override void Load()
        {
#if VIRTUESKY_ADS && VIRTUESKY_LEVELPLAY
            if (AdStatic.IsRemoveAd) return;
            var configBuilder = new LevelPlayInterstitialAd.Config.Builder();
            var config = configBuilder.Build();
            interstitialAd = new LevelPlayInterstitialAd(Id, config);
            interstitialAd.OnAdLoaded += InterstitialOnAdLoadedEvent;
            interstitialAd.OnAdLoadFailed += InterstitialOnAdLoadFailed;
            interstitialAd.OnAdDisplayed += InterstitialOnAdDisplayEvent;
            interstitialAd.OnAdClicked += InterstitialOnAdClickedEvent;
            interstitialAd.OnAdDisplayFailed += InterstitialOnAdDisplayFailedEvent;
            interstitialAd.OnAdClosed += InterstitialOnAdClosedEvent;
            interstitialAd.LoadAd();
#endif
        }

        public override bool IsReady()
        {
#if VIRTUESKY_ADS && VIRTUESKY_LEVELPLAY
            return interstitialAd != null && interstitialAd.IsAdReady();
#else
            return false;
#endif
        }

        protected override void ShowImpl(string placement = null)
        {
#if VIRTUESKY_ADS && VIRTUESKY_LEVELPLAY
            if (interstitialAd != null) interstitialAd.ShowAd(placement);
#endif
        }

        public override AdUnit Show(string placement = null)
        {
            ResetChainCallback();
            if (!Application.isMobilePlatform || AdStatic.IsRemoveAd || !IsReady()) return this;
            ShowImpl(placement);
            return this;
        }

        public override void Destroy()
        {
        }

        protected override void ResetChainCallback()
        {
            base.ResetChainCallback();
            completedCallback = null;
        }

#if VIRTUESKY_ADS && VIRTUESKY_LEVELPLAY

        #region Fun Callback

        void InterstitialOnAdLoadedEvent(LevelPlayAdInfo adInfo)
        {
            var info = new AdsInfo(adInfo);
            Common.CallActionAndClean(ref loadedCallback, info);
            OnLoadAdEvent?.Invoke(info);
        }

        void InterstitialOnAdLoadFailed(LevelPlayAdError ironSourceError)
        {
            var errorInfo = new AdsError(ironSourceError);
            Common.CallActionAndClean(ref failedToLoadCallback, errorInfo);
            OnFailedToLoadAdEvent?.Invoke(errorInfo);
        }

        void InterstitialOnAdDisplayEvent(LevelPlayAdInfo adInfo)
        {
            AdStatic.IsShowingAd = true;
            IsShowing = true;
            var info = new AdsInfo(adInfo);
            Common.CallActionAndClean(ref displayedCallback, info);
            OnDisplayedAdEvent?.Invoke(info);
        }

        void InterstitialOnAdClickedEvent(LevelPlayAdInfo adInfo)
        {
            var info = new AdsInfo(adInfo);
            Common.CallActionAndClean(ref clickedCallback, info);
            OnClickedAdEvent?.Invoke(info);
        }
        
        void InterstitialOnAdDisplayFailedEvent(LevelPlayAdInfo adInfo, LevelPlayAdError adError)
        {
            var errorInfo = new AdsError(adError);
            Common.CallActionAndClean(ref failedToDisplayCallback, errorInfo);
            OnFailedToDisplayAdEvent?.Invoke(errorInfo);
        }

        void InterstitialOnAdClosedEvent(LevelPlayAdInfo adInfo)
        {
            AdStatic.IsShowingAd = false;
            IsShowing = false;
            Common.CallActionAndClean(ref completedCallback);
            var info = new AdsInfo(adInfo);
            Common.CallActionAndClean(ref closedCallback, info);
            OnClosedAdEvent?.Invoke(info);
            Load();
        }
       

        #endregion

#endif
    }
}
