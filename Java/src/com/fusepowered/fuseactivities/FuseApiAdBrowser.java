package com.fusepowered.fuseactivities;

import java.util.Map;
import java.util.HashMap;

import android.annotation.SuppressLint;
import android.content.Intent;

import android.net.Uri;
import android.os.Bundle;
import android.util.Log;

import android.view.View;
import android.view.View.OnClickListener;
import android.view.ViewGroup.LayoutParams;
import android.view.Gravity;
import android.view.Window;
import android.view.WindowManager;
import android.view.animation.Animation;
import android.view.animation.AnimationSet;
import android.view.animation.Animation.AnimationListener;
import android.webkit.WebView;
import android.widget.Button;
import android.widget.FrameLayout;
import android.widget.LinearLayout;
import android.widget.RelativeLayout;
import android.webkit.WebChromeClient;
import android.os.Build;
import android.content.res.Configuration;

import com.fusepowered.activities.FuseApiBrowser;
import com.fusepowered.fuseapi.Constants;
import com.fusepowered.fuseapi.FuseAPI;
import com.fusepowered.util.ActivityResults;
import com.fusepowered.util.FuseAdCallback;
import com.fusepowered.util.FuseAdSkip;
import com.fusepowered.util.FuseAnimationController;

public class FuseApiAdBrowser extends FuseApiBrowser {

    private static final String TAG = "FuseApiAdBrowser ";
    private Callback myClient;
    RelativeLayout layout;
    
    String action;
    int adId;

    int orientation;
	int pwidth;
	int pheight;
    int lwidth;
    int lheight;
    double overallscale;
    boolean callbackActivated = false;
    
    WebView webView;
    RelativeLayout.LayoutParams params;
    FrameLayout.LayoutParams layoutParams;
    private static boolean closing = false;
    
    final int ICE_CREAM_SANDWICH = 14;
    final int ICE_CREAM_SANDWICH_MR1 = 15;
    
    /** Called when the activity is first created. */
    @SuppressLint("NewApi")
	@Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
 
        requestWindowFeature(Window.FEATURE_NO_TITLE);
        getWindow().setFlags(WindowManager.LayoutParams.FLAG_FULLSCREEN,
                WindowManager.LayoutParams.FLAG_FULLSCREEN);
        layout = new RelativeLayout(this);
        params = new RelativeLayout.LayoutParams(LayoutParams.FILL_PARENT, LayoutParams.FILL_PARENT);
        layout.setGravity(Gravity.CENTER);
        layout.setLayoutParams(params);
        layout.setBackgroundColor(0);
        params.addRule(RelativeLayout.CENTER_IN_PARENT);

        Bundle extras = getIntent().getExtras();
        extras.setClassLoader(getClassLoader());

        action = extras.getString(Constants.EXTRA_AD_ACTION);
        adId = extras.getInt(Constants.EXTRA_AD_ID);
        String html = extras.getString(Constants.EXTRA_AD_HTML);
        //Log.d(TAG, String.format("Displaying ad [%d]...", adId));
        //Log.d(TAG, String.format("Ad body: %s", html));
        if (html.length() < 1)
        {
        	FuseAPI.sendFuseAdSkip(FuseAdSkip.FUSE_AD_SKIP_NO_HTML.getErrorCode());
        	return;
        }
        webView = new WebView(this);
        Map<String, String> extraHeaders = new HashMap<String, String>();
        extraHeaders.put("Referer", "about:blank");
        webView.loadUrl("about:blank", extraHeaders);
        webView.setBackgroundColor(0);
        webView.setId(1);
        myClient = new Callback();
        webView.setWebViewClient(myClient);
        webView.getSettings().setJavaScriptEnabled(true);
        webView.setWebChromeClient(new WebChromeClient());
        webView.setLayoutParams(params);
        
        
        
        final String mimeType = "text/html";
        final String encoding = "UTF-8";
        
        webView.loadDataWithBaseURL("http://www.fuseboxx.com", html, mimeType, encoding, null);
        layout.addView(webView);
        
        webView.setHorizontalFadingEdgeEnabled(false);
        webView.setVerticalFadingEdgeEnabled(false);
        webView.setHorizontalScrollBarEnabled(false);
        webView.setVerticalScrollBarEnabled(false);

        if (Build.VERSION.SDK_INT == ICE_CREAM_SANDWICH || Build.VERSION.SDK_INT == ICE_CREAM_SANDWICH_MR1)
        {
        	
        	webView.setLayerType(View.LAYER_TYPE_SOFTWARE, null);
        }
        
       
        //layoutParams = new FrameLayout.LayoutParams(params);
        layoutParams = new FrameLayout.LayoutParams(LayoutParams.WRAP_CONTENT, LayoutParams.WRAP_CONTENT);

        orientation = extras.getInt(Constants.EXTRA_AD_ORIENTATION);
    	pwidth = extras.getInt(Constants.EXTRA_AD_PWIDTH);
    	pheight = extras.getInt(Constants.EXTRA_AD_PHEIGHT);
        lwidth = extras.getInt(Constants.EXTRA_AD_LWIDTH);
        lheight = extras.getInt(Constants.EXTRA_AD_LHEIGHT);
    	
    	int adWidth;
    	int adHeight;
    	
    	
    	// How do we display this ad?  If the ad has both orientations then pick the one that is formated for the screen
        if (orientation == 0)
        {
        	if (getResources().getDisplayMetrics().widthPixels > getResources().getDisplayMetrics().heightPixels)
        	{
        		adWidth = lwidth;
            	adHeight = lheight;
        	}
        	else
        	{
        		adWidth = pwidth;
            	adHeight = pheight;
        	}

        }
        else if (orientation == 1)
        {
        	adWidth = pwidth;
        	adHeight = pheight;
        }
        else
        {
        	adWidth = lwidth;
        	adHeight = lheight;
        }
        
        
        int pixelWidth = adWidth;
        int pixelHeight = adHeight;
        
        adWidth *= getResources().getDisplayMetrics().density;
        adHeight *= getResources().getDisplayMetrics().density;

        layoutParams.height = pixelHeight;
        layoutParams.width = pixelWidth;
        layoutParams.gravity = Gravity.CENTER;
        
        
        //Determine which dimension is too big, and by how much we should scale
        double widthscale = 1.0;
        double heightscale = 1.0;
        overallscale = 1.0;
        if (getResources().getDisplayMetrics().widthPixels < adWidth)
        {
        	widthscale = adWidth / getResources().getDisplayMetrics().widthPixels;
        }
        if (getResources().getDisplayMetrics().heightPixels < adHeight)
        {
        	heightscale = adHeight / getResources().getDisplayMetrics().heightPixels;
        }
        
        //Determine the largest scale factor to use
        if (widthscale > heightscale)
        	overallscale = widthscale;
        else if (heightscale > widthscale)
        	overallscale = heightscale;
        
        overallscale = (1/overallscale) * 100;

        //Set the webview scale to show the entire ad
        webView.setInitialScale((int)overallscale);
        
        this.addContentView(layout, layoutParams);
        
        layout.startAnimation(FuseAnimationController.getSlideInAnimation(500));

        //Log.d("FUSEAD", "Recording a Fuse Ad being Displayed");
        FuseAPI.adDisplay(adId);
        if (FuseAPI.fuseAdCallback != null && FuseAPI.fuseAdCallback instanceof FuseAdCallback) {
            FuseAPI.fuseAdCallback.adDisplayed();
        }       

    }

    protected void showAdButtons(int id) {

        switch (id) {
        case 0:

            Button yesButton = new Button(getApplicationContext());
            yesButton.setText("Yes");

            yesButton.setOnClickListener(new OnClickListener() {

                @Override
                public void onClick(View v) {
                    FuseAPI.adClick();
                    Intent data = new Intent();
                    data.setData(Uri.parse(ActivityResults.AD_CLICKED.name()));
                    setResult(RESULT_OK, data);
                    if (FuseAPI.fuseAdCallback != null && FuseAPI.fuseAdCallback instanceof FuseAdCallback)
                        FuseAPI.fuseAdCallback.adClicked();
                    finish();
                }
            });

            Button noThanksButton = new Button(getApplicationContext());
            noThanksButton.setText("No Thanks");

            noThanksButton.setOnClickListener(new OnClickListener() {

                @Override
                public void onClick(View v) {
                    Intent data = new Intent();
                    data.setData(Uri.parse(ActivityResults.AD_DISPLAYED.name()));
                    setResult(RESULT_OK, data);
                    finish();
                }
            });

            RelativeLayout buttonLayout = new RelativeLayout(getBaseContext());

            RelativeLayout.LayoutParams bottomNavParams = new RelativeLayout.LayoutParams(LayoutParams.WRAP_CONTENT, LayoutParams.WRAP_CONTENT);
            bottomNavParams.addRule(RelativeLayout.ALIGN_PARENT_BOTTOM);
            bottomNavParams.addRule(RelativeLayout.CENTER_HORIZONTAL);
            buttonLayout.setLayoutParams(bottomNavParams);

            LinearLayout linearLayout = new LinearLayout(getBaseContext());
            LinearLayout.LayoutParams params = new LinearLayout.LayoutParams(LayoutParams.WRAP_CONTENT, LayoutParams.WRAP_CONTENT);
            linearLayout.setLayoutParams(params);

            linearLayout.addView(noThanksButton);
            linearLayout.addView(yesButton);

            buttonLayout.addView(linearLayout);

            AnimationSet set = new AnimationSet(true);
            set.addAnimation(FuseAnimationController.getTranslateAnimation(500));

            buttonLayout.setLayoutAnimation(FuseAnimationController.getAdLayoutAnimationController(set));
            layout.addView(buttonLayout);
            buttonLayout.startLayoutAnimation();

        default:
            break;
        }
    }
    
    @Override
    protected void onPause()
    {
        super.onPause();
        
        if( !closing )
        {
            FuseAPI.suspendSession();
        }
        closing = false;
    }

    @Override
    protected void onResume()
    {
        super.onResume();
        FuseAPI.initializeFuseAPI(this, getApplicationContext());
        FuseAPI.resumeSession(this, null);
    }

    @Override
    protected void onStop()
    {
        super.onStop();
   
        if (FuseAPI.fuseAdCallback != null && FuseAPI.fuseAdCallback instanceof FuseAdCallback) {

            if(callbackActivated == false)
            {
                FuseAPI.adDismiss();
                callbackActivated = true;
                FuseAPI.fuseAdCallback.adWillClose();
            }
        }
            
    }
    
    @Override
    public void onBackPressed()
    {
        if(callbackActivated == false)
        {
            FuseAPI.adDismiss();
            FuseAPI.setFuseChildActivityOnDisplay(false);
            callbackActivated = true;
            FuseAPI.fuseAdCallback.adWillClose();
        }
        
        finish();
    }
    
    @Override
    public void onConfigurationChanged(Configuration newConfig) {
        super.onConfigurationChanged(newConfig);

        layoutParams.gravity = Gravity.CENTER;
        
        if (orientation != 0)
        	return;
        //layout.invalidate();
        if (getResources().getDisplayMetrics().widthPixels > getResources().getDisplayMetrics().heightPixels)
        {
        	layoutParams.width = lwidth;
        	layoutParams.height = lheight;
        }
        else
        {
        	layoutParams.width = pwidth;
        	layoutParams.height = pheight;
        }
        
    }
    
    
    
    public void handleOnExit()
    {
        closing = true;
		Animation transition = FuseAnimationController.getSlideOutAnimation(500);
	    transition.setAnimationListener(new AnimationListener() 
	    {			
			@Override
			public void onAnimationEnd(Animation animation) 
			{
				layout.setVisibility(View.GONE);
				//Log.d("AdBrowser", "Got an exit event for an ad");

	            finish();				
			}
	
			@Override
			public void onAnimationRepeat(Animation animation)
			{	
			}
	
			@Override
			public void onAnimationStart(Animation animation)
			{	
			}
		});
	    
	    layout.startAnimation(transition);
	    
    }
    
    
    
    
    

}
