using UnityEngine;
using System.Collections.Generic;

public class When
{
    public List<string> steps;
    public List<Then> thens;

    public When(string step)
    {
        steps = new List<string>();
        thens = new List<Then>();
        steps.Add(step);
    }

    public When And(string step)
    {
        steps.Add(step);
        return this;
    }

    public Then Then(string step)
    {
        Then then = new Then(this, step);
        thens.Add(then);
        return then;
    }

    public Then ThenWithin(string duration, string step)
    {
        Then then = new Then(this, step, duration);
        thens.Add(then);
        return then;
    }
}