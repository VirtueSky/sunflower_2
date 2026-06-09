using System;
using VirtueSky.Core;
#if VIRTUESKY_ADS && VIRTUESKY_ADMOB
using GoogleMobileAds.Api;
#endif
using VirtueSky.Misc;
using VirtueSky.Tracking;


namespace VirtueSky.Ads
{
    [Serializable]
    public class AdmobRewardAdUnit : AdUnit
    {
        public bool useTestId;
        [NonSerialized] internal Action completedCallback;
        [NonSerialized] internal Action skippedCallback;
        [NonSerialized] internal Action receivedRewardCallback;

#if VIRTUESKY_ADS && VIRTUESKY_ADMOB
        private RewardedAd _rewardedAd;
        private ResponseInfo adsInfo = null;
#endif
        private const float FinalizeCloseDelay = 0.2f;
        private DelayHandle _finalizeCloseHandle;
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
            RewardedAd.Load(Id, new AdRequest(), AdLoadCallback);
#endif
        }

        public override bool IsReady()
        {
#if VIRTUESKY_ADS && VIRTUESKY_ADMOB
            return _rewardedAd != null && _rewardedAd.CanShowAd();
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
            _rewardedAd.Show(UserRewardEarnedCallback);
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
            if (_rewardedAd == null) return;
            _rewardedAd.Destroy();
            _rewardedAd = null;
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
        private void AdLoadCallback(RewardedAd ad, LoadAdError error)
        {
            // if error is not null, the load request failed.
            if (error != null || ad == null)
            {
                OnAdFailedToLoad(error);
                return;
            }

            _rewardedAd = ad;
            adsInfo = ad.GetResponseInfo();
            _rewardedAd.OnAdFullScreenContentClosed += OnAdClosed;
            _rewardedAd.OnAdFullScreenContentFailed += OnAdFailedToShow;
            _rewardedAd.OnAdFullScreenContentOpened += OnAdOpening;
            _rewardedAd.OnAdPaid += OnAdPaided;
            _rewardedAd.OnAdClicked += OnAdClicked;
            CacheAdsInfo();
            OnAdLoaded();
        }

        private void CacheAdsInfo()
        {
            if (cacheAdInfo != null) cacheAdInfo = null;
            cacheAdInfo = new AdsInfo(AdMediation.Admob);
            cacheAdInfo.AdFormat = "RewardedAd";
            cacheAdInfo.AdUnitId = Id;
            cacheAdInfo.AdNetwork = adsInfo?.GetLoadedAdapterResponseInfo()?.AdSourceName ?? "";
        }

        private void OnAdClicked()
        {
            ExcuteCallbackOnMainThread(() =>
            {
                Common.CallActionAndClean(ref clickedCallback, cacheAdInfo);
                OnClickedAdEvent?.Invoke(cacheAdInfo);
            });
        }

        private void OnAdPaided(AdValue value)
        {
            cacheAdInfo.Revenue = value.Value / 1000000f;
            cacheAdInfo.Precision = value.Precision.ToString();
            paidedCallback?.Invoke(cacheAdInfo);
        }

        private void OnAdOpening()
        {
            AdStatic.IsShowingAd = true;
            IsShowing = true;
            ExcuteCallbackOnMainThread(() =>
            {
                Common.CallActionAndClean(ref displayedCallback, cacheAdInfo);
                OnDisplayedAdEvent?.Invoke(cacheAdInfo);
            });
        }

        private void OnAdFailedToShow(AdError obj)
        {
            var errorInfo = new AdsError(obj);
            ExcuteCallbackOnMainThread(() =>
            {
                Common.CallActionAndClean(ref failedToDisplayCallback, errorInfo);
                OnFailedToDisplayAdEvent?.Invoke(errorInfo);
            });

            Destroy();
            Load();
        }

        private void OnAdClosed()
        {
            AdStatic.IsShowingAd = false;
            ExcuteCallbackOnMainThread(() =>
            {
                Common.CallActionAndClean(ref closedCallback, cacheAdInfo);
                OnClosedAdEvent?.Invoke(cacheAdInfo);
            });
            App.CancelDelay(_finalizeCloseHandle);
            _finalizeCloseHandle = App.Delay(FinalizeCloseDelay, FinalizeClose);
        }

        private void OnAdLoaded()
        {
            IsLoading = false;
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
            ExcuteCallbackOnMainThread(() =>
            {
                Common.CallActionAndClean(ref failedToLoadCallback, errorInfo);
                OnFailedToLoadAdEvent?.Invoke(errorInfo);
            });
        }

        private void UserRewardEarnedCallback(Reward reward)
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
            androidId = "ca-app-pub-3940256099942544/5224354917";
#elif UNITY_IOS
            iOSId = "ca-app-pub-3940256099942544/1712485313";
#endif
        }
    }
}