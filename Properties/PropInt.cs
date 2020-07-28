using UnityEngine;
using ImGuiNET;

namespace VisualPinball.Engine.Unity.ImgGUI
{
    internal class PropInt : PropDefault<int>, IProperty
    {
        public new bool Draw()
        {
            _isChanged = ImGui.DragInt(_name, ref _val, 1);

            if (_tip != null && ImGui.IsItemHovered())
                ImGui.SetTooltip(_tip);

            return false;
        }
    }
}