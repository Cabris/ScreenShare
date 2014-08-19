package com.stream.source;

import java.util.concurrent.ConcurrentLinkedQueue;

public abstract class StreamSource {

	protected ConcurrentLinkedQueue<byte[]> queue;
	
	public ConcurrentLinkedQueue<byte[]> getQueue() {
		return queue;
	}
	
	public boolean isEmpty() {
		return queue.isEmpty();
	}
	
	public boolean isEOS() {
		return false;
	}
	
}
