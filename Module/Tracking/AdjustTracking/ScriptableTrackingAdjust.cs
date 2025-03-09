using System;
using UnityEngine;
using VirtueSky.Inspector;

namespace VirtueSky.Tracking
{
    [CreateAssetMenu(menuName = "Sunflower2/Tracking Event/Adjust",
        fileName = "tracking_adjust")]
    [EditorIcon("scriptable_adjust2")]
    public class ScriptableTrackingAdjust : ScriptableObject
    {
        public static Action OnTracked;

        [HeaderLine("Event Token"), SerializeField]
        private string eventToken;

        public void TrackEvent()
        {
#if VIRTUESKY_ADJUST
            AdjustSdk.Adjust.TrackEvent(new AdjustSdk.AdjustEvent(eventToken));
            OnTracked?.Invoke();
#endif
        }
    }
}