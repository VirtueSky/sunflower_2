using UnityEngine;
using VirtueSky.Core;

namespace VirtueSky.Tweening
{
    [DefaultExecutionOrder(-32000)]
    internal sealed class TweenRunner : BaseMono
    {
        static TweenRunner instance;

        internal static void Ensure()
        {
            if (instance != null) return;

            var go = new GameObject("[VirtueSky.TweenRunner]");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<TweenRunner>();
        }

        public override void Tick()
        {
            base.Tick();
            TweenRuntime.Update(Time.deltaTime, Time.unscaledDeltaTime);
        }
    }
}