using UnityEngine;
namespace Zios{
	[AddComponentMenu("Zios/Component/Network/Sync Vector3")]
	public class NetworkerSyncVector3 : NetworkerSync<AttributeVector3,Vector3>{
		public override void Set(Vector3 value){this.receiveAttribute.Set(value);}
		public override Vector3 Read(byte[] data){return data.ReadVector3();}
		public override byte[] GetBytes(){return this.last.ToBytes().Prepend((byte)12);}
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