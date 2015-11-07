#pragma warning disable 0618
using System;
using System.Text;
using UnityEngine;
namespace Zios{
	[Serializable][AddComponentMenu("")]
	public class OldAttributeData : DataMonoBehaviour{
		public Target target = new Target();
		public AttributeUsage usage;
		public string referenceID;
		public string referencePath;
		public int operation;
		public int special;
		public string rawValue = "";
		public string rawType;
		[NonSerialized] public AttributeInfo attribute;
		[NonSerialized] public Attribute reference;
		public virtual bool CanCache(){return true;}
		public virtual void Serialize(){}
		public virtual AttributeData Convert(){return null;}
	}
}