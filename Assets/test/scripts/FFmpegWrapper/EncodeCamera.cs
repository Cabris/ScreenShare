using UnityEngine;
using System.Collections;
using System.Text;
public class EncodeCamera : MonoBehaviour {

	EncoderH264 encoder;
	[SerializeField]
	CameraSource source;
	[SerializeField]
	int outWidth,outHeight,fps;

	StreamTcpServer server;

	ASCIIEncoding ascEncoder = new ASCIIEncoding();
	// Use this for initialization
	void Start () {
		encoder=new EncoderH264(source.BufferStack ,source.SourceTexture);
		encoder.OutWidth=outWidth;
		encoder.OutHeight=outHeight;
		encoder.Fps=fps;
		encoder.Prepare();
		server=GetComponent<StreamTcpServer>();
	}
	bool isEncoding=false;
	// Update is called once per frame
	void Update () {
	
		if(Input.GetKeyDown(KeyCode.F2)){
			isEncoding=!isEncoding;
			if(isEncoding)
				encoder.StartEncoder();
			if(!isEncoding)
				encoder.StopEncoder();
			Debug.Log("isEncoding: "+isEncoding);
			if(isEncoding){
				float period=1f/(float)fps;
				InvokeRepeating("Encoding",0,period);
			}else{
				CancelInvoke("Encoding");
			}
		}			
	}

	long tatol=0;

	void  Encoding(){
		if(isEncoding){
		byte[] encoded= encoder.Encoding();
		server.Send(encoded);
		//Debug.Log(encoded.Length);
		tatol+=encoded.Length;
		}
	}

	void OnApplicationQuit() {
		Debug.Log("tatol: "+tatol);
		CancelInvoke("Encoding");
		if(encoder!=null)
			encoder.StopEncoder();
	}
}
