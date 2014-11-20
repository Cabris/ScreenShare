using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;


public class TextureSource : MonoBehaviour {

	[DllImport ("RenderingPlugin")]
	private static extern void SetTextureFromUnity(IntPtr texture);
	[DllImport("RenderingPlugin", CallingConvention = CallingConvention.Cdecl)]
	private static extern void StartCapt();
	[DllImport("RenderingPlugin", CallingConvention = CallingConvention.Cdecl)]
	private static extern void StopCapt();
	[DllImport("RenderingPlugin", CallingConvention = CallingConvention.Cdecl)]
	private static extern void getTexture(IntPtr dataP, out int size);
	[SerializeField]
	Camera listeningCamera;
	bool isCapt=false;
	public System.IntPtr srcP{get; private set;}
	[SerializeField]
	Renderer r;
	[SerializeField]
	RenderTexture tex;
	System.Object obj;
	// Use this for initialization
	void Start () {
		obj=this;
		CreateTextureAndPassToPlugin();
	}

	private void CreateTextureAndPassToPlugin()
	{
		Width=tex.width;
		Height=tex.height;
		//tex=new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32);
		listeningCamera.targetTexture=tex as RenderTexture;
		SetTextureFromUnity (tex.GetNativeTexturePtr());
		r.material.mainTexture=tex;
		int src_size = Width * Height*3;//bgr
		srcP = Marshal.AllocHGlobal(src_size);
	}

	public void StartCapture (){
		lock(obj){
		isCapt=true;
		StartCapt();
			StartCoroutine("CallPluginAtEndOfFrames");
		}
	}
	
	public void StopCapture (){
		lock(obj){
		isCapt=false;
		StopCapt();
		StopCoroutine("CallPluginAtEndOfFrames");
		Marshal.FreeHGlobal(srcP);
		}
	}

	public void GetTexture(){
		int size=0;
		if(isCapt){
			lock(obj){
			getTexture(srcP,out size);
			}
		}
	}

	private IEnumerator CallPluginAtEndOfFrames()
	{
		while (true) {
			if(isCapt){
				yield return new WaitForEndOfFrame();
				GL.IssuePluginEvent (1);
			}	
		}
	}

	public int Width {
		get; protected set;
	}
	
	public int Height {
		get;protected set;
	}
	
	void OnApplicationQuit() {
		if(isCapt){
			StopCapture();
		}
	}
}
