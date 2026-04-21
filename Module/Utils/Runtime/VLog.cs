using Object = UnityEngine.Object;

namespace VirtueSky.Utils
{
    public struct VLog
    {
        public static void Log(object message)
        {
#if UNITY_EDITOR || VIRTUESKU_DEBUG_LOG
            UnityEngine.Debug.Log(message);
#endif
        }

        public static void Log(object message, Object context)
        {
#if UNITY_EDITOR || VIRTUESKU_DEBUG_LOG
            UnityEngine.Debug.Log(message, context);
#endif
        }

        public static void LogWarning(object message)
        {
#if UNITY_EDITOR || VIRTUESKU_DEBUG_LOG
            UnityEngine.Debug.LogWarning(message);
#endif
        }

        public static void LogWarning(object message, Object context)
        {
#if UNITY_EDITOR || VIRTUESKU_DEBUG_LOG
            UnityEngine.Debug.LogWarning(message, context);
#endif
        }

        public static void LogError(object message)
        {
#if UNITY_EDITOR || VIRTUESKU_DEBUG_LOG
            UnityEngine.Debug.LogError(message);
#endif
        }

        public static void LogError(object message, Object context)
        {
#if UNITY_EDITOR || VIRTUESKU_DEBUG_LOG
            UnityEngine.Debug.LogError(message, context);
#endif
        }

        public static void LogException(System.Exception exception)
        {
#if UNITY_EDITOR || VIRTUESKU_DEBUG_LOG
            UnityEngine.Debug.LogException(exception);
#endif
        }

        public static void LogException(System.Exception exception, Object context)
        {
#if UNITY_EDITOR || VIRTUESKU_DEBUG_LOG
            UnityEngine.Debug.LogException(exception, context);
#endif
        }
    }
}