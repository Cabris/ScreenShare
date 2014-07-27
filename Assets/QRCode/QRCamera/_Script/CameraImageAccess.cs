using UnityEngine;
using System.Collections;

using com.google.zxing.qrcode;

public class CameraImageAccess : MonoBehaviour, ITrackerEventHandler {
	
	private bool isFrameFormatSet;
	private Image cameraFeed;
	private string tempText;
	private string qrText;
	public Renderer r;
	public Texture2D tex;
	Vector2 textureScale=new Vector2(1,1);
	Vector2 textureOffset=new Vector2(0,0);

	public delegate void OnQRcodeReadHandler(string msg);
	public OnQRcodeReadHandler OnQRcodeRead;

	void Start () {
		QCARBehaviour qcarBehaviour = GetComponent<QCARBehaviour>();
		
		if (qcarBehaviour) {
			qcarBehaviour.RegisterTrackerEventHandler(this);
		}
		
		isFrameFormatSet = CameraDevice.Instance.SetFrameFormat(Image.PIXEL_FORMAT.GRAYSCALE, true);
		InvokeRepeating("Autofocus", 1f, 2f);
		
		tex = new Texture2D (2048, 1024);
		tex.wrapMode=TextureWrapMode.Repeat;
		tex.Apply();
		r.material.mainTexture = tex;
		QCARRenderer.Vec2I s=new QCARRenderer.Vec2I();
		//1080 720
		s.x=540;
		s.y=360;
		QCARRenderer.VideoBGCfgData cof=QCARRenderer.Instance.GetVideoBackgroundConfig();
		cof.size=s;
		QCARRenderer.Instance.SetVideoBackgroundConfig(cof);

		QCARRenderer.Instance.DrawVideoBackground = false;
		QCARRenderer.Instance.SetVideoBackgroundTexture (tex);

	}
	
	void Autofocus () {
		CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_TRIGGERAUTO);
	}
	
	void Update () {
		if (Input.GetKeyDown(KeyCode.Escape)) {
			Application.Quit();
		}
		if (QCARRenderer.Instance.IsVideoBackgroundInfoAvailable())
		{
			QCARRenderer.VideoTextureInfo texInfo = QCARRenderer.Instance.GetVideoTextureInfo ();
			
			float sx=(float)texInfo.imageSize.x/(float)texInfo.textureSize.x;
			float sy=(float)texInfo.imageSize.y/(float)texInfo.textureSize.y;

			textureScale=new Vector2(-sx,sy);
			textureOffset.x=sx;
			
			r.material.mainTextureScale=textureScale;
			r.material.mainTextureOffset=textureOffset;//0.63,0.038
			//textureOffset.y=0;
		}
	}
	
	void OnGUI () {
		GUIStyle myButtonStyle = new GUIStyle(GUI.skin.button);
		myButtonStyle.fontSize = 50;

		//QCARRenderer.VideoTextureInfo texInfo = QCARRenderer.Instance.GetVideoTextureInfo ();
		
		GUI.Box(new Rect(0, Screen.height - 300, Screen.width, 80), qrText,myButtonStyle);
//		GUI.Box(new Rect(0, Screen.height - 200, Screen.width, 50), 
//		        texInfo.textureSize.x+","+texInfo.textureSize.y,myButtonStyle);//2048,1024
//		GUI.Box(new Rect(0, Screen.height - 150, Screen.width, 50), 
//		        texInfo.imageSize.x+","+texInfo.imageSize.y,myButtonStyle);//1280,720
		
	}
	
	public void OnTrackablesUpdated () {
		try {
			if(!isFrameFormatSet) {
				isFrameFormatSet = CameraDevice.Instance.SetFrameFormat(Image.PIXEL_FORMAT.GRAYSCALE, true);
			}
			
			cameraFeed = CameraDevice.Instance.GetCameraImage(Image.PIXEL_FORMAT.GRAYSCALE);
			tempText = new QRCodeReader().decode(cameraFeed.Pixels, cameraFeed.BufferWidth, cameraFeed.BufferHeight).Text;
			
		}
		catch {
			// Fail detecting QR Code!
		}
		finally {
			if(!string.IsNullOrEmpty(tempText)) {
				qrText = tempText;
				if(OnQRcodeRead!=null)
					OnQRcodeRead(qrText);
			}
		}
	}
	
	public void OnInitialized(){}
	bool isPause=false;
	public void Pause(bool p){
		QCARRenderer.Instance.Pause(p);
	}
}
