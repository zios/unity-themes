using System;
using System.Collections;
using UnityEngine;
namespace Zios{
    public static class BitArrayExtension{
	    public static int GetInt(this BitArray bitArray){
		    int[] array = new int[1];
		    bitArray.CopyTo(array,0);
		    return array[0];
		}
	    public static bool Contains(this BitArray bitArray,int value,int start=0){
		    for(int index=start;index < bitArray.Length;++index){
			    bool active = bitArray[index];
			    if(active && ((value | (1<<index)) == value)){
				    return true;
				}
			}
		    return false;
		}
	    public static void Set(this BitArray bitArray,int value,int start=0){
		    for(int index=start;index < bitArray.Length;++index){
			    if((value | (1<<index)) == value){
				    bitArray[index] = true;
				}
			}
		}
	    public static void Clear(this BitArray bitArray,int value,int start=0){
		    for(int index=start;index < bitArray.Length;++index){
			    if((value | (1<<index)) == value){
				    bitArray[index] = false;
				}
			}
		}
	    public static bool Check(this BitArray bitArray,sbyte[] values){
		    for(int index=0;index < values.Length;++index){
			    bool state = values[index] < 0 ? false : true;
			    int target = Mathf.Abs(values[index]);
			    if(bitArray[target] != state){
				    return false;
				}
			}
		    return true;
		}
	    public static bool Check(this BitArray bitArray,int value){
		    int current = bitArray.GetInt();
		    return (value & current) != 0;
		}
	    public static bool Matches(this BitArray bitArray,int value){
		    int current = bitArray.GetInt();
		    return current == value;
		}
	}
}
