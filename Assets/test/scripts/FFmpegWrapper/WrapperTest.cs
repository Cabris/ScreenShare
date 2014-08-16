using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;

public class WrapperTest : MonoBehaviour {

	[DllImport("FFmpegCppWrapper", CallingConvention = CallingConvention.Cdecl)]
	private static extern void startEncoder(int srcW, int srcH, int decW, int decH, int bitrate,int framerate);
	
	[DllImport("FFmpegCppWrapper", CallingConvention = CallingConvention.Cdecl)]
	private static extern int encode(IntPtr resP, int res_size, IntPtr decP, out int dec_size);
	
	[DllImport("FFmpegCppWrapper", CallingConvention = CallingConvention.Cdecl)]
	private static extern void stopEncoder();


	// Use this for initialization
	void Start () {
		int srcW = 1280, srcH = 720;
		int decW = 1280, decH = 720;
		int fps = 25;
		int length = 10;
		int fc = length * fps;
		string path = @"MyTest_h264.mp4";
		FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write);
		
		startEncoder(srcW, srcH, decW, decH, 800000, fps);
		
		byte[] src = new byte[srcW * srcH*3];
		int res_size = src.Length;
		createDummyFrame(srcW, srcH, src, fc, 0);
		IntPtr resP = Marshal.AllocHGlobal(res_size);
		Marshal.Copy(src, 0, resP, res_size);
		
		byte[] dec = new byte[decW * decH*3/2];
		int dec_size = dec.Length;
		IntPtr decP = Marshal.AllocHGlobal(dec_size);
		Marshal.Copy(dec, 0, decP, dec_size);
		
		Stopwatch stopWatch = new Stopwatch();
		stopWatch.Start();
		for (int c = 0; c < fc; c++)
		{
			createDummyFrame(srcW, srcH, src, fc, c);
			Marshal.Copy(src, 0, resP, src.Length);
			if (encode(resP, res_size, decP, out dec_size) > 0)
			{
				Marshal.Copy(decP, dec, 0, dec_size);
				String s=String.Format("Write frame {0,3} (size={1,7})**" , c, dec_size);
				UnityEngine.Debug.Log(s);
				fs.Write(dec, 0, dec_size);
			}
			else
			{
				UnityEngine.Debug.Log("encode fial");
			}
		}
		stopWatch.Stop();
		TimeSpan ts = stopWatch.Elapsed;

		stopEncoder();
		// Format and display the TimeSpan value.
		string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
		                                   ts.Hours, ts.Minutes, ts.Seconds,
		                                   ts.Milliseconds / 10);
		UnityEngine.Debug.Log("RunTime " + elapsedTime);
		UnityEngine.Debug.Log("press any key to exit");
		
		byte[] endcode = new byte[]{ 0, 0, 1, 0xb7 };
		fs.Write(endcode, 0, endcode.Length);
		fs.Close();
		Marshal.FreeHGlobal(resP);
		Marshal.FreeHGlobal(decP);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	private void createDummyFrame(int srcW, int srcH, byte[] src, int fc, int c)
	{
		int index = 0;
		for (int y = 0; y < srcH; y++)
		{
			for (int x = 0; x < srcW; x++)
			{
				src[index++] = (byte)(x + c * 255 / fc); // B
				src[index++] = (byte)(128 + y - c * 255 / fc); // G
				src[index++] = (byte)(64 - x + c * 255 / fc); // R
			}
		}
	}


}
