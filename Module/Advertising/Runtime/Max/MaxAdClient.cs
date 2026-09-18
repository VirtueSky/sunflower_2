using UnityEngine;
using VirtueSky.Core;

namespace VirtueSky.Ads
{
    public class MaxAdClient : AdClient
    {
        private BackupAdUnitGroup interstitialGroup;
        private BackupAdUnitGroup rewardGroup;

        private BackupAdUnitGroup InterstitialGroup => interstitialGroup ??= new BackupAdUnitGroup(
            AdSettings.MaxInterstitialAdUnit, AdSettings.MaxInterstitialAdUnitBackup,
            () => AdSettings.UseMaxInterstitialBackup, () => AdSettings.MaxBackupAdUnitExpireTime);

        private BackupAdUnitGroup RewardGroup => rewardGroup ??= new BackupAdUnitGroup(
            AdSettings.MaxRewardAdUnit, AdSettings.MaxRewardAdUnitBackup,
            () => AdSettings.UseMaxRewardBackup, () => AdSettings.MaxBackupAdUnitExpireTime);

        public override void Initialize()
        {
            SdkInitializationCompleted = false;
#if VIRTUESKY_ADS && VIRTUESKY_APPLOVIN
            MaxSdk.SetSdkKey(AdSettings.SdkKey);
            MaxSdkCallbacks.OnSdkInitializedEvent += OnSdkInitialized;
            MaxSdk.InitializeSdk();
            AdSettings.MaxBannerAdUnit.Init();
            InterstitialGroup.Init();
            RewardGroup.Init();
            AdSettings.MaxAppOpenAdUnit.Init();
            App.AddPauseCallback(OnAppStateChange);
#endif
        }

        public override AdUnit InterstitialAdUnit() => InterstitialGroup.Select();

        public override void LoadInterstitial()
        {
            InterstitialGroup.Load();
        }

        public override AdUnit RewardAdUnit() => RewardGroup.Select();

        public override void LoadRewarded()
        {
            RewardGroup.Load();
        }

        public override AdUnit RewardedInterstitialAdUnit() => null;

        public override void LoadRewardedInterstitial()
        {
        }

        public override AdUnit AppOpenAdUnit() => AdSettings.MaxAppOpenAdUnit;

        public override void LoadAppOpen()
        {
            if (AdSettings.MaxAppOpenAdUnit == null) return;
            if (!AdSettings.MaxAppOpenAdUnit.IsReady() && !AdSettings.MaxAppOpenAdUnit.IsLoading) AdSettings.MaxAppOpenAdUnit.Load();
        }

        public override void ShowAppOpen()
        {
            if (statusAppOpenFirstIgnore) AdSettings.MaxAppOpenAdUnit.Show();
            statusAppOpenFirstIgnore = true;
        }

        public override AdUnit BannerAdUnit() => AdSettings.MaxBannerAdUnit;

        public override void LoadBanner()
        {
            if (AdSettings.MaxBannerAdUnit == null) return;
            AdSettings.MaxBannerAdUnit.Load();
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
#if VIRTUESKY_ADS && VIRTUESKY_APPLOVIN
            if (SdkInitializationCompleted)
            {
                MaxSdk.ShowMediationDebugger();
                Debug.Log("Ad Mediation Debugger opened successfully.");
            }
            else
            {
                Debug.LogWarning("Failed to open Ad Mediation Debugger: SDK not initialized.");
            }
#endif
        }

#if VIRTUESKY_ADS && VIRTUESKY_APPLOVIN
        private void OnAppStateChange(bool pauseStatus)
        {
            if (!pauseStatus && AdSettings.MaxAppOpenAdUnit.autoShow)
            {
                if (AdSettings.IsApplovin()) ShowAppOpen();
            }
        }

        private void OnSdkInitialized(MaxSdkBase.SdkConfiguration configuration)
        {
            SdkInitializationCompleted = true;
            LoadInterstitial();
            LoadRewarded();
            LoadRewardedInterstitial();
            LoadAppOpen();
            //LoadBanner();
        }
#endif
    }
}