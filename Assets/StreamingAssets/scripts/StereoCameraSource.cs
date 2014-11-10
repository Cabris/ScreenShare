using UnityEngine;
using System;
using System.Collections;
using System.Threading;

public class StereoCameraSource : ImageSource
{
	[SerializeField]
	CameraSource[] sources;
	Thread captureThread;
	bool isCapturing;
	Color32[] map;

	void Awake ()
	{
		Application.targetFrameRate = -1;
		BufferStack = new ConcurrentStack<UnityEngine.Color32[]> ();
		if(sources.Length<2)
			throw new Exception("camera not ready");
		foreach (CameraSource c in sources)
			c.OnSize();
		if(sources[0].Height!=sources[1].Height)
			throw new Exception("camera not fit");
		Width=sources[0].Width+sources[1].Width;
		Height=sources[0].Height;
		Debug.Log(Width+","+Height);
	}

	void OnEnable(){

	}

	void Start ()
	{
		captureThread=new Thread(new ThreadStart(Capturing));
		isCapturing=false;
		map=new Color32[Width*Height];
		for(int i=0;i<Width*Height;i++)
			map[i]=Color.green;
	}
	
	void  Capturing ()
	{
		while(isCapturing){
			try{
				NewMethod ();
			}catch(Exception e){
				Debug.LogException(e);
			}
		}
	}
	
	void NewMethod ()
	{
		if(sources [0].BufferStack.Count>0&&sources [1].BufferStack.Count>0){
			Color32[] leftMap = sources [0].BufferStack.Pop ();
			Color32[] rightMap = sources [1].BufferStack.Pop ();
			int pixelIndex=0,leftIndex=0,rightIndex=0;
			for (int j = 0; j < Height; j++)
			for (int i = 0; i < Width; i++) {
				if(i<Width/2){//left
					Color32 c=leftMap[leftIndex];
					map[pixelIndex]=c;
					leftIndex++;
					pixelIndex++;
				}
				else{//right
					Color32 c=rightMap[rightIndex];
					map[pixelIndex]=c;
					rightIndex++;
					pixelIndex++;
				}
			}
			BufferStack.Push(map.Clone() as UnityEngine.Color32[]);
			if(BufferStack.Count>2)
				BufferStack.Clear();
		}
	}
	
	public override int Width {
		get;
		protected set;
	}
	
	public override int Height {
		get;
		protected set;
	}
	
	public override void StartCapture ()
	{
		foreach (CameraSource c in sources)
			c.StartCapture ();
		isCapturing=true;
		captureThread.Start();
	}
	
	public override void StopCapture ()
	{
		foreach (CameraSource c in sources)
			c.StopCapture ();
		isCapturing=false;
		if(captureThread.IsAlive)
			captureThread.Join();
	}
	
	
}
