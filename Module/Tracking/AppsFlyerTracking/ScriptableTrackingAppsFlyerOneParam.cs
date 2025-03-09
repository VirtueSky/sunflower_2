using System.Collections.Generic;
using UnityEngine;
using VirtueSky.Inspector;

namespace VirtueSky.Tracking
{
    [CreateAssetMenu(menuName = "Sunflower2/Tracking Event/AppsFlyer/Tracking 1 Param",
        fileName = "tracking_appsflyer_1_param")]
    [EditorIcon("scriptable_af")]
    public class ScriptableTrackingAppsFlyerOneParam : TrackingAppsFlyer
    {
        [Space, HeaderLine("Parameter Name"), SerializeField]
        private string parameterName;

        public void TrackEvent(string parameterValue)
        {
#if VIRTUESKY_APPSFLYER
            Dictionary<string, string> eventValues = new Dictionary<string, string>();
            eventValues.Add(parameterName, parameterValue);
            AppsFlyerSDK.AppsFlyer.sendEvent(eventName, eventValues);
            onTracked?.Invoke();
#endif
        }
    }
}