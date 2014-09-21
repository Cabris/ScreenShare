package com.myapp.h264streamingviwer;

import android.content.Context;
import android.hardware.Sensor;
import android.hardware.SensorEvent;
import android.hardware.SensorManager;

public class OrientationSensor extends BaseSensor {

	public OrientationSensor(Context context, IOrientationChange orientationChange) {
		super(context, orientationChange);
	}

	@Override
	public void strat() {
		super.strat();
		mSensorManager.registerListener(this, mSensorManager.getDefaultSensor(Sensor.TYPE_ROTATION_VECTOR),
				SensorManager.SENSOR_DELAY_GAME);
	}

	@Override
	public void destroy() {
		mSensorManager.unregisterListener(this);
		super.destroy();
	}

	@Override
	public void onSensorChanged(SensorEvent event) {
		float[] values=new float[4];
		SensorManager.getQuaternionFromVector(values, event.values.clone());
		this.orientationChange.onOrientationChange(values);
	}

}
