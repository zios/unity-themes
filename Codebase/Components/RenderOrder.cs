using UnityEngine;
using System.Collections;
namespace Zios{
    [AddComponentMenu("Zios/Component/Rendering/Render Order")]
    [RequireComponent(typeof(Renderer))][ExecuteInEditMode]
    public class RenderOrder : MonoBehaviour{
	    public int[] renderQueues;
	    public void OnEnable(){this.Setup();}
	    public void Start(){this.Setup();}
	    public void OnDrawGizmosSelected(){this.Setup();}
	    public void Setup(){
		    if(this.GetComponent<Renderer>() == null || this.GetComponent<Renderer>().sharedMaterial == null){return;}
		    int size = this.GetComponent<Renderer>().sharedMaterials.Length;
		    if(this.renderQueues == null || this.renderQueues.Length != size){
			    this.renderQueues = new int[size];
			    for(int index=0;index<size;++index){
				    this.renderQueues[index] = this.GetComponent<Renderer>().sharedMaterials[index].renderQueue;
			    }			
		    }
		    for(int index=0;index<size;++index){
			    this.GetComponent<Renderer>().sharedMaterials[index].renderQueue = this.renderQueues[index];
		    }
	    }
    }
}