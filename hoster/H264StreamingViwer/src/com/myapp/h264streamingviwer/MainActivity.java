package com.myapp.h264streamingviwer;

import com.example.h264streamingviwer.R;
import com.simpleMessage.sender.MessageSender;
import com.stream.source.StreamReceiver;
import android.app.Activity;
import android.app.Fragment;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.SurfaceView;
import android.view.View;
import android.view.View.OnClickListener;
import android.view.ViewGroup;
import android.view.Window;
import android.widget.Button;
import android.widget.EditText;

public class MainActivity extends Activity implements IOnConnectedListener {

	ConnectionFragment connectionFragment;
	VideoFragment videoFragment;

	@Override
	protected void onCreate(final Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		requestWindowFeature(Window.FEATURE_NO_TITLE);// must 1sf
		setContentView(R.layout.activity_main);
		connectionFragment = new ConnectionFragment();
		connectionFragment.setConnectedListener(this);
		getFragmentManager().beginTransaction().add(R.id.container, connectionFragment).commit();
	}

	protected void onStop() {
		super.onStop();
		//connectionFragment.onDestroy();
		//videoFragment.onDestroy();
	}

	@Override
	public void onConnected(String ip, int port) {
		// TODO Auto-generated method stub
		videoFragment= new VideoFragment(ip, port);
		getFragmentManager().beginTransaction().add(R.id.container,videoFragment).commit();
	}

}
