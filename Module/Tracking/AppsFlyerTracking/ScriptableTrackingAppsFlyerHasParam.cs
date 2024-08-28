using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtueSky.Inspector;
using VirtueSky.Misc;

namespace VirtueSky.Tracking
{
    [CreateAssetMenu(menuName = "Sunflower2/Tracking Event/AppsFlyer/Tracking Has Param",
        fileName = "tracking_appsflyer_has_param")]
    [EditorIcon("scriptable_af")]
    public class ScriptableTrackingAppsFlyerHasParam : ScriptableObject
    {
        [Space, HeaderLine("Event Name"), SerializeField]
        private string eventName;

        public void TrackEvent(Dictionary<string, string> eventValues)
        {
#if VIRTUESKY_APPSFLYER
            AppsFlyerSDK.AppsFlyer.sendEvent(eventName, eventValues);
#endif
        }

        public void TrackEvent(List<string> paramNames, List<string> paramValues)
        {
#if VIRTUESKY_APPSFLYER
            IDictionary<string, string> eventValues = paramNames.MakeDictionary(paramValues);
            AppsFlyerSDK.AppsFlyer.sendEvent(eventName, (Dictionary<string, string>)eventValues);
#endif
        }
    }
}