package com.stream.source;

import java.io.BufferedInputStream;
import java.io.InputStream;
import java.net.*;
import java.nio.ByteBuffer;
import java.util.Arrays;
import java.util.concurrent.ConcurrentLinkedQueue;
import java.util.logging.Logger;
import com.comman.*;

public class StreamReceiver extends StreamSource {

	Socket clientSocket;
	String ipAddress;
	int port;
	InputStream inputStream;
	BufferedInputStream bStream;
	boolean isEof;
	ReceiveThread receiveThread;

	long total = 0;
	boolean firstPacket = false;
	ByteBuffer frameBuffer;
	ByteBuffer lengthBuffer;
	protected int targetLength = 0;
	protected int currentLength = 0;
	int maxBufferSize = 10000000;

	private static Logger log = Logger
			.getLogger(StreamReceiver.class.getName());

	public StreamReceiver(String ip, int port) {
		this.ipAddress = ip;
		this.port = port;
		queue = new ConcurrentLinkedQueue<byte[]>();
		receiveThread = new ReceiveThread();
	}

	public void Connect() {
		receiveThread.start();
	}

	public void onStop() {
		receiveThread.interrupt();
	}

	@Override
	public boolean isEOS() {
		return isEof;
	}

	byte[] lengthData = new byte[4];

	protected void updateLength() throws Exception {
		int tryCount = 0;
		lengthBuffer.clear();
		int haveRead = 0;
		while (true) {
			int bytesRead = bStream.read(lengthData, 0, 4 - haveRead);
			if (bytesRead > 0) {
				lengthBuffer.put(lengthData, 0, bytesRead);
				haveRead += bytesRead;
				log.info("fill targetLength: " + haveRead + "/" + 4 + ",try: "
						+ tryCount);
			}
			if (haveRead == lengthData.length)
				break;
			tryCount++;
		}
		lengthBuffer.flip();
		lengthBuffer.get(lengthData);
		targetLength = BitConverter2.toInt(lengthData);
		log.info("update targetLength: " + targetLength);
		if (targetLength < 0)
			throw new Exception("negi length");
	}

	byte[] fillInBuffer = new byte[maxBufferSize];

	protected void fillBuffer() throws Exception {
		int tryCount = 0;
		currentLength = 0;
		frameBuffer.clear();
		while (true) {
			int bytesRead = bStream.read(fillInBuffer, 0, targetLength
					- currentLength);
			if (bytesRead > 0) {
				frameBuffer.put(fillInBuffer, 0, bytesRead);
				currentLength += bytesRead;
				log.info("fill:" + currentLength + "/" + targetLength
						+ ",try: " + tryCount);
			}
			if (currentLength == targetLength)
				break;
			tryCount++;
		}
		log.info("fill finished:" + currentLength + "/" + targetLength);
		byte[] data = new byte[targetLength];
		frameBuffer.flip();
		frameBuffer.get(data);
		gotPacket(data);
	}

	protected void gotPacket(byte[] packet) {
		if (!firstPacket) {
			extractSPS_PPS(packet);
			firstPacket = true;
		} else {
			total += packet.length;
			enqueue(packet);
		}
	}

	private void extractSPS_PPS(byte[] packet) {
		byte[] SPS_PPS, firstFrame;
		byte[] preCode = new byte[] { 0x0, 0x0, 0x0, 0x1 };
		byte[] uselessCode = new byte[] { 0x0, 0x0, 0x1 };
		int fstP = -1, secP = -1;
		for (int i = 3; i < packet.length; i++) {
			if (isMatch(packet, uselessCode, i)) {
				if (fstP < 0)
					fstP = i - 2;
				if (secP < 0)
					secP = i;
				if (fstP > 0 && secP > 0)
					break;
			}
		}
		try {
			if (!(fstP > 0 && secP > 0))
				throw new Exception("uselessCode: fstP=" + fstP + ", secP="
						+ secP);
		} catch (Exception e) {
			e.printStackTrace();
		}
		SPS_PPS = Arrays.copyOfRange(packet, 0, fstP);
		byte[] temp = Arrays.copyOfRange(packet, secP + 1, packet.length);
		firstFrame = new byte[preCode.length + temp.length];
		System.arraycopy(preCode, 0, firstFrame, 0, preCode.length);
		System.arraycopy(temp, 0, firstFrame, preCode.length, temp.length);
		enqueue(SPS_PPS);
		// Log.d("StreamReceiver", "SPS_PPS: " + SPS_PPS.length);
		enqueue(firstFrame);
		// Log.d("StreamReceiver", "firstFrame" + firstFrame.length);
	}

	private void enqueue(byte[] packet) {
		queue.add(packet);
	}

	private boolean isMatch(byte[] packet, byte[] uselessCode, int i) {
		boolean isMatch = true;
		for (int j = 0; j < uselessCode.length; j++) {// 0-2
			isMatch = isMatch
					&& packet[i - j] == uselessCode[uselessCode.length - 1 - j];
		}
		isMatch = isMatch && packet[i - 3] != 0x0;

		return isMatch;
	}

	private class ReceiveThread extends Thread {

		public ReceiveThread() {
			super();
			frameBuffer = ByteBuffer.allocate(maxBufferSize);
			frameBuffer.clear();
			lengthBuffer = ByteBuffer.allocate(4);
			lengthBuffer.clear();
			targetLength = 0;
			currentLength = 0;
		}

		@Override
		public void run() {
			super.run();
			try {
				clientSocket = new Socket(ipAddress, port);
				// clientSocket.setTcpNoDelay(true);
				clientSocket.setReceiveBufferSize(600000);
				inputStream = clientSocket.getInputStream();
				bStream = new BufferedInputStream(inputStream);
				while (!Thread.interrupted()) {
					updateLength();
					fillBuffer();
				}
				inputStream.close();
				bStream.close();
				clientSocket.close();
			} catch (Exception e) {
				e.printStackTrace();
			} 
		}
	}

}
