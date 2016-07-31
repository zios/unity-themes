using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityObject = UnityEngine.Object;
namespace Zios.Animations{
	[AddComponentMenu("")]
	public class PC2Controller : MonoBehaviour{
		public UnityObject file;
		public GameObject target;
		public int speed = 30;
		[Advanced] public bool showVertexes = true;
		[Advanced] public bool showOriginalVertexes;
		[Internal] public int originalVertexCount;
		[Internal] public int currentFrame;
		[Internal] public string path;
		[Internal] public PC2Data data = new PC2Data();
		[Internal] public MeshSource source = new MeshSource();
		private UnityObject previous;
		private float nextFrameTime;
		private bool ready;
		[ContextMenu("Reload")]
		public void Reload(){this.data.Load(this.path);}
		public void Reset(){this.OnValidate();}
		public void OnValidate(){
			if(!this.CanValidate()){return;}
			if(this.target == null){this.target = this.gameObject;}
			if(this.previous != this.file){
				this.previous = this.file;
				this.path = FileManager.Get(this.file).path;
				this.data.Load(this.path);
			}
			this.source.Setup(this.target);
		}
		public void Start(){
			this.ready = this.source.GetMesh() != null && this.data != null;
			if(!this.ready){return;}
			Mesh mesh = this.source.GetMesh();
			mesh.MarkDynamic();
			foreach(var frames in this.data.frames){
				frames.vertices = frames.vertices.Resize(mesh.vertices.Length);
			}
			this.currentFrame = 0;
		}
		public void Update(){
			if(!this.ready){return;}
			if(Time.time > this.nextFrameTime){
				if(this.currentFrame >= this.data.frames.Count){this.currentFrame = 0;}
				Mesh mesh = this.source.GetMesh();
				mesh.vertices = this.data.frames[this.currentFrame].vertices;
				mesh.RecalculateNormals();
				this.nextFrameTime = Time.time + 1.0f/speed;
				this.currentFrame += 1;
			}
		}
		public void OnDrawGizmosSelected(){
			bool dataReady = this.data != null && this.data.frames.Count > 0;
			bool meshReady = this.source.GetMesh() != null && this.source.GetRenderer() != null;
			if(dataReady && meshReady){
				this.originalVertexCount = this.source.GetMesh().vertices.Length;
				Gizmos.color = Color.white;
				var matrix = this.source.GetRenderer().localToWorldMatrix;
				if(this.showVertexes){
					if(this.currentFrame >= this.data.frames.Count){this.currentFrame = 0;}
					foreach(Vector3 point in this.data.frames[this.currentFrame].vertices){
						Vector3 position = matrix * point;
						position += this.transform.position;
						Gizmos.DrawSphere(position,0.02f);
					}
				}
				if(!Application.isPlaying && this.showOriginalVertexes){
					Gizmos.color = Color.red;
					foreach(Vector3 point in this.source.GetMesh().vertices){
						Vector3 position = matrix * point;
						position += this.transform.position;
						Gizmos.DrawSphere(position,0.02f);
					}
				}
			}
		}
	}
	[Serializable]
	public class PC2Data{
		public string signature;
		public int version;
		public int points;
		public float startFrame;
		public float rate;
		public int totalSamples;
		public List<PC2Frame> frames = new List<PC2Frame>();
		public void Load(string path){
			try{
				this.frames.Clear();
				using(BinaryReader binary = new BinaryReader(File.Open(path,FileMode.Open))){
					this.signature = new string(binary.ReadChars(12));
					this.version = binary.ReadInt32();
					this.points = binary.ReadInt32();
					this.startFrame = binary.ReadSingle();
					this.rate = binary.ReadSingle();
					this.totalSamples = binary.ReadInt32();
					int position = (int)binary.BaseStream.Position;
					int length = (int)binary.BaseStream.Length;
					var frame = new List<Vector3>();
					int pointIndex = 0;
					while(position < length){
						float x = binary.ReadSingle();
						float y = binary.ReadSingle();
						float z = binary.ReadSingle();
						frame.Add(new Vector3(x,y,z));
						if(pointIndex >= this.points-1){
							var pc2Frame = this.frames.AddNew();
							pc2Frame.vertices = frame.ToArray();
							pointIndex = -1;
							frame.Clear();
						}
						pointIndex += 1;
						position += sizeof(float)*3;
					}
				}
				Debug.Log("[PC2Data] : File Successfully parsed -- " + path);
			}
			catch{
				Debug.LogWarning("[PC2Data] : Error parsing file -- " + path);
			}
		}
	}
	[Serializable]
	public class PC2Frame{
		public Vector3[] vertices;
	}
}