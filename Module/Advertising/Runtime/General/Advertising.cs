using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif

#if VIRTUESKY_ADMOB
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
#endif
using UnityEngine;
using VirtueSky.Core;
using VirtueSky.Inspector;
using VirtueSky.Misc;
using VirtueSky.Tracking;

namespace VirtueSky.Ads
{
    [EditorIcon("icon_manager"), HideMonoScript]
    public class Advertising : MonoBehaviour
    {
        [SerializeField] private bool isPersistent = false;
        private IEnumerator autoLoadAdCoroutine;
        private float _lastTimeLoadInterstitialAdTimestamp = DEFAULT_TIMESTAMP;
        private float _lastTimeLoadRewardedTimestamp = DEFAULT_TIMESTAMP;
        private float _lastTimeLoadRewardedInterstitialTimestamp = DEFAULT_TIMESTAMP;
        private float _lastTimeLoadAppOpenTimestamp = DEFAULT_TIMESTAMP;
        private const float DEFAULT_TIMESTAMP = -1000;

        private AdClient maxAdClient;
        private AdClient admobAdClient;
        private AdClient levelPlayAdClient;

        private static Advertising instance;

        private bool isInitAdClient = false;
        private bool isInititalization = false;

        private void Awake()
        {
            if (isPersistent)
            {
                DontDestroyOnLoad(gameObject);
            }

            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            isInititalization = false;
            if (AdSettings.RuntimeInitType == CoreEnum.RuntimeInitType.AfterSceneLoad_Awake ||
                AdSettings.RuntimeInitType == CoreEnum.RuntimeInitType.BeforeSceneLoad_Awake)
            {
                InternalInitialization();
            }
        }

        private void OnEnable()
        {
            if (AdSettings.RuntimeInitType == CoreEnum.RuntimeInitType.AfterSceneLoad_OnEnable ||
                AdSettings.RuntimeInitType == CoreEnum.RuntimeInitType.BeforeSceneLoad_OnEnable)
            {
                InternalInitialization();
            }
        }

        private void Start()
        {
            if (AdSettings.RuntimeInitType == CoreEnum.RuntimeInitType.AfterSceneLoad_Start ||
                AdSettings.RuntimeInitType == CoreEnum.RuntimeInitType.BeforeSceneLoad_Start)
            {
                InternalInitialization();
            }
        }

        private void InternalInitialization()
        {
            if (isInititalization) return;
            isInititalization = true;
            isInitAdClient = false;
            AdStatic.OnChangePreventDisplayAppOpenEvent += OnChangePreventDisplayOpenAd;
            if (AdSettings.EnableGDPR)
            {
#if VIRTUESKY_ADMOB
#if UNITY_IOS
                if (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() ==
                    ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED)
                {
                    InitGDPR();
                }
                else
                {
                    InitAdClient();
                }
#else
                InitGDPR();
#endif

#endif
            }
            else
            {
                InitAdClient();
            }
        }

        void InitAdClient()
        {
            if (AdSettings.IsApplovin()) InitApplovinClient();
            if (AdSettings.IsAdmob()) InitAdmobClient();
            if (AdSettings.IsLevelPlay()) InitLevelPlayClient();
            isInitAdClient = true;
            InitAutoLoadAds();
        }

        void InitApplovinClient()
        {
            maxAdClient = new MaxAdClient();
            maxAdClient.Initialize();
            Debug.Log($"Use AdClient: {maxAdClient}".SetColor(Color.cyan));
        }

        void InitAdmobClient()
        {
            admobAdClient = new AdmobClient();
            admobAdClient.Initialize();
            Debug.Log($"Use AdClient: {admobAdClient}".SetColor(Color.cyan));
        }

        void InitLevelPlayClient()
        {
            levelPlayAdClient = new LevelPlayClient();
            levelPlayAdClient.Initialize();
            Debug.Log($"Use AdClient: {levelPlayAdClient}".SetColor(Color.cyan));
        }

        private void InitAutoLoadAds()
        {
            if (autoLoadAdCoroutine != null) StopCoroutine(autoLoadAdCoroutine);
            autoLoadAdCoroutine = IeAutoLoadAll();
            StartCoroutine(autoLoadAdCoroutine);
        }

        IEnumerator IeAutoLoadAll(float delay = 0)
        {
            if (delay > 0) yield return new WaitForSeconds(delay);
            while (true)
            {
                AutoLoadInterAds();
                AutoLoadRewardAds();
                AutoLoadRewardInterAds();
                AutoLoadAppOpenAds();
                yield return new WaitForSeconds(AdSettings.AdCheckingInterval);
            }
        }

        private void OnChangePreventDisplayOpenAd(bool state)
        {
            AdStatic.IsShowingAd = state;
        }

        #region Method Load Ads

        void AutoLoadInterAds()
        {
            if (Time.realtimeSinceStartup - _lastTimeLoadInterstitialAdTimestamp <
                AdSettings.AdLoadingInterval) return;
            if (AdSettings.IsApplovin() && maxAdClient != null) maxAdClient.LoadInterstitial();
            if (AdSettings.IsAdmob() && admobAdClient != null) admobAdClient.LoadInterstitial();
            if (AdSettings.IsLevelPlay() && levelPlayAdClient != null) levelPlayAdClient.LoadInterstitial();
            _lastTimeLoadInterstitialAdTimestamp = Time.realtimeSinceStartup;
        }

        void AutoLoadRewardAds()
        {
            if (Time.realtimeSinceStartup - _lastTimeLoadRewardedTimestamp <
                AdSettings.AdLoadingInterval) return;
            if (AdSettings.IsApplovin() && maxAdClient != null) maxAdClient.LoadRewarded();
            if (AdSettings.IsAdmob() && admobAdClient != null) admobAdClient.LoadRewarded();
            if (AdSettings.IsLevelPlay() && levelPlayAdClient != null) levelPlayAdClient.LoadRewarded();
            _lastTimeLoadRewardedTimestamp = Time.realtimeSinceStartup;
        }

        void AutoLoadRewardInterAds()
        {
            if (Time.realtimeSinceStartup - _lastTimeLoadRewardedInterstitialTimestamp <
                AdSettings.AdLoadingInterval) return;
            if (AdSettings.IsApplovin() && maxAdClient != null) maxAdClient.LoadRewardedInterstitial();
            if (AdSettings.IsAdmob() && admobAdClient != null) admobAdClient.LoadRewardedInterstitial();
            if (AdSettings.IsLevelPlay() && levelPlayAdClient != null) levelPlayAdClient.LoadRewardedInterstitial();
            _lastTimeLoadRewardedInterstitialTimestamp = Time.realtimeSinceStartup;
        }

        void AutoLoadAppOpenAds()
        {
            if (Time.realtimeSinceStartup - _lastTimeLoadAppOpenTimestamp <
                AdSettings.AdLoadingInterval) return;
            if (AdSettings.IsApplovin() && maxAdClient != null) maxAdClient.LoadAppOpen();
            if (AdSettings.IsAdmob() && admobAdClient != null) admobAdClient.LoadAppOpen();
            if (AdSettings.IsLevelPlay() && levelPlayAdClient != null) levelPlayAdClient.LoadAppOpen();
            _lastTimeLoadAppOpenTimestamp = Time.realtimeSinceStartup;
        }

        #endregion

        #region Admob GDPR

#if VIRTUESKY_ADMOB
        private void InitGDPR()
        {
#if UNITY_EDITOR
            InitAdClient();
#else
            string deviceID = SystemInfo.deviceUniqueIdentifier;
            string deviceIDUpperCase = deviceID.ToUpper();

            Debug.Log("TestDeviceHashedId = " + deviceIDUpperCase);
            var request = new ConsentRequestParameters { TagForUnderAgeOfConsent = false };
            if (AdSettings.EnableGDPRTestMode)
            {
                List<string> listDeviceIdTestMode = new List<string>();
                listDeviceIdTestMode.Add(deviceIDUpperCase);
                request.ConsentDebugSettings = new ConsentDebugSettings
                {
                    DebugGeography = DebugGeography.EEA,
                    TestDeviceHashedIds = listDeviceIdTestMode
                };
            }

            ConsentInformation.Update(request, OnConsentInfoUpdated);
#endif
        }

        private void OnConsentInfoUpdated(FormError consentError)
        {
            if (consentError != null)
            {
                Debug.Log("error consentError = " + consentError);
                InitAdClient();
                return;
            }

            if (ConsentInformation.CanRequestAds())
            {
                InitAdClient();
                return;
            }

            ConsentForm.LoadAndShowConsentFormIfRequired((FormError formError) =>
                {
                    if (formError != null)
                    {
                        Debug.Log("error consentError = " + consentError);
                        InitAdClient();
                        return;
                    }

                    Debug.Log("ConsentStatus = " + ConsentInformation.ConsentStatus.ToString());
                    Debug.Log("CanRequestAds = " + ConsentInformation.CanRequestAds());


                    InitAdClient();
                }
            );
        }

        private void LoadAndShowConsentForm()
        {
            Debug.Log("LoadAndShowConsentForm Start!");

            ConsentForm.Load((consentForm, loadError) =>
            {
                if (loadError != null)
                {
                    Debug.Log("error loadError = " + loadError);
                    return;
                }


                consentForm.Show(showError =>
                {
                    if (showError != null)
                    {
                        Debug.Log("error showError = " + showError);
                        return;
                    }
                });
            });
        }

        private void ShowPrivacyOptionsForm()
        {
            Debug.Log("Showing privacy options form.");

            ConsentForm.ShowPrivacyOptionsForm((FormError showError) =>
            {
                if (showError != null)
                {
                    Debug.LogError("Error showing privacy options form with error: " + showError.Message);
                }
            });
        }
#endif

        #endregion

        #region Internal API

        private AdUnit GetBannerAdUnit(AdMediation adMediation)
        {
            switch (adMediation)
            {
                case AdMediation.AppLovin:
                    if (maxAdClient == null)
                    {
                        Debug.LogWarning("MaxAdClient is not initialized, can't get Max banner ad unit.");
                        return null;
                    }

                    return maxAdClient.BannerAdUnit();
                case AdMediation.Admob:
                    if (admobAdClient == null)
                    {
                        Debug.LogWarning("AdmobAdClient is not initialized, can't get Admob banner ad unit.");
                        return null;
                    }

                    return admobAdClient.BannerAdUnit();
                default:
                    if (levelPlayAdClient == null)
                    {
                        Debug.LogWarning("LevelPlayAdClient is not initialized, can't get LevelPlay banner ad unit.");
                        return null;
                    }

                    return levelPlayAdClient.BannerAdUnit();
            }
        }

        private AdUnit GetInterAdUnit(AdMediation adMediation)
        {
            switch (adMediation)
            {
                case AdMediation.AppLovin:
                    if (maxAdClient == null)
                    {
                        Debug.LogWarning("MaxAdClient is not initialized, can't get Max interstitial ad unit.");
                        return null;
                    }

                    return maxAdClient.InterstitialAdUnit();
                case AdMediation.Admob:
                    if (admobAdClient == null)
                    {
                        Debug.LogWarning("AdmobAdClient is not initialized, can't get Admob interstitial ad unit.");
                        return null;
                    }

                    return admobAdClient.InterstitialAdUnit();
                default:
                    if (levelPlayAdClient == null)
                    {
                        Debug.LogWarning("LevelPlayAdClient is not initialized, can't get LevelPlay interstitial ad unit.");
                        return null;
                    }

                    return levelPlayAdClient.InterstitialAdUnit();
            }
        }

        private AdUnit GetRewardAdUnit(AdMediation adMediation)
        {
            switch (adMediation)
            {
                case AdMediation.AppLovin:
                    if (maxAdClient == null)
                    {
                        Debug.LogWarning("MaxAdClient is not initialized, can't get Max reward ad unit.");
                        return null;
                    }

                    return maxAdClient.RewardAdUnit();
                case AdMediation.Admob:
                    if (admobAdClient == null)
                    {
                        Debug.LogWarning("AdmobAdClient is not initialized, can't get Admob reward ad unit.");
                        return null;
                    }

                    return admobAdClient.RewardAdUnit();
                default:
                    if (levelPlayAdClient == null)
                    {
                        Debug.LogWarning("LevelPlayAdClient is not initialized, can't get LevelPlay reward ad unit.");
                        return null;
                    }

                    return levelPlayAdClient.RewardAdUnit();
            }
        }

        private AdUnit GetRewardInterAdUnit(AdMediation adMediation)
        {
            switch (adMediation)
            {
                case AdMediation.AppLovin:
                    if (maxAdClient == null)
                    {
                        Debug.LogWarning("MaxAdClient is not initialized, can't get Max rewarded interstitial ad unit.");
                        return null;
                    }

                    return maxAdClient.RewardedInterstitialAdUnit();
                case AdMediation.Admob:
                    if (admobAdClient == null)
                    {
                        Debug.LogWarning("AdmobAdClient is not initialized, can't get Admob rewarded interstitial ad unit.");
                        return null;
                    }

                    return admobAdClient.RewardedInterstitialAdUnit();
                default:
                    if (levelPlayAdClient == null)
                    {
                        Debug.LogWarning("LevelPlayAdClient is not initialized, can't get LevelPlay rewarded interstitial ad unit.");
                        return null;
                    }

                    return levelPlayAdClient.RewardedInterstitialAdUnit();
            }
        }

        private AdUnit GetAppOpenAdUnit(AdMediation adMediation)
        {
            switch (adMediation)
            {
                case AdMediation.AppLovin:
                    if (maxAdClient == null)
                    {
                        Debug.LogWarning("MaxAdClient is not initialized, can't get Max app open ad unit.");
                        return null;
                    }

                    return maxAdClient.AppOpenAdUnit();
                case AdMediation.Admob:
                    if (admobAdClient == null)
                    {
                        Debug.LogWarning("AdmobAdClient is not initialized, can't get Admob app open ad unit.");
                        return null;
                    }

                    return admobAdClient.AppOpenAdUnit();
                default:
                    if (levelPlayAdClient == null)
                    {
                        Debug.LogWarning("LevelPlayAdClient is not initialized, can't get LevelPlay app open ad unit.");
                        return null;
                    }

                    return levelPlayAdClient.AppOpenAdUnit();
            }
        }

        private AdUnit GetNativeOverlayAdUnit(AdMediation adMediation)
        {
            switch (adMediation)
            {
                case AdMediation.AppLovin:
                    if (maxAdClient == null)
                    {
                        Debug.LogWarning("MaxAdClient is not initialized, can't get Max native overlay ad unit.");
                        return null;
                    }

                    return maxAdClient.NativeOverlayAdUnit();
                case AdMediation.Admob:
                    if (admobAdClient == null)
                    {
                        Debug.LogWarning("AdmobAdClient is not initialized, can't get Admob native overlay ad unit.");
                        return null;
                    }

                    return admobAdClient.NativeOverlayAdUnit();
                default:
                    if (levelPlayAdClient == null)
                    {
                        Debug.LogWarning("LevelPlayAdClient is not initialized, can't get LevelPlay native overlay ad unit.");
                        return null;
                    }

                    return levelPlayAdClient.NativeOverlayAdUnit();
            }
        }

        #endregion

        #region Public API

        // API for single mediation
        /// <summary>
        /// Get ad client for current mediation, return null if the ad client is not supported or not set up in AdSettings. You can call this method to get the ad client for current mediation and then call the method of ad client to load or show ad, for example: Advertising.CurrentAdClient().InterstitialAdUnit().Load(); Note that you should check IsInitAdClient property before calling this method to make sure the ad client is initialized and ready, otherwise it may cause potential error.
        /// </summary>
        /// <returns></returns>
        public static AdClient CurrentAdClient()
        {
            return AdSettings.CurrentMediation switch
            {
                AdMediation.AppLovin => instance.maxAdClient,
                AdMediation.Admob => instance.admobAdClient,
                _ => instance.levelPlayAdClient
            };
        }

        /// <summary>
        /// Get banner ad unit for current mediation, return null if the ad unit is not supported or not set up in AdSettings
        /// </summary>
        /// <returns></returns>
        public static AdUnit BannerAd() => instance.GetBannerAdUnit(AdSettings.CurrentMediation);

        /// <summary>
        /// Get interstitial ad unit for current mediation, return null if the ad unit is not supported or not set up in AdSettings
        /// </summary>
        /// <returns></returns>
        public static AdUnit InterstitialAd() => instance.GetInterAdUnit(AdSettings.CurrentMediation);

        /// <summary>
        /// Get reward ad unit for current mediation, return null if the ad unit is not supported or not set up in AdSettings
        /// </summary>
        /// <returns></returns>
        public static AdUnit RewardAd() => instance.GetRewardAdUnit(AdSettings.CurrentMediation);

        /// <summary>
        /// Get rewarded interstitial ad unit for current mediation, return null if the ad unit is not supported or not set up in AdSettings
        /// </summary>
        /// <returns></returns>
        public static AdUnit RewardedInterstitialAd() => instance.GetRewardInterAdUnit(AdSettings.CurrentMediation);

        /// <summary>
        /// Get app open ad unit for current mediation, return null if the ad unit is not supported or not set up in AdSettings
        /// </summary>
        /// <returns></returns>
        public static AdUnit AppOpenAd() => instance.GetAppOpenAdUnit(AdSettings.CurrentMediation);

        /// <summary>
        /// Get native overlay ad unit for current mediation, return null if the ad unit is not supported or not set up in AdSettings
        /// </summary>
        /// <returns></returns>
        public static AdUnit NativeOverlayAd() => instance.GetNativeOverlayAdUnit(AdSettings.CurrentMediation);

        /// <summary>
        /// Show ad mediation debugger for current mediation, do nothing if the mediation is not supported or not set up in AdSettings
        /// </summary>
        /// <returns></returns>
        public static void ShowAdMediationDebugger() => CurrentAdClient().ShowAdMediationDebugger();

        // API for mutiple medition
        /// <summary>
        /// Get banner ad unit for specific mediation, return null if the ad unit is not supported or not set up in AdSettings
        /// </summary>
        /// <param name="adMediation"></param>
        /// <returns></returns>
        public static AdUnit BannerAd(AdMediation adMediation) => instance.GetBannerAdUnit(adMediation);

        /// <summary>
        /// Get interstitial ad unit for specific mediation, return null if the ad unit is not supported or not set up in AdSettings
        /// </summary>
        /// <param name="adMediation"></param>
        /// <returns></returns>
        public static AdUnit InterstitialAd(AdMediation adMediation) => instance.GetInterAdUnit(adMediation);

        /// <summary>
        /// Get reward ad unit for specific mediation, return null if the ad unit is not supported or not set up in AdSettings
        /// </summary>
        /// <param name="adMediation"></param>
        /// <returns></returns>
        public static AdUnit RewardAd(AdMediation adMediation) => instance.GetRewardAdUnit(adMediation);

        /// <summary>
        /// Get rewarded interstitial ad unit for specific mediation, return null if the ad unit is not supported or not set up in AdSettings
        /// </summary>
        /// <param name="adMediation"></param>
        /// <returns></returns>
        public static AdUnit RewardedInterstitialAd(AdMediation adMediation) => instance.GetRewardInterAdUnit(adMediation);

        /// <summary>
        /// Get app open ad unit for specific mediation, return null if the ad unit is not supported or not set up in AdSettings
        /// </summary>
        /// <param name="adMediation"></param>
        /// <returns></returns>
        public static AdUnit AppOpenAd(AdMediation adMediation) => instance.GetAppOpenAdUnit(adMediation);

        /// <summary>
        /// Get native overlay ad unit for specific mediation, return null if the ad unit is not supported or not set up in AdSettings
        /// </summary>
        /// <param name="adMediation"></param>
        /// <returns></returns>
        public static AdUnit NativeOverlayAd(AdMediation adMediation) => instance.GetNativeOverlayAdUnit(adMediation);

        /// <summary>
        /// Show ad mediation debugger for specific mediation, do nothing if the mediation is not supported or not set up in AdSettings
        /// </summary>
        /// <param name="adMediation"></param>
        public static void ShowAdMediationDebugger(AdMediation adMediation)
        {
            switch (adMediation)
            {
                case AdMediation.AppLovin:
                    if (instance.maxAdClient == null)
                    {
                        Debug.LogWarning("MaxAdClient is not initialized, can't show Max ad mediation debugger.");
                        return;
                    }

                    instance.maxAdClient.ShowAdMediationDebugger();
                    break;
                case AdMediation.Admob:
                    if (instance.admobAdClient == null)
                    {
                        Debug.LogWarning("AdmobAdClient is not initialized, can't show Admob ad mediation debugger.");
                        return;
                    }

                    instance.admobAdClient.ShowAdMediationDebugger();
                    break;
                case AdMediation.LevelPlay:
                    if (instance.levelPlayAdClient == null)
                    {
                        Debug.LogWarning("LevelPlayAdClient is not initialized, can't show LevelPlay ad mediation debugger.");
                        return;
                    }

                    instance.levelPlayAdClient.ShowAdMediationDebugger();
                    break;
            }
        }


        // General
        /// <summary>
        /// Return true if the Advertising instance exists in scene, otherwise return false. Check this before calling any Advertising API to avoid NullReferenceException.
        /// </summary>
        public static bool IsExist => instance != null;

        /// <summary>
        /// Return true if the ad client is initialized, otherwise return false. The ad client will be initialized after the GDPR flow if GDPR is enabled, or initialized directly in Start() if GDPR is not enabled. You can check this property to make sure the ad client is ready before calling any method of ad unit to avoid potential error.
        /// </summary>
        public static bool IsInitAdClient => instance.isInitAdClient;

        public static void Initialization() => instance.InternalInitialization();

#if VIRTUESKY_ADMOB
        /// <summary>
        /// Load and show GDPR consent form, this method is only for Admob mediation and will do nothing if the current mediation is not Admob or the Admob client is not initialized. You can call this method when you want to trigger the GDPR flow again, for example when user click the "Privacy Settings" button in your game. Note that you should call this method before showing any ad to make sure the consent form can be shown successfully, otherwise it may cause potential error. If you just want to show the GDPR consent form without reloading it, you can call ShowAgainGdpr() method.
        /// </summary>
        public static void LoadAndShowGdpr() => instance.LoadAndShowConsentForm();

        /// <summary>
        /// Show GDPR consent form if it's already loaded, this method is only for Admob mediation and will do nothing if the current mediation is not Admob or the Admob client is not initialized. You can call this method when you want to show the GDPR consent form again without reloading it, for example when user click the "Privacy Settings" button in your game. Note that you should call this method before showing any ad to make sure the consent form can be shown successfully, otherwise it may cause potential error. If you want to reload and show the GDPR consent form, you can call LoadAndShowGdpr() method.
        /// </summary>
        public static void ShowAgainGdpr() => instance.ShowPrivacyOptionsForm();
#endif

        #endregion
    }
}