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
        private IEnumerator autoLoadAdCoroutine;
        private float _lastTimeLoadInterstitialAdTimestamp = DEFAULT_TIMESTAMP;
        private float _lastTimeLoadRewardedTimestamp = DEFAULT_TIMESTAMP;
        private float _lastTimeLoadRewardedInterstitialTimestamp = DEFAULT_TIMESTAMP;
        private float _lastTimeLoadAppOpenTimestamp = DEFAULT_TIMESTAMP;
        private const float DEFAULT_TIMESTAMP = -1000;

        private AdClient maxAdClient;
        private AdClient admobAdClient;
        private AdClient ironSourceAdClient;

        private bool isInitAdClient = false;

        private static event Func<AdNetwork, AdUnit> OnGetBannerAdEvent;
        private static event Func<AdNetwork, AdUnit> OnGetInterAdEvent;
        private static event Func<AdNetwork, AdUnit> OnGetRewardAdEvent;
        private static event Func<AdNetwork, AdUnit> OnGetRewardInterEvent;
        private static event Func<AdNetwork, AdUnit> OnGetAppOpenAdEvent;
        private static event Func<AdNetwork, AdUnit> OnGetNativeOverlayEvent;
        private static event Func<bool> OnInitAdClientEvent;
        private static event Action OnLoadAndShowGdprEvent;
        private static event Action OnShowGdprAgainEvent;


        private void OnEnable()
        {
            OnGetBannerAdEvent += GetBannerAdUnit;
            OnGetInterAdEvent += GetInterAdUnit;
            OnGetRewardAdEvent += GetRewardAdUnit;
            OnGetRewardInterEvent += GetRewardInterAdUnit;
            OnGetAppOpenAdEvent += GetAppOpenAdUnit;
            OnGetNativeOverlayEvent += GetNativeOverlayAdUnit;
            OnInitAdClientEvent += InternalIsInitAdClient;
#if VIRTUESKY_ADMOB
            OnLoadAndShowGdprEvent += LoadAndShowConsentForm;
            OnShowGdprAgainEvent += ShowPrivacyOptionsForm;
#endif
        }

        private void OnDisable()
        {
            OnGetBannerAdEvent -= GetBannerAdUnit;
            OnGetInterAdEvent -= GetInterAdUnit;
            OnGetRewardAdEvent -= GetRewardAdUnit;
            OnGetRewardInterEvent -= GetRewardInterAdUnit;
            OnGetAppOpenAdEvent -= GetAppOpenAdUnit;
            OnGetNativeOverlayEvent -= GetNativeOverlayAdUnit;
            OnInitAdClientEvent -= InternalIsInitAdClient;
#if VIRTUESKY_ADMOB
            OnLoadAndShowGdprEvent -= LoadAndShowConsentForm;
            OnShowGdprAgainEvent -= ShowPrivacyOptionsForm;
#endif
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
            var adSettings = AdSettings.Instance;

            if (AdSettings.UseMax)
            {
                maxAdClient = new MaxAdClient();
                maxAdClient.Initialize();
                Debug.Log($"Use AdClient: {maxAdClient}".SetColor(Color.cyan));
            }

            if (AdSettings.UseAdmob)
            {
                admobAdClient = new AdmobClient();
                admobAdClient.Initialize();
                Debug.Log($"Use AdClient: {admobAdClient}".SetColor(Color.cyan));
            }

            if (AdSettings.UseIronSource)
            {
                ironSourceAdClient = new IronSourceClient();
                ironSourceAdClient.Initialize();
                Debug.Log($"Use AdClient: {ironSourceAdClient}".SetColor(Color.cyan));
            }

            isInitAdClient = true;
            InitAutoLoadAds();
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
            AdStatic.isShowingAd = state;
        }

        #region Method Load Ads

        void AutoLoadInterAds()
        {
            if (Time.realtimeSinceStartup - _lastTimeLoadInterstitialAdTimestamp <
                AdSettings.AdLoadingInterval) return;
            if (AdSettings.UseMax) maxAdClient.LoadInterstitial();
            if (AdSettings.UseAdmob) admobAdClient.LoadInterstitial();
            if (AdSettings.UseIronSource) ironSourceAdClient.LoadInterstitial();
            _lastTimeLoadInterstitialAdTimestamp = Time.realtimeSinceStartup;
        }

        void AutoLoadRewardAds()
        {
            if (Time.realtimeSinceStartup - _lastTimeLoadRewardedTimestamp <
                AdSettings.AdLoadingInterval) return;
            if (AdSettings.UseMax) maxAdClient.LoadRewarded();
            if (AdSettings.UseAdmob) admobAdClient.LoadRewarded();
            if (AdSettings.UseIronSource) ironSourceAdClient.LoadRewarded();
            _lastTimeLoadRewardedTimestamp = Time.realtimeSinceStartup;
        }

        void AutoLoadRewardInterAds()
        {
            if (Time.realtimeSinceStartup - _lastTimeLoadRewardedInterstitialTimestamp <
                AdSettings.AdLoadingInterval) return;
            if (AdSettings.UseMax) maxAdClient.LoadRewardedInterstitial();
            if (AdSettings.UseAdmob) admobAdClient.LoadRewardedInterstitial();
            if (AdSettings.UseIronSource) ironSourceAdClient.LoadRewardedInterstitial();
            _lastTimeLoadRewardedInterstitialTimestamp = Time.realtimeSinceStartup;
        }

        void AutoLoadAppOpenAds()
        {
            if (Time.realtimeSinceStartup - _lastTimeLoadAppOpenTimestamp <
                AdSettings.AdLoadingInterval) return;
            if (AdSettings.UseMax) maxAdClient.LoadAppOpen();
            if (AdSettings.UseAdmob) admobAdClient.LoadAppOpen();
            if (AdSettings.UseIronSource) ironSourceAdClient.LoadAppOpen();
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

            ConsentForm.LoadAndShowConsentFormIfRequired(
                (FormError formError) =>
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

        private AdUnit GetBannerAdUnit(AdNetwork adNetwork)
        {
            return adNetwork switch
            {
                AdNetwork.Max => maxAdClient.BannerAdUnit(),
                AdNetwork.Admob => admobAdClient.BannerAdUnit(),
                _ => ironSourceAdClient.BannerAdUnit()
            };
        }

        private AdUnit GetInterAdUnit(AdNetwork adNetwork)
        {
            return adNetwork switch
            {
                AdNetwork.Max => maxAdClient.InterstitialAdUnit(),
                AdNetwork.Admob => admobAdClient.InterstitialAdUnit(),
                _ => ironSourceAdClient.InterstitialAdUnit()
            };
        }

        private AdUnit GetRewardAdUnit(AdNetwork adNetwork)
        {
            return adNetwork switch
            {
                AdNetwork.Max => maxAdClient.RewardAdUnit(),
                AdNetwork.Admob => admobAdClient.RewardAdUnit(),
                _ => ironSourceAdClient.RewardAdUnit()
            };
        }

        private AdUnit GetRewardInterAdUnit(AdNetwork adNetwork)
        {
            return adNetwork switch
            {
                AdNetwork.Max => maxAdClient.RewardedInterstitialAdUnit(),
                AdNetwork.Admob => admobAdClient.RewardedInterstitialAdUnit(),
                _ => ironSourceAdClient.RewardedInterstitialAdUnit()
            };
        }

        private AdUnit GetAppOpenAdUnit(AdNetwork adNetwork)
        {
            return adNetwork switch
            {
                AdNetwork.Max => maxAdClient.AppOpenAdUnit(),
                AdNetwork.Admob => admobAdClient.AppOpenAdUnit(),
                _ => ironSourceAdClient.AppOpenAdUnit()
            };
        }

        private AdUnit GetNativeOverlayAdUnit(AdNetwork adNetwork)
        {
            return adNetwork switch
            {
                AdNetwork.Max => maxAdClient.NativeOverlayAdUnit(),
                AdNetwork.Admob => admobAdClient.NativeOverlayAdUnit(),
                _ => ironSourceAdClient.NativeOverlayAdUnit()
            };
        }

        private bool InternalIsInitAdClient() => isInitAdClient;

        #endregion

        #region Public API

        public static AdUnit BannerAd(AdNetwork adNetwork) => OnGetBannerAdEvent?.Invoke(adNetwork);
        public static AdUnit InterstitialAd(AdNetwork adNetwork) => OnGetInterAdEvent?.Invoke(adNetwork);
        public static AdUnit RewardAd(AdNetwork adNetwork) => OnGetRewardAdEvent?.Invoke(adNetwork);
        public static AdUnit RewardedInterstitialAd(AdNetwork adNetwork) => OnGetRewardInterEvent?.Invoke(adNetwork);
        public static AdUnit AppOpenAd(AdNetwork adNetwork) => OnGetAppOpenAdEvent?.Invoke(adNetwork);
        public static AdUnit NativeOverlayAd(AdNetwork adNetwork) => OnGetNativeOverlayEvent?.Invoke(adNetwork);
        public static bool IsInitAdClient => (bool)OnInitAdClientEvent?.Invoke();

#if VIRTUESKY_ADMOB
        public static void LoadAndShowGdpr() => OnLoadAndShowGdprEvent?.Invoke();
        public static void ShowAgainGdpr() => OnShowGdprAgainEvent?.Invoke();
#endif

        #endregion
    }
}