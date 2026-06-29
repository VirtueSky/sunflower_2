using System;
using VirtueSky.Misc;
using VirtueSky.Tracking;
using VirtueSky.Utils;

namespace VirtueSky.Ads
{
    [Serializable]
    public class MaxInterstitialAdUnit : AdUnit
    {
        [NonSerialized] internal Action completedCallback;


        public override bool IsShowing { get; internal set; }
        public override bool IsLoading { get; internal set; }

        public override void Init()
        {
#if VIRTUESKY_ADS && VIRTUESKY_APPLOVIN
            if (AdStatic.IsRemoveAd || string.IsNullOrEmpty(Id)) return;
            paidedCallback += TrackRevenue;
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnAdLoaded;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnAdLoadFailed;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnAdRevenuePaid;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnAdDisplayed;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnAdHidden;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnAdDisplayFailed;
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnAdClicked;
#endif
        }

        public override void Load()
        {
#if VIRTUESKY_ADS && VIRTUESKY_APPLOVIN
            if (AdStatic.IsRemoveAd || string.IsNullOrEmpty(Id)) return;
            IsLoading = true;
            VLog.Log($"Advertising: Load MaxInterstitialAd: {Id}");
            OnRequestAdEvent?.Invoke();
            MaxSdk.LoadInterstitial(Id);
#endif
        }


        public override bool IsReady()
        {
#if VIRTUESKY_ADS && VIRTUESKY_APPLOVIN
            return !string.IsNullOrEmpty(Id) && MaxSdk.IsInterstitialReady(Id);
#else
            return false;
#endif
        }

        protected override void ShowImpl(string placement = "")
        {
#if VIRTUESKY_ADS && VIRTUESKY_APPLOVIN
            VLog.Log($"Advertising: MaxInterstitialAd show: {Id}");
            MaxSdk.ShowInterstitial(Id, placement: placement);
#endif
        }

        public override void Destroy()
        {
        }

        protected override void ResetChainCallback()
        {
            base.ResetChainCallback();
            completedCallback = null;
        }

        #region Func Callback

#if VIRTUESKY_ADS && VIRTUESKY_APPLOVIN
        private void OnAdDisplayFailed(string unit, MaxSdkBase.ErrorInfo error,
            MaxSdkBase.AdInfo info)
        {
            var errorInfo = new AdsError(error);
            VLog.LogWarning(
                $"Advertising: MaxInterstitialAd FailedToDisplay: {Id}, errorCode: {errorInfo.ErrorCode}, errorMessage: {errorInfo.ErrorMessage}");
            ExcuteCallbackOnMainThread(() =>
            {
                Common.CallActionAndClean(ref failedToDisplayCallback, errorInfo);
                OnFailedToDisplayAdEvent?.Invoke(errorInfo);
            });
        }

        private void OnAdHidden(string unit, MaxSdkBase.AdInfo info)
        {
            VLog.Log($"Advertising: MaxInterstitialAd Closed: {Id}");
            AdStatic.IsShowingAd = false;
            var adsInfo = new AdsInfo(info);
            ExcuteCallbackOnMainThread(() =>
            {
                Common.CallActionAndClean(ref completedCallback);
                Common.CallActionAndClean(ref closedCallback, adsInfo);
                OnClosedAdEvent?.Invoke(adsInfo);
            });

            IsShowing = false;
            if (!IsReady()) Load();
        }

        private void OnAdDisplayed(string unit, MaxSdkBase.AdInfo info)
        {
            VLog.Log($"Advertising: MaxInterstitialAd Displayed: {Id}");
            AdStatic.IsShowingAd = true;
            IsShowing = true;
            var adsInfo = new AdsInfo(info);
            ExcuteCallbackOnMainThread(() =>
            {
                Common.CallActionAndClean(ref displayedCallback, adsInfo);
                OnDisplayedAdEvent?.Invoke(adsInfo);
            });
        }

        private void OnAdClicked(string arg1, MaxSdkBase.AdInfo arg2)
        {
            VLog.Log($"Advertising: MaxInterstitialAd Clicked: {Id}");
            var info = new AdsInfo(arg2);
            ExcuteCallbackOnMainThread(() =>
            {
                Common.CallActionAndClean(ref clickedCallback, info);
                OnClickedAdEvent?.Invoke(info);
            });
        }

        private void OnAdRevenuePaid(string unit, MaxSdkBase.AdInfo info)
        {
            paidedCallback?.Invoke(new AdsInfo(info));
        }

        private void OnAdLoadFailed(string unit, MaxSdkBase.ErrorInfo info)
        {
            IsLoading = false;
            var errorInfo = new AdsError(info);
            VLog.LogWarning(
                $"Advertising: MaxInterstitialAd FailedToLoad: {Id}, errorCode: {errorInfo.ErrorCode}, errorMessage: {errorInfo.ErrorMessage}");
            ExcuteCallbackOnMainThread(() =>
            {
                Common.CallActionAndClean(ref failedToLoadCallback, errorInfo);
                OnFailedToLoadAdEvent?.Invoke(errorInfo);
            });
        }

        private void OnAdLoaded(string unit, MaxSdkBase.AdInfo info)
        {
            IsLoading = false;
            VLog.Log($"Advertising: MaxInterstitialAd Loaded: {Id}");
            var adsInfo = new AdsInfo(info);
            ExcuteCallbackOnMainThread(() =>
            {
                Common.CallActionAndClean(ref loadedCallback, adsInfo);
                OnLoadedAdEvent?.Invoke(adsInfo);
            });
        }
#endif

        #endregion
    }
}