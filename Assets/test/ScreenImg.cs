using UnityEngine;
using System.Collections;

public class ScreenImg : MonoBehaviour {
	public Texture2D tex ;
	[SerializeField]
	float p;
	float c=0;
	public Renderer r;
	int w=128,h=128;
	// Use this for initialization
	void Start () {
		tex =new Texture2D(w,h);
		if(r!=null)
			r.material.mainTexture=tex;
	}

	void Update ()
	{      
	}

	void OnPostRender () {
		if(c>=p){
			//Texture2D tex = new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);
			//tex.ReadPixels(new Rect(0,0,w,h),0,0,false);
			//tex.Apply();
			//tex.Compress(false);
			c=0;
		}else
			c+=Time.deltaTime;
	}
}
