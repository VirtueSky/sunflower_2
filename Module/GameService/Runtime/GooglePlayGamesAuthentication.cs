#if UNITY_ANDROID && VIRTUESKY_GPGS
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif
using System;
using System.Threading.Tasks;
using UnityEngine;
using VirtueSky.Inspector;

namespace VirtueSky.GameService
{
    [EditorIcon("icon_authentication")]
    public class GooglePlayGamesAuthentication : ServiceAuthentication
    {
        private static event Action OnGetNewServerCodeEvent;

        protected override void OnEnable()
        {
            base.OnEnable();
            OnGetNewServerCodeEvent += InternalGetNewServerCode;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            OnGetNewServerCodeEvent -= InternalGetNewServerCode;
        }

        private async void InternalGetNewServerCode()
        {
#if UNITY_ANDROID && VIRTUESKY_GPGS
            if (!PlayGamesPlatform.Instance.IsAuthenticated()) return;
            (serverCode, statusLogin) = await Excute();
            Task<(string, StatusLogin)> Excute()
            {
                var taskSource = new TaskCompletionSource<(string, StatusLogin)>();
                PlayGamesPlatform.Instance.RequestServerSideAccess(true,
                    code => taskSource.SetResult((code, StatusLogin.Successful)));
                return taskSource.Task;
            }
#endif
        }


        protected override void InternalInit()
        {
#if UNITY_ANDROID && VIRTUESKY_GPGS
            PlayGamesPlatform.Activate();
#endif
        }

        protected override void InternalLogin()
        {
#if UNITY_ANDROID && VIRTUESKY_GPGS
            PlayGamesPlatform.Instance.Authenticate((success) =>
            {
                if (success == SignInStatus.Success)
                {
                    Debug.Log("Login with Google Play games successful.");
                    PlayGamesPlatform.Instance.RequestServerSideAccess(true,
                        code =>
                        {
                            Debug.Log("Authorization code: " + code);
                            serverCode = code;
                            if (!string.IsNullOrEmpty(PlayGamesPlatform.Instance.GetUserDisplayName()))
                            {
                                UserName = PlayGamesPlatform.Instance.GetUserDisplayName();
                            }

                            statusLogin = StatusLogin.Successful;
                        });
                }
                else
                {
                    PlayGamesPlatform.Instance.ManuallyAuthenticate(success =>
                    {
                        if (success == SignInStatus.Success)
                        {
                            PlayGamesPlatform.Instance.RequestServerSideAccess(true,
                                code =>
                                {
                                    Debug.Log("Authorization code: " + code);
                                    serverCode = code;
                                    if (!string.IsNullOrEmpty(PlayGamesPlatform.Instance.GetUserDisplayName()))
                                    {
                                        UserName = PlayGamesPlatform.Instance.GetUserDisplayName();
                                    }

                                    statusLogin = StatusLogin.Successful;
                                });
                        }
                        else
                        {
                            Debug.Log("Login Failed");
                            serverCode = "";
                            statusLogin = StatusLogin.Failed;
                        }
                    });
                }
            });
#endif
        }

        #region Api

        public static void GetNewServerCode() => OnGetNewServerCodeEvent?.Invoke();

        public static bool IsSignIn()
        {
#if UNITY_ANDROID && VIRTUESKY_GPGS
            return PlayGamesPlatform.Instance.IsAuthenticated();

#else
            return false;
#endif
        }

        #endregion
    }
}