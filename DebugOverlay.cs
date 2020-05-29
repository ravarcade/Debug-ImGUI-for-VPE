using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using ImGuiNET;


using Player = VisualPinball.Unity.Game.Player;
using VisualPinball.Engine.Game;
using VisualPinball.Engine.Math;
using VisualPinball.Engine.VPT.Table;
using VisualPinball.Engine.VPT.Ball;
using Unity.Assertions;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using VisualPinball.Unity.Physics;
using VisualPinball.Unity.Game;
using VisualPinball.Unity.DebugUI_Interfaces;

using VisualPinball.Engine.Unity.ImgGUI.Tools;

namespace VisualPinball.Engine.Unity.ImgGUI
{
    internal class DebugOverlay
    {
		private int corner = 0;

		private void RightButtonMenu(DebugUIClient debugUIClient)
        {
			if (ImGui.BeginPopupContextWindow())
			{
				if (ImGui.MenuItem("Top-left", null, corner == 0)) corner = 0;
				if (ImGui.MenuItem("Top-right", null, corner == 1)) corner = 1;
				if (ImGui.MenuItem("Bottom-left", null, corner == 2)) corner = 2;
				if (ImGui.MenuItem("Bottom-right", null, corner == 3)) corner = 3;
				ImGui.Separator();

				if (ImGui.MenuItem("Hide Ovarlay")) debugUIClient.showOverlayWindow = false;
				if (ImGui.MenuItem("Show Debug Window", null, debugUIClient.showDebugWindow)) debugUIClient.showDebugWindow = !debugUIClient.showDebugWindow;
				if (ImGui.MenuItem("Show ImGUI Demo Window", null, debugUIClient.showDemoWindow)) debugUIClient.showDemoWindow = !debugUIClient.showDemoWindow;
				ImGui.Separator();

				if (ImGui.MenuItem("Exit")) Application.Quit();
				ImGui.EndPopup();
			}

		}

		public void Draw(DebugUIClient debugUIClient)
        {
			const float DISTANCE = 10.0f;
			var io = ImGui.GetIO();
			var window_pos = new System.Numerics.Vector2((corner & 1) != 0 ? io.DisplaySize.X - DISTANCE : DISTANCE, (corner & 2) != 0 ? io.DisplaySize.Y - DISTANCE : DISTANCE);
			var window_pos_pivot = new System.Numerics.Vector2((corner & 1) != 0 ? 1.0f : 0.0f, (corner & 2) != 0 ? 1.0f : 0.0f);
			ImGui.SetNextWindowPos(window_pos, ImGuiCond.Always, window_pos_pivot);
			ImGui.SetNextWindowBgAlpha(0.35f); // Transparent background


			if (ImGui.Begin("Simple overlay", ref debugUIClient.showOverlayWindow, (corner != -1 ? ImGuiWindowFlags.NoMove : 0) | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoNav))
			{
				debugUIClient._fps.Draw("FPS: ");
				debugUIClient._physicsTicks.Draw("Physics: ");
				debugUIClient._physicsTimes.Draw("", "Physics time: ","n1");

				if (ImGui.IsMousePosValid())
					ImGui.Text("Mouse Position: (" + io.MousePos.X.ToString("n1") + ", " + io.MousePos.Y.ToString("n1") + ")");
				else
					ImGui.Text("Mouse Position: <invalid>");
					
				ImGui.Separator();
				ImGui.Text("(right-click to change position)");

				RightButtonMenu(debugUIClient);
			}
			ImGui.End();
		}
    }
}