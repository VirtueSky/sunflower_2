using System;
using UnityEngine;

namespace VirtueSky.Iap
{
    [Serializable]
    public class IapDataProduct
    {
//#if UNITY_ANDROID
        public string androidId;

//#elif UNITY_IOS
        public string iOSId;

//#endif
        public IapProductType iapProductType;

        [Tooltip("Config price for UI setup or tracking")]
        public float price;

        [NonSerialized] public Action purchaseSuccessCallback;
        [NonSerialized] public Action purchaseFailedCallback;

        public string Id
        {
            get
            {
#if UNITY_ANDROID
                return androidId;
#elif UNITY_IOS
                return iOSId;
#else
                return string.Empty;
#endif
            }
        }
    }

    public enum IapProductType
    {
        /// <summary>
        /// Consumables may be purchased more than once.
        ///
        /// Purchase history is not typically retained by store
        /// systems once consumed.
        /// </summary>
        Consumable,

        /// <summary>
        /// Non consumables cannot be repurchased and are owned indefinitely.
        /// </summary>
        NonConsumable,

        /// <summary>
        /// Subscriptions have a finite window of validity.
        /// </summary>
        Subscription
    }
}