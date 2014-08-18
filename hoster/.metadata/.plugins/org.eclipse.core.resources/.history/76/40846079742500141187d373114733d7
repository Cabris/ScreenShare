package com.myapp.h264streamingviwer;

import android.app.Activity;
import android.os.Bundle;

public class MainActivity extends Activity {

	Decoder decoder;
	StreamReceiver receiver;

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		decoder = new Decoder(this);
		receiver=new StreamReceiver("192.168.1.48",8888);
		receiver.Connect();
		//decoder.onCreate(savedInstanceState);
	}

	protected void onDestroy() {
		receiver.onDestory();
		super.onDestroy();
	}

}
