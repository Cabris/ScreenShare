package com.myapp.h264streamingviwer.funcs;

import android.content.Context;
import android.hardware.Sensor;
import android.hardware.SensorEventListener;
import android.hardware.SensorManager;

public abstract class BaseSensor implements SensorEventListener{

	protected SensorManager mSensorManager = null;
	protected IOrientationChange orientationChange;
	
	public BaseSensor(Context context, IOrientationChange orientationChange){
		mSensorManager = (SensorManager) context.getSystemService(Context.SENSOR_SERVICE);
		this.orientationChange = orientationChange;
	}
	
	public void strat() {}

	public void destroy() {
		mSensorManager.unregisterListener(this);
	}
	
	@Override
	public void onAccuracyChanged(Sensor sensor, int accuracy) {}
	
}
