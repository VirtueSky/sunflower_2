namespace VirtueSky.Ads
{
    // Each interface describes one ad format. An AdClient implements only the formats its mediation supports.

    public interface IBannerClient
    {
        AdUnit BannerAdUnit();
        void LoadBanner();
    }

    public interface IInterstitialClient
    {
        AdUnit InterstitialAdUnit();
        void LoadInterstitial();
    }

    public interface IRewardClient
    {
        AdUnit RewardAdUnit();
        void LoadRewarded();
    }

    public interface IRewardedInterstitialClient
    {
        AdUnit RewardedInterstitialAdUnit();
        void LoadRewardedInterstitial();
    }

    public interface IAppOpenClient
    {
        AdUnit AppOpenAdUnit();
        void LoadAppOpen();
        void ShowAppOpen();
    }

    public interface INativeOverlayClient
    {
        AdUnit NativeOverlayAdUnit();
        void LoadNativeOverlay();
    }
}