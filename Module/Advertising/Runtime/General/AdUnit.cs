using System;
using UnityEngine;

namespace VirtueSky.Ads
{
    [Serializable]
    public abstract class AdUnit
    {
        [SerializeField] protected string androidId;

        [SerializeField] protected string iOSId;


        [NonSerialized] internal Action loadedCallback;
        [NonSerialized] internal Action failedToLoadCallback;
        [NonSerialized] internal Action displayedCallback;
        [NonSerialized] internal Action failedToDisplayCallback;
        [NonSerialized] internal Action closedCallback;
        [NonSerialized] internal Action clickedCallback;
        [NonSerialized] public Action<double, string, string, string, string> paidedCallback;

        public Action OnLoadAdEvent;
        public Action<string> OnFailedToLoadAdEvent;
        public Action OnDisplayedAdEvent;
        public Action<string> OnFailedToDisplayAdEvent;
        public Action OnClosedAdEvent;
        public Action OnClickedAdEvent;

        [NonSerialized] private string runtimeId = String.Empty;

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

        public virtual AdUnit Show()
        {
            ResetChainCallback();
            if (!Application.isMobilePlatform || string.IsNullOrEmpty(Id) || AdStatic.IsRemoveAd || !IsReady())
                return this;
            ShowImpl();
            return this;
        }

        protected virtual void ResetChainCallback()
        {
            loadedCallback = null;
            failedToDisplayCallback = null;
            failedToLoadCallback = null;
            closedCallback = null;
        }

        protected abstract void ShowImpl();
        public abstract void Destroy();

        public virtual void HideBanner()
        {
        }
    }
}