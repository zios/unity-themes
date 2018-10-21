using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEvent = UnityEngine.Event;

namespace Zios.Interface
{
    public class ThemeWindow : EditorWindow
    {
        public Vector2 lastMouse;
        public static bool setup;
        public static Rect hiddenPosition = new Rect(9001, 9001, 1, 1);
        public static Vector2 hiddenSize = new Vector2(1, 1);

        public void OnEnable()
        {
            ThemeWindow.setup = true;
        }

        public void OnGUI()
        {
            Theme.disabled = Utility.GetPref<bool>("EditorTheme-Disabled", false);
            this.Repaint();
            if (Theme.disabled || !ThemeWindow.setup || EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                return;
            }
            Theme.Update();
            ThemeContent.Monitor();
            bool validTheme = !Theme.active.IsNull() && Theme.active.name != "Default";
            bool mouseChanged = this.lastMouse != UnityEvent.current.mousePosition;
            Utility.DelayCall(RelativeColor.UpdateSystem, 0.2f, false);
            if (validTheme && mouseChanged)
            {
                this.lastMouse = UnityEvent.current.mousePosition;
                float delay = 0;
                if (Theme.hoverResponse == HoverResponse.None) { return; }
                if (Theme.hoverResponse == HoverResponse.Slow) { delay = 0.2f; }
                if (Theme.hoverResponse == HoverResponse.Moderate) { delay = 0.05f; }
                Utility.DelayCall("Redraw", () =>
                {
                    Theme.UpdateColors();
                    var view = Utility.GetUnityType("GUIView").GetVariable("mouseOverView");
                    if (!view.IsNull())
                    {
                        view.CallMethod("Repaint");
                    }
                }, delay, false);
            }
        }

#if !UNITY_2018_3_OR_NEWER
        [MenuItem("Window/Preferences/Themes")]
        public static void ShowWindowFromMenu()
        {
            PreferencesTools.ShowSection("Themes");
        }
#endif

        public static void ShowWindow()
        {
            if (Theme.window.IsNull())
            {
                Theme.window = Locate.GetAssets<ThemeWindow>().FirstOrDefault();
                if (Theme.window.IsNull())
                {
                    Theme.window = ScriptableObject.CreateInstance<ThemeWindow>();
                    Theme.window.position = ThemeWindow.hiddenPosition;
                    Theme.window.minSize = ThemeWindow.hiddenSize;
                    Theme.window.maxSize = ThemeWindow.hiddenSize;
                    Theme.window.wantsMouseMove = Theme.hoverResponse != HoverResponse.None;
                    Theme.window.ShowPopup();
                }
            }

            try
            {
                if (Theme.window == null || (Theme.window != null && Theme.window.position == null))
                    return;

                if (Theme.window.position != ThemeWindow.hiddenPosition)
                {
                    Theme.window.position = ThemeWindow.hiddenPosition;
                }

                if (Theme.window.maxSize != ThemeWindow.hiddenSize)
                {
                    Theme.window.minSize = ThemeWindow.hiddenSize;
                    Theme.window.maxSize = ThemeWindow.hiddenSize;
                }
            }
            catch
            {
            }
        }

        public static void CloseWindow(object sender, EventArgs arguments)
        {
#if UNITY_5_3_4_OR_NEWER
			if(!EditorApplication.isPlayingOrWillChangePlaymode){
				ThemeWindow.CloseWindow();
			}
#endif
        }

        public static void CloseWindow()
        {
            var windows = Theme.window.IsNull() ? Resources.FindObjectsOfTypeAll<ThemeWindow>() : Theme.window.AsArray();
            foreach (var window in windows)
            {
                if (!window.IsNull())
                {
                    window.Close();
                }
            }
            Theme.window = null;
            ThemeWindow.setup = false;
        }
    }
}