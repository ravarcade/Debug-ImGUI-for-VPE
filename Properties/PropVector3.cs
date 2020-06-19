using UnityEngine;
using ImGuiNET;

namespace VisualPinball.Engine.Unity.ImgGUI
{
    internal class PropVector3 : PropDefault<Vector3>, IProperty
    {
        public new bool Draw()
        {
            var v = _val.ToImGui();

            _isChanged = ImGui.DragFloat3(_name, ref v, 1.0f);

            if (_isChanged)
                _val = v.ToVector3();

            if (_tip != null && ImGui.IsItemHovered())
                ImGui.SetTooltip(_tip);

            return false;
        }
    }
}