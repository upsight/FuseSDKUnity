package com.fusepowered.fuseactivities;

import android.content.Intent;
import android.graphics.Bitmap;
import android.graphics.drawable.BitmapDrawable;
import android.net.Uri;
import android.os.Bundle;
import android.view.View;
import android.view.View.OnClickListener;
import android.view.Window;
import android.view.animation.AlphaAnimation;
import android.view.animation.Animation;
import android.view.animation.AnimationSet;
import android.view.animation.LayoutAnimationController;
import android.view.animation.TranslateAnimation;
import android.webkit.WebView;
import android.widget.FrameLayout;
import android.widget.FrameLayout.LayoutParams;
import android.widget.ImageButton;

import com.fusepowered.activities.FuseApiBrowser;
import com.fusepowered.fuseapi.Constants;
import com.fusepowered.fuseapi.FuseAPI;
import com.fusepowered.fuseapi.NetworkService;
import com.fusepowered.util.ActivityResults;

public class FuseApiMoregamesBrowser extends FuseApiBrowser {
	
    /** Called when the activity is first created. */
    @Override
    public void onCreate(Bundle savedInstanceState) {
    	super.onCreate(savedInstanceState);
    	
    	requestWindowFeature(Window.FEATURE_NO_TITLE);
        
        Uri url = getIntent().getData();
        Bundle extras = getIntent().getExtras();
        extras.setClassLoader(getClassLoader());
       
        LayoutParams params = new FrameLayout.LayoutParams(
        		LayoutParams.WRAP_CONTENT,
        		LayoutParams.WRAP_CONTENT);
       
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
        webView.loadUrl(url.toString());
        webView.setLayoutParams(params);
        
        NetworkService ns = new NetworkService();
        
        ImageButton imageButton = new ImageButton(this);
        imageButton.setLayoutParams(params);
        imageButton.bringToFront();
        String imageUrl = extras.getString(Constants.EXTRA_RETURN);
        Bitmap bitmap = ns.downloadImage2(imageUrl);
        imageButton.setBackgroundDrawable(new BitmapDrawable(bitmap));
        imageButton.setOnClickListener(new OnClickListener() {
			@Override
			public void onClick(View v) {
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
	protected void onResume() {
		super.onResume();
		FuseAPI.initializeFuseAPI(this, getApplicationContext());
	}
    
}
