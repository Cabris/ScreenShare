package com.myapp.h264streamingviwer;

import com.example.h264streamingviwer.R;
import com.myapp.h264streamingviwer.funcs.Decoder;
import com.stream.source.StreamReceiver;

import android.app.Fragment;
import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.SurfaceView;
import android.view.View;
import android.view.ViewGroup;

public class VideoFragment extends Fragment {

	String ip;
	int port;
	Decoder decoder;
	StreamReceiver receiver;
	SurfaceView surfaceView;

	public VideoFragment(String ip, int port) {
		this.ip = ip;
		this.port = port;
	}

	@Override
	public View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
		View view = inflater.inflate(R.layout.video_surface, container, false);
		return view;
	}

	@Override
	public void onActivityCreated(Bundle savedInstanceState) {
		super.onActivityCreated(savedInstanceState);
		surfaceView = (SurfaceView) getView().findViewById(R.id.surface);
		receiver = new StreamReceiver(ip, port);
		decoder = new Decoder(surfaceView, receiver);
	}

	@Override
	public void onStart() {
		super.onStart();
		receiver.Connect();
		decoder.onCreate();
	}
	
	@Override
	public void onDestroy() {
		Log.d("VideoFragment", "onDestroy");
		super.onStop();
		if (decoder != null)
			decoder.onStop();
		if (receiver != null)
			receiver.onStop();
	}

}