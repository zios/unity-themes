using UnityEngine;
namespace Zios{
	[AddComponentMenu("Zios/Component/Network/Sync String")]
	public class NetworkerSyncString : NetworkerSync<AttributeString,string>{
		public override void Set(string value){this.receiveAttribute.Set(value);}
		public override string Read(byte[] data){return data.ReadString();}
		public override byte[] GetBytes(){return this.last.ToBytes().Prepend((byte)0);}
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