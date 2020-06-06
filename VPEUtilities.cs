
using UnityEngine;
using VisualPinball.Unity.Game;

namespace  VisualPinball.Engine.Unity.ImgGUI
{
    /// <summary>
    /// All tasks reuiring use of Visual.Pinball.Unity core, go thru this.
    /// </summary>
    public class VPEUtilities 
    {
        DebugUI _debugUI;
        Player _player;

        public VPEUtilities(DebugUI debugUI) 
        {
            _debugUI = debugUI;
            _player = GameObject.FindObjectsOfType<Player>()?[0];
            
        }
        
        public void CreateBall()
        {
            _player?.CreateBall(new DebugBallCreator());
        }

    }

}