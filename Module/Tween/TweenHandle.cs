namespace VirtueSky.Tweening
{
    internal enum TweenValueKind : byte
    {
        None,
        Float,
        Vector2,
        Vector3,
        Vector4,
        Color,
        Quaternion
    }

    public readonly struct TweenHandle
    {
        readonly TweenValueKind kind;
        readonly int index;
        readonly uint version;

        internal TweenHandle(TweenValueKind kind, int index, uint version)
        {
            this.kind = kind;
            this.index = index;
            this.version = version;
        }

        public bool IsActive => TweenRuntime.IsActive(kind, index, version);
        public bool IsPaused => TweenRuntime.IsPaused(kind, index, version);

        public void Cancel() => TweenRuntime.Cancel(kind, index, version);
        public void Complete() => TweenRuntime.Complete(kind, index, version);
        public void Pause() => TweenRuntime.Pause(kind, index, version);
        public void Resume() => TweenRuntime.Resume(kind, index, version);
    }
}