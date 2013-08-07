using UnityEngine;
using System.Collections.Generic;

public class Given
{
    public List<string> steps;
    public When when;

    public Given(string step)
    {
        steps = new List<string>();
        steps.Add(step);
    }

    public Given And(string step)
    {
        steps.Add(step);
        return this;
    }

    public When When(string step)
    {
        when = new When(step);
        return when;
    }
}