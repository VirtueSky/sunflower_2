using System;
using System.Text;

#if UNITY_IOS && VIRTUESKY_APPLE_AUTH
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Extensions;
using AppleAuth.Interfaces;
using AppleAuth.Native;
#endif
using UnityEngine;
using VirtueSky.DataStorage;
using VirtueSky.Inspector;

namespace VirtueSky.GameService
{
    [EditorIcon("icon_authentication")]
    public class AppleAuthentication : ServiceAuthentication
    {
        private static event Func<string> GetAuthorCodeEvent;
        private static event Action TryLoginEvent;

        private string _authorizationCode;
        private string InternalGetAuthorCode() => _authorizationCode;

        public static string Email
        {
            get => GameData.Get("AppleAuthentication_Email", "");
            private set => GameData.Set("AppleAuthentication_Email", value);
        }

        public static string UserId
        {
            get => GameData.Get("AppleAuthentication_UserId", "");
            private set => GameData.Set("AppleAuthentication_UserId", value);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            GetAuthorCodeEvent += InternalGetAuthorCode;
            TryLoginEvent += InternalTryLogin;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            GetAuthorCodeEvent -= InternalGetAuthorCode;
            TryLoginEvent -= InternalTryLogin;
        }

#if UNITY_IOS && VIRTUESKY_APPLE_AUTH
        private IAppleAuthManager _iAppleAuthManager;
#endif


        protected override void InternalInit()
        {
#if UNITY_IOS && VIRTUESKY_APPLE_AUTH
            if (AppleAuthManager.IsCurrentPlatformSupported)
            {
                // Creates a default JSON deserializer, to transform JSON Native responses to C# instances
                var deserializer = new PayloadDeserializer();
                // Creates an Apple Authentication manager with the deserializer
                this._iAppleAuthManager = new AppleAuthManager(deserializer);
            }
#endif
        }

        private void Update()
        {
#if UNITY_IOS && VIRTUESKY_APPLE_AUTH
            // Updates the AppleAuthManager instance to execute
            // pending callbacks inside Unity's execution loop
            if (this._iAppleAuthManager != null)
            {
                this._iAppleAuthManager.Update();
            }
#endif
        }

        protected override void InternalLogin()
        {
#if UNITY_IOS && VIRTUESKY_APPLE_AUTH
            var loginArgs =
                new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName);

            this._iAppleAuthManager.LoginWithAppleId(
                loginArgs,
                credential =>
                {
                    // Obtained credential, cast it to IAppleIDCredential
                    if (credential is IAppleIDCredential appleIdCredential)
                    {
                        // Apple User ID
                        // You should save the user ID somewhere in the device
                        var userId = appleIdCredential.User;

                        // Email (Received ONLY in the first login)
                        var email = appleIdCredential.Email;

                        // Full name (Received ONLY in the first login)
                        var fullName = appleIdCredential.FullName;

                        // Identity token
                        var identityToken = Encoding.UTF8.GetString(
                            appleIdCredential.IdentityToken,
                            0,
                            appleIdCredential.IdentityToken.Length);

                        // Authorization code
                        var authorizationCode = Encoding.UTF8.GetString(
                            appleIdCredential.AuthorizationCode,
                            0,
                            appleIdCredential.AuthorizationCode.Length);

                        // And now you have all the information to create/login a user in your system
                        serverCode = identityToken;
                        _authorizationCode = authorizationCode;
                        if (!string.IsNullOrEmpty(userId)) UserId = userId;
                        if (!string.IsNullOrEmpty(email)) Email = email;
                        if (fullName != null) UserName = $"{fullName.GivenName} {fullName.FamilyName}";
                        statusLogin = StatusLogin.Successful;
                    }
                    else
                    {
                        serverCode = "";
                        UserId = "";
                        statusLogin = StatusLogin.Failed;
                    }
                },
                error =>
                {
                    // Something went wrong
                    var authorizationErrorCode = error.GetAuthorizationErrorCode();
                    serverCode = "";
                    UserId = "";
                    statusLogin = StatusLogin.Failed;
                });
#endif
        }

        /// <summary>
        /// Login when Apple credential still valid.
        /// </summary>
        private void InternalTryLogin()
        {
#if UNITY_IOS && VIRTUESKY_APPLE_AUTH
            if (string.IsNullOrEmpty(UserId)) return;
            this._iAppleAuthManager.GetCredentialState(UserId, state =>
            {
                switch (state)
                {
                    case CredentialState.Revoked:
                        break;
                    case CredentialState.Authorized:
                        Debug.Log($"Apple credential still valid. Auto-login with userId: {UserId}");
                        Login();
                        break;
                    case CredentialState.NotFound:
                        Debug.LogWarning("Apple credential invalid or revoked.");
                        UserId = "";
                        break;
                    case CredentialState.Transferred:
                        break;
                }
            }, error => { Debug.LogError("CredentialState check failed: " + error.ToString()); });
#endif
        }

        #region Api

        public static string GetAuthorizationCode() => GetAuthorCodeEvent?.Invoke();
        public static void TryLogin() => TryLoginEvent?.Invoke();

        #endregion
    }
}