using UnityEngine;
using System.Collections;

public struct Corner
{
    Vector2 c0, c1, c2, c3;
    public Corner(Vector2 c0, Vector2 c1, Vector2 c2, Vector2 c3)
    {
        this.c0 = c0;
        this.c1 = c1;
        this.c2 = c2;
        this.c3 = c3;
    }

    public Vector2 this[int index]
    {
        get
        {
            switch (index)
            {
                case 0:
                    return c0;
                case 1:
                    return c1;
                case 2:
                    return c2;
                case 3:
                    return c3;
                default:
                    return c0;
            }

        }
        set
        {
            switch (index)
            {
                case 0:
                    c0 = value;
                    break;
                case 1:
                    c1 = value;
                    break;
                case 2:
                    c2 = value;
                    break;
                case 3:
                    c3 = value;
                    break;
            }
        }
    }


}

public struct Axis
{
    Vector2 a0, a1;
    public Axis(Vector2 a0, Vector2 a1)
    {
        this.a0 = a0;
        this.a1 = a1;
    }

    public Vector2 this[int index]
    {
        get
        {
            switch (index)
            {
                case 0:
                    return a0;
                case 1:
                    return a1;
                default:
                    return a0;
            }

        }
        set
        {
            switch (index)
            {
                case 0:
                    a0 = value;
                    break;
                case 1:
                    a1 = value;
                    break;
            }
        }
    }
}

public struct Origin
{
    float o1, o2;
    public Origin(float o1, float o2)
    {
        this.o1 = o1;
        this.o2 = o2;
    }

    public float this[int index]
    {
        get
        {
            switch (index)
            {
                case 0:
                    return o1;
                case 1:
                    return o2;
                default:
                    return o1;
            }

        }
        set
        {
            switch (index)
            {
                case 0:
                    o1 = value;
                    break;
                case 1:
                    o2 = value;
                    break;
            }
        }
    }
}
