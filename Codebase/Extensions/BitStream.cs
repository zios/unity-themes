using System;
using System.Collections;
using UnityEngine;
namespace Zios{
    public static class BitStreamExtension{
	    public static void Serialize(this BitStream stream,ref object data,Type type){
		    if(type == typeof(bool)){
			    bool value = (bool)data;
			    stream.Serialize(ref value);
			    data=value;
			}
		    else if(type == typeof(char)){
			    char value = (char)data;
			    stream.Serialize(ref value);
			    data=value;
			}
		    else if(type == typeof(short)){
			    short value = (short)data;
			    stream.Serialize(ref value);
			    data=value;
			}
		    else if(type == typeof(int)){
			    int value = (int)data;
			    stream.Serialize(ref value);
			    data=value;
			}
		    else if(type == typeof(float)){
			    float value = (float)data;
			    stream.Serialize(ref value);
			    data=value;
			}
		    else if(type == typeof(Quaternion)){
			    Quaternion value = (Quaternion)data;
			    stream.Serialize(ref value);
			    data=value;
			}
		    else if(type == typeof(Vector3)){
			    Vector3 value = (Vector3)data;
			    stream.Serialize(ref value);
			    data=value;
			}
		    else if(type == typeof(NetworkPlayer)){
			    NetworkPlayer value = (NetworkPlayer)data;
			    stream.Serialize(ref value);
			    data=value;
			}
		}
	    public static void Serialize(this BitStream stream,ref object data){
		    stream.Serialize(ref data,data.GetType());
		}
	}
}
