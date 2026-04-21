using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using VirtueSky.Core;
using VirtueSky.Inspector;

namespace VirtueSky.Notifications
{
    [EditorIcon("script_noti"), HideMonoScript]
    public class NotificationPrepare : MonoBehaviour
    {
        [SerializeField] private bool dontDestroyOnLoad;
        [Space, SerializeField] private bool autoSchedule = true;
        [SerializeField] private List<NotificationChannel> channels = new List<NotificationChannel>();

        [Header("Schedule On Quit/Pause")]
        [SerializeField] private bool scheduleOnQuit = true;
        [SerializeField] private List<NotificationChannel> channelsOnQuit = new List<NotificationChannel>();

        private void Awake()
        {
            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        private void Start()
        {
#if UNITY_ANDROID
            PermissionPostNotification();
            Prepare();
#endif
            CancelAllScheduledNotifications();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                ScheduleOnQuit();
                if (autoSchedule)
                {
                    AutoSchedule();
                }
            }
            else
            {
                CancelAllScheduledNotifications();
            }
        }

        private void ScheduleOnQuit()
        {
            if (!scheduleOnQuit) return;
            if (!Application.isMobilePlatform) return;

            foreach (var channel in channelsOnQuit)
            {
                if (channel != null)
                {
                    channel.Schedule();
                }
            }
        }

        private void CancelAllScheduledNotifications()
        {
            if (!Application.isMobilePlatform) return;
            NotificationConsole.CancelAllScheduled();
        }

        void AutoSchedule()
        {
            foreach (var channel in channels)
            {
                channel.Schedule();
            }
        }

#if UNITY_ANDROID

        private void Prepare()
        {
            if (Application.isMobilePlatform)
            {
                var strs = new List<string>();

                foreach (var channel in channels)
                {
                    if (!channel.bigPicture) continue;
                    if (!strs.Contains(channel.namePicture)) strs.Add(channel.namePicture);
                }

                foreach (var channel in channelsOnQuit)
                {
                    if (!channel.bigPicture) continue;
                    if (!strs.Contains(channel.namePicture)) strs.Add(channel.namePicture);
                }

                foreach (string s in strs)
                {
                    App.StartCoroutine(PrepareImage(Application.persistentDataPath, s));
                }
            }
        }

        private IEnumerator PrepareImage(string destDir, string filename)
        {
            string path = Path.Combine(destDir, filename);
            if (File.Exists(path)) yield break;
            using var uwr =
                UnityWebRequest.Get(Path.Combine(Application.streamingAssetsPath, filename));
            yield return uwr.SendWebRequest();
            File.WriteAllBytes(path, uwr.downloadHandler.data);
        }

        void PermissionPostNotification()
        {
            if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
            {
                UnityEngine.Android.Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
            }
        }
#endif
    }
}