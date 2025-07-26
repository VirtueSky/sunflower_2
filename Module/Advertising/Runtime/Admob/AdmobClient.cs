#if VIRTUESKY_ADS && VIRTUESKY_ADMOB
using GoogleMobileAds.Api;
using VirtueSky.Tracking;
#endif
using VirtueSky.Core;

namespace VirtueSky.Ads
{
    public class AdmobClient : AdClient
    {
        public override void Initialize()
        {
#if VIRTUESKY_ADS && VIRTUESKY_ADMOB
            // On Android, Unity is paused when displaying interstitial or rewarded video.
            // This setting makes iOS behave consistently with Android.
            MobileAds.SetiOSAppPauseOnBackground(true);

            // When true all events raised by GoogleMobileAds will be raised
            // on the Unity main thread. The default value is false.
            // https://developers.google.com/admob/unity/quick-start#raise_ad_events_on_the_unity_main_thread
            MobileAds.RaiseAdEventsOnUnityMainThread = true;


            MobileAds.Initialize(initStatus =>
            {
                App.RunOnMainThread(() =>
                {
                    if (!AdSettings.AdmobEnableTestMode) return;
                    var configuration = new RequestConfiguration
                        { TestDeviceIds = AdSettings.AdmobDevicesTest };
                    MobileAds.SetRequestConfiguration(configuration);
                });
            });

            FirebaseAnalyticTrackingRevenue.autoTrackAdImpressionAdmob = AdSettings.AutoTrackingAdImpressionAdmob;

            AdSettings.AdmobBannerAdUnit.Init();
            AdSettings.AdmobInterstitialAdUnit.Init();
            AdSettings.AdmobRewardAdUnit.Init();
            AdSettings.AdmobRewardedInterstitialAdUnit.Init();
            AdSettings.AdmobAppOpenAdUnit.Init();
            AdSettings.AdmobNativeOverlayAdUnit.Init();
            RegisterAppStateChange();
            LoadInterstitial();
            LoadRewarded();
            LoadRewardedInterstitial();
            LoadAppOpen();
            LoadNativeOverlay();
            LoadBanner();
#endif
        }

        public override AdUnit InterstitialAdUnit() => AdSettings.AdmobInterstitialAdUnit;

        public override void LoadInterstitial()
        {
            if (!AdSettings.AdmobInterstitialAdUnit.IsReady()) AdSettings.AdmobInterstitialAdUnit.Load();
        }

        public override AdUnit RewardAdUnit() => AdSettings.AdmobRewardAdUnit;

        public override void LoadRewarded()
        {
            if (!AdSettings.AdmobRewardAdUnit.IsReady()) AdSettings.AdmobRewardAdUnit.Load();
        }

        public override AdUnit RewardedInterstitialAdUnit() => AdSettings.AdmobRewardedInterstitialAdUnit;

        public override void LoadRewardedInterstitial()
        {
            if (!AdSettings.AdmobRewardedInterstitialAdUnit.IsReady())
                AdSettings.AdmobRewardedInterstitialAdUnit.Load();
        }

        public override AdUnit AppOpenAdUnit() => AdSettings.AdmobAppOpenAdUnit;

        public override void LoadAppOpen()
        {
            if (!AdSettings.AdmobAppOpenAdUnit.IsReady()) AdSettings.AdmobAppOpenAdUnit.Load();
        }

        public override void ShowAppOpen()
        {
            if (statusAppOpenFirstIgnore) AdSettings.AdmobAppOpenAdUnit.Show();
            statusAppOpenFirstIgnore = true;
        }

        public override AdUnit BannerAdUnit() => AdSettings.AdmobBannerAdUnit;

        public override void LoadBanner()
        {
            AdSettings.AdmobBannerAdUnit.Load();
        }

        public override AdUnit NativeOverlayAdUnit() => AdSettings.AdmobNativeOverlayAdUnit;

        public override void LoadNativeOverlay()
        {
            if (!AdSettings.AdmobNativeOverlayAdUnit.IsReady())
            {
                AdSettings.AdmobNativeOverlayAdUnit.Load();
            }
        }

#if VIRTUESKY_ADS && VIRTUESKY_ADMOB
        public void RegisterAppStateChange()
        {
            GoogleMobileAds.Api.AppStateEventNotifier.AppStateChanged += OnAppStateChanged;
        }

        void OnAppStateChanged(GoogleMobileAds.Common.AppState state)
        {
            if (state == GoogleMobileAds.Common.AppState.Foreground && AdSettings.AdmobAppOpenAdUnit.autoShow)
            {
                if (AdSettings.UseAdmob) ShowAppOpen();
            }
        }
#endif
    }
}