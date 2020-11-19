using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class A
{
    public A()
    {
        p = 1;
    }
    public A(int x)
    {
        p = x;
    }
    public A(A z)
    {
        p = z.p;  
    }
   
    public int p = 2;
}


public class testlight : MonoBehaviour
{
    private Light light;
    // Start is called before the first frame update
    void Start()
    {
        light = GetComponent<Light>();
        A a = new A();
        a.p = 4;
        A b = a;
        b.p = 8;
        b.p = 10;
        b = new A(a);
    }

    // Update is called once per frame
    void Update()
    {
        light.shadowStrength = light.intensity * 0.8f;
    }
}
