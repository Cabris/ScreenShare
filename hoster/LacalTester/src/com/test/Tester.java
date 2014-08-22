package com.test;

import java.io.InputStream;
import java.net.Socket;
import java.nio.ByteBuffer;
import java.util.logging.Logger;

import com.comman.BitConverter2;

public class Tester extends Thread {
	ByteBuffer lengthBuffer;
	Socket clientSocket;
	InputStream inputStream;
	int targetLength=0;
	int oldLength=0;
	byte[] lengthData = new byte[4];
	private static Logger log = Logger.getLogger(Tester.class.getName());
	
	public Tester() {
		super();
		try {
			clientSocket = new Socket("192.168.1.48", 8888);
			clientSocket.setTcpNoDelay(true);
			inputStream = clientSocket.getInputStream();
			lengthBuffer=ByteBuffer.allocate(4);
		} catch (Exception e) {
			e.printStackTrace();
		}
	}

	@Override
	public void run() {
		super.run();
		try {
			while (!Thread.interrupted()) {
				updateLength();
			}
		} catch (Exception e) {
			e.printStackTrace();
		}finally{
			onDestory();
		}
	}

	void onDestory(){
		this.interrupt();
		try {
			clientSocket.close();
		} catch (Exception e) {
			e.printStackTrace();
		}
	}
	
	protected void updateLength() throws Exception {
		lengthBuffer.clear();
		int haveRead = 0;
		while (true) {
			int bytesRead = inputStream.read(lengthData, 0, 4 - haveRead);
			if (bytesRead > 0) {
				lengthBuffer.put(lengthData, 0, bytesRead);
				haveRead += bytesRead;
				log.info("fill targetLength: " + haveRead + "/" + 4);
			}
			if (haveRead == lengthData.length)
				break;
		}
		lengthBuffer.flip();
		lengthBuffer.get(lengthData);
		targetLength = BitConverter2.toInt(lengthData);
		log.info("update targetLength: " + targetLength);
		
		if (targetLength!=oldLength)
			throw new Exception("targetLength!=oldLength");
		oldLength++;
	}

	public static void main(String[] args) {
		Tester test = new Tester();
		test.start();
	}

}
