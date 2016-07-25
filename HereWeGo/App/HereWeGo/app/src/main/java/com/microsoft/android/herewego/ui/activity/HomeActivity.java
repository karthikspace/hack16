package com.microsoft.android.herewego.ui.activity;

import android.content.Intent;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.support.v7.widget.Toolbar;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.LinearLayout;
import android.widget.RelativeLayout;

import com.microsoft.android.herewego.R;
import com.microsoft.android.herewego.ui.component.TripImageStackView;

public class HomeActivity extends AppCompatActivity {

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_home);

        Toolbar myToolbar = (Toolbar) findViewById(R.id.home_actionbar);
        setSupportActionBar(myToolbar);

        TripImageStackView stackView = new TripImageStackView();

        LinearLayout myTripsLayout = (LinearLayout) findViewById(R.id.mytrips_view);
        myTripsLayout.addView(stackView.getView(myTripsLayout, 3));
        myTripsLayout.addView(stackView.getView(myTripsLayout, 3));
        myTripsLayout.addView(stackView.getView(myTripsLayout, 3));
        myTripsLayout.addView(stackView.getView(myTripsLayout, 3));

        LinearLayout suggestedTripsLayout = (LinearLayout) findViewById(R.id.suggestedtrips_view);
        suggestedTripsLayout.addView(stackView.getView(suggestedTripsLayout, 1));
        suggestedTripsLayout.addView(stackView.getView(suggestedTripsLayout, 1));
        suggestedTripsLayout.addView(stackView.getView(suggestedTripsLayout, 1));
        suggestedTripsLayout.addView(stackView.getView(suggestedTripsLayout, 1));

        LinearLayout bookmarkedTripsView = (LinearLayout) findViewById(R.id.bookmarkedtrips_view);
        bookmarkedTripsView.addView(stackView.getView(bookmarkedTripsView, 3));
        bookmarkedTripsView.addView(stackView.getView(bookmarkedTripsView, 3));
        bookmarkedTripsView.addView(stackView.getView(bookmarkedTripsView, 3));
        bookmarkedTripsView.addView(stackView.getView(bookmarkedTripsView, 3));
    }

    public void createNewTrip(View view) {
        Intent intent = new Intent(this, DestinationSelectionActivity.class);
        startActivity(intent);
    }
}
