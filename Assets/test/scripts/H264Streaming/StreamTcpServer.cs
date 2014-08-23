using UnityEngine;
using System.Collections.Generic;
using System.Net;
using System;
using System.Net.Sockets;
using System.Threading;
using System.IO;

public class StreamTcpServer : MonoBehaviour {
	
	private TcpListener tcpListener;
	private Thread listenThread;
	List<TcpClient> clients=new List<TcpClient>();
	List<BufferedStream> bStreams=new List<BufferedStream>();

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
		string ip=((IPEndPoint)tcpListener.LocalEndpoint).Address.ToString();
		//Debug.Log("ListenForClients1: "+ip);
		string sHostName = Dns.GetHostName (); 
		IPHostEntry ipE = Dns.GetHostByName (sHostName); 
		IPAddress [] IpA = ipE.AddressList; 
		for (int i = 0; i < IpA.Length; i++) 
		{ 
			string s= String.Format("IP Address {0}: {1} ", i, IpA[i].ToString ());
			Debug.Log(s);
		}

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
		tcpClient.NoDelay=true;
		tcpClient.SendBufferSize=60000;
		clients.Add(tcpClient);
		BufferedStream bs=new BufferedStream(tcpClient.GetStream());
		bStreams.Add(bs);
	}
	
	public int Send(byte [] data){
		for(int i=0;i<bStreams.Count;i++){
			BufferedStream bs=bStreams[i];
			int length=data.Length;
			byte[] lengthData=getBytes(length);
			bs.Write(lengthData, 0 , lengthData.Length); 
			bs.Write(data, 0 , data.Length);   
			bs.Flush();
//			clientStream.Flush();
//			Debug.Log("send: "+data.Length);
			return length;
		}          
		return -1;
	}

//	public void ForseStopClients(){
//		foreach(TcpClient c in clients){
//			NetworkStream clientStream = c.GetStream();
//			int length=-1;
//			byte[] lengthData=getBytes(length);
//			clientStream.Write(lengthData, 0 , lengthData.Length);          
//			clientStream.Flush();
//			Debug.Log("ForseStopClients: "+length);
//		}   
//	}

	byte[] getBytes(int x) {
		return BitConverter2.getBytes(x);
	}

	public void onDestory(){
		foreach(BufferedStream bs in bStreams)
			bs.Close();
		foreach(TcpClient c in clients)
			c.Close();
		tcpListener.Stop();
		listenThread.Abort();
	}

	void OnApplicationQuit() {

	}
}