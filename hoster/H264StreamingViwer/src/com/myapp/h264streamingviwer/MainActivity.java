package com.myapp.h264streamingviwer;

import java.io.IOException;

import com.example.h264streamingviwer.R;
import com.stream.source.FileReceiver;
import com.stream.source.StreamReceiver;

import android.R.integer;
import android.app.Activity;
import android.os.Bundle;
import android.util.Log;
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
				receiver = new StreamReceiver(adrs, port){
					@Override
					protected int fillBuffer() throws IOException {
						int bytesRead=super.fillBuffer();
						Log.d("StreamReceiver", "fill buffer: " +currentLength+"/"+targetLength);
						return bytesRead;
					}
					
					@Override
					protected void gotPacket(byte[] packet) {
						super.gotPacket(packet);
						Log.d("StreamReceiver", "gotPacket: " + packet.length);
						//Log.d("StreamReceiver", "queue_size: "+getQueue().size());
					}
					
					@Override
					protected int updateLength() throws IOException, Exception {
						int i=super.updateLength();
						Log.d("StreamReceiver", "updateLength: " +currentLength+"/"+ targetLength);
						return i;
					}
					
				};
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
