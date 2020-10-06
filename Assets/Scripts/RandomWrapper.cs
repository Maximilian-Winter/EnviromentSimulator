using System.Collections;
using System.Collections.Generic;

public class RandomWrapper 
{
    private static System.Random rand = new System.Random();

    public static double RandDouble()
    {
        return rand.NextDouble();
    }
}
