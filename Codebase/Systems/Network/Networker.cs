using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
namespace Zios{
	using Events;
	using Interface;
	[InitializeOnLoad]
	public static class NetworkerHook{
		static NetworkerHook(){
			if(Application.isPlaying){return;}
			new Hook<Networker>();
		}
	}
	[Flags]
	public enum NetworkerDebug : int{
		Events = 0x001,
		Errors = 0x002,
		Data   = 0x004,
	}
	public enum NetworkerMode{None,Client,Server,Listen}
	[ExecuteInEditMode][AddComponentMenu("")]
	public class Networker : MonoBehaviour{
		public static NetworkerDebug debug;
		[NonSerialized] public static NetworkerMode mode;
		public static Networker instance;
		//==============
		// Settings
		//==============
		public string clientName = "Client";
		public int maxConnections = 16;
		public int maxPacketSize = 500;
		public short maxReceived = 128;
		public short maxSent = 128;
		public string hostIP = "*";
		public string serverIP = "127.0.0.1";
		public int hostPort = 9001;
		public int serverPort = 9001;
		[Advanced] public ConnectionConfig settings;
		[Advanced] public HostTopology topology;
		[Internal] public int eventChannel;
		[Internal] public int syncChannel;
		//==============
		// Data
		//==============
		[Internal] public List<NetworkerClient> clients = new List<NetworkerClient>();
		public List<NetworkerEvent> events = new List<NetworkerEvent>();
		public List<NetworkerMessage> eventHistory = new List<NetworkerMessage>();
		private Dictionary<int,Dictionary<string,List<byte>>> syncBufferToClients = new Dictionary<int,Dictionary<string,List<byte>>>();
		private Dictionary<string,List<byte>> syncBufferToServer = new Dictionary<string,List<byte>>();
		private bool setup;
		private int socketID;
		private int connectionID;
		private int receivedID;
		private int bufferSize;
		private byte errorCode;
		//==============
		// Unity
		//==============
		public void Awake(){Networker.instance = this;}
		public void Start(){
			Console.AddKeyword("networkDisconnect",this.Disconnect);
			Console.AddKeyword("networkConnect",this.Join);
			Console.AddKeyword("networkHost",this.Host);
			Console.AddKeyword("networkHostSimulated",this.HostSimulated);
			Console.AddKeyword("networkListen",this.Listen);
			Console.AddKeyword("networkChat",this.Chat);
			Console.AddKeyword("networkShowClients",this.ShowClients);
			Console.AddKeyword("networkShowEvents",this.ShowEvents);
			Console.AddKeyword("networkShowHistory",this.ShowHistory);
			Console.AddShortcut("networkDisconnect","disconnect","leave");
			Console.AddShortcut("networkConnect","connect","join");
			Console.AddShortcut("networkChat","chat","msg","query");
			Console.AddShortcut("networkHostSimulated","hostSimulated","startServerSimulated");
			Console.AddShortcut("networkHost","host","startServer");
			Console.AddShortcut("networkListen","listen");
			Console.AddShortcut("networkShowClients","showClients","clients");
			Console.AddShortcut("networkShowEvents","showEvents","events");
			Console.AddShortcut("networkShowHistory","showHistory","history");
			Console.AddShortcut("networkClientName","clientName","name");
			Console.AddCvar("networkServerIP",this,"serverIP","Server IP");
			Console.AddCvar("networkServerPort",this,"serverPort","Server Port");
			Console.AddCvar("networkClientName",this,"clientName","Client Name");
			Networker.AddEvent("SyncData",this.SyncData);
			Networker.AddEvent("SyncClient",this.SyncClient);
			Networker.AddEvent("AddClient",this.AddClient);
			Networker.AddEvent("RemoveClient",this.RemoveClient);
			if(Application.isPlaying){NetworkTransport.Init();}
		}
		public void Update(){
			if(!this.setup){return;}
			int hostID,channelID;
			int maxSize = 1024;
			byte[] buffer = new byte[1024];
			if(this.syncBufferToServer.Count > 0){
				Networker.SendMessage("SyncData",this.syncBufferToServer.SelectMany(x=>x.Value).ToArray(),this.syncChannel,this.connectionID,-1,false);
				this.syncBufferToServer.Clear();
			}
			if(this.syncBufferToClients.Count > 0){
				foreach(var clientBuffer in this.syncBufferToClients){
					Networker.SendMessage("SyncData",clientBuffer.Value.SelectMany(x=>x.Value).ToArray(),this.syncChannel,clientBuffer.Key,-1,false);
				}
				this.syncBufferToClients.Clear();
			}
			while(true){
				var networkEvent = NetworkTransport.Receive(out hostID,out this.receivedID,out channelID,buffer,maxSize,out this.bufferSize,out this.errorCode);
				if(!this.CheckErrors()){continue;}
				if(networkEvent == NetworkEventType.Nothing){break;}
				if(networkEvent == NetworkEventType.ConnectEvent){
					if(this.connectionID == this.receivedID){
						Networker.mode = NetworkerMode.Client;
						Debug.Log("Connection successful.");
						Networker.SendEventToServer("SyncClient",this.clientName.ToStringBytes());
						return;
					}
					Debug.Log("[Server] Client connected.");
				}
				else if(networkEvent == NetworkEventType.DataEvent){
					var eventID = buffer.ReadShort();
					//Debug.Log("Network event received from -- " + this.GetClientName(this.receivedID) + " -- " + this.events[eventID].name);
					this.events[eventID].method(buffer);
				}
				else if(networkEvent == NetworkEventType.DisconnectEvent){
					if(this.connectionID == this.receivedID && this.setup){
						this.Disconnect();
						return;
					}
					Debug.Log("[Server] Client disconnected -- " + this.GetClientName(this.receivedID));
					this.clients.RemoveAll(x=>x.id==this.receivedID);
					this.eventHistory.RemoveAll(x=>x.name=="AddClient"&&x.data.ReadInt(2)==this.receivedID);
					Networker.SendEventToClient("RemoveClient",this.receivedID.ToBytes());
					this.eventHistory.RemoveAll(x=>x.name=="RemoveClient");
				}
			}
		}
		//==============
		// Global
		//==============
		public static int GetActiveID(){return Networker.instance.receivedID;}
		public static void AddEvent(string name,Action<byte[]> method){
			Networker.instance.events.Add(new NetworkerEvent(name,method));
		}
		public static void SendEventToServer(string name,byte[] data){Networker.SendMessage(name,data,Networker.instance.eventChannel,Networker.instance.connectionID,-1);}
		public static void SendEventToClient(string name,byte[] data,int clientID=-1,int excludeID=-1){Networker.SendMessage(name,data,Networker.instance.eventChannel,clientID,excludeID);}
		public static void SendDataToServer(string name,byte[] data){
			if(!Networker.instance.PrepareEvent(name,ref data)){return;}
			Networker.instance.syncBufferToServer.AddNew(name).AddRange(data);
		}
		public static void SendDataToClients(string name,byte[] data,int clientID=-1,int excludeID=-1){
			if(!Networker.instance.PrepareEvent(name,ref data)){return;}
			if(clientID == -1){
				foreach(var client in Networker.instance.clients){
					if(excludeID == client.id){continue;}
					Networker.instance.syncBufferToClients.AddNew(client.id).AddNew(name).AddRange(data);
				}
				return;
			}
			Networker.instance.syncBufferToClients.AddNew(clientID).AddNew(name).AddRange(data);
		}
		public static void SendMessage(string name,byte[] data,int channel,int clientID,int excludeID,bool useHistory=true){
			if(!Networker.instance.PrepareEvent(name,ref data)){return;}
			var message = new NetworkerMessage(name,data,channel,clientID,excludeID);
			if(useHistory && clientID == -1){Networker.instance.eventHistory.Add(message);}
			Networker.SendMessage(message);
		}
		public static void SendMessage(NetworkerMessage message,int? targetID=null){
			var network = Networker.instance;
			int clientID = targetID.IsNull() ? message.clientID : (int)targetID;
			if(clientID == -1){
				foreach(var client in network.clients){
					if(message.excludeID == client.id){continue;}
					NetworkTransport.Send(network.socketID,client.id,message.channel,message.data,1024,out network.errorCode);
				}
				return;
			}
			NetworkTransport.Send(network.socketID,clientID,message.channel,message.data,1024,out network.errorCode);
		}
		//==============
		// Events
		//==============
		public void SyncData(byte [] data){
			int index = 2;
			//int bufferSize = data.ReadShort(2);
			while(index < this.bufferSize){
				int eventID = data.ReadShort(index);
				if(eventID == 0){return;}
				int size = data[index+2];
				byte[] chunk = new byte[0];
				if(size == 0){
					chunk = data.Skip(index+3).TakeWhile(x=>x!=0).ToArray();
					size = chunk.Length;
				}
				else{
					chunk = data.Skip(index+3).Take(size).ToArray();
				}
				//Debug.Log("Event ID -- " + eventID + " -- " + size + " -- " + chunk.Length);
				//Console.AddLog("Event Sync -- " + this.events[eventID].name + " -- " + size);
				this.events[eventID].method(chunk);
				index += size+3;
			}
		}
		public void SyncClient(byte[] data){
			NetworkID network;
			NodeID node;
			string name = data.ReadString(2);
			var client = new NetworkerClient(this.receivedID,name);
			NetworkTransport.GetConnectionInfo(this.socketID,this.receivedID,out client.ip,out client.port,out network, out node,out this.errorCode);
			this.clients.Add(client);
			foreach(var history in this.eventHistory){
				Networker.SendMessage(history,this.receivedID);
			}
			Networker.SendEventToClient("AddClient",this.receivedID.ToBytes().Append(name));
			Debug.Log("[Server] Client " + this.receivedID + " identified as " + name + ".");
		}
		public void AddClient(byte[] data){
			int id = data.ReadInt(2);
			string name = data.ReadString(6);
			var client = new NetworkerClient(id,name);
			this.clients.Add(client);
			Debug.Log("Client connected -- " + name + ".");
			string eventName = id == this.connectionID ? "Network Connection Success" : "Network Client Connected";
			Event.Call(eventName,id);
		}
		public void RemoveClient(byte[] data){
			int id = data.ReadInt(2);
			var client = this.clients.Find(x=>x.id==id);
			this.clients.Remove(client);
			Debug.Log("Client disconnected -- " + client.name + ".");
			Event.Call("Network Client Disconnected",id);
		}
		//==============
		// Commands
		//==============
		public void Host(string[] values){
			NetworkID network;
			NodeID node;
			Networker.mode = NetworkerMode.Server;
			this.ParseAddress(values,ref this.hostIP,ref this.hostPort);
			this.SetupHost();
			NetworkTransport.GetConnectionInfo(this.socketID,this.socketID,out this.hostIP,out this.hostPort,out network, out node,out this.errorCode);
			Debug.Log("[Server] Server created at " + this.hostIP + ":" + this.hostPort + ".");
		}
		public void HostSimulated(string[] values){
			if(values.Length < 2){return;}
			Networker.mode = NetworkerMode.Server;
			int minLatency = values[1].ToInt();
			int maxLatency = values.Length > 2 ? values[2].ToInt() : minLatency;
			this.SetupHost(minLatency,maxLatency);
			Debug.Log("[Server] Simulated Lag Server created at " + this.hostIP + ":" + this.hostPort + ".");
		}
		public void Listen(){
			var local = "".AsArray();
			this.Host(local);
			this.Join(local);
			Networker.mode = NetworkerMode.Listen;
		}
		public void Join(string[] values){
			this.ParseAddress(values,ref this.serverIP,ref this.serverPort);
			this.SetupHost();
			this.connectionID = NetworkTransport.Connect(this.socketID,this.serverIP,this.serverPort,0,out this.errorCode);
			Debug.Log("Connecting to server at " + this.serverIP + ":" + this.serverPort + "...");
		}
		public void Disconnect(){
			if(!this.setup){
				Debug.Log("Not connected.");
				return;
			}
			NetworkTransport.Disconnect(0,this.connectionID,out this.errorCode);
			NetworkTransport.RemoveHost(this.socketID);
			/*NetworkTransport.Shutdown();
			NetworkTransport.Init();*/
			Networker.mode = NetworkerMode.None;
			this.settings.Channels.Clear();
			this.clients.Clear();
			this.setup = false;
			Debug.Log("Disconnected.");
		}
		public void Chat(string[] values){}
		public void ShowClients(){
			foreach(var client in this.clients){
				Debug.Log(client.id + " -- " + client.name);
			}
		}
		public void ShowEvents(){
			for(int index=0;index < this.events.Count;++index){
				Debug.Log(index + " -- " + this.events[index].name);
			}
		}
		public void ShowHistory(){
			foreach(var history in this.eventHistory){
				Debug.Log(history.name);
			}
		}
		//==============
		// Utility
		//==============
		public bool CheckErrors(){
			var error = (NetworkError)this.errorCode;
			if(error != NetworkError.Ok){Debug.Log("^3Network Error : " + error);}
			if(error == NetworkError.WrongHost){}
			if(error == NetworkError.WrongConnection){}
			if(error == NetworkError.WrongChannel){}
			if(error == NetworkError.NoResources){}
			if(error == NetworkError.BadMessage){}
			if(error == NetworkError.Timeout){}
			if(error == NetworkError.MessageToLong){}
			if(error == NetworkError.WrongOperation){}
			if(error == NetworkError.VersionMismatch){}
			if(error == NetworkError.DNSFailure){}
			if(error == NetworkError.CRCMismatch){}
			return true;
		}
		public string GetClientName(int id){
			var client = this.clients.Find(x=>x.id==id);
			if(!client.IsNull()){return client.name;}
			return "Client #" + id.ToString();
		}
		public bool PrepareEvent(string name,ref byte[] data){
			int id = this.events.FindIndex(x=>x.name==name);
			if(id != -1){
				byte[] eventID = id.ToShort().ToBytes();
				data = eventID.Concat(data);
				return true;
			}
			return false;
		}
		private void ParseAddress(string[] values,ref string ip,ref int port){
			if(values.Length > 1){
				ip = values[1].TrySplit(":",0);
				if(values.Contains(":")){
					port = values[1].Parse(":").ToInt();
				}
			}
		}
		private void SetupHost(int minLatency=-1,int maxLatency=-1){
			if(!this.setup){
				this.eventChannel = this.settings.AddChannel(QosType.ReliableSequenced);
				this.syncChannel = this.settings.AddChannel(QosType.StateUpdate);
				this.topology = new HostTopology(this.settings,this.maxConnections);
				if(Networker.mode == NetworkerMode.Server){
					if(minLatency > 0){
						maxLatency = maxLatency > 0 ? minLatency : maxLatency;
						this.socketID = NetworkTransport.AddHostWithSimulator(topology,minLatency,maxLatency,this.hostPort);
						return;
					}
					this.socketID = NetworkTransport.AddHost(topology,this.hostPort);
				}
				else {
					this.socketID = NetworkTransport.AddHost(topology);
				}
				this.setup = true;
			}
		}
	}
	public class NetworkerMessage{
		public string name;
		public byte[] data;
		public int channel;
		public int clientID;
		public int excludeID;
		public NetworkerMessage(string name,byte[] data,int channel,int clientID=-1,int excludeID=-1){
			this.name = name;
			this.data = data;
			this.channel = channel;
			this.clientID = clientID;
			this.excludeID = excludeID;
		}
	}
	public class NetworkerEvent{
		public string name;
		public Action<byte[]> method;
		public NetworkerEvent(string name,Action<byte[]> method){
			this.name = name;
			this.method = method;
		}
	}
	[Serializable]
	public class NetworkerClient{
		public int id;
		public int ping;
		public int port;
		public string name;
		public string ip;
		public NetworkerClient(int id,string name){
			this.id = id;
			this.name = name;
		}
	}
}