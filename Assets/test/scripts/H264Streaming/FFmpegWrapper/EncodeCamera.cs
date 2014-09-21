using UnityEngine;
using System.Collections;
using System.Text;
using System.Timers;
using System;
public class EncodeCamera : MonoBehaviour {

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
	[SerializeField]
	bool isEncoding=false;
	[SerializeField]
	int blocking=0;

	EncoderH264 encoder;
	StreamTcpServer server;
	long tatol=0;
	System.Timers.Timer timer;
	System.Object obj;
	// Use this for initialization
	void Start () {
		if(source==null){
			Debug.LogError("null source");
			Application.Quit();
		}

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
		obj=this;

		double interval=1000.0/(double)fps;
		timer=new System.Timers.Timer(interval);
		timer.Elapsed+=Encoding;
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

	public void CheckStatus(System.Object stateInfo)
	{
		Debug.Log("Encoding");
	}
	
	void startEncoding(){
		source.StartCapture();
		encoder.StartEncoder();
		timer.Start();
		isEncoding=true;
	}

	void  Encoding(object source, ElapsedEventArgs e){
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
		source.StopCapture();
		isEncoding=false;
		timer.Stop();
		timer.Dispose();
		encoder.StopEncoder();
		server.onDestory();
	}
	
	void OnApplicationQuit() {
		stopEncoding();
		Debug.Log("tatol: "+tatol);
	}
}
