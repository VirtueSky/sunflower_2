using VirtueSky.Core;

namespace VirtueSky.Ads
{
    public class MaxAdClient : AdClient
    {
        public override void Initialize()
        {
#if VIRTUESKY_ADS && VIRTUESKY_APPLOVIN
            MaxSdk.SetSdkKey(AdSettings.SdkKey);
            MaxSdk.InitializeSdk();
            AdSettings.MaxBannerAdUnit.Init();
            AdSettings.MaxInterstitialAdUnit.Init();
            AdSettings.MaxRewardAdUnit.Init();
            AdSettings.MaxAppOpenAdUnit.Init();
            App.AddPauseCallback(OnAppStateChange);
            LoadInterstitial();
            LoadRewarded();
            LoadRewardedInterstitial();
            LoadAppOpen();
            LoadBanner();
#endif
        }

        public override AdUnit InterstitialAdUnit() => AdSettings.MaxInterstitialAdUnit;

        public override void LoadInterstitial()
        {
            if (!AdSettings.MaxInterstitialAdUnit.IsReady()) AdSettings.MaxInterstitialAdUnit.Load();
        }

        public override AdUnit RewardAdUnit() => AdSettings.MaxRewardAdUnit;

        public override void LoadRewarded()
        {
            if (!AdSettings.MaxRewardAdUnit.IsReady()) AdSettings.MaxRewardAdUnit.Load();
        }

        public override AdUnit RewardedInterstitialAdUnit() => null;

        public override void LoadRewardedInterstitial()
        {
        }

        public override AdUnit AppOpenAdUnit() => AdSettings.MaxAppOpenAdUnit;

        public override void LoadAppOpen()
        {
            if (!AdSettings.MaxAppOpenAdUnit.IsReady()) AdSettings.MaxAppOpenAdUnit.Load();
        }

        public override void ShowAppOpen()
        {
            if (statusAppOpenFirstIgnore) AdSettings.MaxAppOpenAdUnit.Show();
            statusAppOpenFirstIgnore = true;
        }

        public override AdUnit BannerAdUnit() => AdSettings.MaxBannerAdUnit;

        public override void LoadBanner()
        {
            AdSettings.MaxBannerAdUnit.Load();
        }

        public override AdUnit NativeOverlayAdUnit()
        {
            return null;
        }

        public override void LoadNativeOverlay()
        {
        }

#if VIRTUESKY_ADS && VIRTUESKY_APPLOVIN
        private void OnAppStateChange(bool pauseStatus)
        {
            if (!pauseStatus && AdSettings.MaxAppOpenAdUnit.autoShow && !AdStatic.isShowingAd) ShowAppOpen();
        }
#endif
    }
}