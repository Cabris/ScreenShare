package com.stream.source;

public class FileReceiver extends StreamSource {
	private String SAMPLE = "storage/sdcard1/" + "v2.mp4";
	ReadThread thread;
	public FileReceiver(String sample) {
		SAMPLE=sample;
	}
	
	public void start() {
		thread=new ReadThread();
		thread.start();
	}
	
	class ReadThread extends Thread{
		
		@Override
		public void run() {
			super.run();
			
		}
	}
	
}
