using System;
using UnityEngine;

namespace VirtueSky.Ads
{
    [Serializable]
    public abstract class AdUnit
    {
        [SerializeField] protected string androidId;

        [SerializeField] protected string iOSId;


        [NonSerialized] internal Action<AdsInfo> loadedCallback;
        [NonSerialized] internal Action<AdsError> failedToLoadCallback;
        [NonSerialized] internal Action<AdsInfo> displayedCallback;
        [NonSerialized] internal Action<AdsError> failedToDisplayCallback;
        [NonSerialized] internal Action<AdsInfo> closedCallback;
        [NonSerialized] internal Action<AdsInfo> clickedCallback;
        [NonSerialized] public Action<double, string, string, string, string> paidedCallback;

        public Action<AdsInfo> OnLoadAdEvent;
        public Action<AdsError> OnFailedToLoadAdEvent;
        public Action<AdsInfo> OnDisplayedAdEvent;
        public Action<AdsError> OnFailedToDisplayAdEvent;
        public Action<AdsInfo> OnClosedAdEvent;
        public Action<AdsInfo> OnClickedAdEvent;

        [NonSerialized] private string runtimeId = String.Empty;
        
        public abstract bool IsShowing { get; internal set; }

        public string Id
        {
            get
            {
                if (runtimeId == string.Empty)
                {
#if UNITY_ANDROID
                    return androidId;
#elif UNITY_IOS
                    return iOSId;
#else
                    return string.Empty;
#endif
                }

                return runtimeId;
            }
        }

        public void SetIdRuntime(string unitId)
        {
            runtimeId = unitId;
        }

        public abstract void Init();
        public abstract void Load();
        public abstract bool IsReady();

        public virtual AdUnit Show(string placement = null)
        {
            ResetChainCallback();
            if (!Application.isMobilePlatform || string.IsNullOrEmpty(Id) || AdStatic.IsRemoveAd || !IsReady())
                return this;
            ShowImpl(placement);
            return this;
        }

        protected virtual void ResetChainCallback()
        {
            loadedCallback = null;
            failedToDisplayCallback = null;
            failedToLoadCallback = null;
            closedCallback = null;
        }

        protected abstract void ShowImpl(string placement = null);
        public abstract void Destroy();

        public virtual void HideBanner()
        {
        }
    }
}