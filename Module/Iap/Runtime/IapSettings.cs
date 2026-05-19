using System;
using System.Collections.Generic;
using UnityEngine;
using VirtueSky.Core;
using VirtueSky.Inspector;
using VirtueSky.Utils;

namespace VirtueSky.Iap
{
    [EditorIcon("icon_scriptable")]
    public class IapSettings : ScriptableSettings<IapSettings>
    {
        [SerializeField] private CoreEnum.RuntimeInitType runtimeInitType;

        [SerializeField] private List<IapData> skusData = new List<IapData>();

        // [SerializeField] private List<IapDataProduct> products = new List<IapDataProduct>();
        [SerializeField] private bool isValidatePurchase = true;
        [SerializeField] private bool isCustomValidatePurchase;
        [SerializeField] private ValidatePurchase validatePurchase;
#if UNITY_EDITOR
        [SerializeField, TextArea] private string googlePlayStoreKey;
        public string GooglePlayStoreKey => googlePlayStoreKey;
#endif
        public static CoreEnum.RuntimeInitType RuntimeInitType => Instance.runtimeInitType;

        public static List<IapData> SkusData => Instance.skusData;

        // public static List<IapDataProduct> Products => Instance.products;
        public static bool IsValidatePurchase => Instance.isValidatePurchase;
        public static bool IsCustomValidatePurchase => Instance.isCustomValidatePurchase;
        public static ValidatePurchase ValidatePurchase => Instance.validatePurchase;

        // public static IapDataProduct GetIapProduct(string id)
        // {
        //     foreach (var product in Products)
        //     {
        //         if (product.Id == id) return product;
        //     }
        //
        //     return null;
        // }
    }

    [Serializable]
    public class IapData
    {
        public string androidId;
        public string iosId;

        public string Id
        {
            get
            {
#if UNITY_ANDROID
                return androidId;
#elif UNITY_IOS
                return iosId;
#else
                return string.Empty;
#endif
            }
        }

        public IapProductType productType;
        public float priceConfig;
    }
}