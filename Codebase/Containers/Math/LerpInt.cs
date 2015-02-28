using Zios;
using System;
using UnityEngine;
namespace Zios{
    [Serializable]
    public class LerpInt : LerpTransition{
	    private int start;
	    private int lastEnd;
	    public int Step(int current){
		    return this.Step(current,current);
	    }
	    public virtual int Step(int start,int end){
		    if(end != this.lastEnd && this.isResetOnChange){this.Reset();}
		    if(!this.active){this.start = start;}
		    this.lastEnd = end;
		    this.CheckActive();
		    return (int)this.Lerp(this.start,end,this.transition.Tick());
	    }
    }
}