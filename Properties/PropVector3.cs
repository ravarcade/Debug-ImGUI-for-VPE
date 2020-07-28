using UnityEngine;
using ImGuiNET;

namespace VisualPinball.Engine.Unity.ImgGUI
{
    internal class PropVector3 : PropDefault<Vector3>, IProperty
    {
        public new bool Draw()
        {
            _isChanged = ImGui.DragFloat3(_name, ref _val, 1.0f);

            if (_tip != null && ImGui.IsItemHovered())
                ImGui.SetTooltip(_tip);

            return false;
        }
    }
}