using UnityEngine;
using Unity.Mathematics;
using VisualPinball.Unity.Game;
using VisualPinball.Unity.VPT.Table;

namespace VisualPinball.Engine.Unity.ImgGUI
{
    /// <summary>
    /// All tasks requiring use of Visual.Pinball.Unity core, goes thru this.
    /// Now implemented:
    /// - CreateBall
    /// </summary>
    public class VPEUtilities
    {
        DebugUI _debugUI;
        Player _player;
        Camera _camera = null;
        FreeCam _freeCam = new FreeCam();
        Matrix4x4 _worldToLocal;

        public VPEUtilities(DebugUI debugUI, TableBehavior tableBehavior)
        {
            _debugUI = debugUI;
            _player = GameObject.FindObjectsOfType<Player>()?[0];
            _worldToLocal = tableBehavior.gameObject.transform.worldToLocalMatrix;            
        }

        public void CreateBall()
        {
            _player?.CreateBall(new DebugBallCreator());
        }

        public void CreateBall(float x, float y)
        {
            _player?.CreateBall(new DebugBallCreator(x, y));
        }

        public void CreateBallAtClick()
        {
            float3 p;
            if (GetLocalClickCoords(out p))
            {
                CreateBall(p.x, p.y);
            } else
            {
                CreateBall();
            }
        }

        public bool GetClickCoords(out float3 p)
        {
            if (_camera == null)
                _camera = GameObject.FindObjectOfType<Camera>();

            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            const float epsilon = 0.0001f;
            float dist = math.abs(ray.direction.y) > epsilon ? ray.origin.y / ray.direction.y : 0.0f;
            if (dist < epsilon)
            {
                p = ray.origin - ray.direction * dist;
                return true;
            }

            p = float3.zero;
            return false;
        }

        public bool GetLocalClickCoords(out float3 p)
        {
            bool ret = GetClickCoords(out p);
            if (ret)
                p = ToLocal(p);

            return ret;
        }

        public float3 ToLocal(float3 c)
        {
            return _worldToLocal.MultiplyPoint(c);
        }

        public void FreeCam(bool enableFreeCam)
        {
            if (_camera == null)
                _camera = GameObject.FindObjectOfType<Camera>();
            
            _freeCam.Update(_camera.transform, enableFreeCam);            
        }

    }

}