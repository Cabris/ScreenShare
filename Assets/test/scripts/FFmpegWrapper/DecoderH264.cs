using System.Collections;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System;
using System.Diagnostics;
using System.Threading;

public class DecoderH264
{
	[DllImport("FFmpegCppWrapper", CallingConvention = CallingConvention.Cdecl)]
	private static extern void startDecoder(int width, int height);
	
	[DllImport("FFmpegCppWrapper", CallingConvention = CallingConvention.Cdecl)]
	private static extern int decode(IntPtr resP, int res_size, IntPtr decP, out int dec_size);
	
	[DllImport("FFmpegCppWrapper", CallingConvention = CallingConvention.Cdecl)]
	private static extern void stopDecoder();

	int width, height;
	byte[] src,dec;
	int src_size,dec_size,c;
	IntPtr srcP,decP;
	UnityEngine.Color32[] colors;
	Stack input;
	UnityEngine.Texture2D target;

	public DecoderH264 (Stack  buffer,UnityEngine.Texture2D targetTexture)
	{
		input=buffer;
		target=targetTexture;
	}

	public void Prepare(){
		src = new byte[width * height*3];
		src_size = src.Length;
		srcP = Marshal.AllocHGlobal(src_size);
		Marshal.Copy(src, 0, srcP, src_size);
		
		dec = new byte[width * height*3];
		dec_size = dec.Length;
		decP = Marshal.AllocHGlobal(dec_size);
		Marshal.Copy(dec, 0, decP, dec_size);
		colors=new UnityEngine.Color32[dec_size];
		target.Resize(width,height);
		target.Apply();
	}

	public void StartDecoder(){
		startDecoder(Width,Height);
	}

	public void Decode(){
		src=input.Pop() as byte[];
		Marshal.Copy(src, 0, srcP, src.Length);
		decode(srcP,src_size,decP,out dec_size);
		Marshal.Copy(decP, dec, 0, dec_size);
		int index = 0;
		int pixelIndex=0;
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				UnityEngine.Color32 color=colors[pixelIndex++];
				color.b=dec[index++]; // B
				color.g=dec[index++] ; // G
				color.r=dec[index++]; // R
			}
		}
		target.SetPixels32(colors);
		target.Apply();
	}

	public void StopDecoder(){
		stopDecoder();
	}

	public int Width {
		get {
			return width;
		}
		set {
			width = value;
		}
	}
	
	public int Height {
		get {
			return height;
		}
		set {
			height = value;
		}
	}
}


