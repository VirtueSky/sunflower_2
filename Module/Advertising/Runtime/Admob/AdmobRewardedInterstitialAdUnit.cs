using System;
#if VIRTUESKY_ADMOB && VIRTUESKY_ADS
using GoogleMobileAds.Api;
using VirtueSky.Tracking;
#endif
using VirtueSky.Misc;


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
#endif
        public override bool IsShowing { get; internal set; }

        public override void Init()
        {
            if (useTestId)
            {
                GetUnitTest();
            }
#if VIRTUESKY_ADS && VIRTUESKY_ADMOB
            if (string.IsNullOrEmpty(Id)) return;
            paidedCallback += AppTracking.TrackRevenue;
#endif
        }

        public bool IsEarnRewarded { get; private set; }

        public override void Load()
        {
#if VIRTUESKY_ADS && VIRTUESKY_ADMOB
            if (string.IsNullOrEmpty(Id)) return;
            Destroy();
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
#if VIRTUESKY_ADS && VIRTUESKY_ADMOB
            _rewardedInterstitialAd.Show(UserEarnedRewardCallback);
#endif
        }

        protected override void ResetChainCallback()
        {
            base.ResetChainCallback();
            completedCallback = null;
            skippedCallback = null;
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
            _rewardedInterstitialAd.OnAdFullScreenContentClosed += OnAdClosed;
            _rewardedInterstitialAd.OnAdFullScreenContentOpened += OnAdOpening;
            _rewardedInterstitialAd.OnAdFullScreenContentFailed += OnAdFailedToShow;
            _rewardedInterstitialAd.OnAdPaid += OnAdPaided;
            _rewardedInterstitialAd.OnAdClicked += OnAdClicked;
            OnAdLoaded();
        }

        private void OnAdClicked()
        {
            var info = new AdsInfo(AdMediation.Admob);
            Common.CallActionAndClean(ref clickedCallback, info);
            OnClickedAdEvent?.Invoke(info);
        }

        private void OnAdFailedToLoad(LoadAdError error)
        {
            var errorInfo = new AdsError(error);
            Common.CallActionAndClean(ref failedToLoadCallback, errorInfo);
            OnFailedToLoadAdEvent?.Invoke(errorInfo);
        }

        private void OnAdLoaded()
        {
            var info = new AdsInfo(AdMediation.Admob);
            Common.CallActionAndClean(ref loadedCallback, info);
            OnLoadAdEvent?.Invoke(info);
        }

        private void OnAdPaided(AdValue value)
        {
            paidedCallback?.Invoke(value.Value / 1000000f,
                "Admob",
                Id,
                "RewardedInterstitialAd", AdMediation.Admob.ToString());
        }

        private void OnAdFailedToShow(AdError error)
        {
            var errorInfo = new AdsError(error);
            Common.CallActionAndClean(ref failedToDisplayCallback, errorInfo);
            OnFailedToDisplayAdEvent?.Invoke(errorInfo);
        }

        private void OnAdOpening()
        {
            AdStatic.IsShowingAd = true;
            IsShowing = true;
            var info = new AdsInfo(AdMediation.Admob);
            Common.CallActionAndClean(ref displayedCallback, info);
            OnDisplayedAdEvent?.Invoke(info);
        }

        private void OnAdClosed()
        {
            AdStatic.IsShowingAd = false;
            IsShowing = false;
            var info = new AdsInfo(AdMediation.Admob);
            Common.CallActionAndClean(ref closedCallback, info);
            OnClosedAdEvent?.Invoke(info);
            if (IsEarnRewarded)
            {
                Common.CallActionAndClean(ref completedCallback);
                _rewardedInterstitialAd.Destroy();
                return;
            }

            Common.CallActionAndClean(ref skippedCallback);
            _rewardedInterstitialAd.Destroy();
        }

        private void UserEarnedRewardCallback(Reward reward)
        {
            IsEarnRewarded = true;
            Common.CallActionAndClean(ref receivedRewardCallback);
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
