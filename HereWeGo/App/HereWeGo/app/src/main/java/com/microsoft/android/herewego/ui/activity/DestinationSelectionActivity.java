package com.microsoft.android.herewego.ui.activity;

import android.content.Intent;
import android.os.Bundle;
import android.support.design.widget.FloatingActionButton;
import android.support.design.widget.Snackbar;
import android.support.v7.app.AppCompatActivity;
import android.support.v7.widget.ListViewCompat;
import android.support.v7.widget.Toolbar;
import android.text.TextWatcher;
import android.text.Editable;
import android.view.View;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.EditText;
import android.widget.ListView;
import android.widget.Toast;

import com.microsoft.android.herewego.R;

import java.util.ArrayList;
import java.util.List;

import static android.R.layout.simple_list_item_1;

public class DestinationSelectionActivity extends AppCompatActivity {

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_destination_selection);
        Toolbar toolbar = (Toolbar) findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);

        ArrayList<String> destinationValuesList = new ArrayList<String>();
        destinationValuesList.add(0, "Hyderabad");
        destinationValuesList.add(1, "Goa");
        destinationValuesList.add(2, "New York");
        destinationValuesList.add(3, "Paris");
        destinationValuesList.add(4, "Berlin");
        destinationValuesList.add(5, "London");
        destinationValuesList.add(6, "San Francisco");
        destinationValuesList.add(7, "Rome");
        destinationValuesList.add(8, "Barcelona");
        destinationValuesList.add(9, "Delhi");
        destinationValuesList.add(10, "Mumbai");
        destinationValuesList.add(11, "Chennai");
        destinationValuesList.add(12, "Athens");
        destinationValuesList.add(13, "Amsterdam");
        destinationValuesList.add(14, "Bratislava");
        destinationValuesList.add(15, "Santorini");
        destinationValuesList.add(16, "Bangkok");
        destinationValuesList.add(17, "Pattaya");

        String[] destinationValuesArray = {"Hyderabad", "Goa", "New York", "Paris"};

        final ListView destinationListView = (ListView) findViewById(R.id.destinationList_view);
        ArrayAdapter<String> adapter = new ArrayAdapter<String>(this,
                android.R.layout.simple_list_item_1, destinationValuesList);
        destinationListView.setAdapter(adapter);
        destinationListView.setOnItemClickListener(new AdapterView.OnItemClickListener() {
            @Override
            public void onItemClick(AdapterView<?> parent, View view,
                                    int position, long id) {
                launchSelectDateActivity();
            }
        });

        EditText destinationSearchEdit = (EditText)findViewById(R.id.destinationSearchEdit_text);
        destinationSearchEdit.addTextChangedListener(new TextWatcher() {


            public void onTextChanged(CharSequence cs, int arg1, int arg2, int arg3) {
                // When user changed the Text
//                ((SimpleAdapter)getListAdapter()).getFilter().filter(cs);

            }


            public void beforeTextChanged(CharSequence arg0, int arg1, int arg2,
                                          int arg3) {
                // TODO Auto-generated method stub

            }


            public void afterTextChanged(Editable arg0) {
                // TODO Auto-generated method stub
            }
        });


        FloatingActionButton fab = (FloatingActionButton) findViewById(R.id.fab);
        fab.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                Snackbar.make(view, "Replace with your own action", Snackbar.LENGTH_LONG)
                        .setAction("Action", null).show();
            }
        });
    }

    public void launchSelectDateActivity() {
        Intent intent = new Intent(this, DateSelectionActivity.class);
        startActivity(intent);
    }

}
