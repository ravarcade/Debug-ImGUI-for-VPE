using UnityEngine;
using ImGuiNET;

namespace VisualPinball.Engine.Unity.ImgGUI
{
    internal class PropDefault<T> : Prop<T>
    {
        
        protected T _val;

        public override bool GetValue(ref T val) 
        { 
            if (!_isChanged)
                return false; 

            val = _val;
            
            return true;
        }

        public override void SetValue(T val) { _val = val; }
    }
}