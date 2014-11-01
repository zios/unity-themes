using Zios;
using System;
using UnityEngine;
namespace Zios{
	public enum AttributeMode{Normal,Linked,Formula};
	public enum AttributeUsage{Direct,Shaped};
	public enum AttributeNumeralOperator{Addition,Subtraction,Multiplication,Division,Average,Max,Min}
	public enum AttributeVector3Operator{Addition,Subtraction,Multiplication,Average,Max,Min}
	public enum AttributeStringOperator{Prefix,Suffix}
	public enum AttributeBoolOperator{And,Or}
	public enum AttributeNumeralSpecial{Copy,Flip,Abs,Sign,Floor,Ceil,Cos,Sin,Tan,ATan,Sqrt};
	public enum AttributeVector3Special{Copy,Flip,Abs,Sign};
	public enum AttributeStringSpecial{Copy,Lower,Upper,Capitalize};
	public enum AttributeBoolSpecial{Copy,Flip};
	[Serializable]
	public class Attribute{
		public string path;
		public MonoBehaviour script;
		public AttributeMode mode = AttributeMode.Normal;
	}
	[Serializable]
	public class AttributeData{
		public Target target = new Target();
		public AttributeUsage usage;
	}
	[Serializable]
	public class AttributeData<Type,AttributeType> : AttributeData{
		public AttributeType reference;
		public Type value;
	}
	[Serializable]
	public class AttributeFloatData : AttributeData<float,AttributeFloat>{
		public AttributeNumeralOperator sign;
		public AttributeNumeralSpecial special;
		public bool clamp;
		public AttributeFloat clampMin;
		public AttributeFloat clampMax;
	}
	[Serializable]
	public class AttributeIntData : AttributeData<int,AttributeInt>{
		public AttributeNumeralOperator sign;
		public AttributeNumeralSpecial special;
		public bool clamp;
		public AttributeInt clampMin;
		public AttributeInt clampMax;
	}
	[Serializable]
	public class AttributeVector3Data : AttributeData<Vector3,AttributeVector3>{
		public AttributeVector3Operator sign;
		public AttributeVector3Special special;
		public bool[] clamp = new bool[3]{false,false,false};
		public AttributeVector3 clampMin;
		public AttributeVector3 clampMax;
	}
	[Serializable]
	public class AttributeStringData : AttributeData<string,AttributeString>{
		public AttributeStringOperator sign;
		public AttributeStringSpecial special;
		public int characterLimit;
		public string[] allowed = new string[0];
		public string[] disallowed = new string[0];
	}
	[Serializable]
	public class AttributeBoolData : AttributeData<bool,AttributeBool>{
		public AttributeBoolOperator sign;
		public AttributeBoolSpecial special;
	}
}