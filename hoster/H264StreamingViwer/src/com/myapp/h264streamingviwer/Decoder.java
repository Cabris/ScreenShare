package com.myapp.h264streamingviwer;

import android.os.Bundle;

import java.nio.ByteBuffer;

import com.stream.source.StreamSource;

import android.app.Activity;
import android.content.Context;
import android.media.MediaCodec;
import android.media.MediaCodec.BufferInfo;
import android.media.MediaFormat;
import android.util.Log;
import android.view.Surface;
import android.view.SurfaceHolder;
import android.view.SurfaceView;

public class Decoder{

	//private static String SAMPLE = "storage/sdcard1/" + "v2.mp4";
	private PlayerThread mPlayer = null;
	SurfaceView sv;
	StreamSource inputStream;

	public Decoder(SurfaceView surfaceView, StreamSource input) {
		this.sv = surfaceView;
		this.inputStream = input;
	}

	public void onCreate(Bundle savedInstanceState) {
		if (mPlayer == null) {
			mPlayer = new PlayerThread(sv.getHolder().getSurface());
			mPlayer.start();
		}
	}

	public void onDestroy() {
		if (mPlayer != null) {
			mPlayer.interrupt();
		}
	}

	private class PlayerThread extends Thread {
		// private MediaExtractor extractor;
		private MediaCodec decoder;
		private Surface surface;
		long pts = 0;

		public PlayerThread(Surface surface) {
			this.surface = surface;
		}

		@Override
		public void run() {
			MediaFormat format = new MediaFormat();
			format.setString(MediaFormat.KEY_MIME, "video/avc");
			format.setInteger(MediaFormat.KEY_MAX_INPUT_SIZE, 800000);
			format.setInteger(MediaFormat.KEY_WIDTH, 1280);
			format.setInteger(MediaFormat.KEY_HEIGHT, 720);
			format.setInteger("max-width", 1280);
			format.setInteger("max-height", 720);
			format.setInteger("push-blank-buffers-on-shutdown", 1);
			decoder = MediaCodec.createDecoderByType("video/avc");
			decoder.configure(format, surface, null, 0);

			if (decoder == null) {
				Log.e("DecodeActivity", "Can't find video info!");
				return;
			}

			decoder.start();

			ByteBuffer[] inputBuffers = decoder.getInputBuffers();
			ByteBuffer[] outputBuffers = decoder.getOutputBuffers();
			BufferInfo info = new BufferInfo();
			
			while (!Thread.interrupted()&&!inputStream.isEOS()) {
				if (!inputStream.isEmpty()) {

					int inIndex = decoder.dequeueInputBuffer(100);
					//Log.d("DecodeActivity", "inIndex: " + inIndex);
					if (inIndex >= 0) {
						ByteBuffer buffer = inputBuffers[inIndex];//
						int sampleSize = readSampleData(buffer);//
						if (sampleSize < 0) {
							Log.d("DecodeActivity", "InputBuffer BUFFER_FLAG_END_OF_STREAM");
							decoder.queueInputBuffer(inIndex, 0, 0, 0, MediaCodec.BUFFER_FLAG_END_OF_STREAM);
						} else if (sampleSize > 0) {
							//Log.d("DecodeActivity", "sampleSize>0");
							if (pts == 0) {
								decoder.queueInputBuffer(inIndex, 0, sampleSize, pts,
										MediaCodec.BUFFER_FLAG_CODEC_CONFIG);
							} else {
								decoder.queueInputBuffer(inIndex, 0, sampleSize, pts, 0);
							}
							pts++;
						}
					}

					int outIndex = decoder.dequeueOutputBuffer(info, 100);
					switch (outIndex) {
					case MediaCodec.INFO_OUTPUT_BUFFERS_CHANGED:
						Log.d("DecodeActivity", "INFO_OUTPUT_BUFFERS_CHANGED");
						outputBuffers = decoder.getOutputBuffers();
						break;
					case MediaCodec.INFO_OUTPUT_FORMAT_CHANGED:
						Log.d("DecodeActivity", "New format " + decoder.getOutputFormat());
						break;
					case MediaCodec.INFO_TRY_AGAIN_LATER:
						Log.d("DecodeActivity", "dequeueOutputBuffer timed out!");
						break;
					default:
						ByteBuffer buffer = outputBuffers[outIndex];
						Log.v("DecodeActivity", "We can't use this buffer but render it due to the API limit, "
								+ buffer);
						decoder.releaseOutputBuffer(outIndex, true);
						break;
					}
				}
				// All decoded frames have been rendered, we can stop playing now
				if ((info.flags & MediaCodec.BUFFER_FLAG_END_OF_STREAM) != 0) {
					Log.d("DecodeActivity", "OutputBuffer BUFFER_FLAG_END_OF_STREAM");
					break;
				}
			}

			decoder.stop();
			decoder.release();
			// extractor.release();
		}
	}

	int readSampleData(ByteBuffer buffer) {
		if(inputStream.isEOS())
			return -1;
		if (!inputStream.isEmpty()) {
			byte[] sampleData = inputStream.getQueue().poll();
			buffer.clear();
			buffer.put(sampleData);
			//Log.d("DecodeActivity", "readSampleData");
			return sampleData.length;
		}
		//Log.d("DecodeActivity", "Try readSampleData1");
		return 0;
	}

}
