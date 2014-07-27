using UnityEngine;
using System.Collections;
using com.google.zxing;
using com.google.zxing.common;
using com.google.zxing.qrcode;
public class QRcodeCreater : MonoBehaviour {
public	Renderer r;
	// Use this for initialization
	void Start () {

	}

	public void CreateCode(string code){          
		int QRwidth = 256; // 圖形寬
		int QRheight = 256; //圖形高
		QRCodeWriter writer=new QRCodeWriter();
		ByteMatrix bm = writer.encode(code,BarcodeFormat.QR_CODE, QRwidth, QRheight);
		Texture2D t2d=new Texture2D(QRwidth,QRheight);
		r.material.mainTexture=t2d;
		
		int width = bm.Width;
		int height = bm.Height;
		
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				//bm.get_Renamed(x, y) != -1 ? ColorTranslator.FromHtml("Red") : ColorTranslator.FromHtml("Yellow");
				if(bm.get_Renamed(x, y) != -1){
					t2d.SetPixel(x, y,Color.black);
				}else{
					t2d.SetPixel(x, y,Color.white);
				}
			}
		}
		t2d.Apply();
	}

	// Update is called once per frame
	void Update () {
	
	}
}
