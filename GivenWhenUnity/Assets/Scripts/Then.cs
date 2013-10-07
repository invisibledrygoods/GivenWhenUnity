using UnityEngine;
using System.Collections.Generic;

public class Then
{
    public When when;
    public string step;
    public string duration;

    public Then(When when, string step)
    {
        this.when = when;
        this.step = step;
        this.duration = "1 frame";
    }

    public Then(When when, string step, string duration)
    {
        this.when = when;
        this.step = step;
        this.duration = duration;
    }

    public Then And(string step)
    {
        return when.Then(step);
    }

    public Then AndWithin(string duration, string step)
    {
        return when.ThenWithin(duration, step);
    }

    public Then Because(string reason)
    {
        when.reason = reason;
        return this;
    }
}