
using VisualPinball.Engine.Common;
using VisualPinball.Unity.Physics.DebugUI;
using VisualPinball.Unity.Physics.Engine;
using ImGuiNET;
using UnityEngine;

using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;
using Unity.Entities.UniversalDelegates;
using UnityEditor.Profiling;

namespace VisualPinball.Engine.Unity.ImgGUI
{

    public static unsafe partial class ImGuiExt
    {
        static public void SliderFloat(string label, DebugFlipperSliderParam param, float min, float max)
        {
            var engine = EngineProvider<IPhysicsEngine>.Get();
            float val = engine.GetFlipperDebugValue(param);
            if (ImGui.SliderFloat(label, ref val, min, max))
            {
                engine.SetFlipperDebugValue(param, val);
            }
        }

        static public bool InlineButton(string label)
        {
            var style = ImGui.GetStyle();
            float expectedButtonWidth = ImGui.CalcTextSize(label).X + style.ItemSpacing.X * 2;            
            float rightEdge = ImGui.GetWindowPos().X + ImGui.GetWindowContentRegionMax().X;
            if (ImGui.GetItemRectMax().X + expectedButtonWidth < rightEdge)
                ImGui.SameLine();
            return ImGui.Button(label);
        }

        static public Vector4 HSVtoRGB(float h, float s, float v, float a = 1.0f)
        {
            float r, g, b;
            ImGui.ColorConvertHSVtoRGB(h, s, v, out r, out g, out b);
            return new Vector4(r, g, b, a);
        }

        static public bool Button(string label, string tip = null, float hue = -1)
        {
            var style = ImGui.GetStyle();
            float expectedButtonWidth = ImGui.CalcTextSize(label).X + style.ItemSpacing.X * 2;
            float rightEdge = ImGui.GetWindowPos().X + ImGui.GetWindowContentRegionMax().X;
            if (ImGui.GetItemRectMax().X + expectedButtonWidth < rightEdge)
                ImGui.SameLine();

            if (hue != -1)
            {
                ImGui.PushID(0);
                ImGui.PushStyleColor(ImGuiCol.Button, HSVtoRGB(hue, 0.6f, 0.6f));
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, HSVtoRGB(hue, 0.7f, 0.7f));
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, HSVtoRGB(hue, 0.8f, 0.8f));
            }
            bool ret = ImGui.Button(label);
            if (hue != -1)
            {
                ImGui.PopStyleColor(3);
                ImGui.PopID();
            }

            if (tip != null && ImGui.IsItemHovered())
                ImGui.SetTooltip(tip);

            return ret;
        }

    }

}