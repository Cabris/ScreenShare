// on OpenGL ES there is no way to query texture extents from native texture id
#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
	#define UNITY_GLES_RENDERER
#endif


using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System;

public class UseRenderingPlugin : MonoBehaviour
{
	// Native plugin rendering events are only called if a plugin is used
	// by some script. This means we have to DllImport at least
	// one function in some active script.
	// For this example, we'll call into plugin's SetTimeFromUnity
	// function and pass the current time so the plugin can animate.

#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
	[DllImport ("RenderingPlugin")]
#endif
	private static extern void SetTimeFromUnity(float t);


	// We'll also pass native pointer to a texture in Unity.
	// The plugin will fill texture data from native code.
#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
	[DllImport ("RenderingPlugin")]
#endif
#if UNITY_GLES_RENDERER
	private static extern void SetTextureFromUnity(System.IntPtr texture, int w, int h);
#else
	private static extern void SetTextureFromUnity(System.IntPtr texture);
#endif

//	[DllImport("RenderingPlugin")]
//	private static extern void SetCallback(Callback  c);
//	private delegate int Callback(IntPtr  data,int size);
//	private Callback mInstance;   // Ensure it doesn't get garbage collected

	[DllImport("RenderingPlugin", CallingConvention = CallingConvention.Cdecl)]
	private static extern void StartCapt();
	[DllImport("RenderingPlugin", CallingConvention = CallingConvention.Cdecl)]
	private static extern void StopCapt();
	[DllImport("RenderingPlugin", CallingConvention = CallingConvention.Cdecl)]
	private static extern void getTexture(IntPtr dataP, out int size);


	byte[] data;
	Color32[] colors;
	[SerializeField]
	Renderer r;
	Texture2D tex;
	bool isLoop=true;
	IntPtr p;
	IEnumerator Start () {
		//TestCallback();
		CreateTextureAndPassToPlugin();
		yield return StartCoroutine(CallPluginAtEndOfFrames());
	}

	bool isCapt=false;

	void Update() {

		if(Input.GetKeyDown(KeyCode.C)){
			isCapt=!isCapt;
			if(isCapt)
				StartCapt();
			else{
				StopCapt();
				Marshal.FreeHGlobal(p);
			}
			Debug.Log("isCapt: "+isCapt);
		}
		if(isCapt)
			Handler();

	}

	private int Handler() {
		int size=0;
		getTexture(p,out size);
		Marshal.Copy(p, data, 0, size);
		int index=0;
		for(int i=0;i<colors.Length;i++){//r g b a
			colors[i].r=data[index++];
			colors[i].g=data[index++];
			colors[i].b=data[index++];
			colors[i].a=data[index++];
		}
		tex.SetPixels32(colors);
		tex.Apply();
//		Debug.Log(size);
		return 42;
	}
	
	private void CreateTextureAndPassToPlugin()
	{
		Texture t=GetComponent<Renderer>().material.mainTexture;
		data=new byte[t.width*t.height*4];
		p=Marshal.AllocHGlobal(t.width*t.height*4);
		colors=new Color32[t.width*t.height];
		for(int i=0;i<colors.Length;i++){
			colors[i]=new Color32();
		}

		tex = new Texture2D(t.width,t.height,TextureFormat.ARGB32,false);
		tex.filterMode = FilterMode.Point;
		tex.Apply();
		r.material.mainTexture=tex;
		SetTextureFromUnity (t.GetNativeTexturePtr());
//		mInstance = new Callback(Handler);
//		SetCallback(mInstance);
		StartCapt();
	}
	void OnApplicationQuit() {
		if(isCapt){
			StopCapt();
			Marshal.FreeHGlobal(p);
		}
	}
	private IEnumerator CallPluginAtEndOfFrames()
	{
		while (isLoop) {
			// Wait until all frame rendering is done
			yield return new WaitForEndOfFrame();
			//SetTimeFromUnity (Time.timeSinceLevelLoad);
			GL.IssuePluginEvent (1);
		}
	}
}
