using UnityEngine;
namespace Zios{
	[AddComponentMenu("Zios/Singleton/Console")]
	public class ConsoleSettings : MonoBehaviour{
		public GUISkin skin;
		public Material background;
		public Material inputBackground;
		public Material textArrow;
		public KeyCode triggerKey = KeyCode.F12;
		public float speed = 5.0f;
		public float height = 0.25f;
		public string configFile = "Game.cfg";
		public string logFile = "Log.txt";
		public int logLineSize = 150;
		public int logFontSize = 15;
		public byte logFontColor = 7;
		public bool logFontAllowColors = true;
		public void Awake(){
			Zios.Console.settings = this;
			Zios.Console.Awake();
		}
		public void Start(){
			Zios.Console.Start();
		}
		public void OnEnable(){
			Zios.Console.OnEnable();
		}
		public void OnDisable(){
			Zios.Console.OnDisable();
		}
		public void OnApplicationQuit(){
			Zios.Console.OnApplicationQuit();
		}
		public void OnGUI(){
			Zios.Console.OnGUI();
		}
	}
}