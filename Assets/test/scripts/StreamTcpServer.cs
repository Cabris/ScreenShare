using UnityEngine;
using System.Collections.Generic;
using System.Net;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.IO;
public class StreamTcpServer : MonoBehaviour {
	
	private TcpListener tcpListener;
	private Thread listenThread;
	List<TcpClient> clients=new List<TcpClient>();

	// Use this for initialization
	void Start () {
		this.tcpListener = new TcpListener(IPAddress.Any, 8888);
		this.listenThread = new Thread(new ThreadStart(ListenForClients));
		this.listenThread.Start();

		//StartCoroutine(ListenForClients());
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void ListenForClients()
	{
		Debug.Log("ListenForClients0");
		this.tcpListener.Start();
		Debug.Log("ListenForClients1");
		while (true){
			//blocks until a client has connected to the server
			TcpClient client = this.tcpListener.AcceptTcpClient();
			Debug.Log("client0");
			//create a thread to handle communication 
			//with connected client
//			Thread clientThread = new Thread(
//				new ParameterizedThreadStart(HandleClientComm));
//			clientThread.Start(client);
			HandleClientComm(client);
			break;
			Debug.Log("client");
		}
		
	}
	
	public int Send(byte [] data){
		foreach(TcpClient c in clients){
			NetworkStream clientStream = c.GetStream();
			int length=data.Length;
			byte[] lengthData=getBytes(length);
			clientStream.Write(lengthData, 0 , lengthData.Length); 
			clientStream.Write(data, 0 , data.Length);         
			clientStream.Flush();
			Debug.Log("send: "+data.Length);
			return length;
		}          
		return -1;
	}

	public static byte[] getBytes(int x) {
		return new byte[] { (byte) (x >> 24), (byte) (x >> 16),
			(byte) (x >> 8), (byte) x };
	}
	
	private void HandleClientComm(object client)
	{
		TcpClient tcpClient = (TcpClient)client;
		clients.Add(tcpClient);
//		NetworkStream clientStream = tcpClient.GetStream();
//		ASCIIEncoding encoder = new ASCIIEncoding();
//		byte[] buffer = encoder.GetBytes("Hello Client!");
//		int w;
//		Debug.Log("HandleClientComm");
//		int i=0;
//		while (i<999)
//		{
//			string s="Hello Client!"+i+"\n";
//			buffer = encoder.GetBytes(s);
//			clientStream.Write(buffer, 0 , buffer.Length);
//			clientStream.Flush();
//			Debug.Log("i: "+i);
//			i++;
//		}
//		
//		tcpClient.Close();
	}
	
	void OnApplicationQuit() {
		foreach(TcpClient c in clients)
			c.Close();
		tcpListener.Stop();
		listenThread.Abort();
	}
}