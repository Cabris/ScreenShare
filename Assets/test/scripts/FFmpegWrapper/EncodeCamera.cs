using UnityEngine;
using System.Collections;
using System.Text;
using System.Threading;
using System;
public class EncodeCamera : MonoBehaviour {
	
	EncoderH264 encoder;
	[SerializeField]
	CameraSource source;
	[SerializeField]
	int outWidth,outHeight,fps;
	[SerializeField]
	int bitRate;
	StreamTcpServer server;
	long tatol=0;
	Timer t;
	bool isEncoding=false;
	ASCIIEncoding ascEncoder = new ASCIIEncoding();
	// Use this for initialization
	void Start () {
		encoder=new EncoderH264(source.BufferStack ,source.SourceTexture);
		encoder.OutWidth=outWidth;
		encoder.OutHeight=outHeight;
		encoder.Fps=fps;
		encoder.BitRate=bitRate;
		encoder.Prepare();
		server=GetComponent<StreamTcpServer>();
		t=new Timer(new TimerCallback(Encoding));
	}

	// Update is called once per frame
	void Update () {
		
		if(Input.GetKeyDown(KeyCode.F2)){
			isEncoding=!isEncoding;
			if(isEncoding)
				startEncoding();
			if(!isEncoding)
				stopEncoding();
			Debug.Log("isEncoding: "+isEncoding);
		}			
	}
	

	void startEncoding(){
		encoder.StartEncoder();
		int period=1000/fps;
		t.Change(0,period);
		isEncoding=true;
	}

	void  Encoding(object state){
		try{
		if(isEncoding){
			byte[] encoded= encoder.Encoding();
			server.Send(encoded);
			Debug.Log(encoded.Length);
			tatol+=encoded.Length;
			}
		}catch(Exception e){
			Debug.LogException(e);
		}
	}

	void stopEncoding(){
		isEncoding=false;
		t.Change(Timeout.Infinite,Timeout.Infinite);
		encoder.StopEncoder();
		t.Dispose();
		server.onDestory();
	}

	void OnApplicationQuit() {
		Debug.Log("tatol: "+tatol);
		stopEncoding();
	}
}
