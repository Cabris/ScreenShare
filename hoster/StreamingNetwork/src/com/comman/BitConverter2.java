package com.comman;


/** ���������r�`�����ഫ */
public class BitConverter2 {
	/*
	 * �㫬�ơ]short,int,long,char���^��byte[]�s�x�ɦ���ر��p
	 *  1.����b�e(java�q�{Ū���覡)
	 *  2.�C��b�e(�䥦���p)
	 *  
	 *  �Hint 257;���Ҧb�G
	 *       ����b�e�s�x��4��byte�������O [0,0,1,1]
	 *       �C��b�e�s�x��4��byte�������O [1,1,0,0]
	 */
	public static final  int FLAG_JAVA = 0;  //��μƥΰ���b�e�s�x�覡
	public static final int FLAG_REVERSE = -1;  //��μƥΧC��b�e�s�x�覡
	
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
			throw new IllegalArgumentException("BitConverter:toShort");
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
				throw new IllegalArgumentException("BitConverter:toInt");
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
			throw new IllegalArgumentException("BitConverter:toLong");
		}
	}
	
	/***/
//	public static void main(String[] args){
//		short s = -10;
//		int i = -10000;
//		long lo = -10000;
//		print(BitConverter2.getBytes(s));
//		print(BitConverter2.getBytes(i));
//		print(BitConverter2.getBytes(lo));
//		
//		System.out.println(BitConverter2.toShort(BitConverter2.getBytes(s)));
//		System.out.println(BitConverter2.toInt((BitConverter2.getBytes(i))));
//		System.out.println(BitConverter2.toLong(BitConverter2.getBytes(lo)));
//	}
//	private static void print(byte[] b) {
//		String str = "";
//		for (byte bb : b) {
//			str += bb + ",";
//		}
//		System.out.println(str);
//	}

	/** byte[] -&gt; String */
	public static String getString(byte[] by) {
		if (by == null) {
			return "";
		}
		int len = by.length;
		if (len == 0 || by.length % 2 != 0) {
			return "";
		}
		StringBuffer sb = new StringBuffer(len / 2);
		char ch;
		for (int i = 0; i < len; i += 2) {
			ch = (char) (by[i] & 0xff | ((by[i + 1] & 0xff) << 8));
			sb.append(ch);
		}
		return sb.toString();
	}
	/** String -&gt; byte[] */
    public static byte[] getBytes(String str) {
		if (str == null || str.length() == 0) {
			return null;
		}
		int len = str.length();
		int size = len * 2;
		byte[] by = new byte[size];
		short ch;
		for (int i = 0; i < size; i += 2) {
			ch = (short) str.charAt(i / 2);
			by[i] = (byte) (ch & 0xff);
			by[i + 1] = (byte) ((ch >> 8) & 0xff);
			// sb.append((char)(by[i] & 0xff | (by[i + 1] & 0xff) << 8));
		}
		return by;
	}

}