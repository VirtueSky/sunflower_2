#if VIRTUESKY_ADS && VIRTUESKY_LEVELPLAY
using Unity.Services.LevelPlay;
using UnityEngine;
#endif
using VirtueSky.Core;
using VirtueSky.Tracking;

namespace VirtueSky.Ads
{
    public class LevelPlayClient : AdClient
    {
        private const float InitialLoadDelay = 0.1f;

        public override void Initialize()
        {
            SdkInitializationCompleted = false;
            if (AdSettings.UseTestAppKey)
            {
                AdSettings.AndroidAppKey = "85460dcd";
                AdSettings.IosAppKey = "8545d445";
            }
#if VIRTUESKY_ADS && VIRTUESKY_LEVELPLAY
            if (AdSettings.EnableTestSuite)
            {
                LevelPlay.SetMetaData("is_test_suite", "enable");
            }

            App.AddPauseCallback(OnAppStateChange);
            LevelPlay.OnInitSuccess += SdkInitializationCompletedEvent;
            LevelPlay.OnImpressionDataReady += ImpressionDataReadyEvent;
            AdSettings.LevelPlayBannerAdUnit.Init();
            AdSettings.LevelPlayInterstitialAdUnit.Init();
            AdSettings.LevelPlayRewardAdUnit.Init();
            LevelPlay.ValidateIntegration();
            LevelPlay.Init(AdSettings.AppKey);
#endif
        }

        public override AdUnit InterstitialAdUnit() => AdSettings.LevelPlayInterstitialAdUnit;

        public override void LoadInterstitial()
        {
            if (!SdkInitializationCompleted) return;
            if (AdSettings.LevelPlayInterstitialAdUnit == null || AdSettings.LevelPlayInterstitialAdUnit.IsShowing) return;
            if (!AdSettings.LevelPlayInterstitialAdUnit.IsReady() && !AdSettings.LevelPlayInterstitialAdUnit.IsLoading) AdSettings.LevelPlayInterstitialAdUnit.Load();
        }

        public override AdUnit RewardAdUnit() => AdSettings.LevelPlayRewardAdUnit;

        public override void LoadRewarded()
        {
            if (!SdkInitializationCompleted) return;
            if (AdSettings.LevelPlayRewardAdUnit == null || AdSettings.LevelPlayRewardAdUnit.IsShowing) return;
            if (!AdSettings.LevelPlayRewardAdUnit.IsReady() && !AdSettings.LevelPlayRewardAdUnit.IsLoading) AdSettings.LevelPlayRewardAdUnit.Load();
        }

        public override AdUnit RewardedInterstitialAdUnit()
        {
            return null;
        }

        public override void LoadRewardedInterstitial()
        {
        }

        public override AdUnit AppOpenAdUnit()
        {
            return null;
        }

        public override void LoadAppOpen()
        {
        }

        public override void ShowAppOpen()
        {
        }

        public override AdUnit BannerAdUnit() => AdSettings.LevelPlayBannerAdUnit;

        public override void LoadBanner()
        {
            if (!SdkInitializationCompleted) return;
            if (AdSettings.LevelPlayBannerAdUnit == null) return;
            AdSettings.LevelPlayBannerAdUnit.Load();
        }

        public override AdUnit NativeOverlayAdUnit()
        {
            return null;
        }

        public override void LoadNativeOverlay()
        {
        }

        public override void ShowAdMediationDebugger()
        {
#if VIRTUESKY_ADS && VIRTUESKY_LEVELPLAY
            if (SdkInitializationCompleted)
            {
                LevelPlay.LaunchTestSuite();
                Debug.Log("LevelPlay Test Suite Launched");
            }
            else
            {
                Debug.LogWarning("Failed to launch LevelPlay Test Suite: SDK initialization not completed.");
            }
#endif
        }
#if VIRTUESKY_ADS && VIRTUESKY_LEVELPLAY
        private void ImpressionDataReadyEvent(LevelPlayImpressionData impressionData)
        {
            if (impressionData.Revenue != null)
            {
                AdSettings.LevelPlayBannerAdUnit.OnAdPaidEvent(impressionData);
                AdSettings.LevelPlayInterstitialAdUnit.OnAdPaidEvent(impressionData);
                AdSettings.LevelPlayRewardAdUnit.OnAdPaidEvent(impressionData);
            }
        }

        private void OnAppStateChange(bool pauseStatus)
        {
            if (SdkInitializationCompleted)
            {
                LevelPlay.SetPauseGame(pauseStatus);
            }
        }

        void SdkInitializationCompletedEvent(LevelPlayConfiguration config)
        {
            SdkInitializationCompleted = true;
            App.Delay(InitialLoadDelay, () =>
            {
                LoadInterstitial();
                LoadRewarded();
                //LoadBanner();
            });
        }
#endif
    }
}