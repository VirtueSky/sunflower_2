using System;
using System.Collections.Generic;
using UnityEngine;
using VirtueSky.Core;
using VirtueSky.Inspector;
using VirtueSky.Utils;

namespace VirtueSky.Ads
{
    [EditorIcon("icon_scriptable")]
    public class AdSettings : ScriptableSettings<AdSettings>
    {
        [SerializeField] private bool runtimeAutoInit = true;
        [SerializeField] private CoreEnum.RuntimeAutoInitType runtimeAutoInitType;
        [Range(5, 100), SerializeField] private float adCheckingInterval = 8f;
        [Range(5, 100), SerializeField] private float adLoadingInterval = 15f;
        [SerializeField] private AdNetwork adNetwork = AdNetwork.Max;

        [Tooltip("Install google-mobile-ads sdk to use GDPR"), SerializeField]
        private bool enableGDPR;

        [SerializeField] private bool enableGDPRTestMode;

        #region Max

        [TextArea, SerializeField] private string sdkKey;
        [SerializeField] private MaxBannerAdUnit maxBannerAdUnit;
        [SerializeField] private MaxInterstitialAdUnit maxInterstitialAdUnit;
        [SerializeField] private MaxRewardAdUnit maxRewardAdUnit;
        [SerializeField] private MaxAppOpenAdUnit maxAppOpenAdUnit;

        public static string SdkKey => Instance.sdkKey;

        public static MaxBannerAdUnit MaxBannerAdUnit => Instance.maxBannerAdUnit;
        public static MaxInterstitialAdUnit MaxInterstitialAdUnit => Instance.maxInterstitialAdUnit;
        public static MaxRewardAdUnit MaxRewardAdUnit => Instance.maxRewardAdUnit;
        public static MaxAppOpenAdUnit MaxAppOpenAdUnit => Instance.maxAppOpenAdUnit;

        #endregion

        #region Admob

        [SerializeField] private AdmobBannerAdUnit admobBannerAdUnit;
        [SerializeField] private AdmobInterstitialAdUnit admobInterstitialAdUnit;
        [SerializeField] private AdmobRewardAdUnit admobRewardAdUnit;
        [SerializeField] private AdmobRewardedInterstitialAdUnit admobRewardedInterstitialAdUnit;
        [SerializeField] private AdmobAppOpenAdUnit admobAppOpenAdUnit;
        [SerializeField] private AdmobNativeOverlayAdUnit admobNativeOverlayAdUnit;

        [Tooltip(
             "If you enable and connect admob with firebase, ad_impression will be automatically tracked. If you disable and disconnect admob with firebase, ad_impression will be tracked manually."),
         SerializeField]
        private bool autoTrackingAdImpressionAdmob = true;

        [SerializeField] private bool admobEnableTestMode;
        [SerializeField] private List<string> admobDevicesTest;


        public static AdmobBannerAdUnit AdmobBannerAdUnit => Instance.admobBannerAdUnit;
        public static AdmobInterstitialAdUnit AdmobInterstitialAdUnit => Instance.admobInterstitialAdUnit;
        public static AdmobRewardAdUnit AdmobRewardAdUnit => Instance.admobRewardAdUnit;

        public static AdmobRewardedInterstitialAdUnit AdmobRewardedInterstitialAdUnit =>
            Instance.admobRewardedInterstitialAdUnit;

        public static AdmobAppOpenAdUnit AdmobAppOpenAdUnit => Instance.admobAppOpenAdUnit;
        public static AdmobNativeOverlayAdUnit AdmobNativeOverlayAdUnit => Instance.admobNativeOverlayAdUnit;
        public static bool AutoTrackingAdImpressionAdmob => Instance.autoTrackingAdImpressionAdmob;
        public static bool AdmobEnableTestMode => Instance.admobEnableTestMode;
        public static List<string> AdmobDevicesTest => Instance.admobDevicesTest;

        #endregion

        #region IronSource

        [SerializeField] private string androidAppKey;
        [SerializeField] private string iOSAppKey;
        [SerializeField] private bool useTestAppKey;
        [SerializeField] private IronSourceBannerAdUnit ironSourceBannerAdUnit;
        [SerializeField] private IronSourceInterstitialAdUnit ironSourceInterstitialAdUnit;
        [SerializeField] private IronSourceRewardAdUnit ironSourceRewardAdUnit;

        public static string AndroidAppKey
        {
            get => Instance.androidAppKey;
            set => Instance.androidAppKey = value;
        }

        public static string IosAppKey
        {
            get => Instance.iOSAppKey;
            set => Instance.iOSAppKey = value;
        }

        public static string AppKey
        {
            get
            {
#if UNITY_ANDROID
                return Instance.androidAppKey;
#elif UNITY_IOS
                return Instance.iOSAppKey;
#else
                return string.Empty;
#endif
            }
            set
            {
#if UNITY_ANDROID
                Instance.androidAppKey = value;
#elif UNITY_IOS
                Instance.iOSAppKey = value;
#endif
            }
        }

        public static bool UseTestAppKey => Instance.useTestAppKey;
        public static IronSourceBannerAdUnit IronSourceBannerAdUnit => Instance.ironSourceBannerAdUnit;

        public static IronSourceInterstitialAdUnit IronSourceInterstitialAdUnit =>
            Instance.ironSourceInterstitialAdUnit;

        public static IronSourceRewardAdUnit IronSourceRewardAdUnit => Instance.ironSourceRewardAdUnit;

        #endregion

        public static bool RuntimeAutoInit => Instance.runtimeAutoInit;
        public static CoreEnum.RuntimeAutoInitType RuntimeAutoInitType => Instance.runtimeAutoInitType;
        public static float AdCheckingInterval => Instance.adCheckingInterval;
        public static float AdLoadingInterval => Instance.adLoadingInterval;

        public static AdNetwork CurrentAdNetwork
        {
            get => Instance.adNetwork;
            set => Instance.adNetwork = value;
        }

        public static bool EnableGDPR => Instance.enableGDPR;
        public static bool EnableGDPRTestMode => Instance.enableGDPRTestMode;
    }

    public enum AdNetwork
    {
        Max,
        Admob,
        IronSource
    }

    public enum AdsPosition
    {
        Top = 1,
        Bottom = 0,
        TopLeft = 2,
        TopRight = 3,
        BottomLeft = 4,
        BottomRight = 5,
        Center = 6
    }

    public enum AdsSize
    {
        Banner = 0, // 320x50
        Adaptive = 5, // full width
        MediumRectangle = 1, // 300x250
        IABBanner = 2, // 468x60
        Leaderboard = 3, // 728x90
        //    SmartBanner = 4,
    }
}