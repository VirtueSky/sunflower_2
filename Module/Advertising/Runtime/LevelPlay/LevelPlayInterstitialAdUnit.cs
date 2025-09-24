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
        public override void Init()
        {
#if VIRTUESKY_ADS && VIRTUESKY_LEVELPLAY
            if (AdStatic.IsRemoveAd) return;
            interstitialAd.OnAdLoaded += InterstitialOnAdLoadedEvent;
            interstitialAd.OnAdLoadFailed += InterstitialOnAdLoadFailed;
            interstitialAd.OnAdDisplayed += InterstitialOnAdDisplayEvent;
            interstitialAd.OnAdClicked += InterstitialOnAdClickedEvent;
            interstitialAd.OnAdDisplayFailed += InterstitialOnAdDisplayFailedEvent;
            interstitialAd.OnAdClosed += InterstitialOnAdClosedEvent;
#endif
        }

        public override void Load()
        {
#if VIRTUESKY_ADS && VIRTUESKY_LEVELPLAY
            if (AdStatic.IsRemoveAd) return;
            var configBuilder = new LevelPlayInterstitialAd.Config.Builder();
            var config = configBuilder.Build();
            interstitialAd = new LevelPlayInterstitialAd(Id, config);
           interstitialAd.LoadAd();
            OnAdLoaded();
#endif
        }

        public override bool IsReady()
        {
#if VIRTUESKY_ADS && VIRTUESKY_LEVELPLAY
            return interstitialAd.IsAdReady();
#else
            return false;
#endif
        }

        protected override void ShowImpl()
        {
#if VIRTUESKY_ADS && VIRTUESKY_LEVELPLAY
            interstitialAd.ShowAd();
#endif
        }

        public override AdUnit Show()
        {
            ResetChainCallback();
            if (!Application.isMobilePlatform || AdStatic.IsRemoveAd || !IsReady()) return this;
            ShowImpl();
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
        }

        void InterstitialOnAdLoadFailed(LevelPlayAdError ironSourceError)
        {
            Common.CallActionAndClean(ref failedToLoadCallback);
            OnFailedToLoadAdEvent?.Invoke(ironSourceError.ErrorMessage);
        }

        void InterstitialOnAdDisplayEvent(LevelPlayAdInfo adInfo)
        {
            AdStatic.IsShowingAd = true;
            Common.CallActionAndClean(ref displayedCallback);
            OnDisplayedAdEvent?.Invoke();
        }

        void InterstitialOnAdClickedEvent(LevelPlayAdInfo adInfo)
        {
            Common.CallActionAndClean(ref clickedCallback);
            OnClickedAdEvent?.Invoke();
        }
        
        void InterstitialOnAdDisplayFailedEvent(LevelPlayAdInfo adInfo, LevelPlayAdError adError)
        {
            Common.CallActionAndClean(ref failedToDisplayCallback);
            OnFailedToDisplayAdEvent?.Invoke(adError.ErrorMessage);
        }

        void InterstitialOnAdClosedEvent(LevelPlayAdInfo adInfo)
        {
            AdStatic.IsShowingAd = false;
            Common.CallActionAndClean(ref completedCallback);
            OnClosedAdEvent?.Invoke();
            Load();
        }

        private void OnAdLoaded()
        {
            Common.CallActionAndClean(ref loadedCallback);
            OnLoadAdEvent?.Invoke();
        }

        #endregion

#endif
    }
}