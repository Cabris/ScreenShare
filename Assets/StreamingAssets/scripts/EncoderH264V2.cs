using System.Collections;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System;
using System.Diagnostics;
//using System.Threading;

public class EncoderH264V2
{
	[DllImport("FFmpegCppWrapper", CallingConvention = CallingConvention.Cdecl)]
	private static extern void startEncoder(int srcW, int srcH, int decW, int decH, int bitrate,int framerate);
	
	[DllImport("FFmpegCppWrapper", CallingConvention = CallingConvention.Cdecl)]
	private static extern int encode(IntPtr resP, int res_size, IntPtr decP, out int dec_size);
	
	[DllImport("FFmpegCppWrapper", CallingConvention = CallingConvention.Cdecl)]
	private static extern void stopEncoder();
	
	int outWidth, outHeight,fps; 
	int srcW , srcH;
	int bitRate=800000;
	
	byte[] src,dec;
	int src_size,dec_size;
	IntPtr srcP,decP;
	
	//ConcurrentStack<UnityEngine.Color32[]>  sourceBuffer;
	Stopwatch stopWatch;
	bool isStarted=false;
	bool isStoped=false;
	FileStream fs;
	Object obj;
	UnityEngine.Color32[] colors;
	bool debug=false;
	
	public EncoderH264V2 (IntPtr srcP,int srcW,int srcH,bool isDebug)
	{
		this.srcP=srcP;
		this.src_size=srcW*srcH*3;
		this.debug=isDebug;
		this.srcW=srcW;
		this.srcH=srcH;
		this.obj=this;
	}


	public void Prepare(){
		colors=new UnityEngine.Color32[srcW*srcH];
		for(int i=0;i<srcW*srcH;i++)
			colors[i]=UnityEngine.Color.green;
		if(debug){
			string path = @"MyTest_h264.mp4";
			fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write);
		}

		dec = new byte[outWidth * outHeight*3/2];
		dec_size = dec.Length;
		decP = Marshal.AllocHGlobal(dec_size);
		Marshal.Copy(dec, 0, decP, dec_size);
		stopWatch = new Stopwatch();
	}
	
	public void StartEncoder(){
		lock(obj){
			try{
				startEncoder(srcW, srcH, outWidth, outHeight, bitRate, fps);
				stopWatch.Start();
				isStarted=true;
			}catch(Exception e){
				UnityEngine.Debug.LogException(e);
			}
		}
	}
	
	public byte[] Encoding(){
		lock(obj){
			byte[] encoded=new byte[0];
			if(!isStarted||isStoped)
				throw new Exception("wrong state");
			try{
				if (encode(srcP, src_size, decP, out dec_size) > 0)
				{
					Marshal.Copy(decP, dec, 0, dec_size);
					encoded=new byte[dec_size];
					Buffer.BlockCopy(dec,0,encoded,0,dec_size);
					if(debug)
						fs.Write(dec, 0, dec_size);
				}
				else
				{
					UnityEngine.Debug.Log("encode fial");
				}
			}catch(Exception e){
				UnityEngine.Debug.LogException(e);
			}
			return encoded;
		}
	}
	
	public void StopEncoder(){
		lock(obj){
			if(isStoped||!isStarted)
				return;
			isStoped=true;
			//timer.Change(Timeout.Infinite,Timeout.Infinite);
			if(debug){
				byte[] endcode = new byte[]{ 0, 0, 1, 0xb7 };
				fs.Write(endcode, 0, endcode.Length);
				fs.Close();
			}

			Marshal.FreeHGlobal(decP);
			
			stopWatch.Stop();
			TimeSpan ts = stopWatch.Elapsed;
			// Format and display the TimeSpan value.
			string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
			                                   ts.Hours, ts.Minutes, ts.Seconds,
			                                   ts.Milliseconds / 10);
			UnityEngine.Debug.Log("RunTime " + elapsedTime);
			stopEncoder();
		}
	}
	
//	private void createFrameFromSource(byte[] src)
//	{
//		if(sourceBuffer.Count>0){
//			colors=sourceBuffer.Pop();
//			sourceBuffer.Clear();
//		}
//		
//		int index = 0;
//		int pixelIndex=0;
//		for (int y = 0; y < srcH; y++)
//		{
//			for (int x = 0; x < srcW; x++)
//			{
//				UnityEngine.Color32 color=colors[pixelIndex];//////
//				src[index++] = color.b; // B
//				src[index++] = color.g; // G
//				src[index++] = color.r; // R
//				pixelIndex++;
//			}
//		}
//		
//	}
	
	public int OutWidth {
		get {
			return outWidth;
		}
		set {
			outWidth = value;
		}
	}
	
	public int OutHeight {
		get {
			return outHeight;
		}
		set {
			outHeight = value;
		}
	}
	
	public int Fps {
		get {
			return fps;
		}
		set {
			fps = value;
		}
	}
	
	public int BitRate {
		get {
			return bitRate;
		}
		set {
			if(value>0)
				bitRate = value;
		}
	}
}


