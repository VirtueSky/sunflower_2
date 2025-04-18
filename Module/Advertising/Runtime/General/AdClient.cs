namespace VirtueSky.Ads
{
    public abstract class AdClient
    {
        protected bool statusAppOpenFirstIgnore;

        public abstract void Initialize();
        public abstract AdUnit InterstitialAdUnit();
        public abstract void LoadInterstitial();
        public abstract AdUnit RewardAdUnit();
        public abstract void LoadRewarded();
        public abstract AdUnit RewardedInterstitialAdUnit();
        public abstract void LoadRewardedInterstitial();
        public abstract AdUnit AppOpenAdUnit();
        public abstract void LoadAppOpen();
        public abstract void ShowAppOpen();
        public abstract AdUnit BannerAdUnit();

        public abstract void LoadBanner();

        // Native overlay only for admob
        public abstract AdUnit NativeOverlayAdUnit();
        public abstract void LoadNativeOverlay();
    }
}