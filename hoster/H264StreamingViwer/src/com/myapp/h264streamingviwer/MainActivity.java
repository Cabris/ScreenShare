package com.myapp.h264streamingviwer;

import com.example.h264streamingviwer.R;
import com.stream.source.FileReceiver;
import com.stream.source.StreamReceiver;

import android.app.Activity;
import android.os.Bundle;
import android.view.SurfaceView;
import android.view.View;
import android.view.Window;
import android.widget.Button;
import android.widget.EditText;

public class MainActivity extends Activity {

	Decoder decoder;
	StreamReceiver receiver;
	FileReceiver fileReceiver;

	@Override
	protected void onCreate(final Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		requestWindowFeature(Window.FEATURE_NO_TITLE);
		setContentView(R.layout.activity_main);
		Button connectButton = (Button) findViewById(R.id.connect_button);
		final EditText ipAddressText = (EditText) findViewById(R.id.ip_editText);
		final EditText portText = (EditText) findViewById(R.id.port_editText);
		final SurfaceView surfaceView = (SurfaceView) findViewById(R.id.surface);
		connectButton.setOnClickListener(new View.OnClickListener() {
			@Override
			public void onClick(View v) {
				String adrs = ipAddressText.getText().toString();
				int port = Integer.parseInt(portText.getText().toString());
				receiver = new StreamReceiver(adrs, port);
				receiver.Connect();
				decoder = new Decoder(surfaceView, receiver);
				decoder.onCreate(savedInstanceState);
			}
		});

	}

	protected void onDestroy() {
		if (decoder != null)
			decoder.onDestroy();
		if (receiver != null)
			receiver.onDestory();
		super.onDestroy();
	}

}
