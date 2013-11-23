using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
public struct Events{
	public const int LoadScene = 0;
	public const int SyncScene = 1;
	public const int AddEntity = 2;
	public const int AddClient = 3;
	public const int RemoveEntity = 4;
	public const int RemoveClient = 5;
	public const int SendMessage = 6;
}
public class Client : MonoBehaviour{
	public int id;
	public int port;
	public int latency;
	public int lastUpdate;
	public string ip;
	public string label;
	public NetworkEntity entity;
	public NetworkPlayer networkPlayer;
	public Dictionary<short,NetworkEntity> entityStates;
	public ClientReceive receive;
	public ClientSend send;
}
public class ClientReceive : MonoBehaviour{
	public Client parent;
	public NetworkView connection;
}
public class ClientSend : MonoBehaviour{
	public Client parent;
	public NetworkView connection;
}
[Serializable]
public class NetworkEntity{
	public int id;
	public int ownerID;
	public string label;
	public GameObject gameObject;
	public void SendUpdates(){}
	public void ReceiveUpdates(){}
}
[AddComponentMenu("Zios/Singleton/Network")]
public class NetworkManager : MonoBehaviour{
	public string sceneName;
	public int maxUsers = 32;
	public int port = 65021;
	public string password = "";
	public string clientName = "Player";
	public bool listen = true;
	public bool debug = true;
	[NonSerialized] public Client activeClient;
	[NonSerialized] public NetworkView connection;
	private int nextClientID = 1;
	private int nextEntityID = 1;
	public GameObject clientPrefab;
	public List<GameObject> entityPrefabs;
	public List<Client> clients = new List<Client>();
	public List<NetworkEntity> entities = new List<NetworkEntity>();
	public List<NetworkEntity> localEntities = new List<NetworkEntity>();
	private string[] help = new string[]{
		"^3connect ^9<^7address:port^9> :^10 The ip address/domain and optional port to attempt a network connection to.",
		"^3listen ^9<^7map^9> :^10 Places the game in server mode (on an optional map) and waits for incoming connections.",
		"^3port ^9<^7number^9> :^10 The port used by default for both incoming/outgoing network connections.",
		"^3password ^9<^7string^9> :^10 The required password needed by users to connect when in server mode.",
		"^3maxUsers ^9<^7number^9> :^10 The maximum number of network connections allowed when in server mode.",
		"^3say ^9<^7text^9> :^10 Sends a message to the server to be displayed to all clients.",
		"^3nick ^9<^7text^9> :^10 Changes the alias used in network communications.",
	};
	public void Awake(){
		Global.Network = this;
		DontDestroyOnLoad(this.gameObject);
	}
	public void OnEnable(){
		if(Network.isClient ||  Network.isServer){
			Network.SetSendingEnabled(0,true);
			Network.isMessageQueueRunning = true;
			if(Network.isClient || (Network.isServer && this.listen)){
				this.PrepareClient();
			}
		}
	}
	public void Start(){
		this.connection = this.networkView;
		if(Network.isClient || Network.isServer){Debug.Log("Level has been loaded -- " + Application.loadedLevelName);}
		Global.Console.AddKeyword("connect",this.Connect);
		Global.Console.AddKeyword("listen",this.StartServer);
		Global.Console.AddKeyword("say",this.SendMessage);
		Global.Console.AddShortcut("join","connect");
		Global.Console.AddShortcut("name","nick");
		Global.Console.AddShortcut("alias","nick");
		Global.Console.AddCvar("nick",this,"clientName","Player Name",this.help[6]);
		Global.Console.AddCvar("port",this,"port","Network Port",this.help[2]);
		Global.Console.AddCvar("password",this,"password","Network Password",this.help[3]);
		Global.Console.AddCvar("maxUsers",this,"maxUsers","Maximum Users",this.help[4]);
	}
	// =========================
	// Unity-Derived
	// =========================
	public void OnPlayerConnected(NetworkPlayer player){
		string info = "^2|" + player.externalIP + ":^9|" + player.externalPort + "^10";
		Debug.Log("^10Client ["+info+"] has connected to the server.");
		this.SendRPC("StringEvent",player,Events.LoadScene,this.sceneName);
		this.LocalizeClient(player);
		foreach(NetworkEntity entity in this.entities){
			Vector3 position = entity.gameObject.transform.position;
			this.SendRPC("HandleEvent",player,Events.AddEntity,entity.gameObject.name,position);
		}
	}
	public void OnConnectedToServer(){
		Debug.Log("^10Connection to the server successfully established.");
	}
	public void OnPlayerDisconnected(NetworkPlayer player){
		string info = "^2|" + player.externalIP + ":^9|" + player.externalPort + "^10";
		Debug.Log("^10Client [^2|"+ info + "^10] has disconnected from the server.");
		//this.RemoveEntity
	}
	public void OnDisconnectedFromServer(NetworkDisconnection info){
		if(Network.isServer){return;}
		if(info == NetworkDisconnection.LostConnection){
			Debug.Log("^10Connection to the server has been lost.  Attempting reconnect ...");
			return;
		}
		Debug.Log("^10The server has been closed.  Disconnected.");
	}
	public void OnFailedToConnect(NetworkConnectionError error){
		Debug.Log("^10Unable to connect to server : ^1" + error);
	}
	public void OnServerInitialized(){
		Debug.Log("^10Server successfully started.");
	}
	// =========================
	// Custom
	// =========================
	public void SendMessage(string[] values,bool help){
		if(help || values.Length < 2){
			Debug.Log(this.help[5]);
			return;
		}
		values = values.Skip(1).ToArray();
		string message = this.clientName + "^6:^10" + string.Join(" ",values);
		this.SendRPC("StringEvent",RPCMode.All,Events.SendMessage,message);
	}
	public void Connect(string[] values,bool help){
		if(help){
			Debug.Log(this.help[0]);
			return;
		}
		string[] data = values[1].Split(':');
		string address = data[0];
		int port = data.Length > 1 ? Convert.ToInt32(data[1]) : this.port;
		if(address.Length < 2 || address.IndexOf(".") == -1){
			Debug.Log("^1No connection address specified.");
			return;
		}
		Debug.Log("^10Attempting connection to : ^2|" + address + "^9:" + port + "^10 ...");
		Network.Connect(address,port);
	}

	public void StartServer(string[] values,bool help){
		if(help){
			Debug.Log(this.help[1]);
			return;
		}
		int port = this.port;
		string sceneName = Application.loadedLevelName;
		if(values.Length > 1){
			sceneName = values[1];
		}
		Debug.Log("^10Establishing [^3|" + this.maxUsers + "^10] user listen server on port ^9|" + port + "^10 ...");
		this.sceneName = sceneName;
		int sceneID = Global.Scene.GetMapID(this.sceneName);
		Network.SetLevelPrefix(sceneID);
		Network.incomingPassword = this.password;
		Network.InitializeServer(this.maxUsers,port,true);
		this.SendRPC("StringEvent",RPCMode.All,Events.LoadScene,this.sceneName);
	}
	public Client FindClient(int id){
		foreach(Client client in this.clients){
			if(client.id == id){return client;}
		}
		return null;
	}
	public GameObject FindPrefab(string name){
		foreach(GameObject prefab in this.entityPrefabs){
			if(prefab.name == name){
				return prefab;
			}
		}
		return null;
	}
	public void PrepareClient(){
		if(this.activeClient == null){
			NetworkViewID clientViewID = Network.AllocateViewID();
			GameObject clientObject = new GameObject("Client");
			this.activeClient = clientObject.AddComponent<Client>();
			this.activeClient.networkPlayer = Network.player;	
			ClientSend clientSend = this.activeClient.send = clientObject.AddComponent<ClientSend>();
			ClientReceive clientReceive = this.activeClient.receive = clientObject.AddComponent<ClientReceive>();
			clientReceive.parent = this.activeClient;
			clientSend.connection = (NetworkView)clientObject.AddComponent("NetworkView");
			clientSend.connection.observed = clientSend;
			clientSend.connection.viewID = clientViewID;
			clientSend.parent = this.activeClient;
			this.SendRPC("SetupClient",RPCMode.Server,Network.player,clientViewID,this.clientName);
		}
	}
	public void SendRPC(string name,NetworkPlayer player,params object[] options){
		if(this.debug){Debug.Log("Sending RPC[^2" + name + "^7] to " + player + " on " + this.connection);}
		if(Network.isServer && this.listen && player == this.activeClient.networkPlayer){
			typeof(NetworkManager).GetMethod(name).Invoke(this,options);
			return;
		}
		this.connection.RPC(name,player,options);
	}
	public void SendRPC(string name,RPCMode mode,params object[] options){
		if(this.debug){Debug.Log("Sending RPC..." + name + " on " + this.connection);}
		if(Network.isServer && this.listen && mode == RPCMode.Server){
			typeof(NetworkManager).GetMethod(name).Invoke(this,options);
			return;
		}
		this.connection.RPC(name,mode,options);
	}
	public void LocalizeClient(NetworkPlayer player){
		foreach(Client client in this.clients){
			Network.SetSendingEnabled(player,client.id,false);
			Network.SetReceivingEnabled(player,client.id,false);
		}
	}
	public void LocalizeClients(Client owner){
		foreach(Client client in this.clients){
			if(client.networkPlayer.ToString() == "0"){continue;}
			bool visible = client.id == owner.id;
			Network.SetSendingEnabled(client.networkPlayer,owner.id,visible);
			Network.SetReceivingEnabled(client.networkPlayer,owner.id,visible);
		}
	}
	[RPC]
	public void FinalizeClient(NetworkViewID serverViewID,int clientID){
		Client client = this.activeClient;
		client.id = clientID;
		client.send.connection.group = clientID;
		client.receive.connection = (NetworkView)client.gameObject.AddComponent("NetworkView");
		client.receive.connection.observed = client.receive;
		client.receive.connection.viewID = serverViewID;
		client.receive.connection.group = clientID;
	}
	[RPC]
	public void SetupClient(NetworkPlayer player,NetworkViewID clientViewID,string name){
		NetworkViewID serverViewID = Network.AllocateViewID();
		GameObject clientObject = new GameObject("ServerClient");
		Client client = clientObject.AddComponent<Client>();
		client.port = player.externalPort;
		client.ip = player.externalIP;
		client.id = this.nextClientID;
		client.label = name;
		client.networkPlayer = player;
		ClientReceive clientReceive = client.receive = clientObject.AddComponent<ClientReceive>();
		clientReceive.connection = (NetworkView)clientObject.AddComponent("NetworkView");
		clientReceive.connection.observed = clientReceive;
		clientReceive.connection.viewID = clientViewID;
		clientReceive.connection.group = client.id;
		clientReceive.parent = client;
		ClientSend clientSend = client.send = clientObject.AddComponent<ClientSend>();
		clientSend.connection = (NetworkView)clientObject.AddComponent("NetworkView");
		clientSend.connection.observed = clientSend;
		clientSend.connection.viewID = serverViewID;
		clientSend.connection.group = client.id;
		clientSend.parent = client;
		this.clients.Add(client);
		this.LocalizeClients(client);
		this.nextClientID += 1;
		this.SendRPC("FinalizeClient",player,serverViewID,client.id);
		//this.SendRPC("HandleEvent",RPCMode.Others,Events.AddClient,info);
		if(this.clientPrefab){
			Vector3 spawnPosition = new Vector3(400.0f,85.0f,0);
			string entityData = this.clientPrefab.name+"-"+this.nextEntityID+"-"+client.id+"-"+client.label;
			this.SendRPC("HandleEvent",RPCMode.All,Events.AddEntity,entityData,spawnPosition);
			this.nextEntityID += 1;
		}
	}
	[RPC]
	public void HandleEvent(int eventID,string text,Vector3 position){
		if(this.debug){
			Debug.Log("Event " + eventID + " : " + text + " : " + position);
		}
		if(eventID == Events.LoadScene){
			Debug.Log("Loading level...");
			int sceneID = Global.Scene.GetMapID(text);
			Network.SetSendingEnabled(0,false);	
			Network.isMessageQueueRunning = false;
			Network.SetLevelPrefix(sceneID);
			Application.LoadLevel(text);
		}
		else if(eventID == Events.SyncScene){}
		else if(eventID == Events.AddEntity){
			Debug.Log("Adding NetworkEntity...");
			string[] data = text.Split('-');
			GameObject entityPrefab = this.FindPrefab(data[0]);
			NetworkEntity entity = new NetworkEntity();
			entity.id = Convert.ToInt32(data[1]);
			entity.label = data.Length > 3 ? data[3] : "";
			entity.ownerID = Convert.ToInt32(data[2]);
			entity.gameObject = (GameObject)Instantiate(entityPrefab);
			entity.gameObject.name = text;
			entity.gameObject.transform.position = position;
			//MovableEntity script = (MovableEntity)entity.gameObject.GetComponent(data[0]);
			//script.networkEntity = entity;
			if(Network.isServer && this.clientPrefab == entityPrefab){
				Client client = this.FindClient(entity.ownerID);
				client.entity = entity;
			}
			this.entities.Add(entity);
		}
		else if(eventID == Events.AddClient){}
		else if(eventID == Events.RemoveEntity){}
		else if(eventID == Events.RemoveClient){}
		else if(eventID == Events.SendMessage){
			Debug.Log(text);
		}
	}
	[RPC]
	public void StringEvent(int eventID,string data){
		this.HandleEvent(eventID,data,Vector3.zero);
	}
}
