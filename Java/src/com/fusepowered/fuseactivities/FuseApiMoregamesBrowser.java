package com.fusepowered.fuseactivities;

import android.annotation.SuppressLint;
import android.content.Intent;
//import android.graphics.Bitmap;
//import android.graphics.drawable.BitmapDrawable;
import android.net.Uri;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.view.View.OnClickListener;
import android.view.Window;
import android.view.animation.AlphaAnimation;
import android.view.animation.Animation;
import android.view.animation.AnimationSet;
import android.view.animation.LayoutAnimationController;
import android.view.animation.TranslateAnimation;
import android.webkit.WebView;
import android.webkit.WebViewClient;
import android.webkit.WebSettings;
import android.webkit.WebChromeClient;
import android.widget.FrameLayout;
import android.widget.FrameLayout.LayoutParams;
import android.widget.ImageButton;
import android.util.DisplayMetrics;

import com.fusepowered.activities.FuseApiBrowser;
import com.fusepowered.fuseapi.Constants;
import com.fusepowered.fuseapi.FuseAPI;
import com.fusepowered.fuseapi.NetworkService;
import com.fusepowered.util.ActivityResults;

@SuppressLint("SetJavaScriptEnabled")
public class FuseApiMoregamesBrowser extends FuseApiBrowser
{
	
    public static boolean backPressed = false;
    
    /** Called when the activity is first created. */
    @Override
    public void onCreate(Bundle savedInstanceState) {
    	super.onCreate(savedInstanceState);
    	requestWindowFeature(Window.FEATURE_NO_TITLE);
        
        Uri url = getIntent().getData();
        Bundle extras = getIntent().getExtras();
        extras.setClassLoader(getClassLoader());
       

        LayoutParams params = new FrameLayout.LayoutParams(
        		LayoutParams.MATCH_PARENT,
        		LayoutParams.MATCH_PARENT);
        DisplayMetrics dm = this.getResources().getDisplayMetrics();
             
        int height = (int) Math.floor(65 * dm.density);
        int width = (int) Math.floor(45 * dm.density);
        
        Log.d("GAME CONFIGURATION", "This is the density " + dm.density);
        
        LayoutParams paramsBTN = new FrameLayout.LayoutParams(
        		height,
        		width);
        
        AnimationSet set = new AnimationSet(true);
        Animation animation = new AlphaAnimation(0.0f, 1.0f);
        animation.setDuration(100);
        set.addAnimation(animation);
       
        animation = new TranslateAnimation(
            Animation.RELATIVE_TO_SELF, 0.0f, Animation.RELATIVE_TO_SELF, 0.0f,
            Animation.RELATIVE_TO_SELF, -1.0f, Animation.RELATIVE_TO_SELF, 0.0f
        );
        
        animation.setDuration(500);
        set.addAnimation(animation);
       
        LayoutAnimationController controller =
            new LayoutAnimationController(set, 0.25f);
        
        FrameLayout layout = new FrameLayout(this); 
       
        WebView webView = new WebView(this);
        webView.setWebViewClient(new Callback());
        webView.setHorizontalFadingEdgeEnabled(false);
        webView.setVerticalFadingEdgeEnabled(false);
        webView.setHorizontalScrollBarEnabled(false);
        webView.setVerticalScrollBarEnabled(false);
        webView.loadUrl(url.toString());
        webView.setLayoutParams(params);
        webView.setWebChromeClient(new WebChromeClient()); 

        WebViewClient webClient = new WebViewClient(){
            @Override
            public boolean shouldOverrideUrlLoading(WebView  view, String  url){
            	
            	if(url.startsWith("market://")) {
            		Intent market_intent = new Intent(Intent.ACTION_VIEW, Uri.parse(url));
        			startActivity(market_intent);
        			        			
        		} else {
        			view.loadUrl(url);
        		}
            	
                return true;
            }
        };
        
        webView.setWebViewClient(webClient);
        
        WebSettings webSettings = webView.getSettings();
        webSettings.setJavaScriptEnabled(true);
        
        NetworkService ns = new NetworkService();
        ImageButton imageButton = new ImageButton(this);
        imageButton.setLayoutParams(paramsBTN);
        imageButton.bringToFront();
        String imageUrl = extras.getString(Constants.EXTRA_RETURN);
        
        //Asynchronous call get an image (required for Android 3+)
        
        ns.createImageButton(imageUrl, imageButton);
        
        imageButton.setOnClickListener(new OnClickListener()
        {
			@Override
			public void onClick(View v)
            {
                FuseApiMoregamesBrowser.backPressed = true;
                FuseAPI.showingMoreGames = false;
				Intent data = new Intent();
				data.setData(Uri.parse(ActivityResults.MORE_GAMES_DISPLAYED.name()));
				setResult(RESULT_OK,data);
				finish();
			}
		});

        layout.addView(webView);
        layout.addView(imageButton);
        
        FrameLayout.LayoutParams layoutParams = new FrameLayout.LayoutParams(params);
        
        layout.setLayoutAnimation(controller);
        
        this.addContentView(layout, layoutParams);       
        
        layout.startAnimation(animation);
    }
    
    @Override
    protected void onPause()
    {
        super.onPause();
        FuseAPI.showingMoreGames = false;
        if( !backPressed )
        {
            FuseAPI.suspendSession();
        }
        backPressed = false;
    }
    
    @Override
	protected void onResume()
    {
		super.onResume();
		FuseAPI.initializeFuseAPI(this, getApplicationContext());
        FuseAPI.resumeSession(this, null);
        FuseAPI.showingMoreGames = true;
	}
    
}
