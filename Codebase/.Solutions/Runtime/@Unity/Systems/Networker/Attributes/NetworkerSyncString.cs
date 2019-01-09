#if UNITY_5_4_OR_NEWER
using UnityEngine;
namespace Zios.Unity.Networker.Attributes{
	using Zios.Attributes.Supports;
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	[AddComponentMenu("Zios/Component/Action/Network/Sync String")]
	public class NetworkerSyncString : NetworkerSync<AttributeString,string>{
		public override void Set(string value){this.receiveAttribute.Set(value);}
		public override string Read(byte[] data){return data.ReadString();}
		public override byte[] GetBytes(){return this.last.ToStringBytes().Prepend((byte)0);}
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