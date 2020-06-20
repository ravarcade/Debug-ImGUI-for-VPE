using UnityEngine;
using ImGuiNET;
using Unity.Transforms;

namespace VisualPinball.Engine.Unity.ImgGUI
{
    internal class PropMatrix4x4: PropDefault<Matrix4x4>, IProperty
    {
        public new bool Draw()
        {

            var rot = _val.rotation.eulerAngles.ToImGui();
            var pos = ((UnityEngine.Vector3)_val.GetColumn(3)).ToImGui();
            var scale = _val.lossyScale;

            ImGui.Text(_name + ":");
            if (_tip != null && ImGui.IsItemHovered())
                ImGui.SetTooltip(_tip);

            _isChanged |= ImGui.DragFloat3("Position", ref pos, 1.0f);
            _isChanged |= ImGui.DragFloat3("Rotation", ref rot, 1.0f);

            if (_isChanged) {
                _val = Matrix4x4.TRS(
                        pos.ToVector3(),
                        Quaternion.Euler(rot.X, rot.Y, rot.Z),
                        scale);
            }


            return false;
        }
    }
}