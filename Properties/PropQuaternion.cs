using UnityEngine;
using ImGuiNET;

namespace VisualPinball.Engine.Unity.ImgGUI
{
    internal class PropQuaternion : PropDefault<Quaternion>, IProperty
    {
        public new bool Draw()
        {
            var v = _val.eulerAngles.ToImGui();

            _isChanged = ImGui.DragFloat3(_name, ref v, 1.0f);

            if (_isChanged)
                _val = Quaternion.Euler(v.X, v.Y, v.Z);

            if (_tip != null && ImGui.IsItemHovered())
                ImGui.SetTooltip(_tip);

            return false;
        }
    }
}