using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
public struct NetworkEvents{
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
	public int latency;
	public int lastUpdate;
	public string label;
	public NetworkEntity entity;
	public NetworkPlayer networkPlayer;
	//public Dictionary<short,NetworkEntity> entityStates;
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
	public void OnEnable(){
		this.connection = this.GetComponent<NetworkView>();
		if(Network.isClient ||  Network.isServer){
			Network.SetSendingEnabled(0,true);
			Network.isMessageQueueRunning = true;
			if(Network.isClient || (Network.isServer && this.listen)){
				this.PrepareClient();
			}
		}
	}
	public void Start(){
		Zios.Console.AddKeyword("connect",this.Connect);
		Zios.Console.AddKeyword("listen",this.StartServer);
		Zios.Console.AddKeyword("say",this.SendMessage);
		Zios.Console.AddShortcut("join","connect");
		Zios.Console.AddShortcut("name","nick");
		Zios.Console.AddShortcut("alias","nick");
		Zios.Console.AddCvar("nick",this,"clientName","Player Name",this.help[6]);
		Zios.Console.AddCvar("port",this,"port","Network Port",this.help[2]);
		Zios.Console.AddCvar("password",this,"password","Network Password",this.help[3]);
		Zios.Console.AddCvar("maxUsers",this,"maxUsers","Maximum Users",this.help[4]);
		if(Network.isClient || Network.isServer){Debug.Log("Level has been loaded -- " + Application.loadedLevelName);}
	}
	public void FixedUpdate(){
		if(Network.isClient ||  Network.isServer){
			Network.SetSendingEnabled(0,true);
			Network.isMessageQueueRunning = true;
			if(Network.isClient || (Network.isServer && this.listen)){
				this.PrepareClient();
			}
		}
	}
	// =========================
	// Unity-Derived
	// =========================
	public void OnPlayerConnected(NetworkPlayer player){
		string info = "^2|" + player.externalIP + ":^9|" + player.externalPort + "^10";
		Debug.Log("^10Client ["+info+"] has connected to the server.");
		this.SendRPC("StringEvent",player,NetworkEvents.LoadScene,this.sceneName);
		this.LocalizeClient(player);
		foreach(NetworkEntity entity in this.entities){
			Vector3 position = entity.gameObject.transform.position;
			this.SendRPC("HandleEvent",player,NetworkEvents.AddEntity,entity.gameObject.name,position);
		}
	}
	public void OnConnectedToServer(){
		Debug.Log("^10Connection to the server successfully established.");
	}
	public void OnPlayerDisconnected(NetworkPlayer player){
		string info = "^2|" + player.externalIP + ":^9|" + player.externalPort + "^10";
		Debug.Log("^10Client [^2|"+ info + "^10] has disconnected from the server.");
		if(Network.isServer){
			Client client = this.FindClient(player);
			if(client != null){
				this.SendRPC("HandleEvent",RPCMode.All,NetworkEvents.RemoveClient,client.id+"", Vector3.zero);
				if(this.debug){Debug.Log("^10Client ["+info+"] was removed.");}
				List<NetworkEntity> removedEntities = this.entities.FindAll(entity => entity.ownerID == client.entity.ownerID);
				foreach(NetworkEntity entity in removedEntities){
					this.SendRPC("HandleEvent",RPCMode.All,NetworkEvents.RemoveEntity,entity.id+"", Vector3.zero);
				}
				if(this.debug){Debug.Log("^10Client ["+info+"] entities were removed.");}
			}
		}
	}
	public void OnDisconnectedFromServer(NetworkDisconnection info){
		if(Network.isServer){return;}
		this.StringEvent(NetworkEvents.LoadScene,this.sceneName);
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
	public void SendMessage(string[] values){
		values = values.Skip(1).ToArray();
		string message = this.clientName + "^6:^10" + string.Join(" ",values);
		this.SendRPC("StringEvent",RPCMode.All,NetworkEvents.SendMessage,message);
	}
	public void Connect(string[] values){
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

	public void StartServer(string[] values){
		this.activeClient = null;
		this.clients.RemoveAll(client => true);
		this.entities.RemoveAll(client => true);
		int port = this.port;
		string sceneName = Application.loadedLevelName;
		if(values.Length > 1){
			sceneName = values[1];
		}
		Debug.Log("^10Establishing [^3|" + this.maxUsers + "^10] user listen server on port ^9|" + port + "^10 ...");
		this.sceneName = sceneName;
		int sceneID = Zios.Scene.GetMapID(this.sceneName);
		Network.SetLevelPrefix(sceneID);
		Network.incomingPassword = this.password;
		Network.InitializeServer(this.maxUsers,port,true);
		this.SendRPC("StringEvent",RPCMode.All,NetworkEvents.LoadScene,this.sceneName);
	}
	public NetworkEntity FindEntity(int id){
		foreach(NetworkEntity entity in this.entities){
			if(entity.id == id){ return entity;	}
		}
		return null;
	}
	public Client FindClient(int id){
		foreach(Client client in this.clients){
			if(client.id == id){return client;}
		}
		return null;
	}
	public Client FindClient(NetworkPlayer player){
		foreach(Client client in this.clients){
			if(client.networkPlayer == player){return client;}
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
			clientSend.connection = (NetworkView)clientObject.AddComponent<NetworkView>();
			clientSend.connection.observed = clientSend;
			clientSend.connection.viewID = clientViewID;
			clientSend.parent = this.activeClient;
			if(this.debug){Debug.Log("Preparing a new client");}
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
		client.receive.connection = (NetworkView)client.gameObject.AddComponent<NetworkView>();
		client.receive.connection.observed = client.receive;
		client.receive.connection.viewID = serverViewID;
		client.receive.connection.group = clientID;
	}
	[RPC]
	public void SetupClient(NetworkPlayer player,NetworkViewID clientViewID,string name){
		NetworkViewID serverViewID = Network.AllocateViewID();
		GameObject clientObject = new GameObject("ServerClient");
		Client client = clientObject.AddComponent<Client>();
		//client.port = player.externalPort;
		//client.ip = player.externalIP;
		client.id = this.nextClientID;
		client.label = name;
		client.networkPlayer = player;
		ClientReceive clientReceive = client.receive = clientObject.AddComponent<ClientReceive>();
		clientReceive.connection = (NetworkView)clientObject.AddComponent<NetworkView>();
		clientReceive.connection.observed = clientReceive;
		clientReceive.connection.viewID = clientViewID;
		clientReceive.connection.group = client.id;
		clientReceive.parent = client;
		ClientSend clientSend = client.send = clientObject.AddComponent<ClientSend>();
		clientSend.connection = (NetworkView)clientObject.AddComponent<NetworkView>();
		clientSend.connection.observed = clientSend;
		clientSend.connection.viewID = serverViewID;
		clientSend.connection.group = client.id;
		clientSend.parent = client;
		this.clients.Add(client);
		this.LocalizeClients(client);
		this.nextClientID += 1;
		this.SendRPC("FinalizeClient",player,serverViewID,client.id);
		//this.SendRPC("HandleEvent",RPCMode.Others,NetworkEvents.AddClient,info);
		if(this.clientPrefab){
			Vector3 spawnPosition = new Vector3(-300.77f,-101.98f,185.24f);
			string entityData = this.clientPrefab.name+"-"+this.nextEntityID+"-"+client.id+"-"+client.label;
			this.SendRPC("HandleEvent",RPCMode.All,NetworkEvents.AddEntity,entityData,spawnPosition);
			this.nextEntityID += 1;
		}
	}
	[RPC]
	public void HandleEvent(int eventID,string text,Vector3 position){
		if(this.debug){
			Debug.Log("Event " + eventID + " : " + text + " : " + position);
		}
		if(eventID == NetworkEvents.LoadScene){
			Debug.Log("Loading level...");
			this.sceneName = text;
			int sceneID = Zios.Scene.GetMapID(text);
			Network.SetSendingEnabled(0,false);	
			Network.isMessageQueueRunning = false;
			Network.SetLevelPrefix(sceneID);
			Application.LoadLevel(text);
		}
		else if(eventID == NetworkEvents.SyncScene){}
		else if(eventID == NetworkEvents.AddEntity){
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
			if(this.clientPrefab == entityPrefab){
				Client client = this.FindClient(entity.ownerID);
				if(Network.isServer){
					client.entity = entity;
				}
				if((Network.isServer && (client.entity != null && this.activeClient.id == client.id)) || (Network.isClient && this.activeClient.id == entity.ownerID)){
					//Watch watchComponent = GameObject.Find("Camera").GetComponent<Watch>();
					//watchComponent.target = entity.gameObject.transform;
					//Follow followComponent = GameObject.Find("Camera").GetComponent<Follow>();
					//followComponent.target = entity.gameObject.transform;
				}
			}
			this.entities.Add(entity);
		}
		else if(eventID == NetworkEvents.AddClient){}
		else if(eventID == NetworkEvents.RemoveEntity){
			Debug.Log("Removing NetworkEntity...");
			NetworkEntity entity = this.FindEntity(Convert.ToInt32(text));
			if(entity != null){
				GameObject.Destroy(entity.gameObject);
				this.entities.Remove(entity);
			}
		}
		else if(eventID == NetworkEvents.RemoveClient){
			Debug.Log("Removing Client...");
			Client client = this.FindClient(Convert.ToInt32(text));
			if(client != null){
				this.clients.Remove(client);
				GameObject.Destroy(client.gameObject);
			}
		}
		else if(eventID == NetworkEvents.SendMessage){
			Debug.Log(text);
		}
	}
	[RPC]
	public void StringEvent(int eventID,string data){
		this.HandleEvent(eventID,data,Vector3.zero);
	}
}