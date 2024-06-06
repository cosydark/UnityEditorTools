using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace TextureUtilities
{
        public class TextureRemappingTool : EditorWindow
    {
        // Unreal Private
        private Object textureObject;
        private float offset = 0f;
        private float scale = 1f;
        // Real Private
        private Vector2 resolution = new Vector2(0, 0);
        private Vector2 valueRange = new Vector2(0, 0);
        private ComputeBuffer computeShader;
        private Channels channel;
        private Texture2D texture2D;
        private RenderTexture renderTexture;
        
        [MenuItem("TA/Texture Remapping Tool")]
        public static void ShowWindow()
        {
            var window = ScriptableObject.CreateInstance<TextureRemappingTool>() as TextureRemappingTool;
            window.minSize = new Vector2(720, 880);
            window.maxSize = new Vector2(720, 880);
            window.titleContent = new GUIContent("Texture Remapping Tool");
            window.ShowUtility();
        }

        private void OnEnable()
        {
            renderTexture = RenderTexture.GetTemporary(GetGrayValueRenderTextureDescriptor(new Vector2(256f, 256f)));
        }

        private void OnDestroy()
        {
            CoreUtils.Destroy(renderTexture);
        }

        private void Setup()
        {
            texture2D = textureObject as Texture2D;
            // if (texture2D is null)
            // {
            //     textureObject = null;
            //     return;
            // }
            resolution.x = texture2D.width;
            resolution.y = texture2D.height;
            renderTexture.Release();
            renderTexture = RenderTexture.GetTemporary(GetGrayValueRenderTextureDescriptor(resolution));
            renderTexture.filterMode = FilterMode.Point;
            switch (channel)
            {
                case Channels.R:
                    TextureUtility.SplitR(texture2D, ref renderTexture);
                    break;
                case Channels.G:
                    TextureUtility.SplitG(texture2D, ref renderTexture);
                    break;
                case Channels.B:
                    TextureUtility.SplitB(texture2D, ref renderTexture);
                    break;
                case Channels.A:
                    TextureUtility.SplitA(texture2D, ref renderTexture);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    
        private void OnGUI()
        {
            // GUI
            EditorGUILayout.Space(3);
                textureObject = EditorGUILayout.ObjectField("Target Texture", textureObject, typeof(Object), false);
            EditorGUILayout.Space(3);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(2);
            if (GUILayout.Button("Set up", GUILayout.Height(40), GUILayout.Width(140)))
            {
                if (texture2D is null) {return;}
                Setup();
            }
            EditorGUILayout.Space(3);
                if (GUILayout.Button("Measure Range", GUILayout.Height(40), GUILayout.Width(140)))
                {
                    if (texture2D is null) {return;}
                    valueRange = TextureUtility.MeasureMinMaxPixel(renderTexture, 0);
                }
            EditorGUILayout.Space(3);
                if (GUILayout.Button("Save", GUILayout.Height(40), GUILayout.Width(140)))
                {
                }
            EditorGUILayout.Space(3);
            channel = (Channels)EditorGUILayout.EnumPopup("", channel, GUILayout.Width(90));
            EditorGUILayout.Space(3);
                EditorGUILayout.LabelField($"Texture Info: <{resolution.x},{resolution.y}>", AddColor(Color.white));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(3);
                scale = EditorGUILayout.Slider("Scale", scale, 0, 1,GUILayout.Width(400));
            EditorGUILayout.Space(3);
                offset = EditorGUILayout.Slider("Offset", offset, 0, 1,GUILayout.Width(400));
            EditorGUILayout.Space(3);
                EditorGUILayout.LabelField($"Value Range: <{valueRange.x},{valueRange.y}>", AddColor(GetColorFromChannel(channel)));
                GUI.DrawTexture(new Rect(10, 150, 700, 700), renderTexture);
        }

        private RenderTextureDescriptor GetGrayValueRenderTextureDescriptor(Vector2 resolution)
        {
            return new RenderTextureDescriptor
            {
                width = (int)resolution.x,
                height = (int)resolution.y,
                volumeDepth = 1,
                dimension = TextureDimension.Tex2D,
                colorFormat = RenderTextureFormat.ARGBFloat,
                enableRandomWrite = true,
                msaaSamples = 1,
            };
        }
        private enum Channels {R ,G, B, A}

        private Color GetColorFromChannel(Channels channel)
        {
            return channel switch
            {
                Channels.R => Color.red,
                Channels.G => Color.green,
                Channels.B => Color.blue,
                Channels.A => Color.white,
                _ => Color.magenta
            };
        }
        private static GUIStyle AddColor(Color color)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.normal.textColor = color;
            return style;
        }
    }
}

