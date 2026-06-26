using System;
using VirtueSky.Core;
#if VIRTUESKY_ADMOB && VIRTUESKY_ADS
using GoogleMobileAds.Api;
using VirtueSky.Tracking;
#endif
using VirtueSky.Misc;
using VirtueSky.Utils;


namespace VirtueSky.Ads
{
    [Serializable]
    public class AdmobRewardedInterstitialAdUnit : AdUnit
    {
        public bool useTestId;
        [NonSerialized] internal Action completedCallback;
        [NonSerialized] internal Action skippedCallback;
        [NonSerialized] internal Action receivedRewardCallback;
#if VIRTUESKY_ADS && VIRTUESKY_ADMOB
        private RewardedInterstitialAd _rewardedInterstitialAd;
        private ResponseInfo adsInfo = null;
#endif
        private AdsInfo cacheAdInfo;
        private string placement = "";
        private const float FinalizeCloseDelay = 0.2f;
        private DelayHandle _finalizeCloseHandle;
        public override bool IsShowing { get; internal set; }
        public override bool IsLoading { get; internal set; }

        public override void Init()
        {
            if (useTestId)
            {
                GetUnitTest();
            }
#if VIRTUESKY_ADS && VIRTUESKY_ADMOB
            if (string.IsNullOrEmpty(Id)) return;
            paidedCallback += TrackRevenue;
#endif
        }

        public bool IsEarnRewarded { get; private set; }

        public override void Load()
        {
#if VIRTUESKY_ADS && VIRTUESKY_ADMOB
            if (string.IsNullOrEmpty(Id)) return;
            Destroy();
            IsLoading = true;
            VLog.Log($"Advertising: Load RewardedInterstitialAd: {Id}");
            RewardedInterstitialAd.Load(Id, new AdRequest(), OnAdLoadCallback);
#endif
        }

        public override bool IsReady()
        {
#if VIRTUESKY_ADS && VIRTUESKY_ADMOB
            return _rewardedInterstitialAd != null && _rewardedInterstitialAd.CanShowAd();
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
            VLog.Log($"Advertising: RewardedInterstitialAd show: {Id}");
            _rewardedInterstitialAd.Show(UserEarnedRewardCallback);
#endif
        }

        protected override void ResetChainCallback()
        {
            base.ResetChainCallback();
            completedCallback = null;
            skippedCallback = null;
            receivedRewardCallback = null;
            IsEarnRewarded = false;
        }

        public override AdUnit Show(string placement = "")
        {
            ResetChainCallback();
            if (!UnityEngine.Application.isMobilePlatform || string.IsNullOrEmpty(Id) || !IsReady())
                return this;
            ShowImpl(placement);
            return this;
        }

        public override void Destroy()
        {
#if VIRTUESKY_ADS && VIRTUESKY_ADMOB
            if (_rewardedInterstitialAd == null) return;
            _rewardedInterstitialAd.Destroy();
            _rewardedInterstitialAd = null;
            IsEarnRewarded = false;
#endif
            IsLoading = false;
            IsShowing = false;
        }

        private void ResetFinalizeCloseHandle()
        {
            App.CancelDelay(_finalizeCloseHandle);
            _finalizeCloseHandle = null;
        }

        #region Fun Callback

#if VIRTUESKY_ADS && VIRTUESKY_ADMOB
        private void OnAdLoadCallback(RewardedInterstitialAd ad, LoadAdError error)
        {
            // if error is not null, the load request failed.
            if (error != null || ad == null)
            {
                OnAdFailedToLoad(error);
                return;
            }

            _rewardedInterstitialAd = ad;
            adsInfo = ad.GetResponseInfo();
            _rewardedInterstitialAd.OnAdFullScreenContentClosed += OnAdClosed;
            _rewardedInterstitialAd.OnAdFullScreenContentOpened += OnAdOpening;
            _rewardedInterstitialAd.OnAdFullScreenContentFailed += OnAdFailedToShow;
            _rewardedInterstitialAd.OnAdPaid += OnAdPaided;
            _rewardedInterstitialAd.OnAdClicked += OnAdClicked;
            CacheAdsInfo();
            OnAdLoaded();
        }

        private void OnAdClicked()
        {
            VLog.Log($"Advertising: RewardedInterstitialAd Clicked: {Id}");
            ExcuteCallbackOnMainThread(() =>
            {
                Common.CallActionAndClean(ref clickedCallback, cacheAdInfo);
                OnClickedAdEvent?.Invoke(cacheAdInfo);
            });
        }

        private void OnAdFailedToLoad(LoadAdError error)
        {
            IsLoading = false;
            var errorInfo = new AdsError(error);
            VLog.LogWarning($"Advertising: RewardedInterstitialAd FailedToLoad: {Id}, errorCode: {errorInfo.ErrorCode}, errorMessage: {errorInfo.ErrorMessage}");
            ExcuteCallbackOnMainThread(() =>
            {
                Common.CallActionAndClean(ref failedToLoadCallback, errorInfo);
                OnFailedToLoadAdEvent?.Invoke(errorInfo);
            });
        }

        private void OnAdLoaded()
        {
            IsLoading = false;
            VLog.Log($"Advertising: RewardedInterstitialAd Loaded: {Id}");
            ExcuteCallbackOnMainThread(() =>
            {
                Common.CallActionAndClean(ref loadedCallback, cacheAdInfo);
                OnLoadAdEvent?.Invoke(cacheAdInfo);
            });
        }

        private void OnAdPaided(AdValue value)
        {
            cacheAdInfo.Revenue = value.Value / 1000000f;
            cacheAdInfo.Precision = value.Precision.ToString();
            VLog.Log($"Advertising: RewardedInterstitialAd Paid: {Id}, revenue: {cacheAdInfo.Revenue}, precision: {cacheAdInfo.Precision}");
            paidedCallback?.Invoke(cacheAdInfo);
        }

        private void CacheAdsInfo()
        {
            if (cacheAdInfo != null) cacheAdInfo = null;
            cacheAdInfo = new AdsInfo(AdMediation.Admob);
            cacheAdInfo.AdFormat = "RewardedInterstitialAd";
            cacheAdInfo.AdUnitId = Id;
            cacheAdInfo.AdNetwork = adsInfo?.GetLoadedAdapterResponseInfo()?.AdSourceName ?? "";
        }

        private void OnAdFailedToShow(AdError error)
        {
            var errorInfo = new AdsError(error);
            VLog.LogWarning($"Advertising: RewardedInterstitialAd FailedToDisplay: {Id}, errorCode: {errorInfo.ErrorCode}, errorMessage: {errorInfo.ErrorMessage}");
            ExcuteCallbackOnMainThread(() =>
            {
                Common.CallActionAndClean(ref failedToDisplayCallback, errorInfo);
                OnFailedToDisplayAdEvent?.Invoke(errorInfo);
            });

            Destroy();
            Load();
        }

        private void OnAdOpening()
        {
            VLog.Log($"Advertising: RewardedInterstitialAd Displayed: {Id}");
            AdStatic.IsShowingAd = true;
            IsShowing = true;
            ExcuteCallbackOnMainThread(() =>
            {
                Common.CallActionAndClean(ref displayedCallback, cacheAdInfo);
                OnDisplayedAdEvent?.Invoke(cacheAdInfo);
            });
        }

        private void OnAdClosed()
        {
            VLog.Log($"Advertising: RewardedInterstitialAd Closed: {Id}");
            AdStatic.IsShowingAd = false;
            IsShowing = false;
            ExcuteCallbackOnMainThread(() =>
            {
                Common.CallActionAndClean(ref closedCallback, cacheAdInfo);
                OnClosedAdEvent?.Invoke(cacheAdInfo);
            });

            App.CancelDelay(_finalizeCloseHandle);
            _finalizeCloseHandle = App.Delay(FinalizeCloseDelay, FinalizeClose);
        }

        private void UserEarnedRewardCallback(Reward reward)
        {
            IsEarnRewarded = true;
            ExcuteCallbackOnMainThread(() => { Common.CallActionAndClean(ref receivedRewardCallback); });
        }

        private void FinalizeClose()
        {
            _finalizeCloseHandle = null;
            if (IsEarnRewarded)
            {
                ExcuteCallbackOnMainThread(() => { Common.CallActionAndClean(ref completedCallback); });
                IsEarnRewarded = false;
                ResetFinalizeCloseHandle();
                Destroy();
                Load();
                return;
            }

            ExcuteCallbackOnMainThread(() => { Common.CallActionAndClean(ref skippedCallback); });
            ResetFinalizeCloseHandle();
            Destroy();
            Load();
        }
#endif

        #endregion

        void GetUnitTest()
        {
#if UNITY_ANDROID
            androidId = "ca-app-pub-3940256099942544/5354046379";
#elif UNITY_IOS
            iOSId = "ca-app-pub-3940256099942544/6978759866";
#endif
        }
    }
}