using Unity.Mathematics;
using ImGuiNET;

namespace VisualPinball.Engine.Unity.ImgGUI
{
    internal class PropFloat3 : PropDefault<float3>, IProperty
    {
        public new bool Draw()
        {
            var v = _val.ToImGui();

            _isChanged = ImGui.DragFloat3(_name, ref v, 1.0f);

            if (_isChanged)
                _val = v.ToFloat3();

            if (_tip != null && ImGui.IsItemHovered())
                ImGui.SetTooltip(_tip);

            return false;
        }
    }
}