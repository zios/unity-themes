#if UNITY_5_4_OR_NEWER
using System;
using UnityEngine;
namespace Zios.Unity.Networker.Attributes{
	using Zios.Attributes.Supports;
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	using Zios.State;
	using Zios.SystemAttributes;
	using Zios.Unity.Networker;
	using Zios.Unity.Proxy;
	using Zios.Unity.SystemAttributes;
	//asm Zios.Unity.Components.DataBehaviour;
	//asm Zios.Unity.Components.ManagedBehaviour;
	//asm Zios.Unity.Supports.Singleton;
	[AddComponentMenu("")]
	public class NetworkerSync<AttributeType,DataType> : NetworkSync
	where AttributeType : Attribute,new(){
		[EnumMask] public ClientSyncType clientSync;
		[EnumMask] public ServerSyncType serverSync;
		[Advanced] public SyncUpdateRate syncUpdateRate;
		[Advanced][EnumMask] public ServerSyncOptions serverSyncOptions;
		public AttributeType sendAttribute = new AttributeType();
		[Advanced] public AttributeType receiveAttribute = new AttributeType();
		[NonSerialized] public DataType last;
		[Advanced] public bool onlySendIfChanged;
		private string eventName;
		private bool needsUpdate;
		public override void Awake(){
			this.alias = this.alias.SetDefault("Sync Attribute");
			base.Awake();
			this.sendAttribute.Setup("Send Value From",this);
			this.sendAttribute.info.mode = AttributeMode.Linked;
			this.sendAttribute.locked = true;
			this.receiveAttribute.locked = true;
			this.receiveAttribute.Setup("Receive Value Into",this);
			if(this.receiveAttribute.info.mode != AttributeMode.Linked){
				this.receiveAttribute.data[0].referenceID = this.sendAttribute.info.id;
				this.receiveAttribute.info.mode = AttributeMode.Linked;
			}
		}
		public override void Start(){
			base.Start();
			if(Proxy.IsPlaying()){
				this.eventName = "Sync-"+this.path;
				Networker.AddEvent(this.eventName,this.SyncReceive);
			}
		}
		public override void Step(){
			base.Step();
			if(this.needsUpdate && this.syncUpdateRate == SyncUpdateRate.UpdateAtRate && this.VerifyReceive()){
				this.SyncUpdate();
			}
			this.SyncSend();
		}
		public void SyncSend(){
			if(Networker.mode == NetworkerMode.Client){
				if(this.clientSync != ClientSyncType.SendToServer){return;}
				if(!this.HasChanged() && this.onlySendIfChanged){return;}
				Networker.SendDataToServer(this.eventName,this.GetBytes());
			}
			if(Networker.mode == NetworkerMode.Server){
				if(this.serverSync != ServerSyncType.SendToClients){return;}
				if(!this.HasChanged() && this.onlySendIfChanged){return;}
				Networker.SendDataToClients(this.eventName,this.GetBytes());
			}
		}
		public void SyncReceive(byte[] data){
			if(!this.VerifyReceive()){return;}
			this.last = this.Read(data);
			this.needsUpdate = true;
			if(this.syncUpdateRate == SyncUpdateRate.UpdateImmediate){this.SyncUpdate();}
			if(Networker.mode == NetworkerMode.Client){}
			if(Networker.mode == NetworkerMode.Server){
				if(this.serverSyncOptions.Has("SendUponReceiving")){
					int clientID = this.serverSyncOptions.Has("OnlyOwner") ? Networker.GetActiveID() : -1;
					int excludeID = this.serverSyncOptions.Has("ExcludeOwner") ? Networker.GetActiveID() : -1;
					Networker.SendDataToClients(this.eventName,this.GetBytes(),clientID,excludeID);
				}
			}
		}
		public void SyncUpdate(){
			this.Set(this.last);
			this.needsUpdate = false;
		}
		public bool VerifyReceive(){
			bool legalClientUpdate = Networker.mode == NetworkerMode.Client && this.clientSync.Has("ReceiveFromServer");
			bool legalServerUpdate = Networker.mode == NetworkerMode.Server && this.serverSync.Has("ReceiveFromClients");
			return legalClientUpdate || legalServerUpdate;
		}
		public virtual void Set(DataType value){}
		public virtual DataType Read(byte[] data){return default(DataType);}
		public virtual byte[] GetBytes(){return new byte[0];}
		public virtual bool HasChanged(){return false;}
	}
	public enum SyncUpdateRate{UpdateAtRate,UpdateImmediate}
	[Flags] public enum ServerSyncOptions{ExcludeOwner=1,OnlyOwner=2,SendUponReceiving=4}
	[Flags] public enum ClientSyncType{SendToServer=1,ReceiveFromServer=2}
	[Flags] public enum ServerSyncType{SendToClients=1,ReceiveFromClients=2}
	[AddComponentMenu("")] public class NetworkSync : StateBehaviour{}
}
#endif