#if VIRTUESKY_IAP
using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using VirtueSky.Ads;
using VirtueSky.Core;
using VirtueSky.Inspector;
using VirtueSky.Misc;

namespace VirtueSky.Iap
{
    [EditorIcon("icon_manager"), HideMonoScript]
    public class IapManager : MonoBehaviour, IDetailedStoreListener
    {
        [SerializeField] private bool isPersistent;
        public static event Action<string> OnPurchaseSucceedEvent;
        public static event Action<string> OnPurchaseFailedEvent;

        private IStoreController _controller;
        private IExtensionProvider _extensionProvider;
        public static bool IsInitialized { get; private set; }

        private static IapManager instance;

        private void Awake()
        {
            if (isPersistent)
            {
                DontDestroyOnLoad(this.gameObject);
            }

            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;

            if (IapSettings.RuntimeInitType == CoreEnum.RuntimeInitType.AfterSceneLoad_Awake ||
                IapSettings.RuntimeInitType == CoreEnum.RuntimeInitType.BeforeSceneLoad_Awake)
            {
                InternalInitialization();
            }
        }

        private void OnEnable()
        {
            if (IapSettings.RuntimeInitType == CoreEnum.RuntimeInitType.BeforeSceneLoad_OnEnable ||
                IapSettings.RuntimeInitType == CoreEnum.RuntimeInitType.AfterSceneLoad_OnEnable)
            {
                InternalInitialization();
            }
        }


        private void Start()
        {
            if (IapSettings.RuntimeInitType == CoreEnum.RuntimeInitType.AfterSceneLoad_Start ||
                IapSettings.RuntimeInitType == CoreEnum.RuntimeInitType.BeforeSceneLoad_Start)
            {
                InternalInitialization();
            }
        }

        private async void InternalInitialization()
        {
            if (IsInitialized) return;
            await UniTask.WaitUntil(() => UnityServiceInitialization.IsUnityServiceReady);
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            RequestProductData(builder);
            builder.Configure<IGooglePlayConfiguration>();
            UnityPurchasing.Initialize(this, builder);
        }


        #region Implement

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            switch (error)
            {
                case InitializationFailureReason.AppNotKnown:
                    Debug.LogError("Is your App correctly uploaded on the relevant publisher console?");
                    break;
                case InitializationFailureReason.PurchasingUnavailable:
                    Debug.LogWarning("In App Purchases disabled in device settings!");
                    break;
                case InitializationFailureReason.NoProductsAvailable:
                    Debug.LogWarning("No products available for purchase!");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(error), error, null);
            }
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            OnInitializeFailed(error);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            if (IapSettings.IsValidatePurchase)
            {
                bool validatedPurchase = true;
#if (UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX) && !UNITY_EDITOR
            var validator =
                new UnityEngine.Purchasing.Security.CrossPlatformValidator(UnityEngine.Purchasing.Security.GooglePlayTangle.Data(),
                    UnityEngine.Purchasing.Security.AppleTangle.Data(), Application.identifier);

            try
            {
                // On Google Play, result has a single product ID.
                // On Apple stores, receipts contain multiple products.
                var result = validator.Validate(purchaseEvent.purchasedProduct.receipt);
                Debug.Log("Receipt is valid");
            }
            catch (UnityEngine.Purchasing.Security.IAPSecurityException)
            {
                Debug.Log("Invalid receipt, not unlocking content");
                validatedPurchase = false;
            }
#endif
                if (validatedPurchase) PurchaseVerified(purchaseEvent);
            }
            else
            {
                PurchaseVerified(purchaseEvent);
            }

            return PurchaseProcessingResult.Complete;
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _controller = controller;
            _extensionProvider = extensions;
            IsInitialized = true;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            InternalPurchaseFailed(product.definition.id);
        }


        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            InternalPurchaseFailed(product.definition.id);
        }

        #endregion

        private void PurchaseProductInternal(IapDataProduct product)
        {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            _controller?.InitiatePurchase(product.Id);
#elif UNITY_EDITOR
            InternalPurchaseSuccess(product.Id);
#endif
        }

        private void RequestProductData(ConfigurationBuilder builder)
        {
            foreach (var p in IapSettings.Products)
            {
                builder.AddProduct(p.Id, ConvertProductType(p.iapProductType));
            }
        }

        private void InternalPurchaseFailed(string id)
        {
            AdStatic.OnChangePreventDisplayAppOpenEvent?.Invoke(false);
            foreach (var product in IapSettings.Products)
            {
                if (product.Id != id) continue;
                OnPurchaseFailedEvent?.Invoke(product.Id);
                Common.CallActionAndClean(ref product.purchaseFailedCallback);
            }
        }

        void PurchaseVerified(PurchaseEventArgs purchaseEvent)
        {
            AdStatic.OnChangePreventDisplayAppOpenEvent?.Invoke(false);
            InternalPurchaseSuccess(purchaseEvent.purchasedProduct.definition.id);
        }

        void InternalPurchaseSuccess(string id)
        {
            foreach (var product in IapSettings.Products)
            {
                if (product.Id != id) continue;
                OnPurchaseSucceedEvent?.Invoke(product.Id);
                Common.CallActionAndClean(ref product.purchaseSuccessCallback);
            }
        }


#if UNITY_IOS
        private void InternalRestorePurchase()
        {
            if (!IsInitialized)
            {
                Debug.Log("Restore purchases fail. not initialized!");
                return;
            }

            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.OSXPlayer)
            {
                Debug.Log("Restore purchase started ...");

                var storeProvider = _extensionProvider.GetExtension<IAppleExtensions>();
                storeProvider.RestoreTransactions((b, s) =>
                {
                    // no purchase are avaiable to restore
                    Debug.Log($"Restore purchase continuing: {b}. If no further messages, no purchase available to restore.");
                });
            }
            else
            {
                Debug.Log("Restore purchase fail. not supported on this platform. current = " + Application.platform);
            }
        }
#endif

        ProductType ConvertProductType(IapProductType iapProductType)
        {
            switch (iapProductType)
            {
                case IapProductType.Consumable:
                    return ProductType.Consumable;
                case IapProductType.NonConsumable:
                    return ProductType.NonConsumable;
                case IapProductType.Subscription:
                    return ProductType.Subscription;
            }

            return ProductType.Consumable;
        }


        #region Internal API

        private IapDataProduct InternalPurchaseProductById(string id)
        {
            AdStatic.OnChangePreventDisplayAppOpenEvent?.Invoke(true);
            var product = IapSettings.GetIapProduct(id);
            PurchaseProductInternal(product);
            return product;
        }

        private IapDataProduct InternalPurchaseProductByIapData(IapDataProduct product)
        {
            AdStatic.OnChangePreventDisplayAppOpenEvent?.Invoke(true);
            PurchaseProductInternal(product);
            return product;
        }

        private bool InternalIsPurchasedProductByIapData(IapDataProduct product)
        {
            if (_controller == null) return false;
            return ConvertProductType(product.iapProductType) is ProductType.NonConsumable or ProductType.Subscription &&
                   _controller.products.WithID(product.Id).hasReceipt;
        }

        private bool InternalIsPurchasedProductById(string id)
        {
            if (_controller == null) return false;
            return ConvertProductType(IapSettings.GetIapProduct(id).iapProductType) is ProductType.NonConsumable or ProductType.Subscription &&
                   _controller.products.WithID(id)
                       .hasReceipt;
        }

        private Product InternalGetProductByIapData(IapDataProduct product)
        {
            if (_controller == null) return null;
            return _controller.products.WithID(product.Id);
        }

        private Product InternalGetProductById(string id)
        {
            if (_controller == null) return null;
            return _controller.products.WithID(id);
        }

        private SubscriptionInfo InternalGetSubscriptionInfo(IapDataProduct product)
        {
            if (_controller == null || ConvertProductType(product.iapProductType) != ProductType.Subscription ||
                !_controller.products.WithID(product.Id).hasReceipt) return null;
            var subscriptionManager = new SubscriptionManager(InternalGetProductByIapData(product), null);
            return subscriptionManager.getSubscriptionInfo();
        }

        #endregion

        #region Public API

        public static IapDataProduct PurchaseProduct(string id) => instance != null ? instance.InternalPurchaseProductById(id) : null;

        public static IapDataProduct PurchaseProduct(IapDataProduct product) =>
            instance != null ? instance.InternalPurchaseProductByIapData(product) : null;

        public static bool IsPurchasedProduct(IapDataProduct product) => instance != null && instance.InternalIsPurchasedProductByIapData(product);

        public static bool IsPurchasedProduct(string id) => instance != null && instance.InternalIsPurchasedProductById(id);

        public static Product GetProduct(IapDataProduct product) => instance != null ? instance.InternalGetProductByIapData(product) : null;
        public static Product GetProduct(string id) => instance != null ? instance.InternalGetProductById(id) : null;

        public static SubscriptionInfo GetSubscriptionInfo(IapDataProduct product) =>
            instance != null ? instance.InternalGetSubscriptionInfo(product) : null;

#if UNITY_IOS
        public static void RestorePurchase()
        {
            if (instance != null)
            {
                instance.InternalRestorePurchase();
            }
        }
    }
#endif

        public static void Initialization()
        {
            if (instance != null)
            {
                instance.InternalInitialization();
            }
        }

        #endregion
    }
}

#endif