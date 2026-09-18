using System;
using VirtueSky.Utils;

namespace VirtueSky.Ads
{
    /// <summary>
    /// Pairs a primary ad unit with an optional backup ad unit of the same format.
    /// When the backup is enabled both units are loaded side by side, the primary is always
    /// preferred on show, and the backup is discarded and reloaded if it sits in cache
    /// longer than the configured expire time without being shown.
    /// When the backup is disabled this behaves exactly like loading/showing the primary alone.
    /// </summary>
    public class BackupAdUnitGroup
    {
        private readonly AdUnit primary;
        private readonly AdUnit backup;
        private readonly Func<bool> useBackup;
        private readonly Func<float> expireSeconds;

        private DateTime backupReadyTime;
        private bool backupWasReady;

        public BackupAdUnitGroup(AdUnit primary, AdUnit backup, Func<bool> useBackup, Func<float> expireSeconds)
        {
            this.primary = primary;
            this.backup = backup;
            this.useBackup = useBackup;
            this.expireSeconds = expireSeconds;
        }

        public bool IsBackupEnabled => useBackup != null && useBackup() && backup != null && !string.IsNullOrEmpty(backup.Id);

        public AdUnit Primary => primary;

        public void Init()
        {
            primary?.Init();
            if (IsBackupEnabled) backup.Init();
        }

        public void Load()
        {
            LoadUnit(primary);
            if (!IsBackupEnabled) return;
            CheckBackupExpired();
            LoadUnit(backup);
        }

        /// <summary>
        /// Primary first, backup only when the primary has nothing to show.
        /// Falls back to the primary so callers keep the current no-op show behaviour when neither is ready.
        /// </summary>
        public AdUnit Select()
        {
            if (!IsBackupEnabled) return primary;
            if (primary != null && primary.IsReady()) return primary;
            return backup.IsReady() ? backup : primary;
        }

        private static void LoadUnit(AdUnit unit)
        {
            if (unit == null || unit.IsShowing) return;
            if (!unit.IsReady() && !unit.IsLoading) unit.Load();
        }

        private void CheckBackupExpired()
        {
            var isReady = backup.IsReady();
            // Stamp the moment the backup finishes loading, so the expire window starts from a fresh ad.
            if (isReady && !backupWasReady) backupReadyTime = DateTime.UtcNow;
            backupWasReady = isReady;

            if (!isReady || backup.IsShowing) return;
            if ((DateTime.UtcNow - backupReadyTime).TotalSeconds < expireSeconds()) return;

            VLog.Log($"Advertising: backup ad unit expired, reloading: {backup.Id}");
            backup.Destroy();
            backup.Load();
            backupReadyTime = DateTime.UtcNow;
            backupWasReady = false;
        }
    }
}
