using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEvent = UnityEngine.Event;

namespace Zios
{
    public static class Button
    {
        public static List<string> keyCodes = new List<string>(Enum.GetNames(typeof(KeyCode)));

        public static Dictionary<KeyCode, string> keyNames = new Dictionary<KeyCode, string>(){
            {KeyCode.Keypad0,"0"},
            {KeyCode.Keypad1,"1"},
            {KeyCode.Keypad2,"2"},
            {KeyCode.Keypad3,"3"},
            {KeyCode.Keypad4,"4"},
            {KeyCode.Keypad5,"5"},
            {KeyCode.Keypad6,"6"},
            {KeyCode.Keypad7,"7"},
            {KeyCode.Keypad8,"8"},
            {KeyCode.Keypad9,"9"},
            {KeyCode.KeypadPeriod,"."},
            {KeyCode.KeypadDivide,"/"},
            {KeyCode.KeypadMultiply,"*"},
            {KeyCode.KeypadMinus,"-"},
            {KeyCode.KeypadPlus,"+"},
            {KeyCode.KeypadEquals,"="},
            {KeyCode.Alpha0,"0"},
            {KeyCode.Alpha1,"1"},
            {KeyCode.Alpha2,"2"},
            {KeyCode.Alpha3,"3"},
            {KeyCode.Alpha4,"4"},
            {KeyCode.Alpha5,"5"},
            {KeyCode.Alpha6,"6"},
            {KeyCode.Alpha7,"7"},
            {KeyCode.Alpha8,"8"},
            {KeyCode.Alpha9,"9"},
            {KeyCode.Exclaim,"!"},
            {KeyCode.DoubleQuote,"\""},
            {KeyCode.Hash,"#"},
            {KeyCode.Dollar,"$"},
            {KeyCode.Ampersand,"&"},
            {KeyCode.Quote,"'"},
            {KeyCode.LeftParen,"("},
            {KeyCode.RightParen,")"},
            {KeyCode.Asterisk,"*"},
            {KeyCode.Plus,"+"},
            {KeyCode.Comma,","},
            {KeyCode.Minus,"-"},
            {KeyCode.Period,"."},
            {KeyCode.Slash,"/"},
            {KeyCode.Colon,":"},
            {KeyCode.Semicolon,";"},
            {KeyCode.Less,"<"},
            {KeyCode.Equals,"="},
            {KeyCode.Greater,">"},
            {KeyCode.Question,"?"},
            {KeyCode.At,"@"},
            {KeyCode.LeftBracket,"["},
            {KeyCode.Backslash,"\\"},
            {KeyCode.RightBracket,"]"},
            {KeyCode.Caret,"^"},
            {KeyCode.Underscore,"_"},
            {KeyCode.BackQuote,"`"}
        };

        public static string GetName(string name)
        {
            return Button.keyNames.ContainsValue(name) ? Button.keyNames.GetKey(name) : name;
        }

        public static bool EventKeyDown(string name)
        {
            if (UnityEvent.current.type == EventType.KeyDown)
            {
                KeyCode code = (KeyCode)Enum.Parse(typeof(KeyCode), name);
                return UnityEvent.current.keyCode == code;
            }
            return false;
        }

        public static bool EventKeyUp(string name)
        {
            if (UnityEvent.current.type == EventType.KeyUp)
            {
                KeyCode code = (KeyCode)Enum.Parse(typeof(KeyCode), name);
                return UnityEvent.current.keyCode == code;
            }
            return false;
        }

        public static bool EventKeyDown(KeyCode code)
        {
            return UnityEvent.current.type == EventType.KeyDown && UnityEvent.current.keyCode == code;
        }

        public static bool EventKeyUp(KeyCode code)
        {
            return UnityEvent.current.type == EventType.KeyUp && UnityEvent.current.keyCode == code;
        }
    }
}