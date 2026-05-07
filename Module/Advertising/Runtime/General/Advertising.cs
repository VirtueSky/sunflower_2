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
        }

        private void Start()
        {
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
            AppTracking.Init(AdSettings.EnableTrackAdRevenue);
            if (IsApplovin()) InitApplovinClient();
            if (IsAdmob()) InitAdmobClient();
            if (IsLevelPlay()) InitLevelPlayClient();
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

        private bool IsApplovin()
        {
            return (AdSettings.MediationLoadMode == MediationLoadMode.Multiple && AdSettings.UseAppLovin) ||
                   (AdSettings.MediationLoadMode == MediationLoadMode.Single && AdSettings.CurrentMediation == AdMediation.AppLovin);
        }

        private bool IsAdmob()
        {
            return (AdSettings.MediationLoadMode == MediationLoadMode.Multiple && AdSettings.UseAdmob) ||
                   (AdSettings.MediationLoadMode == MediationLoadMode.Single && AdSettings.CurrentMediation == AdMediation.Admob);
        }

        private bool IsLevelPlay()
        {
            return (AdSettings.MediationLoadMode == MediationLoadMode.Multiple && AdSettings.UseLevelPlay) ||
                   (AdSettings.MediationLoadMode == MediationLoadMode.Single && AdSettings.CurrentMediation == AdMediation.LevelPlay);
        }

        #region Method Load Ads

        void AutoLoadInterAds()
        {
            if (Time.realtimeSinceStartup - _lastTimeLoadInterstitialAdTimestamp <
                AdSettings.AdLoadingInterval) return;
            if (IsApplovin() && maxAdClient != null) maxAdClient.LoadInterstitial();
            if (IsAdmob() && admobAdClient != null) admobAdClient.LoadInterstitial();
            if (IsLevelPlay() && levelPlayAdClient != null) levelPlayAdClient.LoadInterstitial();
            _lastTimeLoadInterstitialAdTimestamp = Time.realtimeSinceStartup;
        }

        void AutoLoadRewardAds()
        {
            if (Time.realtimeSinceStartup - _lastTimeLoadRewardedTimestamp <
                AdSettings.AdLoadingInterval) return;
            if (IsApplovin() && maxAdClient != null) maxAdClient.LoadRewarded();
            if (IsAdmob() && admobAdClient != null) admobAdClient.LoadRewarded();
            if (IsLevelPlay() && levelPlayAdClient != null) levelPlayAdClient.LoadRewarded();
            _lastTimeLoadRewardedTimestamp = Time.realtimeSinceStartup;
        }

        void AutoLoadRewardInterAds()
        {
            if (Time.realtimeSinceStartup - _lastTimeLoadRewardedInterstitialTimestamp <
                AdSettings.AdLoadingInterval) return;
            if (IsApplovin() && maxAdClient != null) maxAdClient.LoadRewardedInterstitial();
            if (IsAdmob() && admobAdClient != null) admobAdClient.LoadRewardedInterstitial();
            if (IsLevelPlay() && levelPlayAdClient != null) levelPlayAdClient.LoadRewardedInterstitial();
            _lastTimeLoadRewardedInterstitialTimestamp = Time.realtimeSinceStartup;
        }

        void AutoLoadAppOpenAds()
        {
            if (Time.realtimeSinceStartup - _lastTimeLoadAppOpenTimestamp <
                AdSettings.AdLoadingInterval) return;
            if (IsApplovin() && maxAdClient != null) maxAdClient.LoadAppOpen();
            if (IsAdmob() && admobAdClient != null) admobAdClient.LoadAppOpen();
            if (IsLevelPlay() && levelPlayAdClient != null) levelPlayAdClient.LoadAppOpen();
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
                return;
            }

            ConsentForm.LoadAndShowConsentFormIfRequired((FormError formError) =>
                {
                    if (formError != null)
                    {
                        Debug.Log("error consentError = " + consentError);
                        return;
                    }

                    Debug.Log("ConsentStatus = " + ConsentInformation.ConsentStatus.ToString());
                    Debug.Log("CanRequestAds = " + ConsentInformation.CanRequestAds());


                    if (ConsentInformation.CanRequestAds())
                    {
                        MobileAds.RaiseAdEventsOnUnityMainThread = true;
                        InitAdClient();
                    }
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
            return adMediation switch
            {
                AdMediation.AppLovin => maxAdClient.BannerAdUnit(),
                AdMediation.Admob => admobAdClient.BannerAdUnit(),
                _ => levelPlayAdClient.BannerAdUnit()
            };
        }

        private AdUnit GetInterAdUnit(AdMediation adMediation)
        {
            return adMediation switch
            {
                AdMediation.AppLovin => maxAdClient.InterstitialAdUnit(),
                AdMediation.Admob => admobAdClient.InterstitialAdUnit(),
                _ => levelPlayAdClient.InterstitialAdUnit()
            };
        }

        private AdUnit GetRewardAdUnit(AdMediation adMediation)
        {
            return adMediation switch
            {
                AdMediation.AppLovin => maxAdClient.RewardAdUnit(),
                AdMediation.Admob => admobAdClient.RewardAdUnit(),
                _ => levelPlayAdClient.RewardAdUnit()
            };
        }

        private AdUnit GetRewardInterAdUnit(AdMediation adMediation)
        {
            return adMediation switch
            {
                AdMediation.AppLovin => maxAdClient.RewardedInterstitialAdUnit(),
                AdMediation.Admob => admobAdClient.RewardedInterstitialAdUnit(),
                _ => levelPlayAdClient.RewardedInterstitialAdUnit()
            };
        }

        private AdUnit GetAppOpenAdUnit(AdMediation adMediation)
        {
            return adMediation switch
            {
                AdMediation.AppLovin => maxAdClient.AppOpenAdUnit(),
                AdMediation.Admob => admobAdClient.AppOpenAdUnit(),
                _ => levelPlayAdClient.AppOpenAdUnit()
            };
        }

        private AdUnit GetNativeOverlayAdUnit(AdMediation adMediation)
        {
            return adMediation switch
            {
                AdMediation.AppLovin => maxAdClient.NativeOverlayAdUnit(),
                AdMediation.Admob => admobAdClient.NativeOverlayAdUnit(),
                _ => levelPlayAdClient.NativeOverlayAdUnit()
            };
        }

        #endregion

        #region Public API

        // API for single mediation
        public static AdClient CurrentAdClient()
        {
            return AdSettings.CurrentMediation switch
            {
                AdMediation.AppLovin => instance.maxAdClient,
                AdMediation.Admob => instance.admobAdClient,
                _ => instance.levelPlayAdClient
            };
        }

        public static AdUnit BannerAd() => instance.GetBannerAdUnit(AdSettings.CurrentMediation);
        public static AdUnit InterstitialAd() => instance.GetInterAdUnit(AdSettings.CurrentMediation);
        public static AdUnit RewardAd() => instance.GetRewardAdUnit(AdSettings.CurrentMediation);
        public static AdUnit RewardedInterstitialAd() => instance.GetRewardInterAdUnit(AdSettings.CurrentMediation);
        public static AdUnit AppOpenAd() => instance.GetAppOpenAdUnit(AdSettings.CurrentMediation);
        public static AdUnit NativeOverlayAd() => instance.GetNativeOverlayAdUnit(AdSettings.CurrentMediation);

        // API for mutiple medition
        public static AdUnit BannerAd(AdMediation adMediation) => instance.GetBannerAdUnit(adMediation);
        public static AdUnit InterstitialAd(AdMediation adMediation) => instance.GetInterAdUnit(adMediation);
        public static AdUnit RewardAd(AdMediation adMediation) => instance.GetRewardAdUnit(adMediation);
        public static AdUnit RewardedInterstitialAd(AdMediation adMediation) => instance.GetRewardInterAdUnit(adMediation);
        public static AdUnit AppOpenAd(AdMediation adMediation) => instance.GetAppOpenAdUnit(adMediation);
        public static AdUnit NativeOverlayAd(AdMediation adMediation) => instance.GetNativeOverlayAdUnit(adMediation);

        // General
        public static bool IsInitAdClient => instance.isInitAdClient;

#if VIRTUESKY_ADMOB
        public static void LoadAndShowGdpr() => instance.LoadAndShowConsentForm();
        public static void ShowAgainGdpr() => instance.ShowPrivacyOptionsForm();
#endif

        #endregion
    }
}