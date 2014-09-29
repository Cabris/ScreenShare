package com.myapp.h264streamingviwer;

import com.example.h264streamingviwer.R;

import android.app.Fragment;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;

public class ConnectionFragment extends Fragment {

	public ConnectionFragment() {
		// TODO Auto-generated constructor stub
	}

	@Override
	public View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
		return inflater.inflate(R.layout.connection_main, container, false);
	}
}
