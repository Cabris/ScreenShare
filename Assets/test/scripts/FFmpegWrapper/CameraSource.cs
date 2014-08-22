using UnityEngine;
using System.Collections;
using System;

public class CameraSource : MonoBehaviour {
	[SerializeField]
	int fps;
	[SerializeField]
	int width,height;
	[SerializeField]
	bool fullScreen;
	[SerializeField]
	int stackSize;
	Texture2D sourceTexture;
	Rect size;
	public ConcurrentStack<UnityEngine.Color32[]>  BufferStack {get;private set;}

	public int Width {
		get {
			return width;
		}
		set {
			width = value;
			isValid ();
		}
	}

	public int Height {
		get {
			return height;
		}
		set {
			height = value;
			isValid ();
		}
	}

	public Texture2D SourceTexture {
		get {
			return sourceTexture;
		}
	}
	
	// Use this for initialization
	void Awake () {
		if(fullScreen){
			width=Screen.width;
			height=Screen.height;
		}
		isValid ();
		sourceTexture=new Texture2D(width,height,TextureFormat.RGB24,false);
		BufferStack =new ConcurrentStack<UnityEngine.Color32[]> ();
	}

	void Start(){
		float p=1f/(float)fps;
		InvokeRepeating("getFrame",0,p);
		size=new Rect(0,0,width,height);
	}

	void isValid ()
	{
		if (width > Screen.width || height > Screen.height) {
			string m = "resolution must be smaller than screen";
			Exception e = new Exception (m);
			Debug.LogException (e);
			throw e;
		}
		if (width % 2 != 0 || height % 2 != 0) {
			string m = "resolution must be a multiple of two";
			Exception e = new Exception (m);
			Debug.LogException (e);
			throw e;
		}
	}
	
	// Update is called once per frame
	void OnPostRender () {
		if(sourceTexture!=null)
			sourceTexture.ReadPixels(size,0,0);
	}

	//public static bool isOK{get;private set;}

	void Update(){
		//BufferQueue.Clear();
		//getFrame ();
		stackSize=BufferStack.Count;
	}

	void OnApplicationQuit() {
		CancelInvoke("getFrame");
	}

	void getFrame ()
	{
		//isOK = false;
		if(BufferStack.Count>2)
			BufferStack.Clear();
		UnityEngine.Color32[] colors = sourceTexture.GetPixels32 ();
		BufferStack.Push (colors);
		//isOK = true;
	}
}
