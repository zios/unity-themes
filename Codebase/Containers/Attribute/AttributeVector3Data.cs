using UnityEngine;
namespace Zios.Attributes{
	[AddComponentMenu("")]
	public class AttributeVector3Data : AttributeData<Vector3,AttributeVector3,AttributeVector3Data>{
		public override Vector3 HandleSpecial(){
			Vector3 value = this.value;
			string special = AttributeVector3.specialList[this.special];
			if(this.attribute.mode == AttributeMode.Linked){return value;}
			else if(special == "Flip"){return value * -1;}
			else if(special == "Abs"){return value.Abs();}
			else if(special == "Sign"){return value.Sign();}
			else if(special == "Floor"){
				float x = Mathf.Floor(value.x);
				float y = Mathf.Floor(value.y);
				float z = Mathf.Floor(value.z);
				return new Vector3(x,y,z);
			}
			else if(special == "Ceil"){
				float x = Mathf.Ceil(value.x);
				float y = Mathf.Ceil(value.y);
				float z = Mathf.Ceil(value.z);
				return new Vector3(x,y,z);
			}
			else if(special == "Normalized"){return value.normalized;}
			else if(special == "Magnitude"){
				float magnitude = value.magnitude;
				return new Vector3(magnitude,magnitude,magnitude);
			}
			else if(special == "SqrMagnitude"){
				float sqrMagnitude = value.sqrMagnitude;
				return new Vector3(sqrMagnitude,sqrMagnitude,sqrMagnitude);
			}
			return value;
		}
		public override void Serialize(){
			if(!this.value.IsEmpty()){
				this.rawValue = this.value.Serialize();
			}
		}
	}
}