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
        [SerializeField] private IapDataProduct[] iapDataProducts;
        [SerializeField] private bool isValidatePurchase = true;
#if UNITY_EDITOR
        [SerializeField, TextArea] private string googlePlayStoreKey;
        public string GooglePlayStoreKey => googlePlayStoreKey;
#endif
        public static CoreEnum.RuntimeInitType RuntimeInitType => Instance.runtimeInitType;
        public static IapDataProduct[] IapDataProducts => Instance.iapDataProducts;
        public static bool IsValidatePurchase => Instance.isValidatePurchase;

        public static IapDataProduct GetIapProduct(string id)
        {
            foreach (var product in IapDataProducts)
            {
                if (product.Id == id) return product;
            }

            return null;
        }
    }
}