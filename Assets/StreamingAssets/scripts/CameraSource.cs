using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class CameraSource : ImageSource{

	[SerializeField]
	protected int fps;
	[SerializeField]
	protected int width,height;
	[SerializeField]
	bool fullScreen;
	[SerializeField]
	int stackSize;
	Texture2D sourceTexture;
	Rect size;
	protected bool isCapture;

	// Use this for initialization
	void Awake () {
		Application.targetFrameRate = -1;
		BufferStack =new ConcurrentStack<UnityEngine.Color32[]> ();
		OnSize ();
	}

	public void OnSize ()
	{
		Rect cr = camera.rect;
		size = new Rect (cr.x * Screen.width*0, cr.y * Screen.height*0, cr.width * Screen.width, cr.height * Screen.height);
		if (fullScreen) {
			width = (int)size.width;
			height = (int)size.height;
		}
	}

    void Start(){
		isValid ();
		isCapture=false;
		sourceTexture=new Texture2D(width,height,TextureFormat.RGB24,false);
	}

	public override void StartCapture(){
		float p=1f/(float)fps;
        isCapture=true;
		InvokeRepeating("getFrame",0,p);
	}

	public override void StopCapture(){
        isCapture=false;
		CancelInvoke("getFrame");
	}
	
	// Update is called once per frame
	void OnPostRender () {
        if(sourceTexture!=null&&isCapture){
           fillTexture();
        }
	}

    void fillTexture(){
        sourceTexture.ReadPixels(size,0,0);
    }

	void Update(){
		stackSize=BufferStack.Count;
	}

	void OnApplicationQuit() {
		CancelInvoke("getFrame");
	}

	void getFrame ()
	{
		if(BufferStack.Count>2)
			BufferStack.Clear();
		UnityEngine.Color32[] colors = sourceTexture.GetPixels32 ();

		BufferStack.Push (colors);
	}

	public override int Width {
        get {
			if(fullScreen)
				width=(int)size.width;
            return width;
        }
        protected set {
            width = value;
            isValid ();
        }
    }
    
	public override int Height {
        get {
			if(fullScreen)
				height=(int)size.height;
            return height;
        }
		protected set {
			height = value;
            isValid ();
        }
    }
   
    void isValid ()
    {
        if (width > Screen.width || height > Screen.height) {
            string m = "resolution must be smaller than screen, width: "+width+",height: "+height;
            Exception e = new Exception (m);
            Debug.LogException (e);
            throw e;
        }
        if (width % 2 != 0 || height % 2 != 0) {
            string m = "resolution must be a multiple of two, width: "+width+",height: "+height;
            Exception e = new Exception (m);
            Debug.LogException (e);
			width=floorEven(width);
			height=floorEven(height);
            //throw e;
        }
    }

	int floorEven(int value){
		if(value%2==0)
			return value;
		else
			return value-1;
	}
}
