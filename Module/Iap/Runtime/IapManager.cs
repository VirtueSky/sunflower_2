#if VIRTUESKY_IAP
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using VirtueSky.Ads;
using VirtueSky.Core;
using VirtueSky.Inspector;
using VirtueSky.Misc;

namespace VirtueSky.Iap
{
    [EditorIcon("icon_manager"), HideMonoScript]
    public class IapManager : MonoBehaviour
    {
        [SerializeField] private bool isPersistent;

        [Space] [ReadOnly, SerializeField] private List<IapDataProduct> products = new List<IapDataProduct>();
        public static event Action<string> OnPurchaseSucceedEvent;
        public static event Action<string, string> OnPurchaseFailedEvent;

        /// <summary>
        /// Raised after purchases are fetched from the store (on init or after RestorePurchase), use it to refresh owned state.
        /// </summary>
        public static event Action OnPurchasesFetchedEvent;

        private StoreController _store;
        private bool isRequestBuilder = false;
        private bool flag = false;

        // NonConsumable / Subscription owned by the user, filled from confirmed orders
        private readonly HashSet<string> _ownedProductIds = new HashSet<string>();
        private readonly Dictionary<string, SubscriptionInfo> _subscriptionInfos = new Dictionary<string, SubscriptionInfo>();

        // Avoid granting the same transaction twice in one session
        private readonly HashSet<string> _processedTransactions = new HashSet<string>();

#if UNITY_ANDROID && !UNITY_EDITOR
        private UnityEngine.Purchasing.Security.CrossPlatformValidator _validator;
#endif

        public static bool IsExist => instance != null;
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
            CreateIapProducts();
            if (IapSettings.RuntimeInitType == CoreEnum.RuntimeInitType.AfterSceneLoad_Awake ||
                IapSettings.RuntimeInitType == CoreEnum.RuntimeInitType.BeforeSceneLoad_Awake)
            {
                StartCoroutine(InternalInitialization());
            }
        }

        private void OnEnable()
        {
            if (IapSettings.RuntimeInitType == CoreEnum.RuntimeInitType.AfterSceneLoad_OnEnable ||
                IapSettings.RuntimeInitType == CoreEnum.RuntimeInitType.BeforeSceneLoad_OnEnable)
            {
                StartCoroutine(InternalInitialization());
            }
        }

        private void Start()
        {
            if (IapSettings.RuntimeInitType == CoreEnum.RuntimeInitType.AfterSceneLoad_Start ||
                IapSettings.RuntimeInitType == CoreEnum.RuntimeInitType.BeforeSceneLoad_Start)
            {
                StartCoroutine(InternalInitialization());
            }
        }

        private void OnDestroy()
        {
            if (instance != this) return;
            UnregisterStoreEvents();
            instance = null;
            IsInitialized = false;
        }

        private IEnumerator InternalInitialization()
        {
            if (IsInitialized || flag) yield break;
            flag = true;
            yield return new WaitUntil(() => UnityServiceInitialization.IsUnityServiceReady);
            ConnectAndFetchProducts();
        }

        private async void ConnectAndFetchProducts()
        {
            if (_store == null)
            {
                _store = UnityIAPServices.StoreController();
                RegisterStoreEvents();
            }

            try
            {
                await _store.Connect();
            }
            catch (Exception e)
            {
                Debug.LogError($"[IapManager] Connect to store failed: {e}");
                flag = false;
                return;
            }

            isRequestBuilder = true;
            _store.FetchProducts(BuildProductDefinitions());
            Debug.Log("IapManager connected to store, fetching products...".SetColor(Color.cyan));
        }

        private void RegisterStoreEvents()
        {
            // OnPurchasePending must be registered before Connect to receive pending orders from previous sessions
            _store.OnPurchasePending += OnPurchasePending;
            _store.OnPurchaseConfirmed += OnPurchaseConfirmed;
            _store.OnPurchaseFailed += OnPurchaseFailed;
            _store.OnPurchaseDeferred += OnPurchaseDeferred;
            _store.OnPurchasesFetched += OnPurchasesFetched;
            _store.OnPurchasesFetchFailed += OnPurchasesFetchFailed;
            _store.OnProductsFetched += OnProductsFetched;
            _store.OnProductsFetchFailed += OnProductsFetchFailed;
            _store.OnStoreDisconnected += OnStoreDisconnected;
        }

        private void UnregisterStoreEvents()
        {
            if (_store == null) return;
            _store.OnPurchasePending -= OnPurchasePending;
            _store.OnPurchaseConfirmed -= OnPurchaseConfirmed;
            _store.OnPurchaseFailed -= OnPurchaseFailed;
            _store.OnPurchaseDeferred -= OnPurchaseDeferred;
            _store.OnPurchasesFetched -= OnPurchasesFetched;
            _store.OnPurchasesFetchFailed -= OnPurchasesFetchFailed;
            _store.OnProductsFetched -= OnProductsFetched;
            _store.OnProductsFetchFailed -= OnProductsFetchFailed;
            _store.OnStoreDisconnected -= OnStoreDisconnected;
        }

        private void CreateIapProducts()
        {
            foreach (var iapData in IapSettings.SkusData)
            {
                if (IsDuplicateProduct(iapData.Id))
                {
                    Debug.LogWarning($"[IapManager] Duplicate product id '{iapData.Id}' in skusData. Skipping.");
                    continue;
                }

                var product = new IapDataProduct(iapData.androidId, iapData.iosId, iapData.productType, iapData.priceConfig);
                products.Add(product);
            }
        }

        private bool IsDuplicateProduct(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            foreach (var p in products)
            {
                if (p.Id == id) return true;
            }

            return false;
        }

        #region Store Callbacks

        private void OnStoreDisconnected(StoreConnectionFailureDescription description)
        {
            Debug.LogWarning($"[IapManager] Store disconnected: {description.Message} (retryable: {description.IsRetryable})");
        }

        private void OnProductsFetched(List<Product> fetchedProducts)
        {
            IsInitialized = true;
            Debug.Log($"IapManager initialized! Fetched {fetchedProducts.Count} products.".SetColor(Color.cyan));
            _store.FetchPurchases();
        }

        private void OnProductsFetchFailed(ProductFetchFailed failed)
        {
            Debug.LogWarning($"[IapManager] Fetch products failed: {failed.FailureReason}. " +
                             $"Failed products: {failed.FailedFetchProducts.Count}. Is your App correctly uploaded on the relevant publisher console?");
            flag = false;
        }

        private void OnPurchasesFetched(Orders orders)
        {
            foreach (var order in orders.ConfirmedOrders)
            {
                MarkTransactionProcessed(order);
                CacheOwnedProducts(order);
            }

            // Pending orders are delivered through OnPurchasePending (Google: ProcessPendingOrdersOnPurchasesFetched is on by default,
            // Apple: StoreKit pushes them at startup), so they are not granted here to avoid double grants.
            OnPurchasesFetchedEvent?.Invoke();
        }

        private void OnPurchasesFetchFailed(PurchasesFetchFailureDescription description)
        {
            Debug.LogWarning($"[IapManager] Fetch purchases failed: {description.FailureReason} - {description.Message}");
        }

        private void OnPurchasePending(PendingOrder order)
        {
            if (IsTransactionProcessed(order))
            {
                _store.ConfirmPurchase(order);
                return;
            }

            var productIds = GetProductIds(order);
            if (IsOrderValid(order, productIds))
            {
                MarkTransactionProcessed(order);
                CacheOwnedProducts(order);
                foreach (var id in productIds) PurchaseVerified(id);
            }
            else
            {
                Debug.Log("Invalid receipt, not unlocking content");
                foreach (var id in productIds) InternalPurchaseFailed(id, "ValidationFailed");
            }

            // Always confirm (same as returning PurchaseProcessingResult.Complete in v4) so the order is not stuck
            _store.ConfirmPurchase(order);
        }

        private void OnPurchaseConfirmed(Order order)
        {
            switch (order)
            {
                case ConfirmedOrder confirmedOrder:
                    MarkTransactionProcessed(confirmedOrder);
                    CacheOwnedProducts(confirmedOrder);
                    break;
                case FailedOrder failedOrder:
                    // Content was already granted in OnPurchasePending, the order will be re-delivered and deduplicated next time
                    Debug.LogWarning($"[IapManager] Confirm purchase failed: {failedOrder.FailureReason} - {failedOrder.Details}");
                    break;
            }
        }

        private void OnPurchaseFailed(FailedOrder order)
        {
            foreach (var id in GetProductIds(order))
            {
                InternalPurchaseFailed(id, order.FailureReason.ToString());
            }
        }

        private void OnPurchaseDeferred(DeferredOrder order)
        {
            // Ask to Buy / slow payment: content is granted later through OnPurchasePending
            Debug.Log("[IapManager] Purchase deferred, waiting for approval.");
            AdStatic.OnChangePreventDisplayAppOpenEvent?.Invoke(false);
        }

        #endregion

        #region Order Helpers

        private static List<string> GetProductIds(Order order)
        {
            var ids = new List<string>();
            var items = order.CartOrdered?.Items();
            if (items != null)
            {
                foreach (var item in items)
                {
                    var id = item.Product?.definition?.id;
                    if (!string.IsNullOrEmpty(id) && !ids.Contains(id)) ids.Add(id);
                }
            }

            if (ids.Count == 0 && order.Info?.PurchasedProductInfo != null)
            {
                foreach (var info in order.Info.PurchasedProductInfo)
                {
                    if (!string.IsNullOrEmpty(info.productId) && !ids.Contains(info.productId)) ids.Add(info.productId);
                }
            }

            return ids;
        }

        private bool IsTransactionProcessed(Order order)
        {
            var transactionId = order.Info?.TransactionID;
            return !string.IsNullOrEmpty(transactionId) && _processedTransactions.Contains(transactionId);
        }

        private void MarkTransactionProcessed(Order order)
        {
            var transactionId = order.Info?.TransactionID;
            if (!string.IsNullOrEmpty(transactionId)) _processedTransactions.Add(transactionId);
        }

        private void CacheOwnedProducts(Order order)
        {
            foreach (var id in GetProductIds(order))
            {
                var iapProduct = GetIapProduct(id);
                if (iapProduct == null) continue;
                var productType = ConvertProductType(iapProduct.iapProductType);
                if (productType is ProductType.NonConsumable or ProductType.Subscription) _ownedProductIds.Add(id);
            }

            if (order.Info?.PurchasedProductInfo == null) return;
            foreach (var info in order.Info.PurchasedProductInfo)
            {
                try
                {
                    if (info.subscriptionInfo != null) _subscriptionInfos[info.productId] = info.subscriptionInfo;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[IapManager] Can not read subscription info of '{info.productId}': {e.Message}");
                }
            }
        }

        private bool IsOrderValid(Order order, List<string> productIds)
        {
            if (!IapSettings.IsValidatePurchase) return true;

            if (IapSettings.IsCustomValidatePurchase && IapSettings.ValidatePurchase != null)
            {
                var productId = productIds.Count > 0 ? productIds[0] : string.Empty;
                return IapSettings.ValidatePurchase.IsValidate(productId, order.Info?.Receipt, order.Info?.TransactionID);
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                _validator ??= new UnityEngine.Purchasing.Security.CrossPlatformValidator(
                    UnityEngine.Purchasing.Security.GooglePlayTangle.Data(), Application.identifier);
                _validator.Validate(order.Info.Receipt);
                Debug.Log("Receipt is valid");
                return true;
            }
            catch (UnityEngine.Purchasing.Security.IAPSecurityException)
            {
                return false;
            }
#else
            // iOS: StoreKit 2 already verifies the transaction. Use order.Info.Apple.jwsRepresentation for server-side validation.
            return true;
#endif
        }

        #endregion

        private void PurchaseProductInternal(IapDataProduct product)
        {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            if (_store == null || !IsInitialized)
            {
                InternalPurchaseFailed(product.Id, "NotInitialized");
                return;
            }

            _store.PurchaseProduct(product.Id);
#elif UNITY_EDITOR
            InternalPurchaseSuccess(product.Id);
#endif
        }

        private List<ProductDefinition> BuildProductDefinitions()
        {
            var definitions = new List<ProductDefinition>();
            foreach (var p in products)
            {
                if (string.IsNullOrEmpty(p.Id)) continue;
                definitions.Add(new ProductDefinition(p.Id, ConvertProductType(p.iapProductType)));
            }

            return definitions;
        }

        private void InternalPurchaseFailed(string id, string failureReason)
        {
            AdStatic.OnChangePreventDisplayAppOpenEvent?.Invoke(false);
            foreach (var product in products)
            {
                if (product.Id != id) continue;
                OnPurchaseFailedEvent?.Invoke(product.Id, failureReason);
                Common.CallActionAndClean<string>(ref product.purchaseFailedCallback, failureReason);
            }
        }

        void PurchaseVerified(string id)
        {
            AdStatic.OnChangePreventDisplayAppOpenEvent?.Invoke(false);
            InternalPurchaseSuccess(id);
        }

        void InternalPurchaseSuccess(string id)
        {
            foreach (var product in products)
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

                // Restored purchases are delivered through OnPurchasesFetched
                _store.RestoreTransactions((success, error) =>
                {
                    Debug.Log($"Restore purchase continuing: {success}. {error} If no further messages, no purchase available to restore.");
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
            var product = GetIapProduct(id);
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
            if (_store == null || product == null) return false;
            return IsOwned(product.Id, ConvertProductType(product.iapProductType));
        }

        private bool InternalIsPurchasedProductById(string id)
        {
            if (_store == null) return false;
            var product = GetIapProduct(id);
            return product != null && IsOwned(id, ConvertProductType(product.iapProductType));
        }

        private bool IsOwned(string id, ProductType productType)
        {
            if (productType is not (ProductType.NonConsumable or ProductType.Subscription)) return false;
            if (!_ownedProductIds.Contains(id)) return false;
            if (productType == ProductType.Subscription && _subscriptionInfos.TryGetValue(id, out var info))
            {
                return info.IsSubscribed() == Result.True && info.IsExpired() != Result.True;
            }

            return true;
        }

        private Product InternalGetProductByIapData(IapDataProduct product)
        {
            if (_store == null || product == null) return null;
            return _store.GetProductById(product.Id);
        }

        private Product InternalGetProductById(string id)
        {
            if (_store == null) return null;
            return _store.GetProductById(id);
        }

        private SubscriptionInfo InternalGetSubscriptionInfo(IapDataProduct product)
        {
            if (_store == null || product == null || ConvertProductType(product.iapProductType) != ProductType.Subscription) return null;
            return _subscriptionInfos.TryGetValue(product.Id, out var info) ? info : null;
        }

        #endregion

        #region Public API

        public static List<IapDataProduct> Products() =>
            instance != null ? new List<IapDataProduct>().Adds(instance.products) : new List<IapDataProduct>();

        public static IapDataProduct GetIapProduct(string id)
        {
            foreach (var product in instance.products)
            {
                if (product.Id == id) return product;
            }

            return null;
        }

        /// <summary>
        /// Add product to IapManager. You can use this API to add product at runtime, but you need to make sure called before IapManager intialization.
        /// </summary>
        /// <param name="product"></param>
        public static void AddProduct(IapDataProduct product)
        {
            if (instance == null)
            {
                Debug.LogWarning("[IapManager] AddProduct called before IapManager instance is ready.");
                return;
            }

            if (instance.IsDuplicateProduct(product.Id))
            {
                Debug.LogWarning($"[IapManager] Duplicate product id '{product.Id}'. Skipping.");
                return;
            }

            if (instance.isRequestBuilder)
            {
                Debug.LogWarning(
                    "[IapManager] AddProduct called after IapManager has requested product data. The new product will not be included in the initialization.");
                return;
            }

            instance.products.Add(product);
        }

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
#endif

        public static void Initialization()
        {
            if (instance != null)
            {
                instance.StartCoroutine(instance.InternalInitialization());
            }
        }

        #endregion
    }
}

#endif
