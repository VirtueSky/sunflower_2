using System;
using UnityEngine;

#if VIRTUESKY_IAP
using UnityEngine.Purchasing;
#endif

using VirtueSky.Inspector;

namespace VirtueSky.Iap
{
    [Serializable]
    public class IapDataProduct
    {
        [ReadOnly] public string androidId;
        [ReadOnly] public string iOSId;
        [ReadOnly] public IapProductType iapProductType;

        [Tooltip("Config price for UI setup or tracking")] [ReadOnly]
        public float priceConfig;

        [NonSerialized] public Action purchaseSuccessCallback;
        [NonSerialized] public Action<string> purchaseFailedCallback;

        public IapDataProduct(string androidId, string iosId, IapProductType iapProductType, float priceConfig)
        {
            this.androidId = androidId;
            this.iOSId = iosId;
            this.iapProductType = iapProductType;
            this.priceConfig = priceConfig;
        }

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

#if VIRTUESKY_IAP
        public Product GetProduct()
        {
            return IapManager.GetProduct(this);
        }

        public SubscriptionInfo GetSubscriptionInfo()
        {
            return IapManager.GetSubscriptionInfo(this);
        }

        public void Purchase()
        {
            IapManager.PurchaseProduct(this);
        }

        public bool IsPurchased()
        {
            return IapManager.IsPurchasedProduct(this);
        }

        public string GetLocalizedPriceString()
        {
            if (GetProduct() == null) return String.Empty;
            return GetProduct().metadata.localizedPriceString;
        }

        public string GetIsoCurrencyCode()
        {
            if (GetProduct() == null) return String.Empty;
            return GetProduct().metadata.isoCurrencyCode;
        }

        public string GetLocalizedDescription()
        {
            if (GetProduct() == null) return String.Empty;
            return GetProduct().metadata.localizedDescription;
        }

        public string GetLocalizedTitle()
        {
            if (GetProduct() == null) return String.Empty;
            return GetProduct().metadata.localizedTitle;
        }

        public decimal GetLocalizedPrice()
        {
            if (GetProduct() == null) return 0;
            return GetProduct().metadata.localizedPrice;
        }
#endif
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