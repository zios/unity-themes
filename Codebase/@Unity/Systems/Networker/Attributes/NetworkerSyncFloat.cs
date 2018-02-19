#if UNITY_5_4_OR_NEWER
using UnityEngine;
namespace Zios.Unity.Networker.Attributes{
	using Zios.Attributes.Supports;
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	[AddComponentMenu("Zios/Component/Action/Network/Sync Float")]
	public class NetworkerSyncFloat : NetworkerSync<AttributeFloat,float>{
		public override void Set(float value){this.receiveAttribute.Set(value);}
		public override float Read(byte[] data){return data.ReadFloat();}
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
#endif