using System;
using UnityEngine;
#if VIRTUESKY_ADS && VIRTUESKY_ADMOB
using GoogleMobileAds.Api;
using VirtueSky.Tracking;
#endif
using VirtueSky.Misc;
using VirtueSky.Utils;

namespace VirtueSky.Ads
{
    [Serializable]
    public class AdmobInterstitialAdUnit : AdUnit
    {
        public bool useTestId;
        [NonSerialized] internal Action completedCallback;
#if VIRTUESKY_ADS && VIRTUESKY_ADMOB
        private InterstitialAd _interstitialAd;
        private ResponseInfo adsInfo = null;
#endif
        private AdsInfo cacheAdInfo;
        private string placement = "";
        public override bool IsShowing { get; internal set; }
        public override bool IsLoading { get; internal set; }

        public override void Init()
        {
            if (useTestId)
            {
                GetUnitTest();
            }
#if VIRTUESKY_ADS && VIRTUESKY_ADMOB
            if (AdStatic.IsRemoveAd || string.IsNullOrEmpty(Id)) return;
            paidedCallback += TrackRevenue;
#endif
        }

        public override void Load()
        {
#if VIRTUESKY_ADS && VIRTUESKY_ADMOB
            if (AdStatic.IsRemoveAd || string.IsNullOrEmpty(Id)) return;

            Destroy();
            IsLoading = true;
            VLog.Log($"Advertising: Load InterstitialAd: {Id}");
            InterstitialAd.Load(Id, new AdRequest(), AdLoadCallback);

#endif
        }

        public override bool IsReady()
        {
#if VIRTUESKY_ADS && VIRTUESKY_ADMOB
            return _interstitialAd != null && _interstitialAd.CanShowAd();
#else
            return false;
#endif
        }

        protected override void ShowImpl(string placement = "")
        {
            this.placement = placement;
            if (cacheAdInfo != null)
            {
                cacheAdInfo.Placement = placement;
            }
#if VIRTUESKY_ADS && VIRTUESKY_ADMOB
            VLog.Log($"Advertising: InterstitialAd show: {Id}");
            _interstitialAd.Show();
#endif
        }

        protected override void ResetChainCallback()
        {
            base.ResetChainCallback();
            completedCallback = null;
        }

        public override void Destroy()
        {
#if VIRTUESKY_ADS && VIRTUESKY_ADMOB
            if (_interstitialAd == null) return;
            _interstitialAd.Destroy();
            _interstitialAd = null;
#endif
            IsLoading = false;
        }

        #region Fun Callback

#if VIRTUESKY_ADS && VIRTUESKY_ADMOB
        private void AdLoadCallback(InterstitialAd ad, LoadAdError error)
        {
            // if error is not null, the load request failed.
            if (error != null || ad == null)
            {
                OnAdFailedToLoad(error);
                return;
            }

            _interstitialAd = ad;
            adsInfo = ad.GetResponseInfo();
            _interstitialAd.OnAdPaid += OnAdPaided;
            _interstitialAd.OnAdFullScreenContentClosed += OnAdClosed;
            _interstitialAd.OnAdFullScreenContentFailed += OnAdFailedToShow;
            _interstitialAd.OnAdFullScreenContentOpened += OnAdOpening;
            _interstitialAd.OnAdClicked += OnAdClicked;
            CacheAdsInfo();
            OnAdLoaded();
        }

        private void OnAdClicked()
        {
            VLog.Log($"Advertising: InterstitialAd Clicked: {Id}");
            ExcuteCallbackOnMainThread(() =>
            {
                Common.CallActionAndClean(ref clickedCallback, cacheAdInfo);
                OnClickedAdEvent?.Invoke(cacheAdInfo);
            });
        }

        private void OnAdOpening()
        {
            VLog.Log($"Advertising: InterstitialAd Displayed: {Id}");
            AdStatic.IsShowingAd = true;
            IsShowing = true;
            ExcuteCallbackOnMainThread(() =>
            {
                Common.CallActionAndClean(ref displayedCallback, cacheAdInfo);
                OnDisplayedAdEvent?.Invoke(cacheAdInfo);
            });
        }

        private void OnAdFailedToShow(AdError error)
        {
            var errorInfo = new AdsError(error);
            VLog.LogWarning($"Advertising: InterstitialAd FailedToDisplay: {Id}, errorCode: {errorInfo.ErrorCode}, errorMessage: {errorInfo.ErrorMessage}");
            ExcuteCallbackOnMainThread(() =>
            {
                Common.CallActionAndClean(ref failedToDisplayCallback, errorInfo);
                OnFailedToDisplayAdEvent?.Invoke(errorInfo);
            });

            IsShowing = false;
            Destroy();
            Load();
        }

        private void OnAdClosed()
        {
            VLog.Log($"Advertising: InterstitialAd Closed: {Id}");
            AdStatic.IsShowingAd = false;
            ExcuteCallbackOnMainThread(() =>
            {
                Common.CallActionAndClean(ref completedCallback);
                Common.CallActionAndClean(ref closedCallback, cacheAdInfo);
                OnClosedAdEvent?.Invoke(cacheAdInfo);
            });
            Destroy();
            IsShowing = false;
            Load();
        }

        private void OnAdPaided(AdValue value)
        {
            cacheAdInfo.Revenue = value.Value / 1000000f;
            cacheAdInfo.Precision = value.Precision.ToString();
            VLog.Log($"Advertising: InterstitialAd Paid: {Id}, revenue: {cacheAdInfo.Revenue}, precision: {cacheAdInfo.Precision}");
            paidedCallback?.Invoke(cacheAdInfo);
        }

        private void CacheAdsInfo()
        {
            if (cacheAdInfo != null) cacheAdInfo = null;
            cacheAdInfo = new AdsInfo(AdMediation.Admob);
            cacheAdInfo.AdFormat = "InterstitialAd";
            cacheAdInfo.AdUnitId = Id;
            cacheAdInfo.AdNetwork = adsInfo?.GetLoadedAdapterResponseInfo()?.AdSourceName ?? "";
        }

        private void OnAdLoaded()
        {
            IsLoading = false;
            VLog.Log($"Advertising: InterstitialAd Loaded: {Id}");
            ExcuteCallbackOnMainThread(() =>
            {
                Common.CallActionAndClean(ref loadedCallback, cacheAdInfo);
                OnLoadAdEvent?.Invoke(cacheAdInfo);
            });
        }

        private void OnAdFailedToLoad(LoadAdError error)
        {
            IsLoading = false;
            var errorInfo = new AdsError(error);
            VLog.LogWarning($"Advertising: InterstitialAd FailedToLoad: {Id}, errorCode: {errorInfo.ErrorCode}, errorMessage: {errorInfo.ErrorMessage}");
            ExcuteCallbackOnMainThread(() =>
            {
                Common.CallActionAndClean(ref failedToLoadCallback, errorInfo);
                OnFailedToLoadAdEvent?.Invoke(errorInfo);
            });
        }
#endif

        #endregion

        void GetUnitTest()
        {
#if UNITY_ANDROID
            androidId = "ca-app-pub-3940256099942544/1033173712";
#elif UNITY_IOS
            iOSId = "ca-app-pub-3940256099942544/4411468910";
#endif
        }
    }
}