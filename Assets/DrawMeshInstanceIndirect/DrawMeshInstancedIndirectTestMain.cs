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

        /// <summary>
        /// C#, ComputeShader, レンダリング用のshaderで共通のstruct。
        /// </summary>
        private struct MyStruct
        {
            public Vector4 color;
            public Vector3 position;
            public Vector3 scale;
        }

        private int _kernel;

        // computeShaderとレンダリングで使用するバッファ。
        private ComputeBuffer _buffer;

        // DrawMeshInstancedIndirectに渡すバッファ。
        private ComputeBuffer _bufferWithArgs;
        private Bounds _bounds;

        private void Start()
        {
            SetUpComputeShader();
            CreateBuffer();
            CreateBounds();
            CreateBufferArgs();

            // computeShaderで使用するバッファをレンダリング用のシェーダーに渡す。CPUを介せずにバッファをやり取りするので高速。
            material.SetBuffer("_Buffer", _buffer);
            // バッファの内容を更新しないのでStart一度だけ呼び出す。
            computeShader.Dispatch(_kernel, countX, countY, countZ);
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
        /// Compute Shaderの設定
        /// </summary>
        private void SetUpComputeShader()
        {
            _kernel = computeShader.FindKernel("CSMain");
            computeShader.SetInt("CountX", countX);
            computeShader.SetInt("CountY", countY);
            computeShader.SetInt("CountZ", countZ);
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
            computeShader.SetBuffer(_kernel, "MyBuffer", _buffer);
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