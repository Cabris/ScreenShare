using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
public class UdpSocketHoster 
{
	#region Fields (3) 
	
	private int mRecieverBuffer = 2048;
	private Socket mSocket;
	bool isLinstening=false;
	Thread receiveThread;
	
	#endregion Fields 
	
	#region Properties (3) 
	
	public string Ip
	{
		get;
		set;
	}
	
	public int Port
	{
		get;
		set;
	}
	
	public int RecieverBuffer
	{
		get { return mRecieverBuffer; }
		set { mRecieverBuffer = value; }
	}
	
	#endregion Properties 
	
	#region Delegates and Events (3) 
	
	// Events (3) 
	
	public delegate void RecievedDataEvent(byte[] data);
	public RecievedDataEvent RecievedData;
	
	#endregion Delegates and Events 
	
	#region Methods (4) 
	
	// Public Methods (3) 
	
	public UdpSocketHoster(string ip, int port)
	{
		this.Ip = ip;
		this.Port = port;
		Application.runInBackground=true;
		//Setting Endpoint

		IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, Port);
		mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		//Binding Endpoint
		mSocket.Bind(endpoint);
		receiveThread=new Thread(new ThreadStart(Listen));
	}
	
	public void Stop()
	{
		mSocket.Close();
		isLinstening=false;
		receiveThread.Join();
		receiveThread.Abort();
	}
	// Private Methods (1) 
	
	public void Start(){
		isLinstening=true;
		receiveThread.Start();
	}
	
	
	private void Listen()
	{            
		//Getting Client Ip
		IPEndPoint clientEndpoint = new IPEndPoint(IPAddress.Any, Port);
		EndPoint Remote = (EndPoint)(clientEndpoint);                       
		while(isLinstening){
			try
			{
				int recv=0;
				byte[] receivePackage = new byte[mRecieverBuffer];
				
				//Receive data from client
				recv = mSocket.ReceiveFrom(receivePackage, ref Remote);

				if (RecievedData != null&&recv>0)
				{
					RecievedData(receivePackage);
				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
				throw;
			}
			//Thread.Sleep(5);
		}
		
	}
	
	#endregion Methods 
}