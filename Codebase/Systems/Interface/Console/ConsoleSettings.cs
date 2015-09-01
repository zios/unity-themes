using UnityEngine;
namespace Zios{
	[AddComponentMenu("Zios/Singleton/Console")]
	public class ConsoleSettings : MonoBehaviour{
		public GUISkin skin;
		public Texture2D background;
		public Texture2D inputBackground;
		public Texture2D textArrow;
		public KeyCode triggerKey = KeyCode.F12;
		public float speed = 5.0f;
		public float height = 0.25f;
		public string configFile = "Game.cfg";
		public string logFile = "Log.txt";
		public int logLineSize = 150;
		public int logFontSize = 15;
		public bool logFontAllowColors = true;
		public byte logFontColor = 7;
		public bool allowLogging = true;
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