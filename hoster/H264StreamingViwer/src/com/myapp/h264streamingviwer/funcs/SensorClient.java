package com.myapp.h264streamingviwer.funcs;

import com.simpleMessage.sender.MessageSender;
import android.content.Context;
import android.util.Log;

public class SensorClient implements IOrientationChange{
	MessageSender sender;
	BaseSensor sensor;

	public SensorClient(Context context, MessageSender sender) {
		this.sender = sender;
		sensor = new OrientationSensor(context,this);
	}

	public void onStart() {
		sensor.strat();
	}

	public void onStop() {
		String exit_code="ORIENT_EXIT";
		sender.getQueue().add(exit_code);
		sensor.destroy();
	}

	@Override
	public void onOrientationChange(float[] values) {
		String data="";
		for (int i = 0; i < values.length; i++) {
			String dataString=Float.toString(values[i]);
			data=data+dataString;
			if(i<values.length-1)
				data+=",";
		}
		//Log.d("SensorClient", data);
		sender.getQueue().add(data);
	}

}
