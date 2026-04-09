using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FXV.ShieldEditorUtils
{
    public partial class fxvIntroWindow : EditorWindow
    {
#pragma warning disable CS0414
        static string urpVersion = "version 2.2.7";
#pragma warning restore CS0414

        void GUI_URP_AfterImport()
        {
            UniversalRenderPipelineAsset currentRP = (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;

            GUILine(Color.gray);
            GUILayout.BeginHorizontal();
            {
                if (currentRP.supportsCameraDepthTexture)
                {
                    GUILayout.Label(" Depth texture enabled.", greenStyle);
                }
                else
                {
                    GUILayout.Label(" Enable depth texture in pipeline asset for depth rim and postprocess effect.\n Alternatively you can do this for specific camera in it's component on scene.", redStyle);
                    if (GUILayout.Button("Show Asset", GUILayout.Width(buttonWidth)))
                    {
                        Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(currentRP));
                    }


                    if (GUILayout.Button("Fix", GUILayout.Width(buttonWidth)))
                    {
                        currentRP.supportsCameraDepthTexture = true;
                    }
                }
            }
            GUILayout.EndHorizontal();

            GUILine(Color.gray);
            GUILayout.BeginHorizontal();
            {
                if (currentRP.supportsCameraOpaqueTexture)
                {
                    GUILayout.Label(" Opaque texture enabled.", greenStyle);
                }
                else
                {
                    GUILayout.Label(" Enable opaque texture in pipeline asset for refraction effect.\n Alternatively you can do this for specific camera in it's component on scene.", redStyle);
                    if (GUILayout.Button("Show Asset", GUILayout.Width(buttonWidth)))
                    {
                        Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(currentRP));
                    }

                    if (GUILayout.Button("Fix", GUILayout.Width(buttonWidth)))
                    {
                        currentRP.supportsCameraOpaqueTexture = true;
                    }
                }
            }
            GUILayout.EndHorizontal();


            GUILine(Color.gray);
            GUILayout.BeginHorizontal();
            {
                var pipeline = GraphicsSettings.defaultRenderPipeline;
                FieldInfo propertyInfo = pipeline.GetType().GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);
                ScriptableRendererData scriptableRendererData = ((ScriptableRendererData[])propertyInfo?.GetValue(pipeline))?[0];
                bool featureAdded = false;
                if (scriptableRendererData)
                {
                    foreach (var feature in scriptableRendererData.rendererFeatures)
                    {
                        if (feature && feature.GetType().ToString().Contains("ShieldPostprocessRenderFeature"))
                        {
                            featureAdded = true;
                        }
                    }
                }
                if (featureAdded)
                {
                    GUILayout.Label(" ShieldPostprocessRenderFeature added.", greenStyle);
                }
                else
                {
                    GUILayout.Label(" Add ShieldPostprocessRenderFeature to ScriptableRendererData.", redStyle);
                    if (GUILayout.Button("Show Asset", GUILayout.Width(buttonWidth)))
                    {
                        Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(scriptableRendererData));
                    }
                }
            }
            GUILayout.EndHorizontal();


            GUILine(Color.gray);
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Check demo scene for shield effect examples.");

                if (GUILayout.Button("Show", GUILayout.Width(buttonWidth)))
                {
                    Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(assetPath + "/URP/Demo_URP.unity");
                }
            }
            GUILayout.EndHorizontal();

            GUILine(Color.gray);
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("All sample objects and materials are placed in Prefabs folder.");

                if (GUILayout.Button("Show", GUILayout.Width(buttonWidth)))
                {
                    Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(assetPath + "/Prefabs/Domes/ShieldDome1.prefab");
                }
            }
            GUILayout.EndHorizontal();

            GUILine(Color.gray);
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("When implementing shield effect on your game scene add \nShieldPostprocess script to camera for postprocess effect.");

                if (GUILayout.Button("Show", GUILayout.Width(buttonWidth)))
                {
                    Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(assetPath + "/Scripts/ShieldPostprocess.cs");
                }
            }
            GUILayout.EndHorizontal();

            GUILine(Color.gray);
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Please read documentation for implementation guidelines.");

                if (GUILayout.Button("Show", GUILayout.Width(buttonWidth)))
                {
                    Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(assetPath + "/Documentation.pdf");
                }
            }
            GUILayout.EndHorizontal();

            GUILine(Color.gray);
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(" Found a bug, need a feature or help with implementation ? Send an email or join Discord server.");
            }
            GUILayout.EndHorizontal();
        }
    }
}