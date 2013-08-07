using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System;
using UnityEditor;

public abstract class TestBehaviour : MonoBehaviour {
    public StepList steps;

    List<Given> givens;
    float initialDuration;
    float timeout;
    string timeUnits;
    string stepAfterTimeout;
    bool failed;

    void Awake()
    {
        givens = new List<Given>();
        steps = new StepList();
    }

    // update isn't called in abstract classes so just have the test runner call this
    public void Monitor()
    {
        if (stepAfterTimeout == null)
        {
            return;
        }

        string prefix = "then";

        if ("seconds".StartsWith(timeUnits)) {
            prefix += " within " + initialDuration + (initialDuration == 1 ? " second" : " seconds");
            timeout -= Time.deltaTime;
        } else if (timeUnits == "ms") {
            prefix += " within " + initialDuration + " ms";
            timeout -= Time.deltaTime * 1000;
        } else if ("frames".StartsWith(timeUnits)) {
            if (initialDuration != 1)
            {
                prefix += " within " + initialDuration + " frames";
            }
            timeout--;
        }

        if (timeout <= 0)
        {
            RunStep(prefix, stepAfterTimeout);
            Destroy(gameObject);
        }
    }
    
    public Given Given(string step)
    {
        Given given = new Given(step);
        givens.Add(given);
        return given;
    }

    public void RunAll(string name)
    {
        Spec();

        foreach (Given given in givens)
        {
            foreach (Then then in given.when.thens)
            {
                TestBehaviour fixture = new GameObject().AddComponent(name) as TestBehaviour;
                fixture.transform.position = GetRandomIsolatedLocation();

                fixture.steps.Add(new Step(Color.black, "if it " + (name.EndsWith("Test") ? name.Remove(name.Length - 4) : name) + ":"));

                string prefix = "given";
                foreach (string step in given.steps)
                {
                    fixture.RunStep(prefix, step);
                    prefix = "and";
                }

                prefix = "when";
                foreach (string step in given.when.steps)
                {
                    fixture.RunStep(prefix, step);
                    prefix = "and";
                }

                fixture.RunStepAfterTimeout(then.duration, then.step);
            }
        }

        Destroy(this);
    }

    public Vector3 GetRandomIsolatedLocation()
    {
        bool fixtureNearby = true;
        Vector3 randomLocation = Vector3.zero;

        int i = 0;
        while (fixtureNearby)
        {
            if (i++ > 1000)
            {
                throw new Exception("testing space is too crowded");
            }

            randomLocation = UnityEngine.Random.insideUnitSphere * 1000;
            fixtureNearby = false;

            foreach (TestBehaviour fixture in Resources.FindObjectsOfTypeAll(typeof(TestBehaviour)) as TestBehaviour[])
            {
                if ((randomLocation - fixture.transform.position).magnitude < 10)
                {
                    fixtureNearby = true;
                }
            }
        }

        return randomLocation;
    }

    public void RunStep(string prefix, string step)
    {
        if (failed)
        {
            steps.Add(new Step(Step.yellow, prefix + " " + step));
            return;
        }

        // replace numbers and strings with ___ and save in object[] args
        List<object> args = new List<object>();

        foreach (Match match in Regex.Matches(Regex.Replace(step, " ", "  "), @"(?:^| )(-?[0-9]+(?:\.[0-9]+)?|'[^']*')(?:$| )"))
        {
            string value = match.Value.Trim();
            if (value.StartsWith("'"))
            {
                args.Add(value.Trim("'".ToCharArray()));
            }
            else if (value.Contains("."))
            {
                args.Add(float.Parse(value));
            }
            else
            {
                args.Add(int.Parse(value));
            }
        }

        string methodName = Regex.Replace(Regex.Replace(Regex.Replace(step.ToLower(), " ", "  "), @"(?:^| )(-?[0-9]+(?:\.[0-9]+)?|'[^']*')(?:$| )", "__"), "[^a-z_]", "");

        try
        {
            bool methodFound = false;
            foreach (MethodInfo method in GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public))
            {
                if (method.Name.ToLower() == methodName)
                {
                    method.Invoke(this, args.ToArray());
                    methodFound = true;
                    break;
                }
            }

            if (methodFound)
            {
                steps.Add(new Step(Step.green, prefix + " " + step));
            }
            else
            {
                steps.Add(new Step(Step.yellow, prefix + " " + step));
                failed = true;
            }
        }
        catch (Exception e)
        {
            Exception error = e.InnerException ?? e;
            string message = Regex.Replace(error.Message.Replace('\n', ' '), @"\s+", " ").Trim();
            steps.Add(new Step(Step.red, prefix + " " + step + " (" + message + ")"));
            failed = true;
        }
    }

    public void RunStepAfterTimeout(string duration, string step)
    {
        stepAfterTimeout = step;
        timeout = float.Parse(Regex.Match(duration.Trim(), @"[\d.]+").Value);
        initialDuration = timeout;
        timeUnits = duration.Replace(timeout.ToString(), "").Trim();
    }

    public abstract void Spec();
}
