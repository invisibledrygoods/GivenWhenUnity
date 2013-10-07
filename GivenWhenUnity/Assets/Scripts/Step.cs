using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class Step
{
    public static Color red = new Color(256f / 256f, 0f / 256f, 0f / 256f);
    public static Color yellow = new Color(181f / 256f, 137f / 256f, 0f / 256f);
    public static Color green = new Color(73f / 256f, 133f / 256f, 0f / 256f);

    public Color status;
    public string step;

    public Step(Color status, string step)
    {
        this.status = status;
        this.step = step;
    }
}

// unity is stupid and can't serialize nested lists
[Serializable]
public class StepList
{
    public List<Step> steps = new List<Step>();
    public Color severity;
    public string reason;
    public string type;
    public bool expanded;

    public StepList()
    {
        steps = new List<Step>();
        severity = Step.green;
    }

    public void Add(Step step) 
    {
        steps.Add(step);

        if (step.status == Step.red || severity == Step.red)
        {
            this.severity = Step.red;
        }
        else if (step.status == Step.yellow)
        {
            this.severity = Step.yellow;
        }

        if (severity != Step.green)
        {
            this.expanded = true;
        }
    }
}