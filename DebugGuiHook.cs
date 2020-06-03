using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using ImGuiNET;
using Unity.Entities;
using VisualPinball.Unity.Game;
using VisualPinball.Unity.Physics.DebugUI;
using VisualPinball.Unity.VPT.Table;

namespace VisualPinball.Engine.Unity.ImgGUI
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
		}

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

		public DebugUIClient debugUI;

		private void SubmitUI()
		{
			// here we create main debug window
			if (debugUI != null && debugUI.Draw())
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
