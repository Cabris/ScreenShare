import java.nio.ByteBuffer;


public class ByteBufferTest {

	/**
	 * @param args
	 */
	public static void main(String[] args) {
		// TODO Auto-generated method stub
		ByteBuffer buffer = ByteBuffer.allocate(60);
		buffer.clear();
		byte[] data=new byte[10];
		for (int i = 0; i < data.length; i++) {
			data[i]=(byte) i;
		}
		buffer.put(data);
		
		byte[] data2=new byte[45];
		for (int i = 0; i < data2.length; i++) {
			data2[i]=(byte) i;
		}
		buffer.put(data2);
		
		buffer.flip(); 
		
		byte[] dataOut=new byte[data.length+data2.length];
		buffer.get(dataOut);
		for (int i = 0; i < dataOut.length; i++) {
			System.out.print(i+": "+dataOut[i]+"\n");
		}
	}

}
