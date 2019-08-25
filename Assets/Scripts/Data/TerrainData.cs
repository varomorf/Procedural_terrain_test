using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class TerrainData : UpdateableData
{
    
    public float uniformScale = 2f;

    [Range(10, 100)]
    public float heightMultiplier;
    public AnimationCurve heighMultiplierCurve;


}
