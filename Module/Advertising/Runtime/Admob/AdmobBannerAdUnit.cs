using System;
using System.Collections.Generic;
using UnityEngine;
using VirtueSky.Core;
#if VIRTUESKY_ADS && VIRTUESKY_ADMOB
using GoogleMobileAds.Api;
#endif
#if VIRTUESKY_ADS && VIRTUESKY_ADMOB && VIRTUESKY_ADMOB_BANNER_REFRESH
using GoogleMobileAds.BannerRefresh;
#endif
using System.Collections;
using VirtueSky.Misc;
using VirtueSky.Tracking;
using VirtueSky.Utils;

namespace VirtueSky.Ads
{
    [Serializable]
    public class AdmobBannerAdUnit : AdUnit
    {
        public AdsSize size = AdsSize.Adaptive;
        public AdsPosition position = AdsPosition.Bottom;
        public bool useCollapsible;
        public bool useTestId;
        public bool useAutoRefresh;
        public int autoRefreshBufferSize = 2;
        public int autoRefreshRateInSeconds = 60;
        public List<AdmobBannerRefreshRateByAdSource> autoRefreshRateByAdSource = new();
#if VIRTUESKY_ADS && VIRTUESKY_ADMOB
        private BannerView _bannerView;
        private ResponseInfo adsInfo = null;
#endif
#if VIRTUESKY_ADS && VIRTUESKY_ADMOB && VIRTUESKY_ADMOB_BANNER_REFRESH
        private BannerRefreshView _bannerRefreshView;
#endif
        private AdsInfo cacheAdInfo;
        private const float BannerReloadInitialDelay = 5f;
        private const float BannerReloadMaxDelay = 60f;
        private IEnumerator _reload;
        private int _bannerReloadAttempt;
        private bool _isBannerShowing;
        private bool _previousBannerShowStatus;
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
            CancelBannerReload();
            DestroyBannerView();
            IsLoading = true;
            OnRequestAdEvent?.Invoke();
            VLog.Log($"Advertising: Load BannerAd: {Id}");

            if (UseBannerRefreshView())
            {
#if VIRTUESKY_ADS && VIRTUESKY_ADMOB && VIRTUESKY_ADMOB_BANNER_REFRESH
                LoadBannerRefreshView();
                return;
#endif
            }
            else if (useAutoRefresh)
            {
                VLog.LogWarning("Advertising: AdMob banner auto refresh requested but VIRTUESKY_ADMOB_BANNER_REFRESH is not defined. Fallback to normal BannerView.");
            }

            _bannerView = new BannerView(Id, ConvertSize(), ConvertPosition());
            _bannerView.OnAdFullScreenContentClosed += OnAdClosed;
            _bannerView.OnBannerAdLoadFailed += OnAdFailedToLoad;
            _bannerView.OnBannerAdLoaded += OnAdLoaded;
            _bannerView.OnAdFullScreenContentOpened += OnAdOpening;
            _bannerView.OnAdPaid += OnAdPaided;
            _bannerView.OnAdClicked += OnAdClicked;
            _bannerView.LoadAd(CreateAdRequest());

#endif
        }

        public bool IsCollapsible()
        {
#if VIRTUESKY_ADS && VIRTUESKY_ADMOB
            if (UseBannerRefreshView()) return false;
            if (_bannerView == null) return false;
            return _bannerView.IsCollapsible();
#else
            return false;
#endif
        }

        void OnWaitAppOpenClosed()
        {
            if (_previousBannerShowStatus)
            {
                _previousBannerShowStatus = false;
                Show();
            }
        }

        void OnWaitAppOpenDisplayed()
        {
            _previousBannerShowStatus = _isBannerShowing;
            if (_isBannerShowing) HideBanner();
        }

        public override bool IsReady()
        {
#if VIRTUESKY_ADS && VIRTUESKY_ADMOB
            if (UseBannerRefreshView())
            {
#if VIRTUESKY_ADS && VIRTUESKY_ADMOB && VIRTUESKY_ADMOB_BANNER_REFRESH
                return _bannerRefreshView != null;
#endif
            }

            return _bannerView != null;
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
            _isBannerShowing = true;
            IsShowing = true;
            AdStatic.waitAppOpenClosedAction = OnWaitAppOpenClosed;
            AdStatic.waitAppOpenDisplayedAction = OnWaitAppOpenDisplayed;
            if (UseBannerRefreshView())
            {
#if VIRTUESKY_ADS && VIRTUESKY_ADMOB && VIRTUESKY_ADMOB_BANNER_REFRESH
                if (_bannerRefreshView == null)
                {
                    Load();
                }

                _bannerRefreshView?.Show();
#endif
                return;
            }

            if (_bannerView == null)
            {
                Load();
            }

            _bannerView?.Show();
#endif
        }

        public override void Destroy()
        {
#if VIRTUESKY_ADS && VIRTUESKY_ADMOB
            ResetBannerReload();
            DestroyBannerView();
#endif
        }


        private void DestroyBannerView()
        {
#if VIRTUESKY_ADS && VIRTUESKY_ADMOB
            _isBannerShowing = false;
            IsShowing = false;
            AdStatic.waitAppOpenClosedAction = null;
            AdStatic.waitAppOpenDisplayedAction = null;
            if (_bannerView != null)
            {
                _bannerView.Destroy();
                _bannerView = null;
            }
#if VIRTUESKY_ADS && VIRTUESKY_ADMOB && VIRTUESKY_ADMOB_BANNER_REFRESH
            if (_bannerRefreshView != null)
            {
                if (!_bannerRefreshView.IsDestroyed)
                {
                    _bannerRefreshView.Destroy();
                }
                _bannerRefreshView = null;
            }
#endif
#endif
        }


        public override void HideBanner()
        {
            base.HideBanner();
#if VIRTUESKY_ADS && VIRTUESKY_ADMOB
            _isBannerShowing = false;
            IsShowing = false;
            if (UseBannerRefreshView())
            {
#if VIRTUESKY_ADS && VIRTUESKY_ADMOB && VIRTUESKY_ADMOB_BANNER_REFRESH
                _bannerRefreshView?.Hide();
#endif
                return;
            }

            if (_bannerView != null) _bannerView.Hide();
#endif
        }

        #region Fun Callback

#if VIRTUESKY_ADS && VIRTUESKY_ADMOB
        private bool UseBannerRefreshView()
        {
#if VIRTUESKY_ADMOB_BANNER_REFRESH
            return useAutoRefresh;
#else
            return false;
#endif
        }

        private AdRequest CreateAdRequest()
        {
            var adRequest = new AdRequest();
            if (useCollapsible)
            {
                adRequest.Extras.Add("collapsible", ConvertPlacementCollapsible());
            }

            return adRequest;
        }

#if VIRTUESKY_ADMOB_BANNER_REFRESH
        private void LoadBannerRefreshView()
        {
            var configuration = new BannerRefreshConfiguration
            {
                AdsBufferSize = autoRefreshBufferSize,
                DefaultRefreshRateInSeconds = autoRefreshRateInSeconds,
                AdSourceRefreshRatesInSeconds = ConvertAdSourceRefreshRates()
            };

            _bannerRefreshView = new BannerRefreshView(Id, ConvertSize(), ConvertPosition(), configuration);
            _bannerRefreshView.OnAdFullScreenContentClosed += OnAdClosed;
            _bannerRefreshView.OnBannerAdLoadFailed += OnAdFailedToLoad;
            _bannerRefreshView.OnBannerAdLoaded += OnAdLoaded;
            _bannerRefreshView.OnAdFullScreenContentOpened += OnAdOpening;
            _bannerRefreshView.OnAdPaid += OnAdPaided;
            _bannerRefreshView.OnAdClicked += OnAdClicked;
            _bannerRefreshView.Hide();
            _bannerRefreshView.LoadAd(CreateAdRequest());
        }

        private Dictionary<string, int> ConvertAdSourceRefreshRates()
        {
            var adSourceRefreshRates = new Dictionary<string, int>();
            if (autoRefreshRateByAdSource == null) return adSourceRefreshRates;

            foreach (var item in autoRefreshRateByAdSource)
            {
                if (item == null || string.IsNullOrEmpty(item.adSourceId)) continue;
                adSourceRefreshRates[item.adSourceId] = item.refreshRateInSeconds;
            }

            return adSourceRefreshRates;
        }
#endif

        public AdSize ConvertSize()
        {
            switch (size)
            {
                case AdsSize.Adaptive:
                    return AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(
                        AdSize.FullWidth);
                case AdsSize.MediumRectangle: return AdSize.MediumRectangle;
                case AdsSize.Leaderboard: return AdSize.Leaderboard;
                case AdsSize.IABBanner: return AdSize.IABBanner;
                //case BannerSize.SmartBanner: return AdSize.SmartBanner;
                default: return AdSize.Banner;
            }
        }

        private void OnAdClicked()
        {
            VLog.Log($"Advertising: BannerAd Clicked: {Id}");
            Common.CallActionAndClean(ref clickedCallback, cacheAdInfo);
            OnClickedAdEvent?.Invoke(cacheAdInfo);
        }

        public AdPosition ConvertPosition()
        {
            switch (position)
            {
                case AdsPosition.Top: return AdPosition.Top;
                case AdsPosition.Bottom: return AdPosition.Bottom;
                case AdsPosition.TopLeft: return AdPosition.TopLeft;
                case AdsPosition.TopRight: return AdPosition.TopRight;
                case AdsPosition.BottomLeft: return AdPosition.BottomLeft;
                case AdsPosition.BottomRight: return AdPosition.BottomRight;
                default: return AdPosition.Bottom;
            }
        }

        public string ConvertPlacementCollapsible()
        {
            if (position == AdsPosition.Top)
            {
                return "top";
            }
            else if (position == AdsPosition.Bottom)
            {
                return "bottom";
            }

            return "bottom";
        }

        private void OnAdPaided(AdValue value)
        {
            if (cacheAdInfo == null)
            {
                CacheAdsInfo();
            }

            cacheAdInfo.Revenue = value.Value / 1000000f;
            cacheAdInfo.Precision = value.Precision.ToString();
            VLog.Log($"Advertising: BannerAd Paid: {Id}, revenue: {cacheAdInfo.Revenue}, precision: {cacheAdInfo.Precision}");
            paidedCallback?.Invoke(cacheAdInfo);
        }

        private void CacheAdsInfo()
        {
            if (cacheAdInfo != null) cacheAdInfo = null;
            adsInfo = GetResponseInfo();
            cacheAdInfo = new AdsInfo(AdMediation.Admob);
            cacheAdInfo.AdFormat = "BannerAd";
            cacheAdInfo.AdUnitId = Id;
            cacheAdInfo.AdNetwork = adsInfo?.GetLoadedAdapterResponseInfo()?.AdSourceName ?? "";
        }

        private void OnAdOpening()
        {
            VLog.Log($"Advertising: BannerAd Displayed: {Id}");
            Common.CallActionAndClean(ref displayedCallback, cacheAdInfo);
            OnDisplayedAdEvent?.Invoke(cacheAdInfo);
        }

        private void OnAdLoaded()
        {
            IsLoading = false;
            ResetBannerReload();
            adsInfo = GetResponseInfo();
            CacheAdsInfo();
            VLog.Log($"Advertising: BannerAd Loaded: {Id}");
            Common.CallActionAndClean(ref loadedCallback, cacheAdInfo);
            OnLoadedAdEvent?.Invoke(cacheAdInfo);
        }

        private ResponseInfo GetResponseInfo()
        {
            if (UseBannerRefreshView())
            {
#if VIRTUESKY_ADMOB_BANNER_REFRESH
                return _bannerRefreshView?.GetResponseInfo();
#endif
            }

            return _bannerView?.GetResponseInfo();
        }

        private void OnAdFailedToLoad(LoadAdError error)
        {
            IsLoading = false;
            var errorInfo = new AdsError(error);
            VLog.LogWarning($"Advertising: BannerAd FailedToLoad: {Id}, errorCode: {errorInfo.ErrorCode}, errorMessage: {errorInfo.ErrorMessage}");
            ExcuteCallbackOnMainThread(() =>
            {
                Common.CallActionAndClean(ref failedToLoadCallback, errorInfo);
                OnFailedToLoadAdEvent?.Invoke(errorInfo);
            });

            if (!UseBannerRefreshView())
            {
                ScheduleBannerReload();
            }
        }

        private void ScheduleBannerReload()
        {
            CancelBannerReload();
            var delay = GetNextBannerReloadDelay();
            _reload = DelayBannerReload(delay);
            App.StartCoroutine(_reload);
        }

        private void OnAdClosed()
        {
            VLog.Log($"Advertising: BannerAd Closed: {Id}");
            ExcuteCallbackOnMainThread(() =>
            {
                Common.CallActionAndClean(ref closedCallback, cacheAdInfo);
                OnClosedAdEvent?.Invoke(cacheAdInfo);
            });
        }

        private float GetNextBannerReloadDelay()
        {
            var delay = BannerReloadInitialDelay * Mathf.Pow(2f, _bannerReloadAttempt);
            _bannerReloadAttempt++;
            return Mathf.Min(delay, BannerReloadMaxDelay);
        }

        private void ResetBannerReload()
        {
            CancelBannerReload();
            _bannerReloadAttempt = 0;
        }

        private void CancelBannerReload()
        {
            if (_reload == null) return;
            App.StopCoroutine(_reload);
            _reload = null;
        }

        private IEnumerator DelayBannerReload(float delay)
        {
            yield return new WaitForSeconds(delay);
            _reload = null;
            Load();
        }
#endif

        #endregion

        void GetUnitTest()
        {
#if UNITY_ANDROID
            androidId = "ca-app-pub-3940256099942544/6300978111";
#elif UNITY_IOS
            iOSId = "ca-app-pub-3940256099942544/2934735716";
#endif
        }
    }

    [Serializable]
    public class AdmobBannerRefreshRateByAdSource
    {
        public string adSourceId;
        public int refreshRateInSeconds = 60;
    }
}
