namespace VirtueSky.Core
{
    public struct CoreEnum
    {
        public enum RuntimeInitType
        {
            /// <summary>
            /// Automatically instantiated after the first scene is loaded. Not destroyed on scene load. Initialized in Awake.
            /// </summary>
            AfterSceneLoad_Awake,

            /// <summary>
            /// Automatically instantiated before the first scene is loaded. Not destroyed on scene load. Initialized in Awake.
            /// </summary>
            BeforeSceneLoad_Awake,

            /// <summary>
            /// Automatically instantiated after the first scene is loaded. Not destroyed on scene load. Initialized in OnEnable.
            /// </summary>
            AfterSceneLoad_OnEnable,

            /// <summary>
            /// Automatically instantiated before the first scene is loaded. Not destroyed on scene load. Initialized in OnEnable.
            /// </summary>
            BeforeSceneLoad_OnEnable,

            /// <summary>
            /// Automatically instantiated after the first scene is loaded. Not destroyed on scene load. Initialized in Start.
            /// </summary>
            AfterSceneLoad_Start,

            /// <summary>
            /// Automatically instantiated before the first scene is loaded. Not destroyed on scene load. Initialized in Start.
            /// </summary>
            BeforeSceneLoad_Start,

            /// <summary>
            /// Automatically instantiated after the first scene is loaded. Not destroyed on scene load. Initialized manually via a custom method.
            /// </summary>
            AfterSceneLoad,

            /// <summary>
            /// Automatically instantiated before the first scene is loaded. Not destroyed on scene load. Initialized manually via a custom method.
            /// </summary>
            BeforeSceneLoad,

            /// <summary>
            /// No automatic instantiation. Attach the component to the scene manually and call Initialize() yourself.
            /// </summary>
            None
        }
    }
}