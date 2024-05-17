using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(MIDController))]
public class MIDControllerEditor : Editor {
    bool[] buttonStates = new bool[] { true, false, false, false, false };
    MIDFeature mIDFeature;
    Rect mainRect;
    Color details_GuiBackground;
    GUIContent detialsButton;
    GUIStyle detailsButtonStyles;
    bool isInit = false;
    void OnSceneGUI() {
        if (isInit) {
            try {
                SetButtonState((int)mIDFeature.midMode);
                ShowMainSection();
                GUILayout.Space(130);
                GUI.color = Color.black;
                ShowDetails();
                GUI.color = Color.white;
            } catch (Exception e) {
                string log = e.Message;
            }
        } else {
            Init();
        }

    }

    void ShowMainSection() {
        GUILayout.BeginArea(mainRect);
        var rect = EditorGUILayout.BeginVertical();
        GUI.Box(rect, GUIContent.none);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("MID Mode");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUI.backgroundColor = buttonStates[0] ? Color.gray : Color.white;
        if (GUILayout.Button("Off")) {
            SetMIDMode(MIDFeature.MIDMode.Off);
            MIDManager.Clear();
        }
        GUI.backgroundColor = buttonStates[1] ? Color.gray : Color.white;
        if (GUILayout.Button("By Material")) {
            SetMIDMode(MIDFeature.MIDMode.ByMaterial);
        }
        GUI.backgroundColor = buttonStates[2] ? Color.gray : Color.white;
        if (GUILayout.Button("By Shader")) {
            SetMIDMode(MIDFeature.MIDMode.ByShader);
        }
        GUI.backgroundColor = buttonStates[3] ? Color.gray : Color.white;
        if (GUILayout.Button("By Shader And Keywords")) {
            SetMIDMode(MIDFeature.MIDMode.ByShaderAndKeywords);
        }
        GUI.backgroundColor = buttonStates[4] ? Color.gray : Color.white;
        if (GUILayout.Button("By Mesh")) {
            SetMIDMode(MIDFeature.MIDMode.ByMesh);
        }
        EditorGUILayout.EndVertical();
        GUILayout.EndArea();
    }

    void ShowDetails() {
        GUI.backgroundColor = details_GuiBackground;
        switch (mIDFeature.midMode) {
            case MIDFeature.MIDMode.ByMaterial:
                GUILayout.Label("Materials Count: " + (MIDManager.materialsSet == null ? 0 : MIDManager.materialsSet.Count).ToString());
                if (MIDManager.materialsSet != null && MIDManager.materialsSet.Count > 0) {
                    foreach (Material material in MIDManager.materialsSet.Keys) {
                        GUILayout.BeginHorizontal();
                        detialsButton.text = material.name;
                        if (GUILayout.Button(detialsButton, detailsButtonStyles, GUILayout.ExpandWidth(false))) {
                            Selection.objects = MIDManager.materialsSetToObjects[material].ToArray();
                        }
                        detialsButton.text = " " + MIDManager.materialsSetToObjects[material].Count.ToString() + " ";
                        GUILayout.Label(detialsButton, detailsButtonStyles, GUILayout.ExpandWidth(false));
                        detialsButton.text = " " + MIDManager.materialsSetToTrisCount[material].ToString() + " ";
                        GUILayout.Label(detialsButton, detailsButtonStyles, GUILayout.ExpandWidth(false));
                        GUILayout.EndHorizontal();
                    }
                }
                break;
            case MIDFeature.MIDMode.ByShader:
                GUILayout.Label("Shaders Count: " + (MIDManager.shadersSet == null ? 0 : MIDManager.shadersSet.Count).ToString());
                if (MIDManager.shadersSet != null && MIDManager.shadersSet.Count > 0) {
                    foreach (Shader shader in MIDManager.shadersSet.Keys) {
                        GUILayout.BeginHorizontal();
                        detialsButton.text = shader.name;
                        if (GUILayout.Button(detialsButton, detailsButtonStyles, GUILayout.ExpandWidth(false))) {
                            Selection.objects = MIDManager.shadersSetToObjects[shader].ToArray();
                        }
                        detialsButton.text = " " + MIDManager.shadersSetToObjects[shader].Count.ToString() + " ";
                        GUILayout.Label(detialsButton, detailsButtonStyles, GUILayout.ExpandWidth(false));
                        detialsButton.text = " " + MIDManager.shadersSetToTrisCount[shader].ToString() + " ";
                        GUILayout.Label(detialsButton, detailsButtonStyles, GUILayout.ExpandWidth(false));
                        GUILayout.EndHorizontal();
                    }

                }
                break;
            case MIDFeature.MIDMode.ByShaderAndKeywords:
                GUILayout.Label("Variants Count: " + (MIDManager.variantsSet == null ? 0 : MIDManager.variantsSet.Count).ToString() + " ( Lightmap keywords might not be collected. )");
                if (MIDManager.variantsSet != null && MIDManager.variantsSet.Count > 0) {
                    foreach (string fullVariantsName in MIDManager.variantsSet.Keys) {
                        GUILayout.BeginHorizontal();
                        detialsButton.text = fullVariantsName;
                        if (GUILayout.Button(detialsButton, detailsButtonStyles, GUILayout.ExpandWidth(false))) {
                            Selection.objects = MIDManager.variantsSetToObjects[fullVariantsName].ToArray();
                        }
                        detialsButton.text = " " + MIDManager.variantsSetToObjects[fullVariantsName].Count.ToString() + " ";
                        GUILayout.Label(detialsButton, detailsButtonStyles, GUILayout.ExpandWidth(false));
                        detialsButton.text = " " + MIDManager.variantsSetToTrisCount[fullVariantsName].ToString() + " ";
                        GUILayout.Label(detialsButton, detailsButtonStyles, GUILayout.ExpandWidth(false));
                        GUILayout.EndHorizontal();
                    }
                }
                break;
            case MIDFeature.MIDMode.ByMesh:
                GUILayout.Label("Meshes Count: " + (MIDManager.meshesSet == null ? 0 : MIDManager.meshesSet.Count).ToString());

                if (MIDManager.meshesSet != null && MIDManager.meshesSet.Count > 0) {
                    foreach (Mesh mesh in MIDManager.meshesSet.Keys) {
                        GUILayout.BeginHorizontal();
                        detialsButton.text = mesh.name;

                        if (GUILayout.Button(detialsButton, detailsButtonStyles, GUILayout.ExpandWidth(false))) {
                            Selection.objects = MIDManager.meshesSetToObjects[mesh].ToArray();
                        }

                        //detialsButton.text = " " + MIDManager.variantsSetToObjects[fullVariantsName].Count.ToString() + " ";
                        //GUILayout.Label(detialsButton, detailsButtonStyles, GUILayout.ExpandWidth(false));
                        detialsButton.text = " " + MIDManager.meshesSetToTrisCount[mesh].ToString() + " ";
                        GUILayout.Label(detialsButton, detailsButtonStyles, GUILayout.ExpandWidth(false));
                        GUILayout.EndHorizontal();
                    }
                }

                break;
        }
        GUI.backgroundColor = Color.gray;

    }

    void SetMIDMode(MIDFeature.MIDMode mode) {
        mIDFeature.midMode = mode;
        SetButtonState((int)mode);
    }

    void Init() {
        MIDController mIDController = target as MIDController;
        if (mIDController.mIDFeature != null) {
            mIDFeature = mIDController.mIDFeature;
            InitGUI();
            isInit = true;
        } else {
            Debug.Log("can't find mid feature");
            isInit = false;
        }
    }

    void InitGUI() {
        mainRect = new Rect(0, 0, 220, 120);
        details_GuiBackground = new Color(1, 1, 1, 0.3f);        // need a transparent background
        detialsButton = new GUIContent("", "Miles");
        detailsButtonStyles = EditorStyles.textArea;
        detailsButtonStyles.fontSize = 8;
        detailsButtonStyles.stretchWidth = true;
        detailsButtonStyles.alignment = TextAnchor.MiddleLeft;
    }

    void SetButtonState(int index) {
        if (buttonStates == null) {
            return;
        }
        if (!buttonStates[index]) {
            for (int i = 0; i < buttonStates.Length; i++) {
                buttonStates[i] = false;
            }
            buttonStates[index] = true;
        }

    }

}
