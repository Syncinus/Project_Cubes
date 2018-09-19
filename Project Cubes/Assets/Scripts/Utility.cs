using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public static class Utility {


    public static T[] ShuffleArray<T>(T[] array, int seed)
    {
        System.Random prng = new System.Random(seed);

        for (int i = 0; i < array.Length - 1; i++)
        {
            int randomIndex = prng.Next(i, array.Length);
            T tempItem = array[randomIndex];
            array[randomIndex] = array[i];
            array[i] = tempItem;
        }

        return array;
    }

	public static void Shuffle<T> (this List<T> list) {
		System.Random rnd = new System.Random ();
		for (var i = 0; i < list.Count; i++) {
			list.Swap (i, rnd.Next (i, list.Count));
		}
	}

	public static void Swap<T> (this List<T> list, int i, int j) {
		var temp = list [i];
		list [i] = list [j];
		list [j] = temp;
	}
}

public class UtilityNonStaticFunctions 
{
    //public void FixTimeScale(float originalTimeScale, float lengthUntilFix)
    //{
    //    StartCoroutine(fixTimeScale(originalTimeScale, lengthUntilFix));
    //}

    public void FixTimeScale(float originalTimeScale, float lengthUntilFix)
    {
        System.Threading.Thread.Sleep(Mathf.RoundToInt(lengthUntilFix));
        Time.timeScale = originalTimeScale;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
    }
}
