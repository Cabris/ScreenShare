package com.stream.source;
import java.io.BufferedInputStream;
import java.io.IOException;
import java.io.InputStream;
import java.net.*;
import java.nio.ByteBuffer;
import java.util.Arrays;
import java.util.concurrent.ConcurrentLinkedQueue;
import com.comman.*;

public class StreamReceiver extends StreamSource {

	Socket clientSocket;
	String ipAddress;
	int port;
	InputStream inputStream;
	boolean isEof;
	ReceiveThread receiveThread;

	long total = 0;
	boolean firstPacket = false;
	ByteBuffer byteBuffer;
	protected int targetLength = 0;
	protected int currentLength = 0;

	BufferedInputStream inFromServer;

	public StreamReceiver(String ip, int port) {
		this.ipAddress = ip;
		this.port = port;
		queue = new ConcurrentLinkedQueue<byte[]>();
		receiveThread = new ReceiveThread();
	}

	public void Connect() {
		receiveThread.start();
	}

	public void onDestory() {
		receiveThread.interrupt();
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

	@Override
	public boolean isEOS() {
		return isEof;
	}
	
	ByteBuffer lengthBuffer=ByteBuffer.allocate(4);
	protected int updateLength() throws IOException, Exception {
		int bytesRead;
		byte[] lengthData = new byte[4];
		
		lengthBuffer.clear();
		int readed=0;
		while (true) {
			bytesRead = inputStream.read(lengthData);
			if(bytesRead>0){
				lengthBuffer.put(lengthData,0,bytesRead);
				readed+=bytesRead;
			}
			if(readed==lengthData.length)
				break;
		}
		lengthBuffer.flip();
		lengthBuffer.get(lengthData);
		targetLength = BitConverter.toInt32(lengthData, 0);
		currentLength = 0;
		return bytesRead;
	}
	
	private void getPacketFromBuffer() {
		byte[] packet = new byte[targetLength];
		byteBuffer.flip();
		byteBuffer.get(packet);
		gotPacket(packet);
		byteBuffer.clear();
		// Log.d("StreamReceiver", "byteBuffer.get: " + targetLength);
	}

//	protected void fillBuffer() throws IOException {
//		//while (currentLength < targetLength) {
//			int b = inFromServer.read();
//			if (b == -1) {
//				return;
//			} else {
//				currentLength += 1;
//				byteBuffer.put((byte) b);
//			}
//		//}
//	}
	int maxBufferSize=10000000;
	byte[] fillInBuffer= new byte[maxBufferSize];
	protected int fillBuffer() throws IOException {
		int bytesRead=0;
		//byte[] buffer = new byte[targetLength - currentLength];
		bytesRead = inputStream.read(fillInBuffer,0,targetLength - currentLength);
		if(bytesRead<0)
			return 0;
		//byte[] actual_read_buffer = new byte[bytesRead];
		//System.arraycopy(buffer, 0, actual_read_buffer, 0, bytesRead);
		currentLength += bytesRead;
		byteBuffer.put(fillInBuffer,0,bytesRead);
		return bytesRead;
		// Log.d("StreamReceiver", "fill buffer: " + bytesRead);
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
				throw new Exception("uselessCode: fstP=" + fstP + ", secP=" + secP);
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
		// Log.d("StreamReceiver", "packet: " + packet.length);
		// Log.d("StreamReceiver_queue_size", "queue size: " + queue.size());
	}

	private boolean isMatch(byte[] packet, byte[] uselessCode, int i) {
		return packet[i - 3] != 0x0 && packet[i - 2] == uselessCode[0] && packet[i - 1] == uselessCode[1]
				&& packet[i - 0] == uselessCode[2];
	}

	private class ReceiveThread extends Thread {

		public ReceiveThread() {
			byteBuffer = ByteBuffer.allocate(maxBufferSize);
			byteBuffer.clear();
		}

		@Override
		public void run() {
			super.run();
			try {
				clientSocket = new Socket(ipAddress, port);
				clientSocket.setTcpNoDelay(true);
				inputStream = clientSocket.getInputStream();
				inFromServer = new BufferedInputStream(inputStream);
			} catch (Exception e) {
				e.printStackTrace();
			}

			try {
				targetLength = 0;
				currentLength = 0;
				while (!Thread.interrupted()) {
					int bytesRead = 0;
					if (targetLength == currentLength) {// need to update the length
						if (targetLength > 0) {// get a packet
							getPacketFromBuffer();
						}
						bytesRead = updateLength();
						if (bytesRead <= 0) {// no input
							// Log.d("StreamReceiver", "interrupted, total: " + total);
							Thread.interrupted();
							isEof = true;
							break;
						}
					} else {// fill in the buffer
						fillBuffer();
					}
				}
			} catch (Exception e) {
				e.printStackTrace();
			}
		}
	}

}
