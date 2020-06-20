using UnityEngine;
using ImGuiNET;
using System;

namespace VisualPinball.Engine.Unity.ImgGUI
{
    internal interface IPropDefault
    {
        Type GetType();
    }

    internal class PropDefault<T> : Prop<T>, IPropDefault
    {        
        protected T _val;

        Type IPropDefault.GetType() { return typeof(T); }

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