using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using ImGuiNET;
using System.Linq;

namespace VisualPinball.Engine.Unity.ImgGUI
{

    public class DebugProperties
    {
        Dictionary<Type, Func<IProperty>> _ctors = new Dictionary<Type, Func<IProperty>>()
        {
            // here add types to display properties
            { typeof(Vector3), () => { return new PropVector3() as IProperty; } },
            { typeof(float3), () => { return new PropFloat3() as IProperty; } },
            { typeof(float), () => { return new PropFloat() as IProperty; } },
            { typeof(int), () => { return new PropInt() as IProperty; } },
            { typeof(Quaternion), () => { return new PropQuaternion() as IProperty; } },
            { typeof(Matrix4x4), () => { return new PropMatrix4x4() as IProperty; } },
        };

        List<TreeNode> _propTree = new List<TreeNode>();
        TreeNode _root = new TreeNode();
        Dictionary<string, int> _quick = new Dictionary<string, int>();
        int _quick_idx = -1;        

        public int AddProperty<T>(int parent, string name, T currentValue, string tip)
        {
            int idx = _propTree.Count;

            var pe = _CreateNewProperty<T>();
            pe.Init(idx, name, tip);
            _GetNode(parent).AddChild(ref _propTree, pe);

            if (_isTypeSupported<T>())
                pe.SetValue(currentValue);

            return idx;
        }

        public bool GetValue<T>(int idx, ref T val)
        {
            if (idx >= 0 && idx < _propTree.Count)
            {
                var pe = _GetNode(idx).prop as Prop<T>;
                
                return pe.GetValue(ref val);
            }
            return false;
        }

        public void SetValue<T>(int idx, T val)
        {
            if (idx >= 0 && idx < _propTree.Count)
            {
                var pe = _GetNode(idx).prop as Prop<T>;
                pe.SetValue(val);
            }
        
        }
        public bool QuickPropertySync<T>(string name, ref T value, string tip)
        {
            Prop<T> pe;
            int idx;

            string uniq = "_Quick_[" + name + "]_[" + typeof(T).FullName + "]";
            if (_quick.ContainsKey(uniq))
            {
                idx = _quick[uniq];
                pe = _GetNode(idx).prop as Prop<T>;
                return pe.GetValue(ref value);
            }

            if (_quick_idx == -1) // add quick node
            {
                _quick_idx = AddProperty(-1, "Quick", this, null);
            }

            idx = AddProperty(_quick_idx, name, value, tip);
            _quick[uniq] = idx;
            pe = _GetNode(idx).prop as Prop<T>;
            pe.SetValue(value);

            return false;
        }

        public void Draw()
        {
            if (_propTree.Count == 0)
                return;

            var nodes = _propTree.ToArray();

            if (_quick_idx != -1)
            {
                var quickNode = _GetNode(_quick_idx);
                if (quickNode.firstChild != -1 && ImGui.TreeNodeEx("_[" + _quick_idx + "]_Quick", ImGuiTreeNodeFlags.DefaultOpen, "Quick"))
                {
                    _Horizontal(ref nodes, quickNode.firstChild, 1);
                    ImGui.TreePop();
                    ImGui.Separator();
                }
            }

            _Horizontal(ref nodes, 0, 0);
        }


        // ==================================================================== ====

        Prop<T> _CreateNewProperty<T>()
        {
            var prop = _isTypeSupported<T>() ?
                _ctors[typeof(T)]() as Prop<T>
                : new Prop<T>();

            return prop;
        }

        public bool _isTypeSupported<T>()
        {
            return _ctors.ContainsKey(typeof(T));
        }

        ref TreeNode _GetNode(int idx)
        {
            if (idx < 0 || idx >= _propTree.Count)
                return ref _root;

            return ref _propTree.ToArray()[idx];
        }

        void _Horizontal(ref TreeNode[] nodes, int i, int level)
        {
            while (i != -1)
            {
                if (level != 0 || i != _quick_idx) // skip quick idx
                {
                    if (nodes[i].prop.Draw())
                    {
                        if (nodes[i].firstChild != -1)
                            _Horizontal(ref nodes, nodes[i].firstChild, level + 1);

                        ImGui.TreePop();
                        if (level == 0)
                            ImGui.Separator();
                    }
                }
                i = nodes[i].sibling;
            }
        }

        // ==================================================================== ====

        class TreeNode
        {
            public IProperty prop;
            public int firstChild;
            public int sibling;
            int lastChild;

            public TreeNode()
            {
                // root only
                prop = null;
                firstChild = -1;
                lastChild = -1;
                sibling = -1;
            }

            private TreeNode(IProperty _prop)
            {
                sibling = -1;
                firstChild = -1;
                lastChild = -1;
                prop = _prop;
            }

            public void AddChild(ref List<TreeNode> tree, IProperty _prop)
            {
                var node = new TreeNode(_prop);
                var nodes = tree.ToArray();
                int idx = tree.Count;

                if (lastChild != -1)
                    nodes[lastChild].sibling = idx;
                else
                    firstChild = idx;
                lastChild = idx;

                tree.Add(node);
            }
        }
    }

    internal interface IProperty
    {
        bool Draw();
    }

    public static class ConversionExtensions
    {
        public static System.Numerics.Vector3 ToImGui(this float3 v) => new System.Numerics.Vector3(v.x, v.y, v.z);
        public static System.Numerics.Vector3 ToImGui(this Vector3 v) => new System.Numerics.Vector3(v.x, v.y, v.z);
        public static Vector3 ToVector3(this System.Numerics.Vector3 v) => new Vector3(v.X, v.Y, v.Z);
        public static Vector3 ToFloat3(this System.Numerics.Vector3 v) => new float3(v.X, v.Y, v.Z);
    }
}