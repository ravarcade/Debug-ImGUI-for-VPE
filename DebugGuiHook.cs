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
using Unity.Mathematics;
using Unity.Transforms;
using VisualPinball.Unity.Physics;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class ReflectionHelpers
 {
	 public static System.Type[] GetAllDerivedTypes(this System.AppDomain aAppDomain, System.Type aType)
	 {
		 var result = new List<System.Type>();
		 var assemblies = aAppDomain.GetAssemblies();
		 foreach (var assembly in assemblies)
		 {
			 var types = assembly.GetTypes();
			 foreach (var type in types)
			 {
				 if (type.IsSubclassOf(aType))
					 result.Add(type);
			 }
		 }
		 return result.ToArray();
	 }
 }

namespace DebugGui
{

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
		}

		private void Start()
		{
			RegisterDebugCallback(new DebugCallback(DebugMethod));
			StartCoroutine("CallPluginAtEndOfFrames");
			var players = GameObject.FindObjectsOfType<Player>();
			player = players?[0];
			player.physicsEngine.PushUI_PhysicsProcessingTime += OnPhysicsProcessingTime;
			player.physicsEngine.PushUI_DebugFlipperData += OnDebugSubmit;
			player.physicsEngine.GetUI_Float += GetUI_Float;
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

		private bool showOverlayWindow = true;
		private bool showDebugWindow = true;
		private int corner = 0;

		private float fps_accu = 0;
		private int fps_frames = 0;
		private float fps_val = 0;
		private int physics_fps_start = 0;
		private float physics_fps_val = 0;

		internal class ChartFloat
		{
			float[] _data;
			int _idx;
			int _size;
			float _max;
			float _min;
			int _height;
			public ChartFloat(int size, float min, float max, int height)
			{
				_size = size;
				_data = new float[size * 2];
				_idx = 0;
				_min = min;
				_max = max;
				_height = height;
			}

			public void Add(float val)
			{
				++_idx;
				if (_idx >= _size)
					_idx -= _size;
				_data[_idx] = _data[_idx + _size] = val;
			}
			public void Draw(string label, string overlay_text)
			{
				//  public static void PlotLines(string label, ref float values, int values_count, int values_offset, string overlay_text, float scale_min, float scale_max, Vector2 graph_size)

				ImGui.PlotLines(label, ref _data[_idx+1], _size, 0, overlay_text, _min, _max, new System.Numerics.Vector2(0, _height));
			}
		};

		private ChartFloat physics_times = new ChartFloat(100, 0.0f, 16.6f, 50);
		void OnPhysicsProcessingTime(float val)
		{
			physics_times.Add(val);
		}
		private void UpdateFPS()
		{
			++fps_frames;
			fps_accu += Time.deltaTime;

			const float updateInterval = 0.5F;
			if (fps_accu > updateInterval)
			{
				int physicframe = 0;
				if (player.physicsEngine != null)
					physicframe = player.physicsEngine.GetFrameCount();
				physics_fps_val = (physicframe - physics_fps_start) / fps_accu;
				fps_val = fps_frames / fps_accu;
				fps_accu = 0;
				fps_frames = 0;
				physics_fps_start = physicframe;
			}
		}

		private void OnOverlay()
		{
			UpdateFPS();
			if (!showOverlayWindow)
				return;

			const float DISTANCE = 10.0f;
			var io = ImGui.GetIO();
			var window_pos = new System.Numerics.Vector2((corner & 1) != 0 ? io.DisplaySize.X - DISTANCE : DISTANCE, (corner & 2) != 0 ? io.DisplaySize.Y - DISTANCE : DISTANCE);
			var window_pos_pivot = new System.Numerics.Vector2((corner & 1) != 0 ? 1.0f : 0.0f, (corner & 2) != 0 ? 1.0f : 0.0f);
			ImGui.SetNextWindowPos(window_pos, ImGuiCond.Always, window_pos_pivot);
			ImGui.SetNextWindowBgAlpha(0.35f); // Transparent background

			if (ImGui.Begin("Simple overlay", ref showOverlayWindow, (corner != -1 ? ImGuiWindowFlags.NoMove : 0) | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoNav))
			{
				ImGui.Text("FPS: " + fps_val.ToString("n0"));
				physics_times.Draw("", "Physics: " + physics_fps_val.ToString("n0"));
//				ImGui.Text("Physics: " + physics_fps_val.ToString("n0"));
				if (ImGui.IsMousePosValid())
					ImGui.Text("Mouse Position: (" + io.MousePos.X.ToString("n1") + ", " + io.MousePos.Y.ToString("n1") + ")");
				else
					ImGui.Text("Mouse Position: <invalid>");
				ImGui.Separator();
				ImGui.Text("(right-click to change position)");
				if (ImGui.BeginPopupContextWindow())
				{
					if (ImGui.MenuItem("Top-left", null, corner == 0)) corner = 0;
					if (ImGui.MenuItem("Top-right", null, corner == 1)) corner = 1;
					if (ImGui.MenuItem("Bottom-left", null, corner == 2)) corner = 2;
					if (ImGui.MenuItem("Bottom-right", null, corner == 3)) corner = 3;
					ImGui.Separator();
					if (ImGui.MenuItem("Hide Ovarlay")) showOverlayWindow = false;
					if (ImGui.MenuItem("Show Debug Window", null, showDebugWindow)) showDebugWindow = !showDebugWindow;
					if (ImGui.MenuItem("Show ImGUI Demo Window", null, showDemoWindow)) showDemoWindow = !showDemoWindow;
					ImGui.Separator();
					if (ImGui.MenuItem("Exit")) Application.Quit();
					ImGui.EndPopup();
				}
			}
			ImGui.End();
		}

		int ballCounter = 0;
		int numFramesOnChart = 100;

		float flipperAcc = float.MaxValue;
		float flipperOffScale = float.MaxValue;
		float flipperOnNearEndScale = float.MaxValue;
		float flipperNumOfDegreeNearEnd = float.MaxValue;
		float flipperMass = float.MaxValue;

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
				player.physicsEngine.ManualBallRoller(p);
			}
		}


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

		

		private void OnDebugSubmit(DebugFlipperData dfd)
		{
			if (ImGui.TreeNode(dfd.Name))
			{
				if (dfd.SolenoidSate == -1)
					ImGui.TextColored(new System.Numerics.Vector4(1.0f, 0.0f, 0.0f, 1.0f), "Solenoid Off");
				else
				if (dfd.SolenoidSate == 1)
					ImGui.TextColored(new System.Numerics.Vector4(0.0f, 1.0f, 0.0f, 1.0f), "Solenoid On");
				else
					ImGui.TextColored(new System.Numerics.Vector4(1.0f, 1.0f, 0.0f, 1.0f), "Solenoid --");

				OnDebugPlot(dfd.SolenoidOnAngles.ToArray(), true,  10*math.PI / 180.0f);
				OnDebugPlot(dfd.SolenoidOffAngles.ToArray(), true,  10*math.PI / 180.0f);

				ImGui.TreePop();
				ImGui.Separator();
			}
		}

		void ImGui_SliderFloat(string label, ref float val, float min, float max)
		{
			if (!ImGui.SliderFloat(label, ref val, min, max))
				val = float.MaxValue;
			
		}
		void _OnDebugFlippers()
		{
			if (ImGui.TreeNode("Flippers"))
			{
				ImGui_SliderFloat("Acceleration", ref flipperAcc, 0.1f, 3.0f);
				ImGui_SliderFloat("Mass (log10)", ref flipperMass, -1.0f, 8.0f);
				ImGui_SliderFloat("Off Scale", ref flipperOffScale, 0.01f, 1.0f);
				ImGui_SliderFloat("On Near End Scale", ref flipperOnNearEndScale, 0.01f, 1.0f);
				ImGui_SliderFloat("Num of degree near end", ref flipperNumOfDegreeNearEnd, 0.1f, 10.0f);
				ImGui.Separator();
				ImGui.SliderInt("Num frames on chart", ref numFramesOnChart, 10, 500);
				player.physicsEngine.OnDebugDraw();
				ImGui.TreePop();
				ImGui.Separator();
			}
		}

		private void OnDebug()
		{

			ImGui.Begin("Debug");
			ImGui.Text("Balls on table: " + ballCounter.ToString("n0"));
			ImGui.Checkbox("ManualBallRoller", ref enableManualBallRoller);
			if (enableManualBallRoller && Input.GetMouseButton(0))
				ManualBallRoller();

			_OnDebugFlippers();

			if (ImGui.Button("Add Ball"))
			{
				player?.CreateBall(new DebugBallCreator());
				++ballCounter;

			}

#if UNITY_EDITOR
			if (ImGui.Button("Add Ball & Pause"))
			{
				player?.CreateBall(new DebugBallCreator());
				++ballCounter;
				EditorApplication.isPaused = true;
			}
#endif
			if (ImGui.Button("Exit"))
			{
				Application.Quit();
			}

			foreach (var component in GetComponentsInChildren<IDebugImGUI>(true))
			{
				component.OnDebug();
			}

			ImGui.End();
		}

		private void SubmitUI()
		{
			// here we create main debug window
			ImGui.SetNextWindowPos(new System.Numerics.Vector2(30, 20), ImGuiCond.FirstUseEver);
			ImGui.SetNextWindowSize(new System.Numerics.Vector2(350, 100), ImGuiCond.FirstUseEver);

			OnOverlay();

			if (showDebugWindow)
				OnDebug();

			if (showDemoWindow)
				ImGui.ShowDemoWindow(ref showDemoWindow);
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


	// =============
	internal class DebugBallCreator : IBallCreationPosition
	{
		public Vertex3D GetBallCreationPosition(Table table)
		{
			return new Vertex3D(UnityEngine.Random.Range(table.Width / 4f, table.Width / 4f * 3f), UnityEngine.Random.Range(table.Height / 5f, table.Height / 2f), UnityEngine.Random.Range(0, 200f));
		}

		public Vertex3D GetBallCreationVelocity(Table table)
		{
			// no velocity
			return Vertex3D.Zero;
		}

		public void OnBallCreated(PlayerPhysics physics, Ball ball)
		{
			// nothing to do
		}
	}

}
