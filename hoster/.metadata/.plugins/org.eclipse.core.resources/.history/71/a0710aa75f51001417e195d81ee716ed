package com.myapp.h264streamingviwer;

import android.app.Fragment;
import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.view.View.OnClickListener;
import android.widget.Button;
import android.widget.EditText;

import com.example.h264streamingviwer.R;
import com.myapp.h264streamingviwer.funcs.SensorClient;
import com.simpleMessage.sender.MessageSender;

class ConnectionFragment extends Fragment implements OnClickListener {
	SensorClient sClient;
	MessageSender sender;
	EditText ipAddressText;
	EditText portText;
	IOnConnectedListener connectedListener;

	public ConnectionFragment() {
	}

	@Override
	public View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
		View view = inflater.inflate(R.layout.connection_main, container, false);
		return view;
	}

	@Override
	public void onActivityCreated(final Bundle savedInstanceState) {
		super.onActivityCreated(savedInstanceState);
		ipAddressText = (EditText) getView().findViewById(R.id.ip_editText);
		portText = (EditText) getView().findViewById(R.id.port_editText);
		Button connectButton = (Button) getView().findViewById(R.id.connect_button);

		connectButton.setOnClickListener(this);
	}

	@Override
	public void onClick(View v) {
		String adrs = ipAddressText.getText().toString();
		adrs = "192.168.0.100";
		int port = Integer.parseInt(portText.getText().toString());
		port = 8888;

		sender = new MessageSender(adrs, 8887);
		sClient = new SensorClient(getActivity(), sender);
		sClient.onStart();
		sender.connect();
		if(connectedListener!=null)
			connectedListener.onConnected(adrs, port);
	}

	public void setConnectedListener(IOnConnectedListener connectedListener) {
		this.connectedListener = connectedListener;
	}
	
	@Override
	public void onDestroy() {
		Log.d("ConnectionFragment", "onDestroy");
		super.onDestroy();
		if (sClient != null)
			sClient.onStop();
		if (sender != null)
			sender.onStop();
	}

}
