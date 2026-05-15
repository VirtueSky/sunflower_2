#if VIRTUESKY_ADS && VIRTUESKY_ADMOB
using GoogleMobileAds.Api;
using VirtueSky.Tracking;
#endif
using UnityEngine;
using VirtueSky.Core;

namespace VirtueSky.Ads
{
    public class AdmobClient : AdClient
    {
        public override void Initialize()
        {
            SdkInitializationCompleted = false;
#if VIRTUESKY_ADS && VIRTUESKY_ADMOB
#if UNITY_IOS
            // On Android, Unity is paused when displaying interstitial or rewarded video.
            // This setting makes iOS behave consistently with Android.
            MobileAds.SetiOSAppPauseOnBackground(true);
#endif
            TestMode();
            MobileAds.Initialize(OnInitializeComplete);
            FirebaseAnalyticTrackingRevenue.autoTrackAdImpressionAdmob = AdSettings.AutoTrackingAdImpressionAdmob;
            AdSettings.AdmobBannerAdUnit.Init();
            AdSettings.AdmobInterstitialAdUnit.Init();
            AdSettings.AdmobRewardAdUnit.Init();
            AdSettings.AdmobRewardedInterstitialAdUnit.Init();
            AdSettings.AdmobAppOpenAdUnit.Init();
            AdSettings.AdmobNativeOverlayAdUnit.Init();
            RegisterAppStateChange();
#endif
        }

        public override AdUnit InterstitialAdUnit() => AdSettings.AdmobInterstitialAdUnit;

        public override void LoadInterstitial()
        {
            if (AdSettings.AdmobInterstitialAdUnit == null || AdSettings.AdmobInterstitialAdUnit.IsShowing) return;
            if (!AdSettings.AdmobInterstitialAdUnit.IsReady() && !AdSettings.AdmobInterstitialAdUnit.IsLoading)
                AdSettings.AdmobInterstitialAdUnit.Load();
        }

        public override AdUnit RewardAdUnit() => AdSettings.AdmobRewardAdUnit;

        public override void LoadRewarded()
        {
            if (AdSettings.AdmobRewardAdUnit == null || AdSettings.AdmobRewardAdUnit.IsShowing) return;
            if (!AdSettings.AdmobRewardAdUnit.IsReady() && !AdSettings.AdmobRewardAdUnit.IsLoading) AdSettings.AdmobRewardAdUnit.Load();
        }

        public override AdUnit RewardedInterstitialAdUnit() => AdSettings.AdmobRewardedInterstitialAdUnit;

        public override void LoadRewardedInterstitial()
        {
            if (AdSettings.AdmobRewardedInterstitialAdUnit == null || AdSettings.AdmobRewardedInterstitialAdUnit.IsShowing) return;
            if (!AdSettings.AdmobRewardedInterstitialAdUnit.IsReady() && !AdSettings.AdmobRewardedInterstitialAdUnit.IsLoading)
                AdSettings.AdmobRewardedInterstitialAdUnit.Load();
        }

        public override AdUnit AppOpenAdUnit() => AdSettings.AdmobAppOpenAdUnit;

        public override void LoadAppOpen()
        {
            if (AdSettings.AdmobAppOpenAdUnit == null) return;
            if (!AdSettings.AdmobAppOpenAdUnit.IsReady() && !AdSettings.AdmobAppOpenAdUnit.IsLoading) AdSettings.AdmobAppOpenAdUnit.Load();
        }

        public override void ShowAppOpen()
        {
            if (statusAppOpenFirstIgnore) AdSettings.AdmobAppOpenAdUnit.Show();
            statusAppOpenFirstIgnore = true;
        }

        public override AdUnit BannerAdUnit() => AdSettings.AdmobBannerAdUnit;

        public override void LoadBanner()
        {
            if (AdSettings.AdmobBannerAdUnit == null) return;
            AdSettings.AdmobBannerAdUnit.Load();
        }

        public override AdUnit NativeOverlayAdUnit() => AdSettings.AdmobNativeOverlayAdUnit;

        public override void LoadNativeOverlay()
        {
            if (AdSettings.AdmobNativeOverlayAdUnit == null) return;
            if (!AdSettings.AdmobNativeOverlayAdUnit.IsReady()) AdSettings.AdmobNativeOverlayAdUnit.Load();
        }

        public override void ShowAdMediationDebugger()
        {
#if VIRTUESKY_ADS && VIRTUESKY_ADMOB
            if (SdkInitializationCompleted)
            {
                MobileAds.OpenAdInspector((result) =>
                {
                    if (result != null)
                    {
                        Debug.LogError($"Failed to open Ad Inspector: {result.GetCode()} / {result.GetMessage()}");
                    }
                    else
                    {
                        Debug.Log("Ad Inspector opened successfully.");
                    }
                });
            }
            else
            {
                Debug.LogWarning("Failed to open Ad Inspector: SDK not initialized.");
            }
#endif
        }

#if VIRTUESKY_ADS && VIRTUESKY_ADMOB
        private void RegisterAppStateChange()
        {
            GoogleMobileAds.Api.AppStateEventNotifier.AppStateChanged += OnAppStateChanged;
        }

        void OnAppStateChanged(GoogleMobileAds.Common.AppState state)
        {
            if (state == GoogleMobileAds.Common.AppState.Foreground && AdSettings.AdmobAppOpenAdUnit.autoShow)
            {
                if (AdSettings.IsAdmob()) ShowAppOpen();
            }
        }

        private void OnInitializeComplete(InitializationStatus initStatus)
        {
            SdkInitializationCompleted = true;
            LoadInterstitial();
            LoadRewarded();
            LoadRewardedInterstitial();
            LoadAppOpen();
            LoadNativeOverlay();
            LoadBanner();
        }

        private void TestMode()
        {
            if (!AdSettings.AdmobEnableTestMode) return;
            var configuration = new RequestConfiguration
                { TestDeviceIds = AdSettings.AdmobDevicesTest };
            MobileAds.SetRequestConfiguration(configuration);
        }
#endif
    }
}