using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Reflection;

public class TestResults : EditorWindow {
    string previousScene;

    Vector2 scrollPosition;
    List<StepList> tests;

    bool lastFrameWasCompiling = false;
    bool wantsToRunTests = false;
    bool autorunTests = true;

    bool showGreen = true;
    bool showYellow = true;
    bool showRed = true;

    float slowTickInterval = 0.1f;
    float slowTickTimeout = 0.0f;

    float startTime;
    float finishTime;

    [MenuItem("Window/Unit Testing")]
    static void Init()
    {
        TestResults window = (TestResults)EditorWindow.GetWindow(typeof(TestResults), false, "Test Results");
        window.minSize = new Vector2(256, 24);
    }

    void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        if (GUILayout.Button("Run Tests", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
        {
            wantsToRunTests = true;
        }

        GUILayout.Space(6);

        autorunTests = GUILayout.Toggle(autorunTests, "Test After Every Compile", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));

        GUILayout.Label("");
        GUILayout.Label("Finished in " + (finishTime - startTime) + " seconds", GUILayout.ExpandWidth(false));

        GUI.color = showGreen ? Step.green : Step.green / 1.3f;
        showGreen = GUILayout.Toggle(showGreen, "" + CountTests(Step.green), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
        GUI.color = showYellow ? Step.yellow : Step.yellow / 1.3f;
        showYellow = GUILayout.Toggle(showYellow, "" + CountTests(Step.yellow), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
        GUI.color = showRed ? Step.red : Step.red / 1.4f;
        showRed = GUILayout.Toggle(showRed, "" + CountTests(Step.red), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
        GUI.color = Color.white;

        EditorGUILayout.EndHorizontal();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        if (tests != null)
        {
            foreach (StepList test in tests)
            {
                if (test.severity == Step.red && showRed
                    || test.severity == Step.yellow && showYellow
                    || test.severity == Step.green && showGreen)
                {
                    EditorGUILayout.BeginHorizontal();
                    foreach (Step step in test.steps)
                    {
                        DrawStep(step.status, step.step);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        GUI.color = Color.white;
    }

    int CountTests(Color color)
    {
        int count = 0;
        if (tests != null)
        {
            foreach (StepList test in tests)
            {
                if (test.severity == color)
                {
                    count++;
                }
            }
        }
        return count;
    }

    void DrawStep(Color color, string step)
    {
        Color oldColor = GUI.color;
        GUI.color = color;
        EditorGUILayout.SelectableLabel(step, EditorStyles.whiteLabel, GUILayout.Width(GUI.skin.label.CalcSize(new GUIContent(step)).x), GUILayout.Height(14));
        GUI.color = oldColor;
    }

    void Update()
    {
        if (!EditorApplication.isCompiling && lastFrameWasCompiling && autorunTests)
        {
            wantsToRunTests = true;
        }

        if (wantsToRunTests && CanRunTests())
        {
            RunTests();
        }

        RunsTests fixture = FindObjectOfType(typeof(RunsTests)) as RunsTests;
        if (fixture != null)
        {
            if (fixture.finished)
            {
                SpinDown();
            }
            else if (fixture.tests.Count != 0)
            {
                tests = fixture.tests;
            }
        }

        slowTickTimeout += 0.01f;
        while (slowTickTimeout > slowTickInterval)
        {
            slowTickTimeout -= slowTickInterval;
            Repaint();
        }

        lastFrameWasCompiling = EditorApplication.isCompiling;
    }

    bool CanRunTests()
    {
        return (!EditorApplication.isCompiling && !EditorApplication.isPlaying && !CheckForCompilerErrors());
    }

    void RunTests()
    {
        CheckForCompilerErrors();
        startTime = Time.time;
        wantsToRunTests = false;

        tests = new List<StepList>();
        previousScene = EditorApplication.currentScene;
        EditorApplication.SaveScene();

        EditorApplication.OpenScene("Assets/Scenes/packages/invisibledrygoods/BDD/TestRunner.unity");
        if (EditorApplication.currentScene == previousScene)
        {
            EditorApplication.OpenScene("Assets/Scenes/TestRunner.unity");
        }

        EditorApplication.isPlaying = true;
    }

    void SpinDown()
    {
        finishTime = Time.time;
        EditorApplication.OpenScene(previousScene);

        EditorApplication.isPlaying = false;
    }

    bool CheckForCompilerErrors()
    {
        Assembly assembly = Assembly.GetAssembly(typeof(SceneView));
        Type logEntries = assembly.GetType("UnityEditorInternal.LogEntries");
        logEntries.GetMethod("Clear").Invoke(new object(), null);
        int count = (int)logEntries.GetMethod("GetCount").Invoke(new object(), null);
        return count != 0;
    }
}
