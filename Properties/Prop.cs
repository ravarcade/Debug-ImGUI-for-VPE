using UnityEngine;
using ImGuiNET;

namespace VisualPinball.Engine.Unity.ImgGUI
{
    internal class Prop<T> : IProperty
    {
        protected string _uniq;
        protected string _name;
        protected string _tip;
        protected bool _isChanged;

        public void Init(int idx, string name, string tip)
        {
            _isChanged = false;
            _uniq = name + "_" + idx;
            _name = name;
            _tip = tip;
        }

        public virtual bool GetValue(ref T val)
        {
            Debug.LogError("DebugUI: unknow type: " + typeof(T).FullName);
            return false;
        }

        public virtual void SetValue(T val)
        {
            // do nothing.... maybe some error report
            Debug.LogError("DebugUI: unknow type: " + typeof(T).FullName);
        }

        public bool Draw()
        {
            if (ImGui.TreeNode(_uniq, _name))
            {
                if (_tip != null && ImGui.IsItemHovered())
                    ImGui.SetTooltip(_tip);

                return true;
            }

            return false;
        }
    }
}