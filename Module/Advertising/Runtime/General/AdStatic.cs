using System;
using UnityEngine;
using VirtueSky.Core;
using VirtueSky.DataStorage;

namespace VirtueSky.Ads
{
    public static class AdStatic
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RuntimeBeforeSceneLoad()
        {
            AutoInitialize(CoreEnum.RuntimeInitType.BeforeSceneLoad_Awake);
            AutoInitialize(CoreEnum.RuntimeInitType.BeforeSceneLoad_OnEnable);
            AutoInitialize(CoreEnum.RuntimeInitType.BeforeSceneLoad_Start);
            AutoInitialize(CoreEnum.RuntimeInitType.BeforeSceneLoad);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void RuntimeAfterSceneLoad()
        {
            AutoInitialize(CoreEnum.RuntimeInitType.AfterSceneLoad_Awake);
            AutoInitialize(CoreEnum.RuntimeInitType.AfterSceneLoad_OnEnable);
            AutoInitialize(CoreEnum.RuntimeInitType.AfterSceneLoad_Start);
            AutoInitialize(CoreEnum.RuntimeInitType.AfterSceneLoad);
        }

        private static void AutoInitialize(CoreEnum.RuntimeInitType adsRuntimeInitType)
        {
            if (AdSettings.Instance == null) return;
            if (AdSettings.RuntimeInitType != adsRuntimeInitType) return;
            var ads = new GameObject("Advertising");
            ads.AddComponent<Advertising>();
            UnityEngine.Object.DontDestroyOnLoad(ads);
        }


        public static Action<bool> OnChangePreventDisplayAppOpenEvent;

        public static bool IsRemoveAd
        {
            get => GameData.Get($"{Application.identifier}_removeads", false);
            set => GameData.Set($"{Application.identifier}_removeads", value);
        }

        public static DateTime AdClosingTime { get; internal set; }

        private static bool isShowingAd;

        public static bool IsShowingAd
        {
            get => isShowingAd;
            internal set
            {
                isShowingAd = value;
                if (!value)
                {
                    AdClosingTime = DateTime.Now;
                }
            }
        }

        internal static Action waitAppOpenDisplayedAction;
        internal static Action waitAppOpenClosedAction;

        public static AdUnit OnDisplayed(this AdUnit unit, Action<AdsInfo> onDisplayed)
        {
            unit.displayedCallback = onDisplayed;
            return unit;
        }

        public static AdUnit OnClosed(this AdUnit unit, Action<AdsInfo> onClosed)
        {
            unit.closedCallback = onClosed;
            return unit;
        }

        public static AdUnit OnLoaded(this AdUnit unit, Action<AdsInfo> onLoaded)
        {
            unit.loadedCallback = onLoaded;
            return unit;
        }

        public static AdUnit OnFailedToLoad(this AdUnit unit, Action<AdsError> onFailedToLoad)
        {
            unit.failedToLoadCallback = onFailedToLoad;
            return unit;
        }

        public static AdUnit OnFailedToDisplay(this AdUnit unit, Action<AdsError> onFailedToDisplay)
        {
            unit.failedToDisplayCallback = onFailedToDisplay;
            return unit;
        }

        public static AdUnit OnClicked(this AdUnit unit, Action<AdsInfo> onClicked)
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
                case LevelPlayInterstitialAdUnit ironSourceInterstitialAdUnit:
                    ironSourceInterstitialAdUnit.completedCallback = onCompleted;
                    return unit;
                case LevelPlayRewardAdUnit ironSourceRewardAdUnit:
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
                case LevelPlayRewardAdUnit ironSourceRewardAdUnit:
                    ironSourceRewardAdUnit.skippedCallback = onSkipped;
                    return unit;
            }

            return unit;
        }

        public static AdUnit OnReceivedReward(this AdUnit unit, Action onReceivedReward)
        {
            switch (unit)
            {
                case AdmobRewardAdUnit admobReward:
                    admobReward.receivedRewardCallback = onReceivedReward;
                    return unit;
                case AdmobRewardedInterstitialAdUnit admobRewardInter:
                    admobRewardInter.receivedRewardCallback = onReceivedReward;
                    return unit;
                case MaxRewardAdUnit maxReward:
                    maxReward.receivedRewardCallback = onReceivedReward;
                    return unit;
                case LevelPlayRewardAdUnit levelPlayRewardAdUnit:
                    levelPlayRewardAdUnit.receivedRewardCallback = onReceivedReward;
                    return unit;
            }

            return unit;
        }
    }
}