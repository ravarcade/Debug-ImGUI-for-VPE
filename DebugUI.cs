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
    internal class DebugUIClient : IDebugUI
    {
		private DebugOverlay debugOverlay = new DebugOverlay();
		public bool showOverlayWindow = true;
		public bool showDebugWindow = true;
		public bool showDemoWindow = false;


		public FPSHelper _fps = new FPSHelper(true, 0, 100, "n1");
		public FPSHelper _physicsTicks = new FPSHelper(true, 0, 1500, "n0", 300);
		public ChartFloat _physicsTimes = new ChartFloat(100, 0.0f, 20.0f, 50);
		public float _phyTimeAccu = 0;
		public Dictionary<string, Entity> _flippers = new Dictionary<string, Entity>();
		public int _ballCounter = 0;
		public bool _enableManualBallRoller = false;

		private Player _player = null;
		public Player player { 
			get { 
				if (_player == null)
                {
					var players = GameObject.FindObjectsOfType<Player>();
					_player = players?[0];
				}
				return _player;
			} }
		
		private void ProcessDataOncePerFrame()
        {
			_fps.Tick();
			_physicsTimes.Add(_phyTimeAccu);
			_phyTimeAccu = 0.0f;
		}

		public bool Draw()
        {
			ProcessDataOncePerFrame();

			if (showOverlayWindow)
				debugOverlay.Draw(this);

			if (showDemoWindow)
				ImGui.ShowDemoWindow(ref showDemoWindow);

			if (showDebugWindow)
			{
				OnDebug();
			}

			return showDebugWindow;
		}

		void _OnDebugFlippers()
		{
			if (ImGui.TreeNode("Flippers"))
			{
				//ImGui_SliderFloat("Acceleration", ref flipperAcc, 0.1f, 3.0f);
				//ImGui_SliderFloat("Mass (log10)", ref flipperMass, -1.0f, 8.0f);
				//ImGui_SliderFloat("Off Scale", ref flipperOffScale, 0.01f, 1.0f);
				//ImGui_SliderFloat("On Near End Scale", ref flipperOnNearEndScale, 0.01f, 1.0f);
				//ImGui_SliderFloat("Num of degree near end", ref flipperNumOfDegreeNearEnd, 0.1f, 10.0f);
				ImGui.Separator();
				//ImGui.SliderInt("Num frames on chart", ref numFramesOnChart, 10, 500);
				//				player.physicsEngine?.OnDebugDraw();
				ImGui.TreePop();
				ImGui.Separator();
			}
		}

		private void OnDebug()
		{
			ImGui.SetNextWindowPos(new System.Numerics.Vector2(30, 20), ImGuiCond.FirstUseEver);
			ImGui.SetNextWindowSize(new System.Numerics.Vector2(350, 100), ImGuiCond.FirstUseEver);

			ImGui.Begin("Debug");
			ImGui.Text("Balls on table: " + _ballCounter.ToString("n0"));
			ImGui.Checkbox("ManualBallRoller", ref _enableManualBallRoller);
			if (_enableManualBallRoller && Input.GetMouseButton(0))
				ManualBallRoller();

			_OnDebugFlippers();

			if (ImGui.Button("Add Ball"))
			{
				player?.CreateBall(new DebugBallCreator());
				++_ballCounter;

			}

#if UNITY_EDITOR
			if (ImGui.Button("Add Ball & Pause"))
			{
				player?.CreateBall(new DebugBallCreator());
				++_ballCounter;
				EditorApplication.isPaused = true;
			}
#endif
			if (ImGui.Button("Exit"))
			{
				Application.Quit();
			}
		}

		Camera camera = null;

		private void ManualBallRoller()
		{
			if (camera == null)
			{
				camera = GameObject.FindObjectOfType<Camera>();
			}

			Ray ray = camera.ScreenPointToRay(Input.mousePosition);
			const float epsilon = 0.0001f;
			float dist = math.abs(ray.direction.y) > epsilon ? ray.origin.y / ray.direction.y : 0.0f;
			if (dist < epsilon)
			{
				var p = ray.origin - ray.direction * dist;
				//player.physicsEngine?.ManualBallRoller(p);
			}
		}

		// ================================================================ IDebugUI ===
		public void OnRegisterFlipper(Entity entity, string name)
        {
			_flippers[name] = entity;
        }

		public void OnPhysicsUpdate()
        {
			_physicsTicks.Tick();
		}

		public void PhysicsFrameProcessingTime(float t)
        {
			_phyTimeAccu += t;
		}

		// ================================================================== Helpers ===
		void ImGui_SliderFloat(string label, ref float val, float min, float max)
		{
			if (!ImGui.SliderFloat(label, ref val, min, max))
				val = float.MaxValue;

		}

	}
}