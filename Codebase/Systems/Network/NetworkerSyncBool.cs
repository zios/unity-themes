using UnityEngine;
namespace Zios.Actions.NetworkComponents{
	using Attributes;
	[AddComponentMenu("Zios/Component/Action/Network/Sync Bool")]
	public class NetworkerSyncBool : NetworkerSync<AttributeBool,bool>{
		public override void Set(bool value){this.receiveAttribute.Set(value);}
		public override bool Read(byte[] data){return data.ReadBool();}
		public override byte[] GetBytes(){return this.last.ToBytes().Prepend((byte)1);}
		public override bool HasChanged(){
			var value = this.sendAttribute.Get();
			if(value != this.last){
				this.last = value;
				return true;
			}
			return false;
		}
	}
}