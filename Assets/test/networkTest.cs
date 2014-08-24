using UnityEngine;
using System.Collections.Generic;
using System.Net;
using System;
using System.Net.Sockets;
using System.Threading;

public class networkTest : MonoBehaviour {
	
	private TcpListener tcpListener;
	private Thread listenThread;
	List<TcpClient> clients=new List<TcpClient>();
	//Timer t;
	int length=0;
	[SerializeField]
	int fps;
	[SerializeField]
	int clientSize;
	
	System.Object obj;

	TimerTest tt;

	// Use this for initialization
	void Start () {
		this.tcpListener = new TcpListener(IPAddress.Any, 8888);
		this.listenThread = new Thread(new ThreadStart(ListenForClients));
		this.listenThread.Start();
		obj=this;
		tt=new TimerTest();
		tt.Start();
//		t=new Timer(new TimerCallback(Send2));
//		t.Change(0,1000/fps);
	}

	void Send2(object s){
		Debug.Log("s");
	}

	// Update is called once per frame
	void Update () {
		clientSize=clients.Count;
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
			HandleClientComm(client);
			break;
			Debug.Log("client");
		}
		
	}
	
	private void HandleClientComm(object client)
	{
		TcpClient tcpClient = (TcpClient)client;
		tcpClient.NoDelay=true;
		tcpClient.SendBufferSize=80000;
		clients.Add(tcpClient);

	}
	
	void Send(object s){
		lock(obj){
			for(int i=0;i<clients.Count;i++){
				TcpClient c=clients[i];
				NetworkStream clientStream = c.GetStream();
				byte[] lengthData=getBytes(length);
				clientStream.Write(lengthData, 0 , lengthData.Length);        
				clientStream.Flush();
				Debug.Log("send_"+i+": "+length);
			}
			length++;
		}
	}
	
	byte[] getBytes(int x) {
		return BitConverter2.getBytes(x);
	}
	
	public void onDestory(){
		tt.Stop();
//		t.Change(Timeout.Infinite,Timeout.Infinite);
//		t.Dispose();
		foreach(TcpClient c in clients)
			c.Close();
		tcpListener.Stop();
		listenThread.Abort();
	}
	
	void OnApplicationQuit() {
		onDestory();
	}
}
