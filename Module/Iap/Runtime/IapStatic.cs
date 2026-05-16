using System;
using UnityEngine;
using VirtueSky.Core;

#if VIRTUESKY_IAP
using UnityEngine.Purchasing;
#endif


namespace VirtueSky.Iap
{
    public static class IapStatic
    {
#if VIRTUESKY_IAP
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RuntimeBeforeSceneLoad()
        {
            AutoInitialize(CoreEnum.RuntimeInitType.BeforeSceneLoad_Awake);
            AutoInitialize(CoreEnum.RuntimeInitType.BeforeSceneLoad_OnEnable);
            AutoInitialize(CoreEnum.RuntimeInitType.BeforeSceneLoad_Start);
            AutoInitialize(CoreEnum.RuntimeInitType.BeforeSceneLoad);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void RuntimeAfterSceneLoad()
        {
            AutoInitialize(CoreEnum.RuntimeInitType.AfterSceneLoad_Awake);
            AutoInitialize(CoreEnum.RuntimeInitType.AfterSceneLoad_OnEnable);
            AutoInitialize(CoreEnum.RuntimeInitType.AfterSceneLoad_Start);
            AutoInitialize(CoreEnum.RuntimeInitType.AfterSceneLoad);
        }

        private static void AutoInitialize(CoreEnum.RuntimeInitType iapRuntimeAutoInitType)
        {
            if (IapSettings.Instance == null) return;
            if (IapSettings.RuntimeInitType != iapRuntimeAutoInitType) return;
            var iapManager = new GameObject("IapManager");
            iapManager.AddComponent<IapManager>();
            UnityEngine.Object.DontDestroyOnLoad(iapManager);
        }
#endif


        public static IapDataProduct OnCompleted(this IapDataProduct product, Action onComplete)
        {
            product.purchaseSuccessCallback = onComplete;
            return product;
        }

        public static IapDataProduct OnFailed(this IapDataProduct product, Action<string> onFailed)
        {
            product.purchaseFailedCallback = onFailed;
            return product;
        }
    }
}