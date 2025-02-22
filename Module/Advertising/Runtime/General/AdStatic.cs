using System;
using UnityEngine;
using VirtueSky.Core;
using VirtueSky.DataStorage;

namespace VirtueSky.Ads
{
    public static class AdStatic
    {
#if VIRTUESKY_ADS
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RuntimeBeforeSceneLoad()
        {
            AutoInitialize(CoreEnum.RuntimeAutoInitType.BeforeSceneLoad);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void RuntimeAfterSceneLoad()
        {
            AutoInitialize(CoreEnum.RuntimeAutoInitType.AfterSceneLoad);
        }

        private static void AutoInitialize(CoreEnum.RuntimeAutoInitType adsRuntimeAutoInitType)
        {
            if (AdSettings.Instance == null) return;
            if (!AdSettings.RuntimeAutoInit) return;
            if (AdSettings.RuntimeAutoInitType != adsRuntimeAutoInitType) return;
            var ads = new GameObject("Advertising");
            ads.AddComponent<Advertising>();
            UnityEngine.Object.DontDestroyOnLoad(ads);
        }

#endif


        public static Action<bool> OnChangePreventDisplayAppOpenEvent;

        public static bool IsRemoveAd
        {
            get => GameData.Get($"{Application.identifier}_removeads", false);
            set => GameData.Set($"{Application.identifier}_removeads", value);
        }

        internal static bool isShowingAd;
        public static bool IsShowingAd => isShowingAd;
        internal static Action waitAppOpenDisplayedAction;
        internal static Action waitAppOpenClosedAction;

        public static AdUnit OnDisplayed(this AdUnit unit, Action onDisplayed)
        {
            unit.displayedCallback = onDisplayed;
            return unit;
        }

        public static AdUnit OnClosed(this AdUnit unit, Action onClosed)
        {
            unit.closedCallback = onClosed;
            return unit;
        }

        public static AdUnit OnLoaded(this AdUnit unit, Action onLoaded)
        {
            unit.loadedCallback = onLoaded;
            return unit;
        }

        public static AdUnit OnFailedToLoad(this AdUnit unit, Action onFailedToLoad)
        {
            unit.failedToLoadCallback = onFailedToLoad;
            return unit;
        }

        public static AdUnit OnFailedToDisplay(this AdUnit unit, Action onFailedToDisplay)
        {
            unit.failedToDisplayCallback = onFailedToDisplay;
            return unit;
        }

        public static AdUnit OnClicked(this AdUnit unit, Action onClicked)
        {
            unit.clickedCallback = onClicked;
            return unit;
        }

        public static AdUnit OnCompleted(this AdUnit unit, Action onCompleted)
        {
            if (!Application.isMobilePlatform)
            {
                onCompleted?.Invoke();
            }

            switch (unit)
            {
                case AdmobInterstitialAdUnit admobInter:
                    admobInter.completedCallback = onCompleted;
                    return unit;
                case AdmobRewardAdUnit admobReward:
                    admobReward.completedCallback = onCompleted;
                    return unit;
                case AdmobRewardedInterstitialAdUnit admobRewardInter:
                    admobRewardInter.completedCallback = onCompleted;
                    return unit;
                case MaxInterstitialAdUnit maxInter:
                    maxInter.completedCallback = onCompleted;
                    return unit;
                case MaxRewardAdUnit maxReward:
                    maxReward.completedCallback = onCompleted;
                    return unit;
                case IronSourceInterstitialAdUnit ironSourceInterstitialAdUnit:
                    ironSourceInterstitialAdUnit.completedCallback = onCompleted;
                    return unit;
                case IronSourceRewardAdUnit ironSourceRewardAdUnit:
                    ironSourceRewardAdUnit.completedCallback = onCompleted;
                    return unit;
            }

            return unit;
        }

        public static AdUnit OnSkipped(this AdUnit unit, Action onSkipped)
        {
            switch (unit)
            {
                case AdmobRewardAdUnit admobReward:
                    admobReward.skippedCallback = onSkipped;
                    return unit;
                case AdmobRewardedInterstitialAdUnit admobRewardInter:
                    admobRewardInter.skippedCallback = onSkipped;
                    return unit;
                case MaxRewardAdUnit maxReward:
                    maxReward.skippedCallback = onSkipped;
                    return unit;
                case IronSourceRewardAdUnit ironSourceRewardAdUnit:
                    ironSourceRewardAdUnit.skippedCallback = onSkipped;
                    return unit;
            }

            return unit;
        }
    }
}