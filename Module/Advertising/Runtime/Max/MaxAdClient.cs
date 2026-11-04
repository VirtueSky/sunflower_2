using UnityEngine;
using VirtueSky.Core;

namespace VirtueSky.Ads
{
    public class MaxAdClient : AdClient, IBannerClient, IInterstitialClient, IRewardClient, IAppOpenClient
    {
        private bool statusAppOpenFirstIgnore;
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

        public AdUnit InterstitialAdUnit() => InterstitialGroup.Select();

        public void LoadInterstitial()
        {
            InterstitialGroup.Load();
        }

        public AdUnit RewardAdUnit() => RewardGroup.Select();

        public void LoadRewarded()
        {
            RewardGroup.Load();
        }

        public AdUnit AppOpenAdUnit() => AdSettings.MaxAppOpenAdUnit;

        public void LoadAppOpen()
        {
            if (AdSettings.MaxAppOpenAdUnit == null) return;
            if (!AdSettings.MaxAppOpenAdUnit.IsReady() && !AdSettings.MaxAppOpenAdUnit.IsLoading) AdSettings.MaxAppOpenAdUnit.Load();
        }

        public void ShowAppOpen()
        {
            if (statusAppOpenFirstIgnore) AdSettings.MaxAppOpenAdUnit.Show();
            statusAppOpenFirstIgnore = true;
        }

        public AdUnit BannerAdUnit() => AdSettings.MaxBannerAdUnit;

        public void LoadBanner()
        {
            if (AdSettings.MaxBannerAdUnit == null) return;
            AdSettings.MaxBannerAdUnit.Load();
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
            LoadAppOpen();
            //LoadBanner();
        }
#endif
    }
}