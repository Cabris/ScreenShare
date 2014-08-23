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
	int outWidth,outHeight;
	[SerializeField]
	bool isSameAsSource;
	[SerializeField]
	int fps;
	[SerializeField]
	int bitRate;

	StreamTcpServer server;
	long tatol=0;
	Timer t;
	bool isEncoding=false;
	System.Object obj;
	// Use this for initialization
	void Start () {
		if(isSameAsSource){
			outWidth=source.Width;
			outHeight=source.Height;
		}
		encoder=new EncoderH264(source.BufferStack ,source.SourceTexture);
		encoder.OutWidth=outWidth;
		encoder.OutHeight=outHeight;
		encoder.Fps=fps;
		encoder.BitRate=bitRate;
		encoder.Prepare();
		server=GetComponent<StreamTcpServer>();
		t=new Timer(new TimerCallback(Encoding));
		obj=this;
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
		//Debug.Log("blocking: "+blocking);
	}
	
	
	void startEncoding(){
		encoder.StartEncoder();
		int period=1000/fps;
		t.Change(0,period);
		isEncoding=true;
	}
	[SerializeField]
	int blocking=0;
	void  Encoding(object state){
		blocking++;
		lock(obj){
			if(isEncoding){
				byte[] encoded= encoder.Encoding();
				server.Send(encoded);//blocking point
				tatol+=encoded.Length;
				//Debug.Log(encoded.Length);
			}
		}
		blocking--;
	}
	
	void stopEncoding(){
		isEncoding=false;
		t.Change(Timeout.Infinite,Timeout.Infinite);
		encoder.StopEncoder();//dead
		t.Dispose();
		server.onDestory();
	}
	
	void OnApplicationQuit() {
		Debug.Log("tatol: "+tatol);
		stopEncoding();
	}
}
