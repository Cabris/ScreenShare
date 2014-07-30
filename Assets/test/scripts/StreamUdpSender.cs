using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
public class StreamUdpSender : MonoBehaviour {
	[SerializeField]
	float jpgQuality=80;
	UdpSocketSender sender;
	bool isReadingPixel=true;
	int dataLength;
	List<byte[]> packageDatas;
	[SerializeField]
	int widthDiv;
	[SerializeField]
	int heightDiv;

	Texture2D tex;
	// Use this for initialization
	void Start () {
		Application.runInBackground=true;
		packageDatas=new List<byte[]>();
		tex=new Texture2D(Screen.width,Screen.height);
		sender=new UdpSocketSender("127.0.0.1", 8001);
		//sender.Ip="192.168.1.34";
		sender.PackageBuffer=NetworkPackage.Size;
		widthDiv=Screen.width/NetworkPackage.Width;
		heightDiv=Screen.height/NetworkPackage.Height;
	}
	
	float p=0.03f;
	float c=0;
	void OnPostRender ()
	{
		if(c>=p){
			StartCoroutine(readPixels());
			c=0;
		}else
			c+=Time.deltaTime;
	}
	
	IEnumerator readPixels(){
		isReadingPixel=true;
		packageDatas.Clear();
		int w=NetworkPackage.Width;
		int h=NetworkPackage.Height;

		tex.ReadPixels (new Rect (0, 0, Screen.width,Screen.height), 0, 0, false);
		for(int i=0;i<heightDiv;i++)
		for(int j=0;j<widthDiv;j++){
			int x=j*w,y=i*h;
			Color[] colors=tex.GetPixels(x, y, w,h);
			JPGEncoder jpgEncoder=new JPGEncoder(colors,w,h,jpgQuality);
			jpgEncoder.doEncoding();
			byte[] jpgData=jpgEncoder.GetBytes();
			int jpgLength=jpgData.Length;
			MemoryStream memoryStream=new MemoryStream();
			byte[] swData=BitConverter.GetBytes(Screen.width);
			byte[] shData=BitConverter.GetBytes(Screen.height);

			byte[] xData=BitConverter.GetBytes(x);
			byte[] yData=BitConverter.GetBytes(y);
			byte[] wData=BitConverter.GetBytes(w);
			byte[] hData=BitConverter.GetBytes(h);

			byte[] lengthData=BitConverter.GetBytes(jpgLength);
			int xDataSize=xData.Length;

			memoryStream.Write(swData,0,xDataSize);
			memoryStream.Write(shData,0,xDataSize);
			memoryStream.Write(xData,0,xDataSize);
			memoryStream.Write(yData,0,xDataSize);
			memoryStream.Write(wData,0,xDataSize);
			memoryStream.Write(hData,0,xDataSize);
			memoryStream.Write(lengthData,0,xDataSize);

			memoryStream.Write(jpgData,0,jpgLength);
			byte[] packageData=memoryStream.ToArray();
			dataLength=packageData.Length;
			memoryStream.Close();
			packageDatas.Add(packageData);
			//Debug.Log(jpgData.Length);
		}
		
		isReadingPixel=false;
		yield return isReadingPixel;
	}
	
	// Update is called once per frame
	void Update () {
		if(!isReadingPixel){
			foreach(byte[] jpgData in packageDatas)
			sender.Send(jpgData);
		}
	}
	
	void OnGUI() {
		GUI.Box(new Rect(50,Screen.height-50,100,50),dataLength+"");
	}
}
