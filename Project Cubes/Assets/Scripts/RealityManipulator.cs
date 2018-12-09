using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chronos;

[CreateAssetMenu(fileName = "New Reality Manipulator", menuName = "Items/Reality Manipulator")]
public class RealityManipulator : ScriptableObject {
    [Header("Manipulator Settings")]
    public new string name;
    public AreaType area;
    public Color color;
    public float recharge;
    public float radius;
    public float delay;
    public float length;

    [Header("Time Settings")]
    public float speed;
    public ClockBlend blend;
    public AreaClockMode mode;

    [Header("Emmision Settings")]
    public ManipulatorEmmision emmision;
}

public enum ManipulatorEmmision { Position, Projected }
public enum AreaType { Sphere, Cube }