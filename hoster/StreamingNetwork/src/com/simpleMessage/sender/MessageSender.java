package com.simpleMessage.sender;

import java.io.OutputStreamWriter;
import java.net.InetSocketAddress;
import java.net.Socket;
import java.util.concurrent.ConcurrentLinkedQueue;
import java.util.logging.Logger;

public class MessageSender {
	private static Logger log = Logger.getLogger(MessageSender.class.getName());
	
	private String address = "127.0.0.1";// 連線的ip
    private int port = 8765;// 連線的port
	Socket client;
	SendThread sendThread;
	OutputStreamWriter outputWriter;
	boolean isSending=true;
	
	protected ConcurrentLinkedQueue<String> queue;
	
	public ConcurrentLinkedQueue<String> getQueue() {
		return queue;
	}

	public MessageSender(String address,int port){
		this.address=address;
		this.port=port;
		queue=new ConcurrentLinkedQueue<String>();
		client=new Socket();
		
	}
	
	public void connect() {
		sendThread=new SendThread();
		sendThread.start();
	}
	
	public void send(String data) {
		
	}
	
	public void onStop() {
		isSending=false;
	}
	
	public class SendThread extends Thread{
		
		public SendThread(){}
		
		@Override
		public void run() {
			super.run();
			InetSocketAddress isa = new InetSocketAddress(address, port);
	        try {
	        	client.setTcpNoDelay(true);
	            client.connect(isa);
	            client.setSendBufferSize(60000);
	            outputWriter=new OutputStreamWriter(client.getOutputStream());
	           
	            while (isSending) {
					if(!queue.isEmpty()){
						String dataString= queue.poll()+"\n";
						outputWriter.write(dataString);
						outputWriter.flush();
						log.info("outputWriter: " +dataString);
					 }
	            }
	            
	            outputWriter.close();
	            client.close();
	        } catch (java.io.IOException e) {
	            System.out.println("Socket連線有問題 !");
	            System.out.println("IOException :" + e.toString());
	        }
		}
		
	}
	
}
