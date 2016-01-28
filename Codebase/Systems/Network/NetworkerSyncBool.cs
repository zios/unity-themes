using UnityEngine;
namespace Zios{
	[AddComponentMenu("Zios/Component/Network/Sync Bool")]
	public class NetworkSyncBool : NetworkerSync<AttributeBool,bool>{
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