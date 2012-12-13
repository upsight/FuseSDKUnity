package com.fusepowered.fuseactivities;

import android.content.Intent;
import android.net.Uri;
import android.os.AsyncTask;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.view.View.OnClickListener;
import android.view.ViewGroup.LayoutParams;
import android.view.Window;
import android.view.animation.AnimationSet;
import android.webkit.WebView;
import android.widget.Button;
import android.widget.FrameLayout;
import android.widget.LinearLayout;
import android.widget.RelativeLayout;

import com.fusepowered.activities.FuseApiBrowser;
import com.fusepowered.fuseapi.Constants;
import com.fusepowered.fuseapi.FuseAPI;
import com.fusepowered.util.ActivityResults;
import com.fusepowered.util.FuseAdCallback;
import com.fusepowered.util.FuseAnimationController;

public class FuseApiAdBrowser extends FuseApiBrowser {

    private static final String TAG = "FuseApiAdBrowser";

    RelativeLayout layout;
    String action;
    int adId;

    private class ShowButtonsAfterXMillisecondsTask extends AsyncTask<Integer, Void, Void> {
        @Override
        protected Void doInBackground(Integer... params) {
            try {
                // All we do is sleep for the given number of milliseconds
                Thread.sleep(params[0]);
            }
            catch (InterruptedException e) {
                Log.e(TAG, "Unexpected interruption", e);
            }
            return null;
        }

        @Override
        protected void onPostExecute(Void result) {
            showAdButtons(0);
        }
    }

    /** Called when the activity is first created. */
    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        requestWindowFeature(Window.FEATURE_NO_TITLE);

        layout = new RelativeLayout(this);

        LayoutParams params = new RelativeLayout.LayoutParams(LayoutParams.WRAP_CONTENT, LayoutParams.FILL_PARENT);
        layout.setLayoutParams(params);

        Bundle extras = getIntent().getExtras();
        extras.setClassLoader(getClassLoader());

        action = extras.getString(Constants.EXTRA_AD_ACTION);
        adId = extras.getInt(Constants.EXTRA_AD_ID);
        String html = extras.getString(Constants.EXTRA_AD_HTML);
        Log.d(TAG, String.format("Displaying ad [%d]...", adId));
        Log.d(TAG, String.format("Ad body: %s", html));

        WebView webView = new WebView(this);
        webView.setWebViewClient(new Callback());
        webView.setLayoutParams(params);
        final String mimeType = "text/html";
        final String encoding = "UTF-8";
        webView.loadDataWithBaseURL("", html, mimeType, encoding, "");

        layout.addView(webView);

        FrameLayout.LayoutParams layoutParams = new FrameLayout.LayoutParams(params);
        this.addContentView(layout, layoutParams);

        layout.startAnimation(FuseAnimationController.getTranslateAnimation(500));

        ShowButtonsAfterXMillisecondsTask task = new ShowButtonsAfterXMillisecondsTask();
        task.execute(Integer.valueOf(5000));

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
                    if (FuseAPI.fuseAdCallback != null && FuseAPI.fuseAdCallback instanceof FuseAdCallback)
                        FuseAPI.fuseAdCallback.adWillClose();
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
    protected void onResume() {
        super.onResume();
        FuseAPI.initializeFuseAPI(this, getApplicationContext());
    }

    @Override
    protected void onStop() {
        super.onStop();
        FuseAPI.adDismiss();
    }
}