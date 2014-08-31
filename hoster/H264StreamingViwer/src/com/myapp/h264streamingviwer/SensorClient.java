package com.myapp.h264streamingviwer;

import com.example.h264streamingviwer.R;

import android.app.Activity;
import android.content.Context;
import android.hardware.Sensor;
import android.hardware.SensorEvent;
import android.hardware.SensorEventListener;
import android.hardware.SensorManager;
import android.util.Log;
import android.widget.TextView;

public class SensorClient extends Thread implements SensorEventListener{
	private SensorManager sManager;
	//private Sensor mSensor;
	TextView h,p,r;
	
	public SensorClient(Context context) {
		sManager = (SensorManager) context.getSystemService(Context.SENSOR_SERVICE);
		sManager.registerListener(this,
				sManager.getDefaultSensor(Sensor.TYPE_GYROSCOPE),
			    SensorManager.SENSOR_DELAY_GAME);
		h=(TextView) ((Activity)context).findViewById(R.id.h_textView);
		p=(TextView) ((Activity)context).findViewById(R.id.p_textView);
		r=(TextView) ((Activity)context).findViewById(R.id.r_textView);
	}

	@Override
	public void run() {
		super.run();
		while (true) {
			
		}
	}

	@Override
	public void onSensorChanged(SensorEvent event) {
		 updateOrientation(event.values[0], event.values[1], event.values[2]);
	}

	@Override
	public void onAccuracyChanged(Sensor sensor, int accuracy) {}
	
	private void updateOrientation(float heading, float pitch, float roll) {
		  Log.d("SensorClient", "heading: "+heading+",pitch: "+pitch+",roll: "+roll);
		  h.setText("heading: "+heading);
		  p.setText("pitch: "+pitch);
		  r.setText("roll: "+roll);
	}

	public void onDestroy() {
		sManager.unregisterListener(this);
	}
	
}