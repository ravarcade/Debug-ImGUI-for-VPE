using UnityEngine;
using UnityEngine.Rendering;
using Unity.Profiling;
using System;
using System.Runtime.InteropServices;

namespace ImGuiNET.Unity
{
    // This component is responsible for setting up ImGui for use in Unity.
    // It holds the necessary context and sets it up before any operation is done to ImGui.
    // (e.g. set the context, texture and font managers before calling Layout)

    /// <summary>
    /// Dear ImGui integration into Unity
    /// </summary>
    public class DearImGui : MonoBehaviour
    {
        ImGuiUnityContext _context;
        IImGuiRenderer _renderer;
        IImGuiPlatform _platform;
        CommandBuffer _cmd;
        bool _usingURP;

        public event System.Action Layout;  // Layout event for *this* ImGui instance
        [SerializeField] bool _doGlobalLayout = true; // do global/default Layout event too

        [SerializeField] Camera _camera = null;
        [SerializeField] RenderImGuiFeature _renderFeature = null;

        [SerializeField] RenderUtils.RenderType _rendererType = RenderUtils.RenderType.Mesh;
        [SerializeField] Platform.Type _platformType = Platform.Type.InputManager;

        [Header("Configuration")]
        [SerializeField] IOConfig _initialConfiguration = default;
        [SerializeField] FontAtlasConfigAsset _fontAtlasConfiguration = null;
        [SerializeField] IniSettingsAsset _iniSettings = null;  // null: uses default imgui.ini file

        [Header("Customization")]
        [SerializeField] ShaderResourcesAsset _shaders = null;
        [SerializeField] StyleAsset _style = null;
        [SerializeField] CursorShapesAsset _cursorShapes = null;

        const string CommandBufferTag = "DearImGui";
        static readonly ProfilerMarker s_prepareFramePerfMarker = new ProfilerMarker("DearImGui.PrepareFrame");
        static readonly ProfilerMarker s_layoutPerfMarker = new ProfilerMarker("DearImGui.Layout");
        static readonly ProfilerMarker s_drawListPerfMarker = new ProfilerMarker("DearImGui.RenderDrawLists");

        void Awake()
        {
            _context = ImGuiUn.CreateUnityContext();
        }

        void OnDestroy()
        {
            ImGuiUn.DestroyUnityContext(_context);
        }

        /// <summary>
        /// Manualy create assets with shaders and cursor textures for ImGUI.
        /// </summary>
        void _CreateAssetResources()
        {
            if (_camera == null)
                _camera = Camera.main;

            _initialConfiguration.SetDefaults();
            
            if (_shaders == null)
            {                
                _shaders = ScriptableObject.CreateInstance<ShaderResourcesAsset>();
                _shaders.shaders = new ShaderResourcesAsset.Shaders();
                _shaders.propertyNames = new ShaderResourcesAsset.PropertyNames();

                _shaders.shaders.mesh = Shader.Find("DearImGui/Mesh");
                _shaders.shaders.procedural = Shader.Find("DearImGui/Procedural");

                _shaders.propertyNames.baseVertex = "_BaseVertex";
                _shaders.propertyNames.vertices = "_Vertices";
                _shaders.propertyNames.tex = "_Tex";
            }

            if (_cursorShapes == null)
            {
                _cursorShapes = ScriptableObject.CreateInstance<CursorShapesAsset>();
                _cursorShapes.Arrow.texture = Resources.Load<Texture2D>("Cursors/dmz-white/left_ptr");
                _cursorShapes.Arrow.hotspot = new Vector2(7, 4);

                _cursorShapes.TextInput.texture = Resources.Load<Texture2D>("Cursors/dmz-white/xterm");
                _cursorShapes.TextInput.hotspot = new Vector2(11, 11);

                _cursorShapes.ResizeAll.texture = Resources.Load<Texture2D>("Cursors/dmz-white/move");
                _cursorShapes.ResizeAll.hotspot = new Vector2(4, 5);

                _cursorShapes.ResizeEW.texture = Resources.Load<Texture2D>("Cursors/dmz-white/sb_h_double_arrow");
                _cursorShapes.ResizeEW.hotspot = new Vector2(11, 11);

                _cursorShapes.ResizeNS.texture = Resources.Load<Texture2D>("Cursors/dmz-white/sb_v_double_arrow");
                _cursorShapes.ResizeNS.hotspot = new Vector2(11, 11);

                _cursorShapes.ResizeNESW.texture = Resources.Load<Texture2D>("Cursors/dmz-white/fd_double_arrow");
                _cursorShapes.ResizeNESW.hotspot = new Vector2(11, 11);


                _cursorShapes.ResizeNWSE.texture = Resources.Load<Texture2D>("Cursors/dmz-white/bd_double_arrow");
                _cursorShapes.ResizeNWSE.hotspot = new Vector2(11, 11);

                _cursorShapes.Hand.texture = Resources.Load<Texture2D>("Cursors/dmz-white/hand2");
                _cursorShapes.Hand.hotspot = new Vector2(9, 5);

                _cursorShapes.NotAllowed.texture = Resources.Load<Texture2D>("Cursors/dmz-white/crossed_circle");
                _cursorShapes.NotAllowed.hotspot = new Vector2(11, 11);
            }
        }

        void _AddFonts()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            var fnt = Resources.Load<Font>("Fonts/Roboto-Medium");
            var bin = (TextAsset)Resources.Load("Fonts/Roboto-Medium");
            GCHandle pinnedArray = GCHandle.Alloc(bin.bytes, GCHandleType.Pinned);
            IntPtr pBytes = pinnedArray.AddrOfPinnedObject();
            io.Fonts.AddFontFromMemoryTTF(pBytes, ((TextAsset)bin).bytes.Length, 18.0f);
            io.Fonts.Build();
        }

        void OnEnable() // No OnEnable() default init.... delayed only
        {
            _CreateAssetResources();

            _usingURP = RenderUtils.IsUsingURP();
            if (_camera == null) Fail(nameof(_camera));
            if (_renderFeature == null && _usingURP) Fail(nameof(_renderFeature));

            _cmd = RenderUtils.GetCommandBuffer(CommandBufferTag);
            if (_usingURP)
                _renderFeature.commandBuffer = _cmd;
            else
                _camera.AddCommandBuffer(CameraEvent.AfterEverything, _cmd);

            ImGuiUn.SetUnityContext(_context);
            ImGuiIOPtr io = ImGui.GetIO();

            _initialConfiguration.ApplyTo(io);
            _style?.ApplyTo(ImGui.GetStyle());

            _context.textures.BuildFontAtlas(io, _fontAtlasConfiguration);
            _context.textures.Initialize(io);

            SetPlatform(Platform.Create(_platformType, _cursorShapes, _iniSettings), io);
            SetRenderer(RenderUtils.Create(_rendererType, _shaders, _context.textures), io);
            if (_platform == null) Fail(nameof(_platform));
            if (_renderer == null) Fail(nameof(_renderer));
            //_AddFonts();
           
            void Fail(string reason)
            {
                OnDisable();
                enabled = false;
                throw new System.Exception($"Failed to start: {reason}");
            }
        }

        void OnDisable()
        {
            ImGuiUn.SetUnityContext(_context);
            ImGuiIOPtr io = ImGui.GetIO();

            SetRenderer(null, io);
            SetPlatform(null, io);

            ImGuiUn.SetUnityContext(null);

            _context.textures.Shutdown();
            _context.textures.DestroyFontAtlas(io);

            if (_usingURP)
            {
                if (_renderFeature != null)
                    _renderFeature.commandBuffer = null;
            }
            else
            {
                if (_camera != null)
                    _camera.RemoveCommandBuffer(CameraEvent.AfterEverything, _cmd);
            }

            if (_cmd != null)
                RenderUtils.ReleaseCommandBuffer(_cmd);
            _cmd = null;
        }

        void Reset()
        {
            _camera = Camera.main;
            _initialConfiguration.SetDefaults();
        }

        public void Reload()
        {
            OnDisable();
            OnEnable();
        }

        void Update()
        {
            ImGuiUn.SetUnityContext(_context);
            ImGuiIOPtr io = ImGui.GetIO();

            s_prepareFramePerfMarker.Begin(this);
            _context.textures.PrepareFrame(io);
            _platform.PrepareFrame(io, _camera.pixelRect);
            ImGui.NewFrame();
            s_prepareFramePerfMarker.End();

            s_layoutPerfMarker.Begin(this);
            try
            {
                if (_doGlobalLayout)
                    ImGuiUn.DoLayout();   // ImGuiUn.Layout: global handlers
                Layout?.Invoke();     // this.Layout: handlers specific to this instance
            }
            finally
            {
                ImGui.Render();
                s_layoutPerfMarker.End();
            }

            s_drawListPerfMarker.Begin(this);
            _cmd.Clear();
            _renderer.RenderDrawLists(_cmd, ImGui.GetDrawData());
            s_drawListPerfMarker.End();
        }

        void SetRenderer(IImGuiRenderer renderer, ImGuiIOPtr io)
        {
            _renderer?.Shutdown(io);
            _renderer = renderer;
            _renderer?.Initialize(io);
        }

        void SetPlatform(IImGuiPlatform platform, ImGuiIOPtr io)
        {
            _platform?.Shutdown(io);
            _platform = platform;
            _platform?.Initialize(io);
        }
    }
}
