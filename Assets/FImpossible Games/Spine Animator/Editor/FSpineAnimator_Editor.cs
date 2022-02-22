using FIMSpace.FEditor;
using FIMSpace.FSpine;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[UnityEditor.CustomEditor(typeof(FSpineAnimator))]
/// <summary>
/// FM: Editor class component to enchance controll over component from inspector window
/// </summary>
[CanEditMultipleObjects]
public class FSpineAnimator_Editor : UnityEditor.Editor
{
    static bool drawDefaultInspector = false;
    static bool drawMain = true;
    static bool drawAnimationOptions = true;
    static bool drawQuickCorrection = false;
    static bool drawAdvancedCorrection = false;
    static bool drawDebug = false;
    static bool drawPreciseAutoCorr = false;

    private Transform startBone;
    private Transform endBone;
    private Transform headBone;

    private bool incorrection = false;

    #region Editor to component stuff

    protected HOEditorUndoManager undoManager;

    protected SerializedProperty sp_spines;
    protected SerializedProperty sp_drawg;

    protected SerializedProperty sp_BlendToOriginal;
    protected SerializedProperty sp_ReversedLeadBone;
    protected SerializedProperty sp_ConnectWithAnimator;
    protected SerializedProperty sp_PhysicalClock;
    protected SerializedProperty sp_AnchoredSpine;
    protected SerializedProperty sp_AnchorRoot;
    protected SerializedProperty sp_QueueToLastUpdate;
    protected SerializedProperty sp_PositionsNotAnimated;
    protected SerializedProperty sp_SelectivePosNotAnimated;
    protected SerializedProperty sp_RotationsNotAnimated;
    protected SerializedProperty sp_SelectiveRotNotAnimated;
    protected SerializedProperty sp_ManualRotationOffsets;
    protected SerializedProperty sp_ManualPositionOffsets;
    protected SerializedProperty sp_RoundCorrection;
    protected SerializedProperty sp_UnifyCorrection;
    protected SerializedProperty sp_StartAfterTPose;
    protected SerializedProperty sp_InversedVerticalRotation;

    protected SerializedProperty sp_PositionsSmoother;
    protected SerializedProperty sp_RotationsSmoother;
    protected SerializedProperty sp_AngleLimit;
    protected SerializedProperty sp_LimitingAngleSmoother;
    protected SerializedProperty sp_StraighteningSpeed;
    protected SerializedProperty sp_TurboStraighten;
    protected SerializedProperty sp_GoBackSpeed;
    protected SerializedProperty sp_SegmentsPivotOffset;
    protected SerializedProperty sp_DistancesMul;
    protected SerializedProperty sp_AnimateLeadingBone;
    protected SerializedProperty sp_LeadingAnimateAfterMotion;
    protected SerializedProperty sp_CustomAnchorRotationOffset;
    

    protected SerializedProperty sp_DrawDebug;
    protected SerializedProperty sp_DebugAlpha;
    protected SerializedProperty sp_MainPivotOffset;

    #endregion

    protected virtual void OnEnable()
    {
        undoManager = new HOEditorUndoManager(target as FSpineAnimator, "Undo Manager");

        sp_spines = serializedObject.FindProperty("SpineTransforms");
        sp_drawg = serializedObject.FindProperty("drawGizmos");

        sp_BlendToOriginal = serializedObject.FindProperty("BlendToOriginal");
        sp_ReversedLeadBone = serializedObject.FindProperty("LastBoneLeading");
        sp_ConnectWithAnimator = serializedObject.FindProperty("ConnectWithAnimator");
        sp_PhysicalClock = serializedObject.FindProperty("PhysicalUpdate");
        sp_AnchoredSpine = serializedObject.FindProperty("AnchorToThis");
        sp_AnchorRoot = serializedObject.FindProperty("AnchorRoot");
        sp_QueueToLastUpdate = serializedObject.FindProperty("QueueToLastUpdate");
        sp_PositionsNotAnimated = serializedObject.FindProperty("PositionsNotAnimated");
        sp_SelectivePosNotAnimated = serializedObject.FindProperty("SelectivePosNotAnimated");
        sp_RotationsNotAnimated = serializedObject.FindProperty("RotationsNotAnimated");
        sp_SelectiveRotNotAnimated = serializedObject.FindProperty("SelectiveRotNotAnimated");
        sp_ManualRotationOffsets = serializedObject.FindProperty("ManualRotationOffsets");
        sp_ManualPositionOffsets = serializedObject.FindProperty("ManualPositionOffsets");
        sp_RoundCorrection = serializedObject.FindProperty("RoundCorrection");
        sp_UnifyCorrection = serializedObject.FindProperty("UnifyCorrection");
        sp_StartAfterTPose = serializedObject.FindProperty("StartAfterTPose");
        sp_InversedVerticalRotation = serializedObject.FindProperty("InversedVerticalRotation");


        sp_PositionsSmoother = serializedObject.FindProperty("PosSmoother");
        sp_RotationsSmoother = serializedObject.FindProperty("RotSmoother");
        sp_AngleLimit = serializedObject.FindProperty("AngleLimit");
        sp_LimitingAngleSmoother = serializedObject.FindProperty("LimitSmoother");
        sp_StraighteningSpeed = serializedObject.FindProperty("StraightenSpeed");
        sp_TurboStraighten = serializedObject.FindProperty("TurboStraighten");
        sp_GoBackSpeed = serializedObject.FindProperty("GoBackSpeed");
        sp_SegmentsPivotOffset = serializedObject.FindProperty("SegmentsPivotOffset");
        sp_MainPivotOffset = serializedObject.FindProperty("MainPivotOffset");
        sp_DistancesMul = serializedObject.FindProperty("DistancesMultiplier");
        sp_AnimateLeadingBone = serializedObject.FindProperty("AnimateLeadingBone");
        sp_LeadingAnimateAfterMotion = serializedObject.FindProperty("LeadingAnimateAfterMotion");
        sp_CustomAnchorRotationOffset = serializedObject.FindProperty("CustomAnchorRotationOffset");

        sp_DrawDebug = serializedObject.FindProperty("DrawDebug");
        sp_DebugAlpha = serializedObject.FindProperty("DebugAlpha");

        FSpineAnimator spineA = (FSpineAnimator)target;

        if (spineA.SpineTransforms != null)
        {
            if (spineA.SpineTransforms.Count > 0)
                startBone = spineA.SpineTransforms[0];

            if (spineA.SpineTransforms.Count > 1)
                endBone = spineA.SpineTransforms[spineA.SpineTransforms.Count - 1];
        }

        EditorUtility.SetDirty(target);
    }

    public override void OnInspectorGUI()
    {
        // Update component from last changes
        serializedObject.Update();

        FSpineAnimator spineA = (FSpineAnimator)target;

        #region Incorrection handling

        if (Application.isPlaying)
        {
            if (!spineA.wasIncorrectRemind)
                if (spineA.incorrectionWarning)
                {
                    if (!incorrection)
                    {
                        incorrection = true;
                        drawAnimationOptions = false;
                        drawAdvancedCorrection = false;
                        drawQuickCorrection = true;
                        EditorPrefs.SetInt(spineA.name + "-" + spineA.GetInstanceID(), 1);
                    }
                }
        }
        else
        {
            if (!spineA.wasIncorrectRemind)
                if (EditorPrefs.GetInt(spineA.name + "-" + spineA.GetInstanceID()) == 1)
                {
                    EditorPrefs.SetInt(spineA.name + "-" + spineA.GetInstanceID(), 0);
                    spineA.wasIncorrectRemind = true;
                    drawAnimationOptions = false;
                    drawAdvancedCorrection = false;
                    drawQuickCorrection = true;
                    incorrection = true;
                }
        }

        #endregion

        //if (GUILayout.Button(new GUIContent("Dev Log"))) spineA.DevLog();

        #region Default Inspector
        if (drawDefaultInspector)
        {
            EditorGUILayout.BeginVertical(FEditor_Styles.GrayBackground);
            drawDefaultInspector = GUILayout.Toggle(drawDefaultInspector, "Default inspector");

            #region Exluding from view not needed properties

            List<string> excludedVars = new List<string>();

            if (!spineA.PositionsNotAnimated) excludedVars.Add("SelectivePosNotAnimated");
            if (!spineA.RotationsNotAnimated) excludedVars.Add("SelectiveRotNotAnimated");

            #endregion

            EditorGUILayout.EndVertical();

            // Draw default inspector without not needed properties
            DrawPropertiesExcluding(serializedObject, excludedVars.ToArray());
        }
        else
        #endregion
        {
            if (!incorrection)
            {
                EditorGUILayout.BeginVertical(FEditor_Styles.GrayBackground);
            }
            else
            {
                EditorGUILayout.BeginVertical(FEditor_Styles.Style(new Color(0.9f, 0.44f, 0.33f, 0.95f)));
                EditorGUILayout.HelpBox("There was detected strange behaviour of your spine in playmode, check highlighted parameters for solution guidement.", MessageType.Error);

                if (spineA.ConnectWithAnimator) EditorGUILayout.HelpBox("Are you sure you have enabled animator? If you not using animator, disable 'ConnectWithAnimator' option.", MessageType.Warning);

                incorrection = GUILayout.Toggle(incorrection, "Show highlighting");
            }

            EditorGUILayout.BeginHorizontal();
            drawDefaultInspector = GUILayout.Toggle(drawDefaultInspector, "Default inspector");

            GUILayout.FlexibleSpace();
            EditorGUIUtility.labelWidth = 88;
            EditorGUILayout.PropertyField(sp_drawg);

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            GUILayout.Space(5f);
            Color preCol = GUI.color;

            EditorGUILayout.BeginVertical(FEditor_Styles.Style(FColorMethods.ChangeColorAlpha(Color.white, 0.25f)));
            EditorGUI.indentLevel++;

            drawMain = EditorGUILayout.Foldout(drawMain, "Main Parameters");

            #region Main Tab

            if (drawMain)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                if (spineA.SpineTransforms == null || spineA.SpineTransforms.Count < 1)
                {
                    GUILayout.BeginHorizontal(FEditor_Styles.YellowBackground);
                    EditorGUILayout.HelpBox("Put here two marginal bones from hierarchy and click 'Get' to create spine chain of section you want to animate with spine animator", MessageType.Info);
                    GUILayout.EndHorizontal();
                }

                EditorGUILayout.BeginHorizontal(FEditor_Styles.LGrayBackground);

                int wrong = 0;
                if (spineA.SpineTransforms != null)
                {
                    if (spineA.SpineTransforms.Count < 2) wrong = 2;
                    else
                    {
                        if (startBone != spineA.SpineTransforms[0] || endBone != spineA.SpineTransforms[spineA.SpineTransforms.Count - 1])
                        {
                            wrong = 3;
                        }
                    }
                }
                else wrong = 1;

                if (wrong == 1) GUI.color = new Color(1f, 0.3f, 0.3f, 0.85f);
                if (wrong == 2) GUI.color = new Color(1f, 0.7f, 0.2f, 0.85f);

                EditorGUI.indentLevel--;
                EditorGUIUtility.labelWidth = 42f;
                if (startBone == null) startBone = spineA.transform;
                startBone = (Transform)EditorGUILayout.ObjectField(new GUIContent("Start", "Put here first bone in hierarchy depth for automatically get chain of bones to end one"), startBone, typeof(Transform), true);
                endBone = (Transform)EditorGUILayout.ObjectField(new GUIContent("End", "Put here last bone in hierarchy depth for automatically get chain of bones from start one"), endBone, typeof(Transform), true);
                EditorGUIUtility.labelWidth = 0f;

                if (GUILayout.Button(new GUIContent("L", "Automatically get last bone in hierarchy - it depends of children placement, then sometimes last bone can be found wrong, whne you have arms/legs bones inside, if they're higher, algorithm will go through them"), new GUILayoutOption[2] { GUILayout.MaxWidth(24), GUILayout.MaxHeight(14) })) GetLastBoneInHierarchy();

                if (wrong == 3) GUI.color = new Color(0.2f, 1f, 0.4f, 0.85f);
                if (startBone != null && endBone != null)
                {
                    GUI.color = new Color(0.3f, 1f, 0.4f, 0.8f);

                    if (spineA.SpineTransforms != null)
                    {
                        if (spineA.SpineTransforms.Count > 0)
                        {
                            if (startBone != spineA.SpineTransforms[0] || endBone != spineA.SpineTransforms[spineA.SpineTransforms.Count - 1]) wrong = 3; else GUI.color = FColorMethods.ChangeColorAlpha(preCol, 0.7f);
                        }
                    }
                }

                if (GUILayout.Button(new GUIContent("Get"), new GUILayoutOption[2] { GUILayout.MaxWidth(36), GUILayout.MaxHeight(14) }))
                {
                    GetBonesChainFromStartToEnd();
                    EditorUtility.SetDirty(target);
                }

                GUI.color = preCol;

                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel++;

                if (spineA.SpineTransforms == null || spineA.SpineTransforms.Count < 1)
                {
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndVertical();
                    return;
                }

                if (spineA.SpineTransforms.Count < 1)
                    EditorGUILayout.BeginHorizontal(FEditor_Styles.Style(new Color32(99, 166, 50, 45)));
                else
                    EditorGUILayout.BeginHorizontal(FEditor_Styles.Style(new Color32(10, 175, 66, 25)));

                EditorGUILayout.PropertyField(sp_spines, true);
                EditorGUILayout.EndHorizontal();

                EditorGUIUtility.labelWidth = 144f;
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(sp_BlendToOriginal, true);
                GUILayout.Space(2f);

                EditorGUILayout.BeginVertical(FEditor_Styles.Style(new Color32(0, 200, 100, 22)));

                EditorGUIUtility.labelWidth = 170f;

                GUI.color = new Color(0.3f, 1f, 0.4f, 0.8f);
                EditorGUILayout.PropertyField(sp_ReversedLeadBone, true);
                GUI.color = preCol;

                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(sp_ConnectWithAnimator, true);

                if (!Application.isPlaying) EditorGUILayout.PropertyField(sp_PhysicalClock, true);
                EditorGUILayout.EndVertical();

                EditorGUIUtility.labelWidth = 0f;

                GUILayout.Space(2f);

                EditorGUILayout.EndVertical();
            }

            #endregion

            #region Animation Options

            EditorGUILayout.BeginVertical(FEditor_Styles.LGrayBackground);
            drawAnimationOptions = EditorGUILayout.Foldout(drawAnimationOptions, "Animation Options");

            if (drawAnimationOptions)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUI.indentLevel--;
                GUI.color = new Color(0.55f, 0.9f, 0.75f, 0.85f);

                GUILayout.BeginVertical(FEditor_Styles.Style(new Color32(33, 200, 130, 24)));

                EditorGUIUtility.labelWidth = 105f;
                GUILayout.Space(4f);

                EditorGUILayout.PropertyField(sp_PositionsSmoother, true);
                EditorGUILayout.PropertyField(sp_RotationsSmoother, true);
                GUILayout.Space(5f);

                EditorGUILayout.EndVertical();

                GUILayout.BeginVertical(FEditor_Styles.Emerald);

                EditorGUILayout.PropertyField(sp_AngleLimit, true);
                EditorGUILayout.PropertyField(sp_LimitingAngleSmoother, true);

                EditorGUILayout.EndVertical();

                GUILayout.BeginVertical(FEditor_Styles.Style(new Color32(33, 200, 130, 24)));

                GUILayout.Space(5f);
                EditorGUILayout.PropertyField(sp_StraighteningSpeed, true);

                if (spineA.StraightenSpeed > 0f)
                {
                    EditorGUI.indentLevel++;
                    EditorGUIUtility.labelWidth = 121f;
                    EditorGUILayout.PropertyField(sp_TurboStraighten, true);
                    EditorGUI.indentLevel--;
                    GUILayout.Space(3f);
                    EditorGUIUtility.labelWidth = 105f;
                }

                EditorGUILayout.PropertyField(sp_GoBackSpeed, true);

                EditorGUILayout.EndVertical();

                EditorGUIUtility.labelWidth = 0f;
                EditorGUILayout.EndVertical();
                GUILayout.Space(5f);
                EditorGUI.indentLevel++;

                GUI.color = preCol;
            }

            EditorGUILayout.EndVertical();

            #endregion

            #region Basic Correction

            if (incorrection)
                EditorGUILayout.BeginVertical(FEditor_Styles.Style(new Color(1f, 0.3f, 0.3f, 0.4f)));
            else
                EditorGUILayout.BeginVertical(FEditor_Styles.LGrayBackground);

            EditorGUILayout.BeginHorizontal();
            drawQuickCorrection = EditorGUILayout.Foldout(drawQuickCorrection, "Basic Correction Options");
            GUILayout.FlexibleSpace();

            if (!drawPreciseAutoCorr)
                GUI.color = new Color(0.7f, 1f, 0.9f, 0.9f);
            else
                GUI.color = new Color(0.95f, 1f, 0.95f, 0.85f);

            GUI.color = new Color(0.7f, 1f, 0.9f, 0.9f);
            if (GUILayout.Button(new GUIContent("Auto", "Algorithm will analyze your skeleton and will try to find correct options, this correction is done automatically when you add coponent, but when you make some changes you can reset them by clicking here again."), new GUILayoutOption[2] { GUILayout.MaxWidth(44), GUILayout.MaxHeight(14) }))
            {
                spineA.TryAutoCorrect();
                EditorUtility.SetDirty(target);
            }

            if (GUILayout.Button(new GUIContent("Precise", "Opening auto correction field with more precise option"), new GUILayoutOption[2] { GUILayout.MaxWidth(57), GUILayout.MaxHeight(14) }))
            {
                drawPreciseAutoCorr = !drawPreciseAutoCorr;
            }

            GUI.color = preCol;

            EditorGUILayout.EndHorizontal();

            if (drawPreciseAutoCorr)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUI.indentLevel--;
                EditorGUIUtility.labelWidth = 74f;

                if (!headBone) GUI.color = new Color(0.9f, 0.3f, 0.3f, 0.9f);
                headBone = (Transform)EditorGUILayout.ObjectField(new GUIContent("Head bone", "Head bone or some bone before, it's important to be in front of spine and not included in spine animator's chain"), headBone, typeof(Transform), true);
                GUI.color = preCol;

                if (headBone)
                {
                    if (GUILayout.Button(new GUIContent("Try Correct", "Auto correcting in reference to head bone position"), new GUILayoutOption[2] { GUILayout.MaxWidth(88), GUILayout.MaxHeight(14) }))
                    {
                        spineA.TryAutoCorrect(headBone);
                        EditorUtility.SetDirty(target);
                    }
                }

                EditorGUI.indentLevel++;
                EditorGUIUtility.labelWidth = 0f;

                EditorGUILayout.EndHorizontal();
            }


            if (drawQuickCorrection)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUIUtility.labelWidth = 171f;

                GUILayout.Space(5f);

                EditorGUILayout.PropertyField(sp_InversedVerticalRotation, true);

                if (spineA.ConnectWithAnimator)
                {
                    if (!incorrection)
                        GUILayout.BeginVertical(FEditor_Styles.Style(new Color32(33, 200, 130, 24)));
                    else
                        GUILayout.BeginVertical(FEditor_Styles.Style(new Color32(255, 111, 111, 55)));

                    EditorGUILayout.PropertyField(sp_PositionsNotAnimated, true);

                    if (spineA.PositionsNotAnimated)
                    {
                        spineA.RefreshSelectivePosNotAnimated();

                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(sp_SelectivePosNotAnimated, true);
                        EditorGUI.indentLevel--;
                    }
                    GUILayout.EndVertical();


                    if (incorrection) GUILayout.BeginVertical(FEditor_Styles.Style(new Color32(255, 111, 111, 55)));

                    EditorGUIUtility.labelWidth = 174f;
                    EditorGUILayout.PropertyField(sp_RotationsNotAnimated, true);
                    if (spineA.RotationsNotAnimated)
                    {
                        spineA.RefreshSelectiveRotNotAnimated();

                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(sp_SelectiveRotNotAnimated, true);
                        EditorGUI.indentLevel--;
                    }
                    GUILayout.Space(5f);

                    if (incorrection) GUILayout.EndVertical();
                }

                EditorGUIUtility.labelWidth = 146f;
                //EditorGUILayout.PropertyField(sp_RefinedCorrection, true);

                GUILayout.BeginVertical(FEditor_Styles.Style(new Color32(33, 200, 130, 24)));

                if (!Application.isPlaying) EditorGUILayout.PropertyField(sp_StartAfterTPose, true);
                EditorGUILayout.PropertyField(sp_RoundCorrection, true);
                EditorGUILayout.PropertyField(sp_UnifyCorrection, true);
                GUILayout.EndVertical();

                EditorGUIUtility.labelWidth = 0f;
                GUILayout.Space(5f);

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();

            #endregion

            #region Advanced correction

            EditorGUILayout.BeginVertical(FEditor_Styles.LGrayBackground);
            drawAdvancedCorrection = EditorGUILayout.Foldout(drawAdvancedCorrection, "Advanced Options");

            if (drawAdvancedCorrection)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Space(5f);

                EditorGUIUtility.labelWidth = 166f;

                if (spineA.AnchorRoot && !spineA.AnchorToThis && !spineA.QueueToLastUpdate) GUI.color = new Color(0.5f, 1f, 0.65f, 0.85f);
                EditorGUILayout.PropertyField(sp_QueueToLastUpdate, true);
                GUI.color = preCol;

                GUILayout.BeginVertical(FEditor_Styles.Style(new Color32(33, 200, 130, 24)));
                EditorGUILayout.PropertyField(sp_AnchoredSpine, true);
                GUILayout.EndVertical();

                if (!spineA.AnchorToThis)
                {
                    EditorGUI.indentLevel++;

                    if (spineA.AnchorRoot == null)
                    {
                        EditorGUILayout.HelpBox("If you connecting tail to spine animated by SpineAnimator, try to put into 'Anchor Root' same bone as first bone for the spine, enable 'QueueToLastUpdate' and set some 'GoBack' value", MessageType.Info);

                        EditorGUILayout.BeginHorizontal();
                    }

                    EditorGUILayout.PropertyField(sp_AnchorRoot, true);

                    if (spineA.AnchorRoot == null)
                    {
                        if (GUILayout.Button(new GUIContent("Parent", "Putting this transform's parent onto field"), new GUILayoutOption[2] { GUILayout.MaxWidth(88), GUILayout.MaxHeight(14) }))
                        {
                            spineA.AnchorRoot = spineA.transform.parent;
                            EditorUtility.SetDirty(target);
                        }

                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.PropertyField(sp_CustomAnchorRotationOffset, true);

                    EditorGUI.indentLevel--;
                }

                GUILayout.Space(5f);
                EditorGUILayout.PropertyField(sp_MainPivotOffset, true);
                EditorGUILayout.PropertyField(sp_SegmentsPivotOffset, true);
                EditorGUILayout.PropertyField(sp_DistancesMul, true);

                EditorGUILayout.PropertyField(sp_AnimateLeadingBone, true);

                if (spineA.AnimateLeadingBone)
                {
                    EditorGUI.indentLevel++;
                    EditorGUIUtility.labelWidth = 216f;
                    EditorGUILayout.PropertyField(sp_LeadingAnimateAfterMotion, true);
                    EditorGUIUtility.labelWidth = 0f;
                    EditorGUI.indentLevel--;
                }

                GUILayout.Space(3f);
                EditorGUI.indentLevel++;

                spineA.RefreshManualPosOffs();
                spineA.RefreshManualRotOffs();

                EditorGUILayout.PropertyField(sp_ManualPositionOffsets, true);
                EditorGUILayout.PropertyField(sp_ManualRotationOffsets, true);
                GUILayout.Space(5f);
                EditorGUI.indentLevel--;

                EditorGUIUtility.labelWidth = 0f;
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();

            #endregion


            #region Debug Options

            EditorGUILayout.BeginVertical(FEditor_Styles.LGrayBackground);
            drawDebug = EditorGUILayout.Foldout(drawDebug, "Debugging");

            if (drawDebug)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                GUILayout.BeginHorizontal(FEditor_Styles.LBlueBackground);
                EditorGUILayout.HelpBox("When 'DrawDebug' is toggled, you can use button '~' to instantly deactivate SpineAnimator's motion for time you hold this button (not on build)", MessageType.None);
                GUILayout.EndHorizontal();

                GUILayout.Space(5f);
                EditorGUILayout.PropertyField(sp_DrawDebug, true);
                EditorGUILayout.PropertyField(sp_DebugAlpha, true);
                EditorGUILayout.PropertyField(sp_drawg, true);
                GUILayout.Space(5f);

                EditorGUIUtility.labelWidth = 0f;
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();

            #endregion


            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
        }

        // Apply changed parameters variables
        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// Getting last bone in hierarhy going up by first children
    /// </summary>
    void GetLastBoneInHierarchy()
    {
        if (startBone == null)
        {
            Debug.LogWarning("Start bone is not defined in " + target.name);
            return;
        }

        Transform c = startBone;

        // Try to find bones with spine word in it and go through deepest found with this name
        Transform spine = null;
        foreach (Transform t in c.GetComponentsInChildren<Transform>())
        {
            if (t.name.ToLower().Contains("spine"))
            {
                spine = t;
                break;
            }
        }

        if (spine != null) c = spine;

        // I'm scared of while() loops so I just put here iterator to limit in some case
        for (int i = 0; i < 1000; i++)
        {
            if (c.childCount > 0)
            {
                for (int j = 0; j < c.childCount; j++)
                    if (c.GetChild(j).name.ToLower().Contains("spine"))
                    {
                        c = c.GetChild(j);
                        break;
                    }

                c = c.GetChild(0);
            }
            else break;
        }

        endBone = c;
    }

    /// <summary>
    /// Getting bones automatically by defining start and end in hierarchy
    /// </summary>
    private void GetBonesChainFromStartToEnd()
    {
        if (startBone == null)
        {
            Debug.LogWarning("Start bone is not defined in " + target.name);
            return;
        }

        if (endBone == null)
        {
            Debug.LogWarning("End bone is not defined in " + target.name);
            return;
        }

        List<Transform> bones = new List<Transform>();
        Transform p = endBone;
        bool wrong = false;

        // I'm scared of while() loops so I just put here iterator to limit in some case
        for (int i = 0; i < 1000; i++)
        {
            bool willStop = false;
            if (p == startBone) willStop = true;

            if (p == null)
            {
                wrong = true;
                break;
            }

            bones.Add(p);
            p = p.parent;

            if (willStop) break;
        }

        if (wrong)
        {
            Debug.LogError("Something went wrong during getting bones automatically for " + target.name + ". Did you assigned start and end bone correctly? It should go only up in hierarchy.");
            return;
        }

        FSpineAnimator spineA = target as FSpineAnimator;
        if (spineA)
        {
            bones.Reverse();
            spineA.SpineTransforms = bones;
        }
    }

}
