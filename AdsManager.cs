using GoogleMobileAds.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface RewardListener
{
    void onRewarded();
    void onRewardFailed();
    void onRewardClosed();
}


public class AdsManager : MonoBehaviour
{
    public bool isTesting = true;
    public bool isHasReward = false;
    public int timeBetweenShowAds = 240;   
    private InterstitialAd interstitial;
    private RewardBasedVideoAd rewardBasedVideo;

    private static AdsManager _instance;
    private bool isFbAdsLoaded = false;
    private DateTime lastTimeShowAds;
    public int numberReward;
    public string idFullAdmobAndroid;
    public string idRewardAndroid;
    public string idBannerAndroid;
    public BannerView bannerView;
    public string ID = "";
    public int percentFb;
    public int delay;
    public static AdsManager Instance
    {
        get
        {
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }

    // Use this for initialization
    void Start()
    {
        MobileAds.Initialize(ID);
        lastTimeShowAds = DateTime.Now;
        requestFullAdmob();
        RequestBanner();
        if (PlayerPrefs.GetInt(Contants.LevelLock, 1) < 3)
        {
            if (bannerView != null)
            {
                HideBanner();
            }               
        }
        if (isHasReward)
        {
            this.rewardBasedVideo = RewardBasedVideoAd.Instance;
            rewardBasedVideo.OnAdRewarded += RewardBasedVideo_OnAdRewarded;
            rewardBasedVideo.OnAdClosed += HandleRewardBasedVideoClosed;
            rewardBasedVideo.OnAdFailedToLoad += HandleRewardBasedVideoFailedToLoad;
            requestRewardBasedVideo();
        }      
        timeBetweenShowAds = delay;      
    }
    public AdRequest createAdRequest()
    {
        return new AdRequest.Builder()
                .Build();

    }
    public void showBanner()
    {
        if (bannerView != null)
        {
            bannerView.Show();

        }
    }
    public void HideBanner()
    {
        if (bannerView != null)
        {
            bannerView.Hide();
        }
    }
    public void RequestBanner()
    {
        // Create a 320x50 banner at the top of the screen.    
        bannerView = new BannerView(idBannerAndroid, AdSize.Banner, AdPosition.Bottom);
        bannerView.LoadAd(createAdRequest());
    }


    private void requestFullAdmob()
    {
        interstitial = new InterstitialAd(idFullAdmobAndroid);
        AdRequest request = new AdRequest.Builder().Build();
        interstitial.LoadAd(request);
    }
    
    public void showFullAds()
    {
        if (lastTimeShowAds == null)
            return;

        DateTime current = DateTime.Now;
        TimeSpan between = current.Subtract(lastTimeShowAds);
        if (between.TotalSeconds <= timeBetweenShowAds)
            return;
        if (interstitial != null)
        {
            if (interstitial.IsLoaded())
            {
                interstitial.Show();
                lastTimeShowAds = DateTime.Now;
                timeBetweenShowAds = 60;
            }
            else
                requestFullAdmob();
        }        
    }

    public void showRewardAdsVideo()
    {
        if (rewardBasedVideo != null && rewardBasedVideo.IsLoaded())
        {
            //rewardListener = listener;
            rewardBasedVideo.Show();
        }
        else
        {
            requestRewardBasedVideo();
            //listener.onRewardFailed();
             if (numberReward == 2)
            {
                GameManager.instanceGame.txtNotifySkips.gameObject.active = true;
            }
            else if (numberReward == 1)
            {
                GameManager.instanceGame.txtNotifyHints.gameObject.active = true;

            }
        }
    }


    private void requestRewardBasedVideo()
    {
        AdRequest request = new AdRequest.Builder().Build();
        this.rewardBasedVideo.LoadAd(request, idRewardAndroid);
    }



    private void RewardBasedVideo_OnAdRewarded(object sender, Reward e)
    {
        //if (this.rewardListener != null)
        //    rewardListener.onRewarded();
        if (numberReward == 1)
        {
            MapLevel.instance.numberShowHints = 1;
            GameManager.instanceGame. panelBuyHints.active = false;
            MapLevel.instance.panelShowHints.active = true;
        }
        else if (numberReward == 2)
        {
            PlayerPrefs.SetInt(Contants.listInt + (GameManager.instanceGame.level - 1), 1);
           GameManager.instanceGame. panelSkip.active = false;
            if (GameManager.instanceGame.levelLock <= 41 && GameManager.instanceGame.level == GameManager.instanceGame.levelLock)
            {
                GameManager.instanceGame.levelLock++;
                PlayerPrefs.SetInt(Contants.LevelLock, GameManager.instanceGame.levelLock);
            }
            if (GameManager.instanceGame.level < 42 && GameManager.instanceGame.level <= GameManager.instanceGame.levelLock)
            {
                GameManager.instanceGame.level++;
                PlayerPrefs.SetInt(Contants.Levels, GameManager.instanceGame.level);
            }
            Application.LoadLevel("Play");
        }       
    }

    private void HandleRewardBasedVideoClosed(object sender, EventArgs args)
    {
        //if (this.rewardListener != null)
        //    rewardListener.onRewardClosed();

        requestRewardBasedVideo();
    }

    private void HandleRewardBasedVideoFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        //if (this.rewardListener != null)
        //    rewardListener.onRewardFailed();
    }

    public bool isHaveReward()
    {
        if (rewardBasedVideo == null)
            return false;
        else
            return rewardBasedVideo.IsLoaded();
    }

    void OnDestroy()
    {
        // Dispose of interstitial ad when the scene is destroyed
      
        Debug.Log("InterstitialAdTest was destroyed!");
    }

}
