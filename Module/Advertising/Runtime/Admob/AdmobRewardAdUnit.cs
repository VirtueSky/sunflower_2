using System;
using VirtueSky.Core;
#if VIRTUESKY_ADS && VIRTUESKY_ADMOB
using GoogleMobileAds.Api;
#endif
using VirtueSky.Misc;
using VirtueSky.Tracking;
using VirtueSky.Utils;


namespace VirtueSky.Ads
{
    [Serializable]
    public class AdmobRewardAdUnit : AdUnit
    {
        public bool useTestId;
        public bool usePreload;
        [UnityEngine.Min(0)] public int preloadBufferSize = 2;
        [NonSerialized] internal Action completedCallback;
        [NonSerialized] internal Action skippedCallback;
        [NonSerialized] internal Action receivedRewardCallback;

#if VIRTUESKY_ADS && VIRTUESKY_ADMOB
        private RewardedAd _rewardedAd;
        private RewardedAd _preloadedRewardedAd;
        private ResponseInfo adsInfo = null;
        private bool isPreloadStarted;
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

            if (usePreload)
            {
                StartPreload();
                return;
            }

            DestroyLoadedAd();
            IsLoading = true;
            VLog.Log($"Advertising: Load RewardedAd: {Id}");
            OnRequestAdEvent?.Invoke();
            RewardedAd.Load(Id, new AdRequest(), AdLoadCallback);
#endif
        }

        public override bool IsReady()
        {
#if VIRTUESKY_ADS && VIRTUESKY_ADMOB
            if (usePreload)
            {
                return RewardedAdPreloader.IsAdAvailable(Id);
            }

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
            if (usePreload)
            {
                _preloadedRewardedAd = RewardedAdPreloader.DequeueAd(Id);
                if (_preloadedRewardedAd == null)
                {
                    VLog.LogWarning($"Advertising: RewardedAd preload dequeue failed, ad is not ready: {Id}");
                    return;
                }

                adsInfo = _preloadedRewardedAd.GetResponseInfo();
                BindAdEvents(_preloadedRewardedAd);
                CacheAdsInfo();
                if (cacheAdInfo != null)
                {
                    cacheAdInfo.Placement = placement;
                }

                VLog.Log($"Advertising: RewardedAd show: {Id}");
                _preloadedRewardedAd.Show(UserRewardEarnedCallback);
                return;
            }

            VLog.Log($"Advertising: RewardedAd show: {Id}");
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
            if (usePreload)
            {
                DestroyPreloadedAd();
                return;
            }

            DestroyLoadedAd();
#endif
            IsLoading = false;
            IsShowing = false;
        }

#if VIRTUESKY_ADS && VIRTUESKY_ADMOB
        private void DestroyLoadedAd()
        {
            if (_rewardedAd == null) return;
            _rewardedAd.Destroy();
            _rewardedAd = null;
            IsEarnRewarded = false;
        }

        private void DestroyPreloadedAd()
        {
            IsLoading = false;
            IsShowing = false;
            if (_preloadedRewardedAd == null) return;
            _preloadedRewardedAd.Destroy();
            _preloadedRewardedAd = null;
            IsEarnRewarded = false;
        }
#endif

        private void ResetFinalizeCloseHandle()
        {
            App.CancelDelay(_finalizeCloseHandle);
            _finalizeCloseHandle = null;
        }

        #region Fun Callback

#if VIRTUESKY_ADS && VIRTUESKY_ADMOB
        private void StartPreload()
        {
            if (isPreloadStarted) return;

            isPreloadStarted = true;
            IsLoading = true;
            VLog.Log($"Advertising: Preload RewardedAd: {Id}");
            OnRequestAdEvent?.Invoke();

            var config = new PreloadConfiguration
            {
                AdUnitId = Id,
                Request = new AdRequest(),
                BufferSize = (uint)Math.Max(1, preloadBufferSize)
            };

            RewardedAdPreloader.Preload(
                Id,
                config,
                onAdPreloaded: (adUnitId, responseInfo) =>
                {
                    adsInfo = responseInfo;
                    CacheAdsInfo();
                    OnAdLoaded();
                },
                onAdFailedToPreload: (adUnitId, error) =>
                {
                    OnAdFailedToLoad(error);
                },
                onAdsExhausted: adUnitId =>
                {
                    VLog.LogWarning($"Advertising: RewardedAd preload exhausted: {adUnitId}");
                }
            );
        }

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
            BindAdEvents(_rewardedAd);
            CacheAdsInfo();
            OnAdLoaded();
        }

        private void BindAdEvents(RewardedAd ad)
        {
            ad.OnAdFullScreenContentClosed += OnAdClosed;
            ad.OnAdFullScreenContentFailed += OnAdFailedToShow;
            ad.OnAdFullScreenContentOpened += OnAdOpening;
            ad.OnAdPaid += OnAdPaided;
            ad.OnAdClicked += OnAdClicked;
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
            VLog.Log($"Advertising: RewardedAd Clicked: {Id}");
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
            VLog.Log($"Advertising: RewardedAd Paid: {Id}, revenue: {cacheAdInfo.Revenue}, precision: {cacheAdInfo.Precision}");
            paidedCallback?.Invoke(cacheAdInfo);
        }

        private void OnAdOpening()
        {
            VLog.Log($"Advertising: RewardedAd Displayed: {Id}");
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
            VLog.LogWarning(
                $"Advertising: RewardedAd FailedToDisplay: {Id}, errorCode: {errorInfo.ErrorCode}, errorMessage: {errorInfo.ErrorMessage}");
            ExcuteCallbackOnMainThread(() =>
            {
                Common.CallActionAndClean(ref failedToDisplayCallback, errorInfo);
                OnFailedToDisplayAdEvent?.Invoke(errorInfo);
            });

            Destroy();
            if (!usePreload) Load();
        }

        private void OnAdClosed()
        {
            VLog.Log($"Advertising: RewardedAd Closed: {Id}");
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
            VLog.Log($"Advertising: RewardedAd Loaded: {Id}");
            ExcuteCallbackOnMainThread(() =>
            {
                Common.CallActionAndClean(ref loadedCallback, cacheAdInfo);
                OnLoadedAdEvent?.Invoke(cacheAdInfo);
            });
        }

        private void OnAdFailedToLoad(AdError error)
        {
            IsLoading = false;
            var errorInfo = new AdsError(error);
            VLog.LogWarning($"Advertising: RewardedAd FailedToLoad: {Id}, errorCode: {errorInfo.ErrorCode}, errorMessage: {errorInfo.ErrorMessage}");
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
                if (!usePreload) Load();
                return;
            }

            ExcuteCallbackOnMainThread(() => { Common.CallActionAndClean(ref skippedCallback); });
            ResetFinalizeCloseHandle();
            Destroy();
            if (!usePreload) Load();
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
