using System;
using UnityEngine;
namespace Zios{
	[RequireComponent(typeof(Camera))]
	[AddComponentMenu("Zios/Component/Rendering/Camera Advanced")]
	public class CameraAdvanced : MonoBehaviour{
		[Internal] public Camera target;
		[ReadOnly] public Vector3 velocity;
		[ReadOnly] public Matrix4x4 worldToCameraMatrix;
		public Matrix4x4 projectionMatrix;
		//[ReadOnly] public Vector2 aspectRatio;
		//public bool stereoEnabled;
		//public float stereoSeparation;
		//public float stereoConvergence;
		public LayerMask eventMask;
		public DepthTextureMode depthTextureMode;
		public TransparencySortMode transparencySortMode;
		public float aspect;
		public LayerDistances layerCullDistances = new LayerDistances();
		public bool layerCullSpherical;
		public void Reset(){
			this.target = this.GetComponent<Camera>();
			this.velocity = this.target.velocity;
			this.projectionMatrix = this.target.projectionMatrix;
			this.worldToCameraMatrix = this.target.worldToCameraMatrix;
			//this.stereoEnabled = this.target.stereoEnabled;
			//this.stereoSeparation = this.target.stereoSeparation;
			//this.stereoConvergence = this.target.stereoConvergence;
			this.eventMask = (LayerMask)this.target.eventMask;
			this.depthTextureMode = this.target.depthTextureMode;
			this.transparencySortMode = this.target.transparencySortMode;
			this.aspect = this.target.aspect;
			this.layerCullDistances.values = this.target.layerCullDistances;
			this.layerCullSpherical = this.target.layerCullSpherical;
		}
		public void OnValidate(){
			this.target.projectionMatrix = this.projectionMatrix;
			//this.target.worldToCameraMatrix = this.worldToCameraMatrix;
			//this.target.stereoEnabled = this.stereoEnabled;
			//this.target.stereoSeparation = this.stereoSeparation;
			//this.target.stereoConvergence = this.stereoConvergence;
			this.target.eventMask = this.eventMask.value;
			this.target.depthTextureMode = this.depthTextureMode;
			this.target.transparencySortMode = this.transparencySortMode;
			this.target.aspect = this.aspect;
			this.target.layerCullDistances = this.layerCullDistances.values;
			this.target.layerCullSpherical = this.layerCullSpherical;
		}
		public void Start(){
			if(Application.isPlaying){
				this.OnValidate();
			}
		}
		public void FixedUpdate(){
			if(Application.isPlaying){
				this.velocity = this.target.velocity;
				this.projectionMatrix = this.target.projectionMatrix;
				this.worldToCameraMatrix = this.target.worldToCameraMatrix;
			}
		}
	}
	[Serializable]
	public class LayerDistances{
		public float[] values;
	}
}