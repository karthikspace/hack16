package com.microsoft.android.herewego.ui.activity;

import android.app.DatePickerDialog;
import android.app.TimePickerDialog;
import android.os.Bundle;
import android.support.design.widget.FloatingActionButton;
import android.support.design.widget.Snackbar;
import android.support.v7.app.AppCompatActivity;
import android.support.v7.widget.Toolbar;
import android.view.View;
import android.widget.DatePicker;
import android.widget.TextView;
import android.widget.TimePicker;

import com.microsoft.android.herewego.R;

import org.w3c.dom.Text;

import java.util.Calendar;

public class DateSelectionActivity extends AppCompatActivity {

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_date_selection);
        Toolbar toolbar = (Toolbar) findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);

        Calendar calendar = Calendar.getInstance();
        StringBuilder fromDate = new StringBuilder(calendar.get(Calendar.DATE) + "/" +
                calendar.get(Calendar.MONTH) + "/" +
                calendar.get(Calendar.YEAR));
        TextView fromDateText = (TextView)findViewById(R.id.fromDate_text);
        fromDateText.setText(fromDate);
        TextView toDateText = (TextView)findViewById(R.id.toDate_text);
        toDateText.setText(fromDate);
        TextView fromTimeText = (TextView)findViewById(R.id.fromTime_text);
        fromTimeText.setText("12:00");
        TextView toTimeText = (TextView)findViewById(R.id.toTime_text);
        toTimeText.setText("12:00");
        FloatingActionButton fab = (FloatingActionButton) findViewById(R.id.fab);
        fab.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {

            }
        });
    }


    public void selectFromDate(View view) {
        Calendar calendar = Calendar.getInstance();
        int maxYear = calendar.get(Calendar.YEAR);
        int maxMonth = calendar.get(Calendar.MONTH);
        int maxDay = calendar.get(Calendar.DAY_OF_MONTH);

        DatePickerDialog dialog =
                new DatePickerDialog(view.getContext(), setFromDate, maxYear , maxMonth , maxDay);

        dialog.show();
    }

    private DatePickerDialog.OnDateSetListener setFromDate = new DatePickerDialog.OnDateSetListener() {
        public void onDateSet(DatePicker view, int selectedYear,
                              int selectedMonth, int selectedDay) {
            StringBuilder fromDate = new StringBuilder(selectedDay + "/" +
                    selectedMonth + "/" +
                    selectedYear);
            TextView fromDateText = (TextView) findViewById(R.id.fromDate_text);
            fromDateText.setText(fromDate);
        }
    };

    public void selectFromTime(View view) {
        TimePickerDialog fromTimePicker = new TimePickerDialog(view.getContext(), setFromTime, 12, 0, true);
        fromTimePicker.show();
    }

    private TimePickerDialog.OnTimeSetListener setFromTime = new TimePickerDialog.OnTimeSetListener() {
        public void onTimeSet(TimePicker view, int hourOfDay, int minute) {
            StringBuilder fromTime = new StringBuilder(hourOfDay + ":" + minute);
            TextView fromTimeText = (TextView) findViewById(R.id.fromTime_text);
            fromTimeText.setText(fromTime);
        }
    };

    public void selectToDate(View view) {
        Calendar cal = Calendar.getInstance();
        int maxYear = cal.get(Calendar.YEAR);
        int maxMonth = cal.get(Calendar.MONTH);
        int maxDay = cal.get(Calendar.DAY_OF_MONTH);

        DatePickerDialog dialog =
                new DatePickerDialog(view.getContext(), setToDate, maxYear , maxMonth , maxDay);
        dialog.show();
    }

    private DatePickerDialog.OnDateSetListener setToDate = new DatePickerDialog.OnDateSetListener() {
        public void onDateSet(DatePicker view, int selectedYear,
                              int selectedMonth, int selectedDay) {
            StringBuilder toDate = new StringBuilder(selectedDay + "/" +
                    selectedMonth + "/" +
                    selectedYear);
            TextView toDateText = (TextView) findViewById(R.id.toDate_text);
            toDateText.setText(toDate);
        }
    };

    public void selectToTime(View view) {
        TimePickerDialog toTimePicker = new TimePickerDialog(view.getContext(), setToTime, 12, 0, true);
        toTimePicker.show();
    }

    private TimePickerDialog.OnTimeSetListener setToTime = new TimePickerDialog.OnTimeSetListener() {
        public void onTimeSet(TimePicker view, int hourOfDay, int minute) {
            StringBuilder toTime = new StringBuilder(hourOfDay + ":" + minute);
            TextView toTimeText = (TextView) findViewById(R.id.toTime_text);
            toTimeText.setText(toTime);
        }
    };
}
