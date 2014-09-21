package com.stream.source;

import java.util.logging.Logger;

public class StreamingTest {

	StreamReceiver receiver;
	private static Logger log = Logger.getLogger(StreamingTest.class.getName());

	public static void main(String[] args) {
		StreamingTest test = new StreamingTest();
		test.start();
	}

	public void start() {
		String adrs = "127.0.0.1";
		int port = 8888;
		receiver = new StreamReceiver(adrs, port);
		receiver.Connect();
		TestThread testThread = new TestThread();
		testThread.start();
	}

	class TestThread extends Thread {
		@Override
		public void run() {
			super.run();
			while (true) {
				if (receiver != null) {
					byte[] data = receiver.getQueue().poll();
					if(data!=null)
					log.info("data size: " + data.length);
				}
			}
		}
	}

}
