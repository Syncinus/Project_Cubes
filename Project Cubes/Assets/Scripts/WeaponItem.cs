using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Items/Weapon")]
public class WeaponItem : ScriptableObject {

    public List<FiringPoint> Points = new List<FiringPoint>();

	public void Equip() {
		EquipmentManager.instance.Equip(this);
		Inventory.instance.Remove(this);
	}
}

[System.Serializable]
public class FiringPoint
{
    public string Name = "Point";
    public ShotMode Shooting;
    public EmmisionMode Emmision;
    public ParticleMode Particles;
    public SoundMode Sound;
}

#region Modes
[System.Serializable] public class EmmisionMode
{
    public EmmisionType Type;
    public float EmmisionSize;
    public Vector3 EmmisionOffset;
    public Vector3 RotationOffset;
}

[System.Serializable] public class ShotMode
{
    public float Damage;
    public float Range;
    public float FireRate;
    public float Recharge;
    public float ShotCount;
    public float Force;
}

[System.Serializable] public class ParticleMode
{
    public string Prefab;
    public string Type;
    public Vector3 Position;
    public Color Coloring;
    public float Size;
}

[System.Serializable] public class SoundMode
{
    public AudioClip Clip;
    public float Refresh;
    public float Volume;
}

#endregion

public enum EmmisionType { Line, Tube }
