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

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VisualPinball.Engine.Unity.ImgGUI
{
	using Tools;	

	[AddComponentMenu("Visual Pinball/Debug GUI")]
	[DisallowMultipleComponent]
	public class DebugGuiHook : MonoBehaviour
	{
		[DllImport("UnityImGuiRenderer")]
		private static extern System.IntPtr GetRenderEventFunc();

		private delegate void DebugCallback(string message);

		[DllImport("UnityImGuiRenderer")]
		private static extern void RegisterDebugCallback(DebugCallback callback);

		[DllImport("UnityImGuiRenderer")]
		public static extern System.IntPtr GenerateImGuiFontTexture(System.IntPtr pixels, int width, int height, int bytesPerPixel);

		[DllImport("UnityImGuiRenderer")]
		public static extern void SendImGuiDrawCommands(ImGuiNET.ImDrawDataPtr ptr);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		public static extern void OutputDebugString(string message);

		private ImGuiController _controller;

		private void Awake()
		{
			_controller = new ImGuiController();
            DebugUI.RegisterDebugUI(debugUI);
		}

		private void Start()
		{
			RegisterDebugCallback(new DebugCallback(DebugMethod));
			StartCoroutine("CallPluginAtEndOfFrames");
			var players = GameObject.FindObjectsOfType<Player>();
			player = players?[0];
			//if (player.physicsEngine != null)
			//{
			//	player.physicsEngine.PushUI_PhysicsProcessingTime += OnPhysicsProcessingTime;
			//	player.physicsEngine.PushUI_DebugFlipperData += OnDebugSubmit;
			//	player.physicsEngine.GetUI_Float += GetUI_Float;
			//}
		}

		float SyncParam(ref float param, float currentValue)
		{
			if (param == float.MaxValue)
				param = currentValue;

			return param;
		}

		float GetUI_Float(int idx, float currentValue) 
		{			
			if (idx == 0) return SyncParam(ref flipperAcc, currentValue);
			if (idx == 1) return SyncParam(ref flipperMass, currentValue);
			if (idx == 2) return SyncParam(ref flipperOffScale, currentValue);
			if (idx == 3) return SyncParam(ref flipperOnNearEndScale, currentValue);
			if (idx == 4) return SyncParam(ref flipperNumOfDegreeNearEnd, currentValue);

			return 0;  
		}
		
		private Player player = null;
		private WaitForEndOfFrame frameWait = new WaitForEndOfFrame();

		private IEnumerator CallPluginAtEndOfFrames()
		{
			yield return frameWait;
			//_controller.RecreateFontDeviceTexture(true);

			while (true)
			{
				//At the end of the frame, have ImGui render before invoking the draw on the GPU.
				yield return frameWait;
				_controller.Render();
				GL.IssuePluginEvent(GetRenderEventFunc(), 1);
			}
		}

		private void Update()
		{
			_controller.Update();
			SubmitUI();
		}

		private static void DebugMethod(string message)
		{
			Debug.Log("UnityImGuiRenderer: " + message);
		}


		// ============================================================
		public bool showDemoWindow = false;
		public bool enableManualBallRoller = true;

		DebugUIClient debugUI = new DebugUIClient();

		private ChartFloat physics_times = new ChartFloat(100, 0.0f, 16.6f, 50);
		void OnPhysicsProcessingTime(float val)
		{
			physics_times.Add(val);
		}

// 		private void OnOverlay()
// 		{
// 			//UpdateFPS();
// 			if (!showOverlayWindow)
// 				return;
			
// 			const float DISTANCE = 10.0f;
// 			var io = ImGui.GetIO();
// 			var window_pos = new System.Numerics.Vector2((corner & 1) != 0 ? io.DisplaySize.X - DISTANCE : DISTANCE, (corner & 2) != 0 ? io.DisplaySize.Y - DISTANCE : DISTANCE);
// 			var window_pos_pivot = new System.Numerics.Vector2((corner & 1) != 0 ? 1.0f : 0.0f, (corner & 2) != 0 ? 1.0f : 0.0f);
// 			ImGui.SetNextWindowPos(window_pos, ImGuiCond.Always, window_pos_pivot);
// 			ImGui.SetNextWindowBgAlpha(0.35f); // Transparent background

// 			if (ImGui.Begin("Simple overlay", ref showOverlayWindow, (corner != -1 ? ImGuiWindowFlags.NoMove : 0) | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoNav))
// 			{
// 				debugUI.DrawOverlay();
// //				ImGui.Text("FPS: " + fps_val.ToString("n0"));
// //				physics_times.Draw("", "Physics: " + physics_fps_val.ToString("n0"));
// //				ImGui.Text("Physics: " + physics_fps_val.ToString("n0"));
// 				if (ImGui.IsMousePosValid())
// 					ImGui.Text("Mouse Position: (" + io.MousePos.X.ToString("n1") + ", " + io.MousePos.Y.ToString("n1") + ")");
// 				else
// 					ImGui.Text("Mouse Position: <invalid>");

// 				ImGui.Separator();
// 				ImGui.Text("(right-click to change position)");
// 				if (ImGui.BeginPopupContextWindow())
// 				{
// 					if (ImGui.MenuItem("Top-left", null, corner == 0)) corner = 0;
// 					if (ImGui.MenuItem("Top-right", null, corner == 1)) corner = 1;
// 					if (ImGui.MenuItem("Bottom-left", null, corner == 2)) corner = 2;
// 					if (ImGui.MenuItem("Bottom-right", null, corner == 3)) corner = 3;
// 					ImGui.Separator();
// 					if (ImGui.MenuItem("Hide Ovarlay")) showOverlayWindow = false;
// 					if (ImGui.MenuItem("Show Debug Window", null, showDebugWindow)) showDebugWindow = !showDebugWindow;
// 					if (ImGui.MenuItem("Show ImGUI Demo Window", null, showDemoWindow)) showDemoWindow = !showDemoWindow;
// 					ImGui.Separator();
// 					if (ImGui.MenuItem("Exit")) Application.Quit();
// 					ImGui.EndPopup();
// 				}
// 			}
// 			ImGui.End();
// 		}

		
		int numFramesOnChart = 100;

		float flipperAcc = float.MaxValue;
		float flipperOffScale = float.MaxValue;
		float flipperOnNearEndScale = float.MaxValue;
		float flipperNumOfDegreeNearEnd = float.MaxValue;
		float flipperMass = float.MaxValue;

		private void OnDebugPlot(float [] arr, bool drawSpeed, float scale)
		{
			if (arr.Length < 3)
				arr = new float[3] { 0, 0, 0 };

			if (drawSpeed)
			{
				float[] speed = new float[arr.Length - 1];
				for (int i = 0; i < speed.Length; ++i)
					speed[i] = (arr[i + 1] - arr[i]) * scale;
				arr = speed;
			}
			float scale_min = 3.402823466e+38F;
			float scale_max = 3.402823466e+38F;
			int len = math.min(numFramesOnChart, arr.Length);
			ImGui.PlotLines("", ref arr[0], len, 0, "", scale_min, scale_max, new System.Numerics.Vector2(0, 50.0f));
		}

		

		//private void OnDebugSubmit(DebugFlipperData dfd)
		//{
		//	if (ImGui.TreeNode(dfd.Name))
		//	{
		//		if (dfd.SolenoidSate == -1)
		//			ImGui.TextColored(new System.Numerics.Vector4(1.0f, 0.0f, 0.0f, 1.0f), "Solenoid Off");
		//		else
		//		if (dfd.SolenoidSate == 1)
		//			ImGui.TextColored(new System.Numerics.Vector4(0.0f, 1.0f, 0.0f, 1.0f), "Solenoid On");
		//		else
		//			ImGui.TextColored(new System.Numerics.Vector4(1.0f, 1.0f, 0.0f, 1.0f), "Solenoid --");

		//		OnDebugPlot(dfd.SolenoidOnAngles.ToArray(), true,  10*math.PI / 180.0f);
		//		OnDebugPlot(dfd.SolenoidOffAngles.ToArray(), true,  10*math.PI / 180.0f);

		//		ImGui.TreePop();
		//		ImGui.Separator();
		//	}
		//}

		
		
		private void SubmitUI()
		{
			// here we create main debug window
			if (debugUI.Draw())
            {
				foreach (var component in GetComponentsInChildren<IDebugImGUI>(true))
				{
					component.OnDebug();
				}

				ImGui.End();
			}
		}


		[RuntimeInitializeOnLoadMethod]
		public static void OnLoadSetup()
		{
		}		

	}

	public interface IDebugImGUI
	{
		void OnDebug();
	}
}
