using UnityEngine;
namespace Zios{
	[AddComponentMenu("Zios/Component/Network/Sync Int")]
	public class NetworkerSyncInt : NetworkerSync<AttributeInt,int>{
		public override void Set(int value){this.receiveAttribute.Set(value);}
		public override int Read(byte[] data){return data.ReadInt();}
		public override byte[] GetBytes(){return this.last.ToBytes().Prepend((byte)4);}
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