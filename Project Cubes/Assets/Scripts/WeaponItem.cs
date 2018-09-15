using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Items/Weapon")]
public class WeaponItem : ScriptableObject {
    public string gunName;
	public int cost = 50;
	public string description;
    public float shotDelay;
	public Color particlesColor;
	public Sprite icon;
    public bool auto = false;
	public float weaponRechargeTime = 0f;
	public bool weaponHasRecharge = false;
    public GameObject projectile;
    public GameObject particles;
    public float lineWidth;
    public bool hasParticles = true;
    public bool shakesCamera = false;
    public float shakeMagnitude;
    public float shakeRoughness;
    public float firerate = 10f;
	public float damage = 10;
	public float range = 100;
    public WeaponType type;
	public WeaponRarity rarity;
    public AudioClip sound;
    public float soundRefreshTime;

	public void Equip() {
		EquipmentManager.instance.Equip(this);
		Inventory.instance.Remove(this);
	}

}

public enum WeaponType{Mowing, Laser, Rail, Projectile, SeekingProjectile}
public enum WeaponRarity{Basic, Advanced, Elite, Legendary, Ultimate}
