using UnityEngine;
using System.Collections.Generic;
using System;
using System.Reflection;

public class RunsTests : MonoBehaviour {
    [HideInInspector]
    public List<StepList> tests;

    [HideInInspector]
    public bool finished;

    int framesSinceStarted;

    void Awake()
    {
        tests = new List<StepList>();
    }

	// Use this for initialization
    void Start()
    {
        foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
        {
            if (type.IsSubclassOf(typeof(TestBehaviour)))
            {
                TestBehaviour testRunner = gameObject.AddComponent(type) as TestBehaviour;
                testRunner.RunAll(type.Name);
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
        TestBehaviour[] fixtures = GameObject.FindObjectsOfType(typeof(TestBehaviour)) as TestBehaviour[];

        foreach (TestBehaviour test in fixtures)
        {
            test.Monitor();

            if (!tests.Contains(test.steps))
            {
                tests.Add(test.steps);
            }
        }

        if (framesSinceStarted > 3 && fixtures.Length == 0)
        {
            finished = true;
        }

        framesSinceStarted++;
	}
}
