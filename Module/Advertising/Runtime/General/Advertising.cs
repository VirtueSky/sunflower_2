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

        private bool isInitAllAdClient = false;
        private bool isInititalization = false;
        private int pendingAdClientInitializations;
        private int mainThreadId;

        private void Awake()
        {
            mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
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
            isInitAllAdClient = false;
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
            // Callers like UMP consent callbacks run on a native (Java) thread, but everything below touches Unity API
            // (App.Delay -> Time.realtimeSinceStartup, StartCoroutine, SDK init), so always continue on main thread.
            if (System.Threading.Thread.CurrentThread.ManagedThreadId != mainThreadId)
            {
                App.RunOnMainThread(InitAdClient);
                return;
            }

            bool initializeAppLovin = AdSettings.IsApplovin();
            bool initializeAdmob = AdSettings.IsAdmob();
            bool initializeLevelPlay = AdSettings.IsLevelPlay();

            pendingAdClientInitializations = 0;
            if (initializeAppLovin) pendingAdClientInitializations++;
            if (initializeAdmob) pendingAdClientInitializations++;
            if (initializeLevelPlay) pendingAdClientInitializations++;

            if (pendingAdClientInitializations == 0)
            {
                CompleteAdClientInitialization();
                return;
            }

            if (initializeAppLovin)
                ScheduleAdClientInitialization(AdSettings.AppLovinInitDelay, InitApplovinClient, "AppLovin");
            if (initializeAdmob)
                ScheduleAdClientInitialization(AdSettings.AdmobInitDelay, InitAdmobClient, "AdMob");
            if (initializeLevelPlay)
                ScheduleAdClientInitialization(AdSettings.LevelPlayInitDelay, InitLevelPlayClient, "LevelPlay");
        }

        private void ScheduleAdClientInitialization(float delay, Action initialize, string mediationName)
        {
            if (delay <= 0f)
            {
                InitializeAdClient(initialize);
                return;
            }

            Debug.Log($"Initialize {mediationName} AdClient after {delay:0.##} seconds.".SetColor(Color.cyan));
            App.Delay(this, delay, () => InitializeAdClient(initialize), useRealTime: true);
        }

        private void InitializeAdClient(Action initialize)
        {
            initialize();

            // Start retries as soon as the first mediation begins initializing; delayed mediations load independently
            // from their own SDK initialization callbacks.
            if (autoLoadAdCoroutine == null) InitAutoLoadAds();

            pendingAdClientInitializations--;
            if (pendingAdClientInitializations == 0) CompleteAdClientInitialization();
        }

        private void CompleteAdClientInitialization()
        {
            isInitAllAdClient = true;
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

        void AutoLoadInterAds() =>
            AutoLoad<IInterstitialClient>(ref _lastTimeLoadInterstitialAdTimestamp, c => c.LoadInterstitial());

        void AutoLoadRewardAds() =>
            AutoLoad<IRewardClient>(ref _lastTimeLoadRewardedTimestamp, c => c.LoadRewarded());

        void AutoLoadRewardInterAds() =>
            AutoLoad<IRewardedInterstitialClient>(ref _lastTimeLoadRewardedInterstitialTimestamp, c => c.LoadRewardedInterstitial());

        void AutoLoadAppOpenAds() =>
            AutoLoad<IAppOpenClient>(ref _lastTimeLoadAppOpenTimestamp, c => c.LoadAppOpen());

        /// <summary>
        /// Calls <paramref name="load"/> on every enabled, SDK-ready client that supports <typeparamref name="TCapability"/>.
        /// </summary>
        private void AutoLoad<TCapability>(ref float lastLoadTimestamp, Action<TCapability> load) where TCapability : class
        {
            if (Time.realtimeSinceStartup - lastLoadTimestamp < AdSettings.AdLoadingInterval) return;
            LoadIfReady(AdSettings.IsApplovin(), maxAdClient, load);
            LoadIfReady(AdSettings.IsAdmob(), admobAdClient, load);
            LoadIfReady(AdSettings.IsLevelPlay(), levelPlayAdClient, load);
            lastLoadTimestamp = Time.realtimeSinceStartup;
        }

        private static void LoadIfReady<TCapability>(bool isEnabled, AdClient client, Action<TCapability> load) where TCapability : class
        {
            if (isEnabled && client is { SdkInitializationCompleted: true } && client is TCapability capability) load(capability);
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

            // UMP callbacks are invoked on a Java thread, marshal them back to Unity main thread
            ConsentInformation.Update(request, App.ToMainThread<FormError>(OnConsentInfoUpdated));
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

            ConsentForm.LoadAndShowConsentFormIfRequired(App.ToMainThread<FormError>(formError =>
                {
                    if (formError != null)
                    {
                        Debug.Log("error formError = " + formError);
                        InitAdClient();
                        return;
                    }

                    Debug.Log("ConsentStatus = " + ConsentInformation.ConsentStatus.ToString());
                    Debug.Log("CanRequestAds = " + ConsentInformation.CanRequestAds());


                    InitAdClient();
                })
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

        private AdClient GetAdClient(AdMediation adMediation)
        {
            return adMediation switch
            {
                AdMediation.AppLovin => maxAdClient,
                AdMediation.Admob => admobAdClient,
                _ => levelPlayAdClient
            };
        }

        private AdClient GetInitializedAdClient(AdMediation adMediation, string featureName)
        {
            var client = GetAdClient(adMediation);
            if (client == null) Debug.LogWarning($"{adMediation} AdClient is not initialized, can't use {adMediation} {featureName}.");
            return client;
        }

        /// <summary>
        /// Returns the ad unit of the given format, or null if the client is not initialized or the mediation doesn't support that format.
        /// </summary>
        private AdUnit GetAdUnit<TCapability>(AdMediation adMediation, Func<TCapability, AdUnit> getAdUnit, string adName)
            where TCapability : class
        {
            var client = GetInitializedAdClient(adMediation, $"{adName} ad unit");
            return client is TCapability capability ? getAdUnit(capability) : null;
        }

        private AdUnit GetBannerAdUnit(AdMediation adMediation) =>
            GetAdUnit<IBannerClient>(adMediation, c => c.BannerAdUnit(), "banner");

        private AdUnit GetInterAdUnit(AdMediation adMediation) =>
            GetAdUnit<IInterstitialClient>(adMediation, c => c.InterstitialAdUnit(), "interstitial");

        private AdUnit GetRewardAdUnit(AdMediation adMediation) =>
            GetAdUnit<IRewardClient>(adMediation, c => c.RewardAdUnit(), "reward");

        private AdUnit GetRewardInterAdUnit(AdMediation adMediation) =>
            GetAdUnit<IRewardedInterstitialClient>(adMediation, c => c.RewardedInterstitialAdUnit(), "rewarded interstitial");

        private AdUnit GetAppOpenAdUnit(AdMediation adMediation) =>
            GetAdUnit<IAppOpenClient>(adMediation, c => c.AppOpenAdUnit(), "app open");

        private AdUnit GetNativeOverlayAdUnit(AdMediation adMediation) =>
            GetAdUnit<INativeOverlayClient>(adMediation, c => c.NativeOverlayAdUnit(), "native overlay");

        #endregion

        #region Public API

        // API for single mediation
        /// <summary>
        /// Get ad client for current mediation, return null if the ad client is not supported or not set up in AdSettings. You can call this method to get the ad client for current mediation and then call the method of ad client to load or show ad, for example: if (Advertising.CurrentAdClient() is IInterstitialClient client) client.LoadInterstitial(); Ad formats are exposed through capability interfaces (IBannerClient, IInterstitialClient, IRewardClient, IRewardedInterstitialClient, IAppOpenClient, INativeOverlayClient), so check whether the client implements the one you need. Note that you should check IsInitAdClient property before calling this method to make sure the ad client is initialized and ready, otherwise it may cause potential error.
        /// </summary>
        /// <returns></returns>
        public static AdClient CurrentAdClient() => instance.GetAdClient(AdSettings.CurrentMediation);

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
        public static void ShowAdMediationDebugger() => ShowAdMediationDebugger(AdSettings.CurrentMediation);

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
        public static void ShowAdMediationDebugger(AdMediation adMediation) =>
            instance.GetInitializedAdClient(adMediation, "ad mediation debugger")?.ShowAdMediationDebugger();


        // General
        /// <summary>
        /// Return true if the Advertising instance exists in scene, otherwise return false. Check this before calling any Advertising API to avoid NullReferenceException.
        /// </summary>
        public static bool IsExist => instance != null;

        /// <summary>
        /// Return true after all enabled ad clients have started initialization, including their configured delays. You can check this property before accessing an ad client to avoid potential errors.
        /// </summary>
        public static bool IsInitAllAdClient => instance.isInitAllAdClient;

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