#if VIRTUESKY_APPSFLYER
using AppsFlyerSDK;
#endif

using System;
using UnityEngine;

namespace VirtueSky.Tracking
{
#if VIRTUESKY_APPSFLYER
    public class AppsFlyerObject : MonoBehaviour, IAppsFlyerConversionData
#else
    public class AppsFlyerObject : MonoBehaviour
#endif
    {
        public static event Action OnAfterInitEvent;
        public static event Action<string> OnConversionDataSuccessEvent;
        public static event Action<string> OnConversionDataFailEvent;
        public static event Action<string> OnAppOpenAttributionEvent;
        public static event Action<string> OnAppOpenAttributionFailureEvent;


        private void Awake()
        {
#if !UNITY_EDITOR
            DontDestroyOnLoad(this);
#endif
        }

        private void Start()
        {
#if VIRTUESKY_APPSFLYER
            // These fields are set from the editor so do not modify!
            //******************************//
            AppsFlyer.setIsDebug(AppsFlyerConfig.IsDebug);
#if UNITY_WSA_10_0 && !UNITY_EDITOR
            AppsFlyer.initSDK(AppsFlyerSetting.DevKey, AppsFlyerSetting.UWPAppID,
                AppsFlyerSetting.GetConversionData ? this : null);
#elif UNITY_STANDALONE_OSX && !UNITY_EDITOR
            AppsFlyer.initSDK(AppsFlyerSetting.DevKey, AppsFlyerSetting.MacOSAppID,
                AppsFlyerSetting.GetConversionData ? this : null);
#else

            AppsFlyer.initSDK(AppsFlyerConfig.DevKey, AppsFlyerConfig.AppID,
                AppsFlyerConfig.GetConversionData ? this : null);
#endif
            OnAfterInitEvent?.Invoke();
            //******************************/

            if (AppsFlyerConfig.AutoStartSDK)
            {
                AppsFlyer.startSDK();
            }

#endif
        }

        public static void StartSDK()
        {
#if VIRTUESKY_APPSFLYER
            AppsFlyer.startSDK();
#endif
        }

        public void onConversionDataSuccess(string conversionData)
        {
            OnConversionDataSuccessEvent?.Invoke(conversionData);
        }

        public void onConversionDataFail(string error)
        {
            OnConversionDataFailEvent?.Invoke(error);
        }

        public void onAppOpenAttribution(string attributionData)
        {
            OnAppOpenAttributionEvent?.Invoke(attributionData);
        }

        public void onAppOpenAttributionFailure(string error)
        {
            OnAppOpenAttributionFailureEvent?.Invoke(error);
        }
    }
}