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

        private BackupAdUnitGroup interstitialGroup;
        private BackupAdUnitGroup rewardGroup;

        private BackupAdUnitGroup InterstitialGroup => interstitialGroup ??= new BackupAdUnitGroup(
            AdSettings.LevelPlayInterstitialAdUnit, AdSettings.LevelPlayInterstitialAdUnitBackup,
            () => AdSettings.UseLevelPlayInterstitialBackup, () => AdSettings.LevelPlayBackupAdUnitExpireTime);

        private BackupAdUnitGroup RewardGroup => rewardGroup ??= new BackupAdUnitGroup(
            AdSettings.LevelPlayRewardAdUnit, AdSettings.LevelPlayRewardAdUnitBackup,
            () => AdSettings.UseLevelPlayRewardBackup, () => AdSettings.LevelPlayBackupAdUnitExpireTime);

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
            InterstitialGroup.Init();
            RewardGroup.Init();
            LevelPlay.ValidateIntegration();
            LevelPlay.Init(AdSettings.AppKey);
#endif
        }

        public override AdUnit InterstitialAdUnit() => InterstitialGroup.Select();

        public override void LoadInterstitial()
        {
            if (!SdkInitializationCompleted) return;
            InterstitialGroup.Load();
        }

        public override AdUnit RewardAdUnit() => RewardGroup.Select();

        public override void LoadRewarded()
        {
            if (!SdkInitializationCompleted) return;
            RewardGroup.Load();
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
                // Each ad unit filters by its own MediationAdUnitId, so the backups can safely listen too.
                if (AdSettings.UseLevelPlayInterstitialBackup) AdSettings.LevelPlayInterstitialAdUnitBackup.OnAdPaidEvent(impressionData);
                if (AdSettings.UseLevelPlayRewardBackup) AdSettings.LevelPlayRewardAdUnitBackup.OnAdPaidEvent(impressionData);
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