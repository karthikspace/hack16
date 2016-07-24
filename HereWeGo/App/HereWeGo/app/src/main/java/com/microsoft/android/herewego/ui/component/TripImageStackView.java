package com.microsoft.android.herewego.ui.component;

import android.app.Activity;
import android.graphics.Color;
import android.support.v7.app.ActionBar;
import android.support.v7.app.AppCompatActivity;
import android.view.Gravity;
import android.view.ViewGroup;
import android.widget.FrameLayout;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.RelativeLayout;
import android.widget.TextView;

import com.microsoft.android.herewego.R;

import java.util.Random;

/**
 * Created by kabaska on 24-Jul-16.
 */
public class TripImageStackView extends Activity{
    public FrameLayout getView(LinearLayout container, int stackSize) {
        int[] imageList = new int[]{R.drawable.image1, R.drawable.image2, R.drawable.image3, R.drawable.image4,
                R.drawable.image5, R.drawable.image6, R.drawable.image7, R.drawable.image8, R.drawable.image9,
                R.drawable.image10, R.drawable.image11, R.drawable.image12, R.drawable.image13, R.drawable.image14,
                R.drawable.image15};
        FrameLayout imageStack = new FrameLayout(container.getContext());
        FrameLayout.LayoutParams imageStackLP = new FrameLayout.LayoutParams(500 + (stackSize * 50), 300 + (stackSize * 50));

        imageStack.setLayoutParams(imageStackLP);
        for(int i = 0; i < stackSize; i++) {
            FrameLayout stackItem = new FrameLayout(container.getContext());

            Random r = new Random();
            int imageIndex = r.nextInt(15);

            ImageView imageView = new ImageView(container.getContext());
            imageView.setImageResource(imageList[imageIndex]);
            imageView.setScaleType(ImageView.ScaleType.CENTER_CROP);
            ViewGroup.LayoutParams layoutParams = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.MATCH_PARENT);
            imageView.setLayoutParams(layoutParams);

            stackItem.addView(imageView);
            FrameLayout.LayoutParams stackItemLP = new FrameLayout.LayoutParams(500, 300);
            stackItemLP.leftMargin = 50 * (i + 1);
            stackItemLP.topMargin = 50 * (i + 1);
            stackItem.setLayoutParams(stackItemLP);

            if(i == stackSize - 1){
                TextView textView = new TextView(container.getContext());
                textView.setText("Trip Name");
                textView.setGravity(Gravity.CENTER);
                textView.setTextColor(Color.WHITE);
                textView.setTextSize(15);

                stackItem.addView(textView);
            }
            imageStack.addView(stackItem);
        }
        return imageStack;
    }
}
