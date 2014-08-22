using System;

public class BitConverter2 {
	public  const int FLAG_JAVA = 0;  
	public  const int FLAG_REVERSE = -1;  
	
	/** short -&gt; byte[] */
	public static  byte[] getBytes(short s){
		return getBytes(s, FLAG_JAVA);
	}
	/** short -&gt; byte[] */
	public static byte[] getBytes(short s, int flag) {
		byte[] b = new byte[2];
		switch (flag) {
		case BitConverter2.FLAG_JAVA:
			b[0] = (byte) ((s >> 8) & 0xff);
			b[1] = (byte) (s & 0xff);
			break;
		case BitConverter2.FLAG_REVERSE:
			b[1] = (byte) ((s >> 8) & 0xff);
			b[0] = (byte) (s & 0xff);
			break;
		default:
			break;	
		}
		return b;
	}
	
	/** int -&gt; byte[] */
	public static byte[] getBytes(int i) {
		return getBytes(i, FLAG_JAVA);
	}
	
	/** int -&gt; byte[] */
	public static byte[] getBytes(int i, int flag) {
		byte[] b = new byte[4];
		switch (flag) {
		case BitConverter2.FLAG_JAVA:
			b[0] = (byte) ((i >> 24) & 0xff);
			b[1] = (byte) ((i >> 16) & 0xff);
			b[2] = (byte) ((i >> 8) & 0xff);
			b[3] = (byte) (i & 0xff);
			break;
		case BitConverter2.FLAG_REVERSE:
			b[3] = (byte) ((i >> 24) & 0xff);
			b[2] = (byte) ((i >> 16) & 0xff);
			b[1] = (byte) ((i >> 8) & 0xff);
			b[0] = (byte) (i & 0xff);
			break;
		default:
			break;	
		}
		return b;
	}
	
	/** long -&gt; byte[] */
	public static byte[] getBytes(long i) {
		return getBytes(i, FLAG_JAVA);
	}
	
	/** long -&gt; byte[] */
	public static byte[] getBytes(long i, int flag) {
		byte[] b = new byte[8];
		switch (flag) {
		case BitConverter2.FLAG_JAVA:
			b[0] = (byte) ((i >> 56) & 0xff);
			b[1] = (byte) ((i >> 48) & 0xff);
			b[2] = (byte) ((i >> 40) & 0xff);
			b[3] = (byte) ((i >> 32) & 0xff);
			b[4] = (byte) ((i >> 24) & 0xff);
			b[5] = (byte) ((i >> 16) & 0xff);
			b[6] = (byte) ((i >> 8) & 0xff);
			b[7] = (byte) ((i >> 0) & 0xff);
			break;
		case BitConverter2.FLAG_REVERSE:
			b[7] = (byte) ((i >> 56) & 0xff);
			b[6] = (byte) ((i >> 48) & 0xff);
			b[5] = (byte) ((i >> 40) & 0xff);
			b[4] = (byte) ((i >> 32) & 0xff);
			b[3] = (byte) ((i >> 24) & 0xff);
			b[2] = (byte) ((i >> 16) & 0xff);
			b[1] = (byte) ((i >> 8) & 0xff);
			b[0] = (byte) ((i >> 0) & 0xff);
			break;
		default:
			break;	
		}
		return b;
	}
	
	/** byte[] -&gt; short */
	public static short toShort(byte[] b) {
		return toShort(b, FLAG_JAVA);
	}
	/** byte[] -&gt; short */
	public static short toShort(byte[] b, int flag) {
		switch (flag) {
		case BitConverter2.FLAG_JAVA:
			return (short) (((b[0] & 0xff) << 8) | (b[1] & 0xff));
		case BitConverter2.FLAG_REVERSE:
			return (short) (((b[1] & 0xff) << 8) | (b[0] & 0xff));
		default:
			throw new Exception("BitConverter:toShort");
		}
	}
	
	/** byte[] -&gt; int */
	public static int toInt(byte[] b){
		return toInt(b,FLAG_JAVA);
	}
	/** byte[] -&gt; int */
	public static int toInt(byte[] b, int flag) {
		switch(flag){
		case BitConverter2.FLAG_JAVA:
			return (int)(((b[0] & 0xff)<<24) | ((b[1] & 0xff)<<16) | ((b[2] & 0xff)<<8) | (b[3] & 0xff));
		case BitConverter2.FLAG_REVERSE:
			return (int)(((b[3] & 0xff)<<24) | ((b[2] & 0xff)<<16) | ((b[1] & 0xff)<<8) | (b[0] & 0xff));
		default:
			throw new Exception("BitConverter:toInt");
		}
	}
	
	/** byte[] -&gt; long */
	public static long toLong(byte[] b) {
		return toLong(b, FLAG_JAVA);
	}
	/** byte[] -&gt; long */
	public static long toLong(byte[] b, int flag) {
		switch (flag) {
		case BitConverter2.FLAG_JAVA:
			return (((long) (b[0] & 0xff) << 56) | ((long) (b[1] & 0xff) << 48) | ((long) (b[2] & 0xff) << 40)
			        | ((long) (b[3] & 0xff) << 32) | ((long) (b[4] & 0xff) << 24) | ((long) (b[5] & 0xff) << 16)
			        | ((long) (b[6] & 0xff) << 8) | ((long) (b[7] & 0xff)));
		case BitConverter2.FLAG_REVERSE:
			return (((long) (b[7] & 0xff) << 56) | ((long) (b[6] & 0xff) << 48) | ((long) (b[5] & 0xff) << 40)
			        | ((long) (b[4] & 0xff) << 32) | ((long) (b[3] & 0xff) << 24) | ((long) (b[2] & 0xff) << 16)
			        | ((long) (b[1] & 0xff) << 8) | ((long) (b[0] & 0xff)));
		default:
			throw new Exception("BitConverter:toLong");
		}
	}

}