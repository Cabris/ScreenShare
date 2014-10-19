package com.myapp.h264streamingviwer.funcs;

import java.nio.ByteBuffer;

import com.myapp.h264streamingviwer.IHandleVideoSize;
import com.stream.source.StreamSource;
import android.media.MediaCodec;
import android.media.MediaCodec.BufferInfo;
import android.media.MediaFormat;
import android.util.Log;
import android.view.Surface;
import android.view.SurfaceHolder;

public class Decoder {

	private PlayerThread mPlayer = null;
	public IHandleVideoSize videoSize;
	//SurfaceView surfaceView;
	SurfaceHolder surfaceHolder;
	StreamSource inputStream;
	
	public Decoder( StreamSource input) {
		this.inputStream = input;
	}

	public void onCreate(SurfaceHolder surfaceHolder) {
		this.surfaceHolder = surfaceHolder;
		if (mPlayer == null) {
			mPlayer = new PlayerThread(surfaceHolder.getSurface());
			mPlayer.start();
		}
	}

	public void onStop() {
		if (mPlayer != null) {
			mPlayer.interrupt();
		}
	}

	private class PlayerThread extends Thread {
		private MediaCodec decoder;
		private Surface surface;
		private long pts;

		public PlayerThread(Surface surface) {
			this.surface = surface;
			this.pts = 0;
		}

		@Override
		public void run() {
			MediaFormat format = new MediaFormat();
			format.setString(MediaFormat.KEY_MIME, "video/avc");
			format.setInteger(MediaFormat.KEY_MAX_INPUT_SIZE, 2000000);
			format.setInteger(MediaFormat.KEY_WIDTH, 1280);
			format.setInteger(MediaFormat.KEY_HEIGHT, 720);
			format.setInteger(MediaFormat.KEY_MAX_WIDTH, 1280);
			format.setInteger(MediaFormat.KEY_MAX_HEIGHT, 720);
			format.setInteger(MediaFormat.KEY_PUSH_BLANK_BUFFERS_ON_STOP, 1);
			decoder = MediaCodec.createDecoderByType("video/avc");
			try {
				decoder.configure(format, surface, null, 0);
			} catch (Exception e) {
				e.printStackTrace();
			}

			if (decoder == null) {
				Log.e("Decoder", "Can't find video info!");
				return;
			}

			//decoder.stop();
			decoder.start();

			ByteBuffer[] inputBuffers = decoder.getInputBuffers();
			// ByteBuffer[] outputBuffers = decoder.getOutputBuffers();
			BufferInfo info = new BufferInfo();
			try {
				while (!Thread.interrupted() && !inputStream.isEOS()) {
					if (!inputStream.isEmpty()) {
						int inIndex = decoder.dequeueInputBuffer(100);
						// Log.d("DecodeActivity", "inIndex: " + inIndex);
						if (inIndex >= 0) {
							ByteBuffer buffer = inputBuffers[inIndex];//
							int sampleSize = readSampleData(buffer);//
							if (sampleSize < 0) {
								Log.d("Decoder", "InputBuffer BUFFER_FLAG_END_OF_STREAM");
								decoder.queueInputBuffer(inIndex, 0, 0, 0, MediaCodec.BUFFER_FLAG_END_OF_STREAM);
							} else if (sampleSize > 0) {
								if (pts == 0)
									decoder.queueInputBuffer(inIndex, 0, sampleSize, pts,
											MediaCodec.BUFFER_FLAG_CODEC_CONFIG);
								else
									decoder.queueInputBuffer(inIndex, 0, sampleSize, pts, 0);
								pts++;
							}
						}

						int outIndex = decoder.dequeueOutputBuffer(info, 100);
						switch (outIndex) {
						case MediaCodec.INFO_OUTPUT_BUFFERS_CHANGED:
							break;
						case MediaCodec.INFO_OUTPUT_FORMAT_CHANGED:
							MediaFormat format1 = decoder.getOutputFormat();
							int width = format1.getInteger(MediaFormat.KEY_WIDTH);
							int height = format1.getInteger(MediaFormat.KEY_HEIGHT);
							videoSize.handleVideoSize(width, height);
							break;
						case MediaCodec.INFO_TRY_AGAIN_LATER:
							// Log.d("DecodeActivity", "dequeueOutputBuffer timed out!");
							break;
						default:
							// ByteBuffer buffer = outputBuffers[outIndex];
							// Log.v("DecodeActivity", "We can't use this buffer but render it due to the API limit, "+ buffer);
							decoder.releaseOutputBuffer(outIndex, true);
							break;
						}
					}
					// All decoded frames have been rendered, we can stop playing now
					if ((info.flags & MediaCodec.BUFFER_FLAG_END_OF_STREAM) != 0) {
						Log.d("Decoder", "OutputBuffer BUFFER_FLAG_END_OF_STREAM");
						break;
					}
				}
				//int outIndex = decoder.dequeueOutputBuffer(info, 100);
				//decoder.releaseOutputBuffer(outIndex, true);
				decoder.stop();
				decoder.release();
			} catch (Exception e) {
				e.printStackTrace();
			}
		}
	
		@Override
		public void interrupt() {
			super.interrupt();
			//decoder.stop();
			//decoder.release();
		}
	
	}

	int readSampleData(ByteBuffer buffer) {
		if (inputStream.isEOS())
			return -1;
		if (!inputStream.isEmpty()) {
			byte[] sampleData = inputStream.getQueue().poll();
			buffer.clear();
			buffer.put(sampleData);
			// Log.d("DecodeActivity", "readSampleData");
			Log.d("Decoder", "size=" + inputStream.getQueue().size());
			return sampleData.length;
		}
		return 0;
	}

}
