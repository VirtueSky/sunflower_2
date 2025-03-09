using UnityEngine;
using VirtueSky.Inspector;

namespace VirtueSky.Tracking
{
    [CreateAssetMenu(menuName = "Sunflower2/Tracking Event/Firebase Analytic/Tracking 1 Param",
        fileName = "tracking_firebase_1_param")]
    [EditorIcon("scriptable_firebase")]
    public class ScriptableTrackingFirebaseOneParam : TrackingFirebase
    {
        [Space] [HeaderLine("Parameter Name")] [SerializeField]
        private string parameterName;

        public void TrackEvent(string parameterValue)
        {
            if (!Application.isMobilePlatform) return;
#if VIRTUESKY_FIREBASE_ANALYTIC
            Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName, parameterName, parameterValue);
            onTracked?.Invoke();
#endif
        }
    }
}