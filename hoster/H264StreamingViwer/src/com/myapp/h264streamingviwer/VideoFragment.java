package com.myapp.h264streamingviwer;
import com.example.h264streamingviwer.R;

import android.app.Fragment;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;

public class VideoFragment extends Fragment {

	String ip;
	int port;
	
	public VideoFragment(String ip,int port) {
		this.ip=ip;
		this.port=port;
	}
	
	@Override
	public View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
		 return inflater.inflate(R.layout.video_surface, container, false);
	}

}
