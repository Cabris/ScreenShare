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
	
	#endregion Fields 
	
	#region Constructors (1) 
	
	public UdpSocketSender(string ip, int port)
	{
		this.Ip = ip;
		this.Port = port;
		Application.runInBackground=true;
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
		IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(Ip), Port);
		Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		byte[] package = new byte[mPackageBuffer];
		for(int i =0;i<data.Length;i++){
			package[i]=data[i];
		}
		server.SendTo(package, package.Length, SocketFlags.None, endPoint);
	}
	
	#endregion Methods 
}