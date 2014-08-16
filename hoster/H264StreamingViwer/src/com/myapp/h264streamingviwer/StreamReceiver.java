package com.myapp.h264streamingviwer;
import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.net.*;
import java.nio.ByteBuffer;

import android.R.integer;
import android.R.string;
import android.util.Log;
import com.comman.*;

public class StreamReceiver {
	
	Socket clientSocket;
	String ipAddress;
	int port;
	BufferedReader inFromServer;
	InputStream inputStream;
	SubThread subThread;
	ReceiveThread receiveThread;
	
	public StreamReceiver(String ip,int port) {
		this.ipAddress=ip;
		this.port=port;
		subThread=new SubThread();
	}
	
	public void Connect() {
		subThread.start();
	}
	
	public void onDestory() {
		receiveThread.interrupt();
	}
	
	private class SubThread extends Thread{
		@Override
		public void run() {
			super.run();
			try {
				clientSocket = new Socket(ipAddress, port);
				inputStream=clientSocket.getInputStream();
				inFromServer = new BufferedReader(new InputStreamReader(inputStream)); 
				StartReceive();
				} 
			catch (Exception e) {
				e.printStackTrace();
			}
		}
		private void StartReceive() {
			receiveThread=new ReceiveThread();
			receiveThread.start();
		}
	}
	
	private class ReceiveThread extends Thread {
		ByteBuffer byteBuffer;
		public ReceiveThread() {
			byteBuffer=ByteBuffer.allocate(800000);
			byteBuffer.clear();
		}
		int targetLength=0;
		int currentLength=0;
		long total=0;
		@Override
		public void run() {
			super.run();
			while (!Thread.interrupted()) {
				
				try {
					int bytesRead=0;
					//bytesRead=inputStream.read(buffer);
					if(targetLength==currentLength){//need to update the length
						if(targetLength>0){//get a packet
							byte[] packet=new byte[targetLength];
							Log.d("DecodeActivity receive", "byteBuffer.get: "+targetLength);
							byteBuffer.get(packet);
							gotPacket(packet);
							byteBuffer.clear();
						}
						byte[] lengthData=new byte[4];
						bytesRead=inputStream.read(lengthData);
						if(bytesRead<=0){//no input
							Log.d("DecodeActivity receive", "interrupted, total: "+total);
							Thread.interrupted();
							break;
						}
						targetLength=BitConverter.toInt32(lengthData, 0);
						Log.d("DecodeActivity receive", "targetLength updated: "+targetLength);
						currentLength=0;
					}else {//fill in the buffer
						byte[] buffer=new byte[targetLength-currentLength];
						bytesRead=inputStream.read(buffer);
						Log.d("DecodeActivity receive", "targetLength: "+targetLength);
						Log.d("DecodeActivity receive", "currentLength: "+currentLength);
						Log.d("DecodeActivity receive", "fill buffer: "+bytesRead);
						currentLength+=bytesRead;
						byteBuffer.put(buffer);
					}
					
				} catch (Exception e) {
					e.printStackTrace();
				}
			}
		}
		
		private void gotPacket(byte[] packet) {
			total+=packet.length;
			
			Log.d("DecodeActivity receive", "packet: "+packet.length);
		}
	}
	
	public String getIpAddress() {
		return ipAddress;
	}

	public void setIpAddress(String ipAddress) {
		this.ipAddress = ipAddress;
	}

	public int getPort() {
		return port;
	}

	public void setPort(int port) {
		this.port = port;
	}

}
