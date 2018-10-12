using UnityEngine;

namespace Zios
{
    public static class GUIContentExtension
    {
        public static string ToString(this GUIContent current)
        {
            return current.text;
        }

        public static string Serialize(this GUIContent current)
        {
            var data = current.image.IsNull() ? "" : current.image.As<Texture2D>().Serialize();
            return current.text + "||" + current.tooltip + "||" + data;
        }

        public static GUIContent Deserialize(this GUIContent current, string value)
        {
            var data = value.Split("||");
            current.text = data[0];
            current.tooltip = data[1];
            current.image = data[2].IsEmpty() ? null : new Texture2D(1, 1).Deserialize(data[2]);
            return current;
        }
    }
}