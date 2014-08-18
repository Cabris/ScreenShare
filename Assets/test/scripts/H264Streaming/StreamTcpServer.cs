using UnityEngine;
using System.Collections.Generic;
using System.Net;
using System;
using System.Net.Sockets;
using System.Threading;

public class StreamTcpServer : MonoBehaviour {

	public delegate void OnClientSendMessageEvent(int cid,string msg);
	public OnClientSendMessageEvent OnClientSendMessage;

	private TcpListener tcpListener;
	private Thread listenThread;
	List<TcpClient> clients=new List<TcpClient>();
	
	// Use this for initialization
	void Start () {
		this.tcpListener = new TcpListener(IPAddress.Any, 8888);
		this.listenThread = new Thread(new ThreadStart(ListenForClients));
		this.listenThread.Start();
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
			Debug.Log("client:"+clients.Count);
			ParameterizedThreadStart tStart=new ParameterizedThreadStart(HandleClientComm);
			//Thread clientThread=new Thread(tStart);
			//clientThread.Start(client);
			HandleClientComm(client);
			break;
			Debug.Log("client");
		}
		
	}

	private void HandleClientComm(object client)
	{
		TcpClient tcpClient = (TcpClient)client;
		clients.Add(tcpClient);
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

	public void ForseStopClients(){
		foreach(TcpClient c in clients){
			NetworkStream clientStream = c.GetStream();
			int length=-1;
			byte[] lengthData=getBytes(length);
			clientStream.Write(lengthData, 0 , lengthData.Length);          
			clientStream.Flush();
			Debug.Log("ForseStopClients: "+length);
		}   
	}

	public static byte[] getBytes(int x) {
		return new byte[] { (byte) (x >> 24), (byte) (x >> 16),
			(byte) (x >> 8), (byte) x };
	}

	void OnApplicationQuit() {
		foreach(TcpClient c in clients)
			c.Close();
		tcpListener.Stop();
		listenThread.Abort();
	}
}