using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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

    bool isCapture;
    [SerializeField]
    Camera[] cameras;

	// Use this for initialization
	void Awake () {
		if(fullScreen){
			width=Screen.width;
			height=Screen.height;
		}
		isValid ();
        isCapture=false;
		sourceTexture=new Texture2D(width,height,TextureFormat.RGB24,false);
		BufferStack =new ConcurrentStack<UnityEngine.Color32[]> ();

	}

    public virtual  void Start(){
		size=new Rect(0,0,width,height);
        cameras=GetComponentsInChildren<Camera>();
	}

	public void StartCapture(){
		float p=1f/(float)fps;
        isCapture=true;
		InvokeRepeating("getFrame",0,p);

	}

	public void StopCapture(){
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

    void FixedUpdate(){
        //StartCoroutine(fillTexture());
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
