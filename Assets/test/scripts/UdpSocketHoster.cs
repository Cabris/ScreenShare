using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

public class UdpSocketHoster 
{
	#region Fields (3) 
	
	private int mRecieverBuffer = 2048;
	private Socket mSocket;
	
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

		//Setting Endpoint
		IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, Port);
		mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		Application.runInBackground=true;
		//Binding Endpoint
		mSocket.Bind(endpoint);
	}
	
	public void Stop()
	{
		mSocket.Close();
	}
	// Private Methods (1) 
	
	public void Listen()
	{            

		//Getting Client Ip
		IPEndPoint clientEndpoint = new IPEndPoint(IPAddress.Any, Port);
		EndPoint Remote = (EndPoint)(clientEndpoint);                       

		try
		{
			int recv=0;
			byte[] receivePackage = new byte[mRecieverBuffer];
			
			//Receive data from client
			recv = mSocket.ReceiveFrom(receivePackage, ref Remote);
			
			//Deserialize data
			
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
		
	}
	
	#endregion Methods 
}