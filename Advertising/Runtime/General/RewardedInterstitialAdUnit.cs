using System;

namespace Wolf.Ads
{
    [Serializable]
    public class RewardedInterstitialAdUnit : AdUnit
    {
        public RewardedInterstitialAdUnit(string _androidId, string _iOSId) : base(_androidId, _iOSId)
        {
        }

        public override void Init()
        {
            throw new NotImplementedException();
        }

        public override void Load()
        {
            throw new NotImplementedException();
        }

        public override bool IsReady()
        {
            throw new NotImplementedException();
        }

        protected override void ShowImpl()
        {
            throw new NotImplementedException();
        }

        public override void Destroy()
        {
            throw new NotImplementedException();
        }
    }
}