using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;


public class StreamUdpSender : MonoBehaviour
{
	[SerializeField]
	float jpgQuality = 80;
	[SerializeField]
	int widthDiv;
	[SerializeField]
	int heightDiv;
	UdpSocketSender sender;
	bool isReadingPixel = true;
	int dataLength;
	List<byte[]> packageDatas;
	Texture2D tex;
	MemoryStream memStream;
	JPGEncoder jpgEncoder;

	Thread sendThread;
	bool isSending=true;
	int scrW=320;
	int scrH=240;
	// Use this for initialization
	void Start ()
	{
		Application.runInBackground = true;
		packageDatas = new List<byte[]> ();
		tex = new Texture2D (scrW, scrH);
		sender = new UdpSocketSender ("127.0.0.1", 8081);
		//sender.Ip="192.168.0.198";
		sender.PackageBuffer = NetworkPackage.Size;
		widthDiv = scrW / NetworkPackage.Width;
		heightDiv = scrH / NetworkPackage.Height;
		memStream = new MemoryStream (NetworkPackage.Size);
		jpgEncoder = new JPGEncoder (jpgQuality);
		sendThread=new Thread(new ThreadStart(Sending));
		//sendThread.Start();
	}
	
	float p = 0.033f;
	float c = 0;
	
	void OnPostRender ()
	{
		//StartCoroutine ("readPixels");
		if (c >= p) {
			//StartCoroutine ("readPixels");
			readPixels();
			c = 0;
		} else
			c += Time.deltaTime;
	}
	
	void readPixels ()
	{
		isReadingPixel = true;
		packageDatas.Clear ();
		int w = NetworkPackage.Width;
		int h = NetworkPackage.Height;
		
		tex.ReadPixels (new Rect (0, 0, scrW, scrH), 0, 0, false);/////
		
		for(int i=0;i<heightDiv;i++)
		for(int j=0;j<widthDiv;j++){
			int x=j*w,y=i*h;
			Color[] colors=tex.GetPixels(x, y, w,h);
			
			jpgEncoder.setImage(colors,w,h);
			jpgEncoder.doEncoding();////////
			byte[] jpgData=jpgEncoder.GetBytes();
			int jpgLength=jpgData.Length;
			//Debug.Log(jpgLength);
			byte[] packageData = createPackage (w, h, x, y, jpgData);
			packageDatas.Add(packageData);
			if(jpgData.Length>maxLengh)
				maxLengh=jpgData.Length;

			queue.Enqueue(packageData);
		}
		
		isReadingPixel = false;

		while(queue.Count>0){
			byte[] jpgData=queue.Dequeue() as Byte[];
			sender.Send (jpgData);
		}

		Debug.Log(maxLengh);
		//yield return isReadingPixel;
	}
	
	int maxLengh=0;
	Queue queue=new Queue();


	byte[] createPackage (int w, int h, int x, int y, byte[] jpgData)
	{
		int jpgLength = jpgData.Length;
		memStream.SetLength (0);
		byte[] swData = BitConverter.GetBytes (Screen.width);
		byte[] shData = BitConverter.GetBytes (Screen.height);
		byte[] xData = BitConverter.GetBytes (x);
		byte[] yData = BitConverter.GetBytes (y);
		byte[] wData = BitConverter.GetBytes (w);
		byte[] hData = BitConverter.GetBytes (h);
		byte[] lengthData = BitConverter.GetBytes (jpgLength);
		int xDataSize = xData.Length;
		memStream.Write (swData, 0, xDataSize);
		memStream.Write (shData, 0, xDataSize);
		memStream.Write (xData, 0, xDataSize);
		memStream.Write (yData, 0, xDataSize);
		memStream.Write (wData, 0, xDataSize);
		memStream.Write (hData, 0, xDataSize);
		memStream.Write (lengthData, 0, xDataSize);
		memStream.Write (jpgData, 0, jpgLength);
		byte[] packageData = memStream.ToArray ();
		dataLength = packageData.Length;
		return packageData;
	}

	// Update is called once per frame
	void Update ()
	{
		
	}
	
	void Sending ()
	{
		while(isSending){
			//if (!isReadingPixel) {
//			while(queue.Count>0){
//				byte[] jpgData=queue.Dequeue() as Byte[];
//				sender.Send (jpgData);
//			}
			//}
			//Thread.Sleep(15);
			Debug.Log("Sending..."+isReadingPixel);
		}
	}

	void OnApplicationQuit() {
		sender.server.Close();
		//if (_Thread.IsAlive) {
		sendThread.Join ();
		sendThread.Abort ();
		isSending = false;
		//}
	}

	void OnGUI ()
	{
		GUI.Box (new Rect (50, Screen.height - 50, 100, 50), dataLength + "\n"+queue.Count);
	}
}
