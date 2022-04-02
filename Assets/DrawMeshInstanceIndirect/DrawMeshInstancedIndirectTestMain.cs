using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace GPUInstancingTest.DrawMeshInstancedIndirect
{
    public class DrawMeshInstancedIndirectTestMain : MonoBehaviour
    {
        [SerializeField] private Mesh mesh;
        [SerializeField] private Material material;

        [SerializeField] private ComputeShader computeShader;
        [SerializeField] private int countX;
        [SerializeField] private int countY;
        [SerializeField] private int countZ;

        private struct MyStruct
        {
            public float red;
            public float green;
            public float blue;
        }

        private int _kernel;
        private ComputeBuffer _buffer;

        // DrawMeshInstancedIndirectに渡すバッファ。
        private ComputeBuffer _bufferWithArgs;
        private Bounds _bounds;

        private void Start()
        {
            _kernel = computeShader.FindKernel("CSMain");

            CreateBuffer();
            CreateBounds();
            CreateBufferArgs();
        }

        private void Update()
        {
            // 毎フレーム呼び出す必要がある
            Graphics.DrawMeshInstancedIndirect(mesh, 0, material, _bounds, _bufferWithArgs);
        }

        private void OnDestroy()
        {
            _buffer.Dispose();
            _bufferWithArgs.Dispose();
        }

        /// <summary>
        /// バッファの作成＆ComputeShaderと紐付け
        /// </summary>
        private void CreateBuffer()
        {
            var count = countX * countY * countZ;
            // バッファを作成。
            _buffer = new ComputeBuffer(count, Marshal.SizeOf<MyStruct>());
            // 作成したバッファをComputeShaderのバッファと紐づける
            computeShader.SetBuffer(_kernel, "ColorBuffer", _buffer);
        }

        private void CreateBounds()
        {
            _bounds = new Bounds(Vector3.zero, Vector3.one * 30);
        }

        private void CreateBufferArgs()
        {
            // uint型で要素数が5の配列を渡す決まり。
            uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

            // 0番目はメッシュの頂点数
            args[0] = mesh.GetIndexCount(0);
            // 1番目はインスタンス数
            args[1] = (uint)(countX * countY * countZ);
            // 2番目以降は0でいいらしい。何も設定するかがイマイチわからない。
            
            _bufferWithArgs = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
            _bufferWithArgs.SetData(args);
        }
    }
}