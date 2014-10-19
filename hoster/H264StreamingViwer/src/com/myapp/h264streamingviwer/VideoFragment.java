package com.myapp.h264streamingviwer;

import java.util.logging.Logger;

import com.example.h264streamingviwer.R;
import com.myapp.h264streamingviwer.funcs.Decoder;
import com.stream.source.StreamReceiver;

import android.app.Fragment;
import android.os.Bundle;
import android.os.Handler;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.SurfaceHolder;
import android.view.SurfaceView;
import android.view.View;
import android.view.ViewGroup;

public class VideoFragment extends Fragment implements SurfaceHolder.Callback, IHandleVideoSize{

	String ip;
	int port;
	Decoder decoder;
	StreamReceiver receiver;
	private SurfaceHolder holder;
	

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
		SurfaceView surfaceView = (SurfaceView) getView().findViewById(R.id.surface);
		holder=surfaceView.getHolder();
		holder.addCallback(this);
		receiver = new StreamReceiver(ip, port);
		decoder = new Decoder(receiver);
		decoder.videoSize=this;
	}

	@Override
	public void onStart() {
		super.onStart();
		receiver.Connect();
		
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

	@Override
	public void surfaceCreated(SurfaceHolder holder) {
		Log.d("VideoFragment", "surfaceCreated");
		decoder.onCreate(holder);
	}

	@Override
	public void surfaceChanged(SurfaceHolder holder, int format, int width,
			int height) {
		Log.d("VideoFragment", "surfaceChanged");
	}

	@Override
	public void surfaceDestroyed(SurfaceHolder holder) {
		Log.d("VideoFragment", "surfaceDestroyed");
	}

	@Override
	public void handleVideoSize(final int videoWidth, final int videoHeight) {
		Log.d("Decoder", "New format :" + videoWidth + ", " + videoHeight);
		final SurfaceView surfaceView = (SurfaceView) getView().findViewById(R.id.surface);
		Handler mainHandler = new Handler(surfaceView.getContext().getMainLooper());

		Runnable runnable = new Runnable() {

			@Override
			public void run() {
				float videoProportion = (float) videoWidth / (float) videoHeight;

				android.view.ViewGroup.LayoutParams lp = surfaceView.getLayoutParams();
				int screenWidth = lp.width;
				int screenHeight = lp.height;
				float screenProportion = (float) screenWidth / (float) screenHeight;

				if (videoProportion > screenProportion) {
					lp.width = screenWidth;
					lp.height = (int) ((float) screenWidth / videoProportion);
				} else {
					lp.width = (int) (videoProportion * (float) screenHeight);
					lp.height = screenHeight;
				}
				surfaceView.setLayoutParams(lp);
			}
		};
		// mainHandler.post(runnable);
	}
}
