using System;
using UnityEngine;
using VirtueSky.Misc;
using VirtueSky.Tracking;

namespace VirtueSky.Ads
{
    [Serializable]
    public class MaxAppOpenAdUnit : AdUnit
    {
        [Tooltip("Automatically show AppOpenAd when app status is changed")]
        public bool autoShow = false;

        [Tooltip("Time between closing the previous full-screen ad and starting to show the app open ad - in seconds")]
        public float timeBetweenFullScreenAd = 2f;

        public override void Init()
        {
#if VIRTUESKY_ADS && VIRTUESKY_APPLOVIN
            if (AdStatic.IsRemoveAd || string.IsNullOrEmpty(Id)) return;
            paidedCallback += AppTracking.TrackRevenue;
            MaxSdkCallbacks.AppOpen.OnAdDisplayedEvent += OnAdDisplayed;
            MaxSdkCallbacks.AppOpen.OnAdHiddenEvent += OnAdHidden;
            MaxSdkCallbacks.AppOpen.OnAdLoadedEvent += OnAdLoaded;
            MaxSdkCallbacks.AppOpen.OnAdDisplayFailedEvent += OnAdDisplayFailed;
            MaxSdkCallbacks.AppOpen.OnAdLoadFailedEvent += OnAdLoadFailed;
            MaxSdkCallbacks.AppOpen.OnAdRevenuePaidEvent += OnAdRevenuePaid;
            MaxSdkCallbacks.AppOpen.OnAdClickedEvent += OnAdClicked;
#endif
        }

        public override void Load()
        {
#if VIRTUESKY_ADS && VIRTUESKY_APPLOVIN
            if (AdStatic.IsRemoveAd || string.IsNullOrEmpty(Id)) return;
            MaxSdk.LoadAppOpenAd(Id);
#endif
        }

        public override bool IsReady()
        {
#if VIRTUESKY_ADS && VIRTUESKY_APPLOVIN
            return !string.IsNullOrEmpty(Id) && MaxSdk.IsAppOpenAdReady(Id) &&
                   (DateTime.Now - AdStatic.AdClosingTime).TotalSeconds > timeBetweenFullScreenAd;
#else
            return false;
#endif
        }

        protected override void ShowImpl()
        {
#if VIRTUESKY_ADS && VIRTUESKY_APPLOVIN
            MaxSdk.ShowAppOpenAd(Id);
#endif
        }

        public override void Destroy()
        {
        }

        #region Func Callback

#if VIRTUESKY_ADS && VIRTUESKY_APPLOVIN
        private void OnAdLoaded(string unit, MaxSdkBase.AdInfo info)
        {
            Common.CallActionAndClean(ref loadedCallback);
            OnLoadAdEvent?.Invoke();
        }

        private void OnAdRevenuePaid(string unit, MaxSdkBase.AdInfo info)
        {
            paidedCallback?.Invoke(info.Revenue,
                info.NetworkName,
                unit,
                info.AdFormat, AdNetwork.Max.ToString());
        }

        private void OnAdLoadFailed(string unit, MaxSdkBase.ErrorInfo info)
        {
            Common.CallActionAndClean(ref failedToLoadCallback);
            OnFailedToLoadAdEvent?.Invoke(info.Message);
        }

        private void OnAdClicked(string arg1, MaxSdkBase.AdInfo arg2)
        {
            Common.CallActionAndClean(ref clickedCallback);
            OnClickedAdEvent?.Invoke();
        }

        private void OnAdDisplayFailed(string unit, MaxSdkBase.ErrorInfo errorInfo,
            MaxSdkBase.AdInfo info)
        {
            Common.CallActionAndClean(ref failedToDisplayCallback);
            OnFailedToDisplayAdEvent?.Invoke(errorInfo.Message);
        }

        private void OnAdHidden(string unit, MaxSdkBase.AdInfo info)
        {
            AdStatic.waitAppOpenClosedAction?.Invoke();
            AdStatic.IsShowingAd = false;
            Common.CallActionAndClean(ref closedCallback);
            OnClosedAdEvent?.Invoke();

            if (!string.IsNullOrEmpty(Id)) MaxSdk.LoadAppOpenAd(Id);
        }

        private void OnAdDisplayed(string unit, MaxSdkBase.AdInfo info)
        {
            AdStatic.waitAppOpenDisplayedAction?.Invoke();
            AdStatic.IsShowingAd = true;
            Common.CallActionAndClean(ref displayedCallback);
            OnDisplayedAdEvent?.Invoke();
        }
#endif

        #endregion
    }
}