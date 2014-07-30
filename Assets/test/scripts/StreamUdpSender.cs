using UnityEngine;
using System.Collections;

public class StreamUdpSender : MonoBehaviour {

	Texture2D tex ;
	Texture2D testTex;
	[SerializeField]
	Renderer testRenderer;
	[SerializeField]
 	float jpgQuality=80;
	int w=NetworkPackage.Width;
	int h=NetworkPackage.Height;
	bool isReadPixel=true;
	int length;
	byte[] jpgData;
	UdpSocketSender sender;
	// Use this for initialization
	void Start () {
		if(w>Screen.width)
			w=Screen.width;
		if(h>Screen.height)
			h=Screen.height;
		Application.runInBackground=true;
		tex=new Texture2D(w,h,TextureFormat.RGB24,false);
		testTex=new Texture2D(w,h,TextureFormat.RGB24,false);
		testRenderer.material.mainTexture=testTex;
		sender=new UdpSocketSender("127.0.0.1", 8001);
		sender.Ip="192.168.1.34";
		sender.PackageBuffer=NetworkPackage.Size;
	}

	float p=0.03f;
	float c=0;
	void OnPostRender ()
	{
		if(c>=p){
			StartCoroutine("readPixels");
			c=0;
		}else
			c+=Time.deltaTime;
	}

	IEnumerator readPixels(){
		isReadPixel=true;
		tex.ReadPixels (new Rect (0, 0, w, h), 0, 0, false);
		JPGEncoder jpgEncoder=new JPGEncoder(tex.GetPixels(),tex.width,tex.height,jpgQuality);
		jpgEncoder.doEncoding();
		jpgData=jpgEncoder.GetBytes();
		length=jpgData.Length;
		//Debug.Log(jpgData.Length);
		testTex.LoadImage(jpgData);
		isReadPixel=false;
		yield return isReadPixel;
	}

	// Update is called once per frame
	void Update () {
		if(jpgData!=null)
			sender.Send(jpgData);
	}

	void OnGUI() {
		GUI.Box(new Rect(50,Screen.height-50,100,50),length+"");
	}
}
