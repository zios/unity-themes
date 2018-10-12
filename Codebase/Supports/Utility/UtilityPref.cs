using System.Collections.Generic;
using UnityEngine;
using System.Collections;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace Zios
{
#if UNITY_EDITOR

    using Interface;

#endif

    public static partial class Utility
    {
#if UNITY_EDITOR
        public static Dictionary<string, object> cache = new Dictionary<string, object>();

        public static void SetPref<T>(string name, T value)
        {
            Utility.cache[name] = value;
            if (value is bool) { EditorPrefs.SetBool(name, value.As<bool>()); }
            else if (value is int) { EditorPrefs.SetInt(name, value.As<int>()); }
            else if (value is float) { EditorPrefs.SetFloat(name, value.As<float>()); }
            else if (value is string) { EditorPrefs.SetString(name, value.As<string>()); }
        }

        public static bool HasPref(string name)
        {
            return EditorPrefs.HasKey(name);
        }

        public static T GetPref<T>(string name, T fallback = default(T))
        {
            if (Utility.cache.ContainsKey(name)) { return Utility.cache[name].As<T>(); }
            object value = fallback;
            if (fallback is bool) { value = EditorPrefs.GetBool(name, fallback.As<bool>()); }
            else if (fallback is int) { value = EditorPrefs.GetInt(name, fallback.As<int>()); }
            else if (fallback is float) { value = EditorPrefs.GetFloat(name, fallback.As<float>()); }
            else if (fallback is string) { value = EditorPrefs.GetString(name, fallback.As<string>()); }
            Utility.cache[name] = value;
            return value.As<T>();
        }

        public static void CallEditorPref(string name, bool showWarnings = false)
        {
            var callbacks = EditorPrefs.GetString(name);
            var called = new List<string>();
            var success = new List<string>();
            bool debug = ObjectExtension.debug;
            ObjectExtension.debug = showWarnings;
            foreach (var method in callbacks.Split("|"))
            {
                if (called.Contains(method) || method.IsEmpty()) { continue; }
                if (!method.CallMethod().IsNull())
                {
                    success.Add(method);
                }
                called.Add(method);
            }
            ObjectExtension.debug = debug;
            var value = success.Count > 0 ? success.Join("|") : "";
            EditorPrefs.SetString(name, value);
        }

        public static void ToggleEditorPref(string name, bool fallback = false)
        {
            bool value = !Utility.GetPref(name, fallback);
            Utility.SetPref(name, value);
        }

        public static void DeleteEditorPrefs(bool prompt)
        {
            if (!prompt || EditorUI.DrawDialog("Clear Editor Prefs", "Delete all the editor preferences?", "Yes", "No"))
            {
                EditorPrefs.DeleteAll();
            }
        }

#if !UNITY_THEMES

        [MenuItem("Zios/Prefs/Clear Editor")]
        public static void DeleteEditorPrefs() { Utility.DeleteEditorPrefs(true); }

#endif
#endif

        //============================
        // Player Pref
        //============================
        public static Dictionary<string, object> cachePlayer = new Dictionary<string, object>();

        public static bool HasPlayerPref(string name)
        {
            return PlayerPrefs.HasKey(name);
        }

        public static void SetPlayerPref<T>(string name, T value)
        {
            Utility.cachePlayer[name] = value;
            if (value is bool) { PlayerPrefs.SetInt(name, value.As<bool>().ToInt()); }
            else if (value is int) { PlayerPrefs.SetInt(name, value.As<int>()); }
            else if (value is float) { PlayerPrefs.SetFloat(name, value.As<float>()); }
            else if (value is string) { PlayerPrefs.SetString(name, value.As<string>()); }
            else if (value is Vector3) { PlayerPrefs.SetString(name, value.As<Vector3>().ToString()); }
            else if (value is byte) { PlayerPrefs.SetString(name, value.As<byte>().ToString()); }
            else if (value is short) { PlayerPrefs.SetInt(name, value.As<short>().ToInt()); }
            else if (value is double) { PlayerPrefs.SetFloat(name, value.As<double>().ToFloat()); }
            else if (value is ICollection) { PlayerPrefs.SetString(name, value.As<IEnumerable>().SerializeAuto()); }
        }

        public static T GetPlayerPref<T>(string name, T fallback = default(T))
        {
            if (Utility.cachePlayer.ContainsKey(name)) { return Utility.cachePlayer[name].As<T>(); }
            object value = fallback;
            if (fallback is bool) { value = PlayerPrefs.GetInt(name, fallback.As<bool>().ToInt()); }
            else if (fallback is int) { value = PlayerPrefs.GetInt(name, fallback.As<int>()); }
            else if (fallback is float) { value = PlayerPrefs.GetFloat(name, fallback.As<float>()); }
            else if (fallback is string) { value = PlayerPrefs.GetString(name, fallback.As<string>()); }
            else if (fallback is Vector3) { value = PlayerPrefs.GetString(name, fallback.As<Vector3>().Serialize()); }
            else if (fallback is byte) { value = PlayerPrefs.GetString(name, fallback.As<byte>().Serialize()); }
            else if (fallback is short) { value = PlayerPrefs.GetInt(name, fallback.As<short>().ToInt()); }
            else if (fallback is double) { value = PlayerPrefs.GetFloat(name, fallback.As<double>().ToFloat()); }
            else if (fallback is ICollection) { value = PlayerPrefs.GetString(name, fallback.As<IEnumerable>().SerializeAuto()); }
            Utility.cachePlayer[name] = value;
            return value.As<T>();
        }

        public static void TogglePlayerPref(string name, bool fallback = false)
        {
            bool value = !(Utility.GetPlayerPref<int>(name) == fallback.ToInt());
            Utility.SetPlayerPref<int>(name, value.ToInt());
        }

        public static void DeletePlayerPrefs(bool prompt)
        {
#if UNITY_EDITOR
            if (!prompt || EditorUI.DrawDialog("Clear Player Prefs", "Delete all the player preferences?", "Yes", "No"))
            {
                PlayerPrefs.DeleteAll();
            }
#endif
        }

#if !UNITY_THEMES && UNITY_EDITOR

        [MenuItem("Zios/Prefs/Clear Player")]
        public static void DeletePlayerPrefs() { Utility.DeletePlayerPrefs(true); }

#endif
    }
}