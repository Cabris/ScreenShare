package com.myapp.h264streamingviwer;

import com.myapp.h264streamingviwer.SensorFusion.IOrientationChange;
import com.simpleMessage.sender.MessageSender;

import android.content.Context;
import android.hardware.Sensor;
import android.hardware.SensorEvent;
import android.hardware.SensorEventListener;
import android.hardware.SensorManager;
import android.util.Log;

public class SensorClient implements IOrientationChange{
	MessageSender sender;
	SensorFusion sensorFusion;

	public SensorClient(Context context, MessageSender sender) {
		this.sender = sender;
		sensorFusion = new SensorFusion(context,this);
	}

	public void onStart() {
		sensorFusion.strat();
	}

	public void onDestroy() {
		sensorFusion.destroy();
	}

	@Override
	public void onOrientationChange(float azimuth, float pitch, float roll) {
		Log.d("SensorClient", "azimuth: " + azimuth + ",pitch: " + pitch + ",roll: " + roll);
		String data = azimuth + "," + pitch + "," + roll;
		sender.getQueue().add(data);
	}

}
