using UnityEngine;
using VirtueSky.Inspector;

namespace VirtueSky.Tracking
{
    [CreateAssetMenu(menuName = "Sunflower2/Tracking Event/Adjust",
        fileName = "tracking_adjust")]
    [EditorIcon("scriptable_adjust2")]
    public class ScriptableTrackingAdjust : ScriptableObject
    {
        [HeaderLine("Event Token"), SerializeField]
        private string eventToken;

        public void TrackEvent()
        {
#if VIRTUESKY_ADJUST
            com.adjust.sdk.Adjust.trackEvent(new com.adjust.sdk.AdjustEvent(eventToken));
#endif
        }
    }
}