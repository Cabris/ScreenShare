package com.simpleMessage.sender;

public class MessageSenderMain {

	/**
	 * @param args
	 */
	public static void main(String[] args) {
		// TODO Auto-generated method stub
		MessageSender sender=new MessageSender("192.168.1.48", 8887);
		sender.connect();
	}

}
