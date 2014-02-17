using UnityEngine;
using System.Collections;

public class Blip {

    private bool currSignal;
    private bool lastSignal;

    public Blip()
    {
        this.currSignal = false;
        this.currSignal = false;
    }

    public bool Listen(bool input)
    {
        this.currSignal = input;

        if (this.currSignal != lastSignal)
        {
            lastSignal = input;
            return true;
        }

        else
            return false;
    }
}
