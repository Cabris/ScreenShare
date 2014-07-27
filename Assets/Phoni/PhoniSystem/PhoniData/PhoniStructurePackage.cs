using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Phoni.PhoniCore;
	
namespace Phoni {
	
	#region customized phoni data
	public class PhoniCustomDataSample : PhoniDataBase {
		public int intData;
		public string stringData;
		
		public PhoniCustomDataSample () : base() {
		}
		
		public PhoniCustomDataSample (int paramInt, string paramStr) : base() {
			intData = paramInt;
			stringData = paramStr;
		}
		
		public override uint GetCode ()
		{
			return (uint)PhoniPackageCode.PACKAGE_CUSTOM_SAMPLE;
		}
		
		public override byte[] Serialize ()
		{
			List<byte> bytes = new List<byte>();
			bytes.AddRange(BitConverter.GetBytes(intData));
			bytes.AddRange(System.Text.Encoding.Unicode.GetBytes(stringData));
			return bytes.ToArray();
		}
						
		public override PhoniDataBase Deserialize (byte[] data)
		{
			PhoniCustomDataSample result = new PhoniCustomDataSample();
			result.Deserialize(data);
			return result;
		}
		
		
	}
	#endregion
	
	// connect
	[System.Serializable]
	public class PhoniConnectData {
		public string ipAddress;
		public int port;
		public uint platformCode;
		
		public PhoniConnectData() {
			ipAddress = "0.0.0.0";
			port = 0;
			platformCode = 0;
		}
		
		public PhoniConnectData(string ipAddress, int port, uint platformCode) {
			this.ipAddress = ipAddress;
			this.port = port;
			this.platformCode = platformCode;
		}
		
		public PhoniConnectData(NetworkInfo info, uint platformCode) {
			this.ipAddress = info.ipAddress;
			this.port = info.port;
			this.platformCode = platformCode;
		}
		
	}
	
	// touch
	public class PhoniTouchData {
		public int TouchCount {
			get {
				if(touches == null) {
					return 0;
				}
				else {
					return touches.Length;
				}
			}
		}
		public PhoniTouch[] touches;
		public bool multiTouchEnabled;
		
		public PhoniTouchData() {
			touches = new PhoniTouch[0];
			multiTouchEnabled = false;
		}
		
		public PhoniTouch GetTouch(int fingerId) {
			PhoniTouch result = null;
			foreach(PhoniTouch touch in touches) {
				if(touch.fingerId == fingerId && 
					touch.IsTouchOn) {
					result = touch;
					break;
				}
			}
			return result;
		}
		
		public bool HasTouch(int fingerId) {
			bool result = false;
			foreach(PhoniTouch touch in touches) {
				if(touch.fingerId == fingerId && 
					touch.IsTouchOn) {
					result = true;
					break;
				}
			}
			return result;
		}
	}
	
	[System.Serializable]
	public class PhoniTouchDataSerializer : IPhoniPackage<PhoniTouchData>{
		public TouchSerializer[] touches;
		public bool multiTouchEnabled;
		
		public PhoniTouchDataSerializer() {
			touches = new TouchSerializer[0];
			multiTouchEnabled = false;
		}
				
		public object Unpack() {
			PhoniTouchData ptd = new PhoniTouchData();
			ptd.touches = new PhoniTouch[touches.Length];
			for(int i=0;i<touches.Length;i++) {
				ptd.touches[i] = touches[i].Unpacked;
			}
			ptd.multiTouchEnabled = multiTouchEnabled;
			return ptd;
		}		
		
		public void Pack (PhoniTouchData data) {
			touches = new TouchSerializer[data.touches.Length];
			for(int i=0;i<touches.Length;i++) {
				touches[i] = new TouchSerializer(data.touches[i]);
			}
			multiTouchEnabled = data.multiTouchEnabled;
		}
	}	
	
	public class PhoniTestData {
		
		public Vector3 acceleration;
		
		public PhoniTestData() {
			acceleration = Vector3.zero;
		}
	}
	
	// motion
	public class PhoniMotionData {
		public Vector3 acceleration;
		public PhoniGyroscope gyro;
		
		public PhoniMotionData() {
			acceleration = Vector3.zero;
			gyro = new PhoniGyroscope();
		}
	}
	
	[System.Serializable]
	public class PhoniMotionDataSerializer : IPhoniPackage<PhoniMotionData>{
		public Vector3Serializer acceleration;
		public GyroscopeSerializer gyro;
		
		public PhoniMotionDataSerializer() {
			acceleration = new Vector3Serializer();
			gyro = new GyroscopeSerializer();
		}
				
		public object Unpack() {
			PhoniMotionData pmd = new PhoniMotionData();
			pmd.acceleration = acceleration.Unpacked;
			pmd.gyro = gyro.Unpacked;
			return pmd;
		}		
		
		public void Pack (PhoniMotionData data) {
			acceleration = new Vector3Serializer(data.acceleration);
			gyro = new GyroscopeSerializer(data.gyro);
		}
	}	
	
	public class PhoniAnalogData {
		public Vector2 left;
		public Vector2 right;
		
		public PhoniAnalogData() {
			left = Vector2.zero;
			right = Vector2.zero;
		}
	}
	
	[System.Serializable]
	public class PhoniAnalogDataSerializer : IPhoniPackage<PhoniAnalogData> {
		public Vector2Serializer left;
		public Vector2Serializer right;
		
		public PhoniAnalogDataSerializer() {
			left = new Vector2Serializer();
			right = new Vector2Serializer();
		}
		
		public object Unpack ()
		{
			PhoniAnalogData data = new PhoniAnalogData();
			data.left = left.Unpacked;
			data.right = right.Unpacked;
			return data;
		}
		
		public void Pack (PhoniAnalogData data)
		{
			left = new Vector2Serializer(data.left);
			right = new Vector2Serializer(data.right);
		}
	}
	
	[System.Serializable]
	public class PhoniButtonData {
		public PhoniButton buttons;
		
		public PhoniButtonData() {
			buttons = PhoniButton.None;
		}
		
		public PhoniButtonData(PhoniButton buttons) {
			this.buttons = buttons;
		}
		
		public bool GetButton(PhoniButton requestButtons) {
			return (buttons & requestButtons) == requestButtons;
		}
		
		public bool GetButtonAny(PhoniButton requestButtons) {
			return (buttons & requestButtons) > 0;
		}
		
		public bool GetButtonAny() {
			return GetButtonAny(PhoniButton.All);
		}
	}
	
			
	
	#region serializable data structure
	
	[System.Serializable]
	public class Vector2Serializer : IPhoniPackage<Vector2> {
		public float x;
		public float y;
		
		public Vector2Serializer() {
			Pack (Vector2.zero);
		}
		
		public Vector2Serializer(Vector2 v) {
			Pack(v);
		}
		
		public object Unpack ()
		{
			return new Vector2(x,y);
		}
		
		public void Pack (Vector2 data)
		{
			x = data.x;
			y = data.y;
		}
		
		public Vector2 Unpacked {
			get { return (Vector2)Unpack();}
		}
	}
	
	[System.Serializable]
	public class Vector3Serializer : IPhoniPackage<Vector3> {
		public float x;
		public float y;
		public float z;
		
		public Vector3Serializer() {
			Pack (Vector3.zero);
		}
		
		public Vector3Serializer(Vector3 v) {
			Pack (v);
		}
		
		public object Unpack ()
		{
			return new Vector3(x,y,z);
		}
		
		public void Pack (Vector3 data)
		{
			x = data.x;
			y = data.y;
			z = data.z;
		}
		
		public Vector3 Unpacked {
			get { return (Vector3)Unpack(); }
		}
	}
	
	[System.Serializable]
	public class QuaternionSerializer : IPhoniPackage<Quaternion> {
		public float x;
		public float y;
		public float z;
		public float w;
		
		public QuaternionSerializer() {
			Pack(Quaternion.identity);
		}
		
		public QuaternionSerializer(Quaternion q) {
			Pack(q);
		}
				
		public object Unpack ()
		{
			return new Quaternion(x,y,z,w);
		}
		
		public void Pack (Quaternion data)
		{
			x = data.x;
			y = data.y;
			z = data.z;
			w = data.w;
		}
		
		public Quaternion Unpacked {
			get { return (Quaternion)Unpack(); }
		}
	}
	
	[System.Serializable]
	public class ColorSerializer : IPhoniPackage<Color> {
		public float r;
		public float g;
		public float b;
		public float a;
		
		public ColorSerializer() {
			Pack(Color.black);
		}
		
		public ColorSerializer(Color color) {
			Pack(color);
		}
		
		public object Unpack ()
		{
			return new Color(r,g,b,a);
		}
		
		public void Pack (Color data)
		{
			r = data.r;
			g = data.g;
			b = data.b;
			a = data.a;
		}
		
		public Color Unpacked {
			get { return (Color)Unpack();}
		}
	}
		
	[System.Serializable]
	public class TouchSerializer {
		public int fingerId;
		public Vector2Serializer position;
		public Vector2Serializer positionNormalized;
		public Vector2Serializer deltaPosition;
		public float deltaTime;
		public int tapCount;
		public int phase;
		
		public TouchSerializer() {
			fingerId = 0;
			position = new Vector2Serializer();
			positionNormalized = new Vector2Serializer();
			deltaPosition = new Vector2Serializer();
			deltaTime = 0;
			tapCount = 0;
			phase = 0;
		}
		
		public TouchSerializer(PhoniTouch touch) {
			fingerId = touch.fingerId;
			position = new Vector2Serializer(touch.position);
			positionNormalized = new Vector2Serializer(touch.positionNormalized);
			deltaPosition = new Vector2Serializer(touch.deltaPosition);
			deltaTime = touch.deltaTime;
			tapCount = touch.tapCount;
			phase = (int)touch.phase;
		}
		
		public PhoniTouch Unpacked {
			get { return new PhoniTouch(this);}
		}
	}
	
	[System.Serializable]
	public class GyroscopeSerializer {
		public Vector3Serializer rotationRate;
		public Vector3Serializer rotationRateUnbiased;
		public Vector3Serializer gravity;
		public Vector3Serializer userAcceleration;
		public QuaternionSerializer attitude;
		public bool enabled;
		public float updateInterval;
		
		public GyroscopeSerializer() {
			rotationRate = new Vector3Serializer();
			rotationRateUnbiased = new Vector3Serializer();
			gravity = new Vector3Serializer();
			userAcceleration = new Vector3Serializer();
			attitude = new QuaternionSerializer();
			enabled = false;
			updateInterval = 0;
		}
		
		public GyroscopeSerializer(Gyroscope gyro) {
			rotationRate = new Vector3Serializer(gyro.rotationRate);
			rotationRateUnbiased = new Vector3Serializer(gyro.rotationRateUnbiased);
			gravity = new Vector3Serializer(gyro.gravity);
			userAcceleration = new Vector3Serializer(gyro.userAcceleration);
			attitude = new QuaternionSerializer(gyro.attitude);
			enabled = gyro.enabled;
			updateInterval = gyro.updateInterval;
		}
		
		public GyroscopeSerializer(PhoniGyroscope gyro) {
			rotationRate = new Vector3Serializer(gyro.rotationRate);
			rotationRateUnbiased = new Vector3Serializer(gyro.rotationRateUnbiased);
			gravity = new Vector3Serializer(gyro.gravity);
			userAcceleration = new Vector3Serializer(gyro.userAcceleration);
			attitude = new QuaternionSerializer(gyro.attitude);
			enabled = gyro.enabled;
			updateInterval = gyro.updateInterval;
		}
		
		public PhoniGyroscope Unpacked {
			get { return new PhoniGyroscope(this); }
		}
	}
	
	
	public class PhoniTouch {
		public int fingerId;
		public Vector2 position;
		public Vector2 positionNormalized;
		public Vector2 deltaPosition;
		public float deltaTime;
		public int tapCount;
		public TouchPhase phase;
		
		public bool IsTouchOn {
			get {
				return (phase == TouchPhase.Began ||
					phase == TouchPhase.Moved ||
					phase == TouchPhase.Stationary);
			}
		}
		
		public PhoniTouch() {
			fingerId = 0;
			position = Vector2.zero;
			positionNormalized = Vector2.zero;
			deltaPosition = Vector2.zero;
			deltaTime = 0;
			tapCount = 0;
			phase = TouchPhase.Canceled;
		}
		
		public PhoniTouch(TouchSerializer touch) {
			fingerId = touch.fingerId;
			position = touch.position.Unpacked;
			positionNormalized = touch.positionNormalized.Unpacked;
			deltaPosition = touch.deltaPosition.Unpacked;
			deltaTime = touch.deltaTime;
			tapCount = touch.tapCount;
			phase = (TouchPhase)touch.phase;
		}
		
		public PhoniTouch(Touch touch, Rect touchRect) {
			fingerId = touch.fingerId;
			position = touch.position;
			deltaPosition = touch.deltaPosition;
			deltaTime = touch.deltaTime;
			tapCount = touch.tapCount;
			phase = touch.phase;
			
			float rateX = Mathf.Clamp01((position.x - touchRect.xMin)/touchRect.width);
			float rateY = Mathf.Clamp01((position.y - touchRect.yMin)/touchRect.height);
			positionNormalized = new Vector2(rateX, rateY);
		}
		
		public static PhoniTouch[] ConvertTouchArray(Touch[] touches, Rect touchRect) {
			int len = touches.Length;
			PhoniTouch[] phoniTouches = new PhoniTouch[len];
			for(int i = 0; i < len; i++) {
				phoniTouches[i] = new PhoniTouch(touches[i], touchRect);
			}
			return phoniTouches;
		}
		
		public override string ToString ()
		{
			return "fingerId: "+fingerId+ " " +
				"position: "+position+" " +
				"deltaPosition: "+deltaPosition + " " +
				"deltaTime: "+deltaTime+" " +
				"tapCount: "+tapCount+" " +
				"phase: "+phase;
		}
	}

	public class PhoniGyroscope {
		public Vector3 rotationRate;
		public Vector3 rotationRateUnbiased;
		public Vector3 gravity;
		public Vector3 userAcceleration;
		public Quaternion attitude;
		public bool enabled;
		public float updateInterval;
		
		public PhoniGyroscope() {
			rotationRate = Vector3.zero;
			rotationRateUnbiased = Vector3.zero;
			gravity = Vector3.zero;
			userAcceleration = Vector3.zero;
			attitude = Quaternion.identity;
			enabled = false;
			updateInterval = 0;
		}
		
		public PhoniGyroscope(GyroscopeSerializer gyro) {
			rotationRate = gyro.rotationRate.Unpacked;
			rotationRateUnbiased = gyro.rotationRateUnbiased.Unpacked;
			gravity = gyro.gravity.Unpacked;
			userAcceleration = gyro.userAcceleration.Unpacked;
			attitude = gyro.attitude.Unpacked;
			enabled = gyro.enabled;
			updateInterval = gyro.updateInterval;
		}
		
		public PhoniGyroscope(Gyroscope gyro) {
			rotationRate = gyro.rotationRate;
			rotationRateUnbiased = gyro.rotationRateUnbiased;
			gravity = gyro.gravity;
			userAcceleration = gyro.userAcceleration;
			attitude = gyro.attitude;
			enabled = gyro.enabled;
			updateInterval = gyro.updateInterval;
		}
	}
	#endregion
	
} //namespace