using UnityEditor;
using UnityEngine;

namespace VirtueSky.Ads
{
    [CustomEditor(typeof(AdSettings))]
    public class AdSettingsEditor : Editor
    {
        private AdSettings _adSettings;
        private SerializedProperty _runtimeAutoInit;
        private SerializedProperty _runtimeAutoInitType;
        private SerializedProperty _adCheckingInterval;
        private SerializedProperty _adLoadingInterval;
        private SerializedProperty _adNetwork;

        private SerializedProperty _sdkKey;
        private SerializedProperty _maxBannerAdUnit;
        private SerializedProperty _maxInterstitialAdUnit;
        private SerializedProperty _maxRewardAdUnit;
        private SerializedProperty _maxAppOpenAdUnit;

        private SerializedProperty _admobBannerAdUnit;
        private SerializedProperty _admobInterstitialAdUnit;
        private SerializedProperty _admobRewardAdUnit;
        private SerializedProperty _admobRewardedInterstitialAdUnit;
        private SerializedProperty _admobNativeOverlayAdUnit;
        private SerializedProperty _admobAppOpenAdUnit;
        private SerializedProperty _admobEnableTestMode;
        private SerializedProperty _enableGDPR;
        private SerializedProperty _enableGDPRTestMode;
        private SerializedProperty _admobDevicesTest;

        private SerializedProperty _androidAppKey;
        private SerializedProperty _iOSAppKey;

        private SerializedProperty _useTestAppKey;
        // private SerializedProperty _ironSourceBannerAdUnit;
        // private SerializedProperty _ironSourceInterstitialAdUnit;
        // private SerializedProperty _ironSourceRewardAdUnit;

        void Initialize()
        {
            _adSettings = target as AdSettings;
            _runtimeAutoInit = serializedObject.FindProperty("runtimeAutoInit");
            _runtimeAutoInitType = serializedObject.FindProperty("runtimeAutoInitType");
            _adCheckingInterval = serializedObject.FindProperty("adCheckingInterval");
            _adLoadingInterval = serializedObject.FindProperty("adLoadingInterval");
            _adNetwork = serializedObject.FindProperty("adNetwork");
            _sdkKey = serializedObject.FindProperty("sdkKey");
            _maxBannerAdUnit = serializedObject.FindProperty("maxBannerAdUnit");
            _maxInterstitialAdUnit = serializedObject.FindProperty("maxInterstitialAdUnit");
            _maxRewardAdUnit = serializedObject.FindProperty("maxRewardAdUnit");
            _maxAppOpenAdUnit = serializedObject.FindProperty("maxAppOpenAdUnit");
            _admobBannerAdUnit = serializedObject.FindProperty("admobBannerAdUnit");
            _admobInterstitialAdUnit = serializedObject.FindProperty("admobInterstitialAdUnit");
            _admobRewardAdUnit = serializedObject.FindProperty("admobRewardAdUnit");
            _admobRewardedInterstitialAdUnit = serializedObject.FindProperty("admobRewardedInterstitialAdUnit");
            _admobAppOpenAdUnit = serializedObject.FindProperty("admobAppOpenAdUnit");
            _admobNativeOverlayAdUnit = serializedObject.FindProperty("admobNativeOverlayAdUnit");
            _admobEnableTestMode = serializedObject.FindProperty("admobEnableTestMode");
            _enableGDPR = serializedObject.FindProperty("enableGDPR");
            _enableGDPRTestMode = serializedObject.FindProperty("enableGDPRTestMode");
            _admobDevicesTest = serializedObject.FindProperty("admobDevicesTest");
            _androidAppKey = serializedObject.FindProperty("androidAppKey");
            _iOSAppKey = serializedObject.FindProperty("iOSAppKey");
            _useTestAppKey = serializedObject.FindProperty("useTestAppKey");
            // _ironSourceBannerAdUnit = serializedObject.FindProperty("ironSourceBannerAdUnit");
            // _ironSourceInterstitialAdUnit = serializedObject.FindProperty("ironSourceInterstitialAdUnit");
            // _ironSourceRewardAdUnit = serializedObject.FindProperty("ironSourceRewardAdUnit");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Initialize();
            // EditorGUILayout.LabelField("ADS SETTING", EditorStyles.boldLabel);
            //   GuiLine(2);
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(_runtimeAutoInit);
            if (_runtimeAutoInit.boolValue)
            {
                EditorGUILayout.PropertyField(_runtimeAutoInitType);
            }

            GUILayout.Space(10);
            EditorGUILayout.PropertyField(_adCheckingInterval);
            EditorGUILayout.PropertyField(_adLoadingInterval);
            EditorGUILayout.PropertyField(_adNetwork);
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(_enableGDPR);
            if (_enableGDPR.boolValue)
            {
                EditorGUILayout.PropertyField(_enableGDPRTestMode);
            }

            GUILayout.Space(10);

            switch (_adNetwork.enumValueIndex)
            {
                case (int)AdNetwork.Max:
                    DrawMax();
                    break;
                case (int)AdNetwork.Admob:
                    DrawAdmob();
                    break;
                case (int)AdNetwork.IronSource:
                    DrawIronSource();
                    break;
            }


            EditorUtility.SetDirty(target);
            serializedObject.ApplyModifiedProperties();
        }


        void DrawMax()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("AppLovin-Max", EditorStyles.boldLabel);
            GuiLine();
            GUILayout.Space(5);
            EditorGUILayout.PropertyField(_sdkKey);
            GUILayout.Space(5);
            EditorGUILayout.PropertyField(_maxBannerAdUnit);
            EditorGUILayout.PropertyField(_maxInterstitialAdUnit);
            EditorGUILayout.PropertyField(_maxRewardAdUnit);
            EditorGUILayout.PropertyField(_maxAppOpenAdUnit);
        }

        void DrawAdmob()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Google-Admob", EditorStyles.boldLabel);
            GuiLine();
            GUILayout.Space(5);
            EditorGUILayout.PropertyField(_admobBannerAdUnit);
            EditorGUILayout.PropertyField(_admobInterstitialAdUnit);
            EditorGUILayout.PropertyField(_admobRewardAdUnit);
            EditorGUILayout.PropertyField(_admobRewardedInterstitialAdUnit);
            EditorGUILayout.PropertyField(_admobAppOpenAdUnit);
            EditorGUILayout.PropertyField(_admobNativeOverlayAdUnit);
            EditorGUILayout.PropertyField(_admobEnableTestMode);
            EditorGUILayout.PropertyField(_admobDevicesTest);
            GUILayout.Space(10);
            GUI.enabled = false;
            EditorGUILayout.TextField("App Id Test", "ca-app-pub-3940256099942544~3347511713");
            GUI.enabled = true;
            GUILayout.Space(10);
            if (GUILayout.Button("Open GoogleAdmobSetting", GUILayout.Height(20)))
            {
                EditorApplication.ExecuteMenuItem("Assets/Google Mobile Ads/Settings...");
            }
        }

        void DrawIronSource()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("LevelPlay-IronSource", EditorStyles.boldLabel);
            GuiLine();
            GUILayout.Space(5);
            EditorGUILayout.PropertyField(_androidAppKey);
            EditorGUILayout.PropertyField(_iOSAppKey);
            EditorGUILayout.PropertyField(_useTestAppKey);
            // EditorGUILayout.PropertyField(_ironSourceBannerAdUnit);
            // EditorGUILayout.PropertyField(_ironSourceInterstitialAdUnit);
            // EditorGUILayout.PropertyField(_ironSourceRewardAdUnit);
        }

        void GuiLine(int i_height = 1)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, i_height);

            rect.height = i_height;

            EditorGUI.DrawRect(rect, new Color32(0, 0, 0, 255));
        }
    }
}