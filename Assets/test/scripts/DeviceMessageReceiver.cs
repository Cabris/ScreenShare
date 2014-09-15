using UnityEngine;
using System.Collections.Generic;
using System.Net;
using System;
using System.Net.Sockets;
using System.Threading;
using System.IO;

public class DeviceMessageReceiver : MonoBehaviour {
	
	private TcpListener tcpListener;
	private Thread listenThread;
	List<TcpClient> clients=new List<TcpClient>();
	public delegate void OnClientMessage(string msg);
	public OnClientMessage onClientMessage;
	[SerializeField]
	Vector4 ort;
	//Quaternion inv=Quaternion.identity;
	public Transform test;
	ConcurrentStack<Vector4> orientationStack=new ConcurrentStack<Vector4>();
	
	void Start () {
		this.tcpListener = new TcpListener(IPAddress.Any, 8887);
		this.listenThread = new Thread(new ThreadStart(ListenForClients));
		this.listenThread.Start();
		onClientMessage+=clientMsg;
	}
	
	
	// Update is called once per frame
	void Update () {
		while(orientationStack.Count>0){
			ort=orientationStack.Pop();
			Debug.Log(ort);
			Quaternion q=new Quaternion(ort.x,
			                            ort.y,
			                            ort.z,
			                            ort.w);	
			test.localRotation=q;
		}
		if(Input.GetKeyDown(KeyCode.R)){
			Quaternion inv=Quaternion.Inverse(test.localRotation);
			test.localRotation*=inv;
		}
	}
	
	void clientMsg(string msg){
		string[] values= msg.Split(',');
		float w=Convert.ToSingle( values[0]);
		float x=Convert.ToSingle( values[1]);
		float y=Convert.ToSingle( values[2]);
		float z=Convert.ToSingle( values[3]);
		Vector4 orientation=new Vector4(x,y,z,w);
		orientationStack.Push(orientation);
	}
	
	void ListenForClients()
	{
		Debug.Log("DeviceMessageReceiver ListenForClients");
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
			Debug.Log("DeviceMessageReceiver client:"+clients.Count);
			ParameterizedThreadStart tStart=new ParameterizedThreadStart(HandleClientComm);
			//Thread clientThread=new Thread(tStart);
			//clientThread.Start(client);
			HandleClientComm(client);
			break;
			Debug.Log(" DeviceMessageReceiver client end");
		}
		
	}
	
	private void HandleClientComm(object client)
	{
		TcpClient tcpClient = (TcpClient)client;
		tcpClient.NoDelay=true;
		//tcpClient.SendBufferSize=60000;
		clients.Add(tcpClient);
		StreamReader reader=new StreamReader(tcpClient.GetStream());
		try{
			while(true){
				string msg= reader.ReadLine();
				if(msg!=null){
					if(onClientMessage!=null)
						onClientMessage(msg);
				}
			}
		}catch(Exception e){
			Debug.LogException(e);
		}
		
	}
	
	public void onDestory(){
		foreach(TcpClient c in clients)
			c.Close();
		tcpListener.Stop();
		listenThread.Abort();
	}
	
	void OnApplicationQuit() {
		onDestory();	
	}
}
