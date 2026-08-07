using System;
#if VIRTUESKY_ADS && VIRTUESKY_LEVELPLAY
using Unity.Services.LevelPlay;
using VirtueSky.Tracking;
#endif
using UnityEngine;
using VirtueSky.Misc;
using VirtueSky.Utils;

namespace VirtueSky.Ads
{
    [Serializable]
    public class LevelPlayBannerAdUnit : AdUnit
    {
        public AdsSize size;
        public AdsPosition position;
        public bool isShowOnLoad = false;
        [Tooltip("Destroy and recreate the LevelPlay ad object when reloading ads.")]
        public bool isDestroyAdOnReload = true;
        private bool _isBannerDestroyed = true;
        private bool _isBannerShowing;
        private bool _previousBannerShowStatus;
        private string _placement;
        private string _configuredPlacement;
        private AdsSize _configuredSize;
        private AdsPosition _configuredPosition;
        private bool _configuredShowOnLoad;
        private bool _hasBannerConfig;
#if VIRTUESKY_ADS && VIRTUESKY_LEVELPLAY
        private LevelPlayBannerAd bannerAd;
#endif

        public override bool IsShowing { get; internal set; }
        public override bool IsLoading { get; internal set; }

        public override void Init()
        {
#if VIRTUESKY_ADS && VIRTUESKY_LEVELPLAY
            if (AdStatic.IsRemoveAd) return;
            _isBannerDestroyed = true;
            paidedCallback += TrackRevenue;
#endif
        }

        public override void Load()
        {
#if VIRTUESKY_ADS && VIRTUESKY_LEVELPLAY
            if (AdStatic.IsRemoveAd) return;
            if (IsLoading) return;
            if (bannerAd != null && IsBannerConfigChanged())
            {
                ResetBannerAd(true);
            }

            if (_isBannerDestroyed || bannerAd == null)
            {
                CreateBannerAd();
            }

            IsLoading = true;
            VLog.Log($"Advertising: Load LevelPlayBannerAd: {Id}");
            OnRequestAdEvent?.Invoke();
            bannerAd.LoadAd();
#endif
        }

        void OnWaitAppOpenClosed()
        {
            if (_previousBannerShowStatus)
            {
                _previousBannerShowStatus = false;
                Show(_placement);
            }
        }

        void OnWaitAppOpenDisplayed()
        {
            _previousBannerShowStatus = _isBannerShowing;
            if (_isBannerShowing) HideBanner();
        }

        public override bool IsReady()
        {
#if VIRTUESKY_ADS && VIRTUESKY_LEVELPLAY
            return bannerAd != null;
#else
            return false;
#endif
        }

        protected override void ShowImpl(string placement = "")
        {
#if VIRTUESKY_ADS && VIRTUESKY_LEVELPLAY
            _placement = placement;
            AdStatic.waitAppOpenClosedAction = OnWaitAppOpenClosed;
            AdStatic.waitAppOpenDisplayedAction = OnWaitAppOpenDisplayed;
            Load();
            if (bannerAd != null)
            {
                _isBannerShowing = true;
                IsShowing = true;
                bannerAd.ShowAd();
            }
#endif
        }

        public override AdUnit Show(string placement = "")
        {
            ResetChainCallback();
            if (!Application.isMobilePlatform || AdStatic.IsRemoveAd || !IsReady()) return this;
            ShowImpl(placement);
            return this;
        }

        public override void Destroy()
        {
#if VIRTUESKY_ADS && VIRTUESKY_LEVELPLAY
            _isBannerShowing = false;
            IsShowing = false;
            AdStatic.waitAppOpenClosedAction = null;
            AdStatic.waitAppOpenDisplayedAction = null;
            ResetBannerAd(true);
#endif
        }

        public override void HideBanner()
        {
            base.HideBanner();
#if VIRTUESKY_ADS && VIRTUESKY_LEVELPLAY
            _isBannerShowing = false;
            IsShowing = false;
            if (bannerAd != null) bannerAd.HideAd();
#endif
        }


#if VIRTUESKY_ADS && VIRTUESKY_LEVELPLAY

        private void CreateBannerAd()
        {
            LevelPlayBannerAd.Config.Builder builder = new LevelPlayBannerAd.Config.Builder();
            builder.SetPosition(ConvertBannerPosition());
            builder.SetSize(ConvertBannerSize());
            builder.SetDisplayOnLoad(isShowOnLoad);
            builder.SetPlacementName(_placement);
            var config = builder.Build();
            bannerAd = new LevelPlayBannerAd(Id, config);
            bannerAd.OnAdLoaded += BannerOnAdLoadedEvent;
            bannerAd.OnAdLoadFailed += BannerOnAdLoadFailedEvent;
            bannerAd.OnAdClicked += BannerOnAdClickedEvent;
            bannerAd.OnAdDisplayed += BannerOnAdDisplayedEvent;
            bannerAd.OnAdDisplayFailed += BannerOnAdDisplayFailedEvent;
            bannerAd.OnAdLeftApplication += BannerOnAdLeftApplicationEvent;
            _configuredPlacement = _placement;
            _configuredSize = size;
            _configuredPosition = position;
            _configuredShowOnLoad = isShowOnLoad;
            _hasBannerConfig = true;
            _isBannerDestroyed = false;
        }

        private bool IsBannerConfigChanged()
        {
            return _hasBannerConfig &&
                   (_configuredPlacement != _placement ||
                    _configuredSize != size ||
                    _configuredPosition != position ||
                    _configuredShowOnLoad != isShowOnLoad);
        }

        private void ResetBannerAd(bool isDestroy = false, bool keepObject = false)
        {
            IsLoading = false;
            if (bannerAd == null)
            {
                _isBannerDestroyed = true;
                _hasBannerConfig = false;
                return;
            }

            if (keepObject) return;

            _isBannerShowing = false;
            IsShowing = false;
            bannerAd.OnAdLoaded -= BannerOnAdLoadedEvent;
            bannerAd.OnAdLoadFailed -= BannerOnAdLoadFailedEvent;
            bannerAd.OnAdClicked -= BannerOnAdClickedEvent;
            bannerAd.OnAdDisplayed -= BannerOnAdDisplayedEvent;
            bannerAd.OnAdDisplayFailed -= BannerOnAdDisplayFailedEvent;
            bannerAd.OnAdLeftApplication -= BannerOnAdLeftApplicationEvent;
            if (isDestroy) bannerAd.DestroyAd();
            bannerAd = null;
            _isBannerDestroyed = true;
            _hasBannerConfig = false;
        }

        private LevelPlayAdSize ConvertBannerSize()
        {
            switch (size)
            {
                case AdsSize.Banner: return LevelPlayAdSize.BANNER;
                case AdsSize.Adaptive: return LevelPlayAdSize.LARGE;
                case AdsSize.MediumRectangle: return LevelPlayAdSize.MEDIUM_RECTANGLE;
                case AdsSize.Leaderboard: return LevelPlayAdSize.LEADERBOARD;
                default: return LevelPlayAdSize.BANNER;
            }
        }

        private LevelPlayBannerPosition ConvertBannerPosition()
        {
            switch (position)
            {
                case AdsPosition.Bottom: return LevelPlayBannerPosition.BottomCenter;
                case AdsPosition.Top: return LevelPlayBannerPosition.TopCenter;
                default: return LevelPlayBannerPosition.BottomCenter;
            }
        }

        #region Fun Callback

        internal void OnAdPaidEvent(LevelPlayImpressionData impressionData)
        {
            if (impressionData.MediationAdUnitId.Equals(Id))
            {
                paidedCallback?.Invoke(new AdsInfo(impressionData));
            }
        }

        void BannerOnAdLoadedEvent(LevelPlayAdInfo adInfo)
        {
            IsLoading = false;
            VLog.Log($"Advertising: LevelPlayBannerAd Loaded: {Id}");
            var info = new AdsInfo(adInfo);
            ExcuteCallbackOnMainThread(() =>
            {
                Common.CallActionAndClean(ref loadedCallback, info);
                OnLoadedAdEvent?.Invoke(info);
            });
        }

        void BannerOnAdLoadFailedEvent(LevelPlayAdError ironSourceError)
        {
            IsLoading = false;
            var errorInfo = new AdsError(ironSourceError);
            VLog.LogWarning(
                $"Advertising: LevelPlayBannerAd FailedToLoad: {Id}, errorCode: {errorInfo.ErrorCode}, errorMessage: {errorInfo.ErrorMessage}");
            ExcuteCallbackOnMainThread(() =>
            {
                Common.CallActionAndClean(ref failedToLoadCallback, errorInfo);
                OnFailedToLoadAdEvent?.Invoke(errorInfo);
            });

            if (!isDestroyAdOnReload)
            {
                ResetBannerAd(false, true);
                return;
            }

            Destroy();
        }

        void BannerOnAdClickedEvent(LevelPlayAdInfo adInfo)
        {
            VLog.Log($"Advertising: LevelPlayBannerAd Clicked: {Id}");
            var info = new AdsInfo(adInfo);
            ExcuteCallbackOnMainThread(() =>
            {
                Common.CallActionAndClean(ref clickedCallback, info);
                OnClickedAdEvent?.Invoke(info);
            });
        }

        void BannerOnAdDisplayedEvent(LevelPlayAdInfo adInfo)
        {
            VLog.Log($"Advertising: LevelPlayBannerAd Displayed: {Id}");
            var info = new AdsInfo(adInfo);
            ExcuteCallbackOnMainThread(() =>
            {
                Common.CallActionAndClean(ref displayedCallback, info);
                OnDisplayedAdEvent?.Invoke(info);
            });
        }

        void BannerOnAdDisplayFailedEvent(LevelPlayAdInfo adInfo, LevelPlayAdError adError)
        {
            var errorInfo = new AdsError(adError);
            VLog.LogWarning(
                $"Advertising: LevelPlayBannerAd FailedToDisplay: {Id}, errorCode: {errorInfo.ErrorCode}, errorMessage: {errorInfo.ErrorMessage}");
            ExcuteCallbackOnMainThread(() =>
            {
                Common.CallActionAndClean(ref failedToDisplayCallback, errorInfo);
                OnFailedToDisplayAdEvent?.Invoke(errorInfo);
            });
        }

        void BannerOnAdLeftApplicationEvent(LevelPlayAdInfo adInfo)
        {
        }

        #endregion

#endif
    }
}
