using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TextureUtilities
{
    public class TextureUtility
    {
        public static void SplitR(Texture2D texture2D, ref RenderTexture renderTexture) => RGBASplit(texture2D, ref renderTexture, 0);
        public static void SplitG(Texture2D texture2D, ref RenderTexture renderTexture) => RGBASplit(texture2D, ref renderTexture, 1);
        public static void SplitB(Texture2D texture2D, ref RenderTexture renderTexture) => RGBASplit(texture2D, ref renderTexture, 2);
        public static void SplitA(Texture2D texture2D, ref RenderTexture renderTexture) => RGBASplit(texture2D, ref renderTexture, 3);
        public static Vector2 MeasureMinMaxPixel(Texture source, int channelIndex) => MeasureMinMaxPixelValue(source, channelIndex);
        private static readonly string ComputeShaderPath = "Assets/TextureUtility/CS_TextureUtility.compute";
        
        private static readonly int MeasureSourceTexture = Shader.PropertyToID("_Measure_SourceTexture");
        private static readonly int MeasurePixelColor = Shader.PropertyToID("_Measure_PixelColor");
        private static readonly int GraphicsBufferLength = 4194304;
        private static Vector2 MeasureMinMaxPixelValue(Texture source, int channelIndex)
        {
            PixelColor[] pixelColors = new PixelColor[GraphicsBufferLength];
            GraphicsBuffer colorBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, pixelColors.Length, Marshal.SizeOf(typeof(PixelColor)));
            colorBuffer.SetData(pixelColors);
            ComputeShader cs = AssetDatabase.LoadAssetAtPath<ComputeShader>(ComputeShaderPath);
            cs.SetTexture(MeasureMinMaxPixelValueKernel, MeasureSourceTexture, source);
            cs.SetBuffer(MeasureMinMaxPixelValueKernel, MeasurePixelColor, colorBuffer);
            cs.Dispatch(MeasureMinMaxPixelValueKernel, 16 + source.width / 16, 16 + source.height / 16, 1);
            colorBuffer.GetData(pixelColors);
            List<float> value = new List<float>();
            for (int i = 0; i < pixelColors.Length; i++)
            {
                value.Add(GetPixelValueByIndex(pixelColors[i].Color, channelIndex));
            }
            colorBuffer.Release();
            Vector2 minMax = new Vector2(value.Min(), value.Max());
            Debug.Log(minMax);
            return minMax;
        }
        
        private static readonly int SourceTexture = Shader.PropertyToID("_RGBASplit_SourceTexture");
        private static readonly int TextureIO = Shader.PropertyToID("_RGBASplit_TextureIO");
        private static readonly int ChannelIndex = Shader.PropertyToID("_RGBASplit_ChannelIndex");
        private static void RGBASplit(Texture2D source, ref RenderTexture renderTexture, int channelIndex)
        {
            ComputeShader cs = AssetDatabase.LoadAssetAtPath<ComputeShader>(ComputeShaderPath);
            cs.SetTexture(RGBASplitKernel, SourceTexture, source);
            cs.SetTexture(RGBASplitKernel, TextureIO, renderTexture);
            cs.SetInt(ChannelIndex, channelIndex);
            cs.Dispatch(RGBASplitKernel, 16 + renderTexture.width / 16, 16 + renderTexture.height / 16, 1);
        }
        private static float GetPixelValueByIndex(Color color, int channelIndex)
        {
            return channelIndex switch
            {
                0 => color.r,
                1 => color.g,
                2 => color.b,
                3 => color.a,
                _ => 0f
            };
        }
        private static float GetPixelValueByIndex(Vector4 color, int channelIndex)
        {
            return channelIndex switch
            {
                0 => color.x,
                1 => color.y,
                2 => color.z,
                3 => color.w,
                _ => 0f
            };
        }
        private struct PixelColor
        {
            public Vector4 Color;
        }
        private static readonly int RGBASplitKernel = 0;
        private static readonly int MeasureMinMaxPixelValueKernel = 1;
    }
}

