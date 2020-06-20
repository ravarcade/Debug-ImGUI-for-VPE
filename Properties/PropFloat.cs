using UnityEngine;
using ImGuiNET;

namespace VisualPinball.Engine.Unity.ImgGUI
{
    internal class PropFloat : PropDefault<float>, IProperty
    {
        public bool Draw()
        {
            _isChanged = ImGui.DragFloat(_name, ref _val, 1.0f);

            if (_tip != null && ImGui.IsItemHovered())
                ImGui.SetTooltip(_tip);

            return false;
        }
    }
}