using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

public class UdpSocketSender
{
	#region Fields (1) 
	
	private int mPackageBuffer = 2048;
	public Socket server;
	#endregion Fields 
	
	#region Constructors (1) 
	
	public UdpSocketSender(string ip, int port)
	{
		this.Ip = ip;
		this.Port = port;
		Application.runInBackground=true;
		server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		server.EnableBroadcast=true;
		server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
	}
	
	#endregion Constructors 
	
	#region Properties (3) 
	
	public string Ip { get; set; }
	
	public int PackageBuffer
	{
		get { return mPackageBuffer; }
		set { mPackageBuffer = value; }
	}
	
	public int Port { get; set; }
	
	#endregion Properties 
	
	#region Methods (1) 
	
	// Public Methods (1) 



	public void Send(byte[] data)
	{
		//Setting Server Endpoint
		IPEndPoint endPoint = new IPEndPoint(IPAddress.Broadcast, Port);
		
		byte[] package = new byte[mPackageBuffer];
		for(int i =0;i<data.Length;i++){
			package[i]=data[i];
		}
		try{
			server.SendTo(package, package.Length, SocketFlags.None, endPoint);
			Debug.Log("send package: "+package.Length);
		}
		catch(Exception e){
			Debug.LogException(e);
			throw e;
		}
	}
	
	#endregion Methods 
}