using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class UpgradeSystem : MonoBehaviourPunCallbacks {

    public PlayerCube cubeToUpgrade;
    public Text SpeedUpgradeText;
    public Text ArmorUpgradeText;
    public List<Upgrade> ArmorUpgrades = new List<Upgrade>();
    public List<Upgrade> SpeedUpgrades = new List<Upgrade>();
    private Upgrade CurrentSpeedUpgrade;
    private Upgrade CurrentArmorUpgrade;

    public static UpgradeSystem instance;

    private void Awake()
    {
        instance = this;
        cubeToUpgrade = GameObject.FindObjectOfType<PlayerCube>();
    }

    public void FixedUpdate()
    {

        if (SpeedUpgrades.Count > 0)
        {
            CurrentSpeedUpgrade = SpeedUpgrades[0];
            SpeedUpgradeText.text = "Speed, Cost: " + CurrentSpeedUpgrade.Cost.ToString();
        } else
        {
            SpeedUpgradeText.text = "Speed Fully Upgraded!";
        }

        if (ArmorUpgrades.Count > 0)
        {
            CurrentArmorUpgrade = ArmorUpgrades[0];
            ArmorUpgradeText.text = "Armor, Cost: " + CurrentArmorUpgrade.Cost.ToString();
        } else
        {
            ArmorUpgradeText.text = "Armor Fully Upgraded!";
        }

        if (cubeToUpgrade == null)
        {
            cubeToUpgrade = GameObject.FindObjectOfType<PlayerCube>();
        }
    }

    public void SpeedUpgrade()
    {
        Debug.Log("UPGRADED SPEED!");
        if (cubeToUpgrade != null && SpeedUpgrades.Count > 0)
        {
            ScoreSystem.Score -= CurrentSpeedUpgrade.Cost;
            DestroyableObject desObj = cubeToUpgrade.GetComponent<DestroyableObject>();
            ShootShots shootShots = cubeToUpgrade.GetComponent<ShootShots>();
            cubeToUpgrade.speed += CurrentSpeedUpgrade.modifier;
            if (CurrentSpeedUpgrade.upgradePrefab != null)
            {
                GameObject upgradePrefab = PhotonNetwork.Instantiate("Armor/" + CurrentSpeedUpgrade.upgradePrefab.name, cubeToUpgrade.transform.position, cubeToUpgrade.transform.rotation, 0);
                upgradePrefab.transform.SetParent(cubeToUpgrade.transform);
            }
            int upgradeIndex = SpeedUpgrades.IndexOf(CurrentSpeedUpgrade);
            SpeedUpgrades.RemoveAt(upgradeIndex);
        }

    }

    public void ArmorUpgrade()
    {
        Debug.Log("UPGRADED ARMOR!");
        if (cubeToUpgrade != null && ArmorUpgrades.Count > 0)
        {
            ScoreSystem.Score -= CurrentArmorUpgrade.Cost;
            DestroyableObject desObj = cubeToUpgrade.GetComponent<DestroyableObject>();
            ShootShots shootShots = cubeToUpgrade.GetComponent<ShootShots>();

            desObj.maxHealth += CurrentArmorUpgrade.modifier;
            desObj.health = desObj.maxHealth;
            if (CurrentArmorUpgrade.upgradePrefab != null)
            {
                //Transform cube = GameObject.FindObjectOfType<PlayerCube>().transform;
                //cube.position = new Vector3(cube.position.x, cube.position.y + 0.2f, cube.position.z);
                GameObject upgradePrefab = PhotonNetwork.Instantiate("Armor/" + CurrentArmorUpgrade.upgradePrefab.name, cubeToUpgrade.transform.position, cubeToUpgrade.transform.rotation, 0);
                upgradePrefab.transform.parent = GameObject.FindObjectOfType<PlayerCube>().transform.Find("ArmorHolder").transform;
                Transform cube = GameObject.FindObjectOfType<PlayerCube>().transform;
                cube.position = new Vector3(cube.position.x, cube.position.y + 0.2f, cube.position.z);

                foreach (Transform child in upgradePrefab.transform)
                {
                    //child.gameObject.AddComponent<Rigidbody>();
                    //Rigidbody body = child.GetComponent<Rigidbody>();
                    //body.useGravity = false;
                    //body.mass = 10;
                    //child.position = new Vector3(child.position.x, child.position.y - 0.2f, child.position.z);
                    //body.isKinematic = false;
                }

            }
            int upgradeIndex = ArmorUpgrades.IndexOf(CurrentArmorUpgrade);
            ArmorUpgrades.RemoveAt(upgradeIndex);
        }
    }

    /*
    public void UpgradeCube(UpgradeType upType)
    {
        List<Upgrade> upgrades = new List<Upgrade>();
        if (upType == UpgradeType.Armor)
        {
            upgrades = SpeedUpgrades;
        }
        if (upType == UpgradeType.Speed)
        {
            upgrades = ArmorUpgrades;
        }

        Upgrade currentUpgrade = upgrades[0];

        if (cubeToUpgrade != null && upgrades.Count > 0)
        {
            ScoreSystem.Score -= currentUpgrade.Cost;
            //currentUpgrade.Cost = currentUpgrade.Cost * 2;
            DestroyableObject desObj = cubeToUpgrade.GetComponent<DestroyableObject>();
            ShootShots shootShots = cubeToUpgrade.GetComponent<ShootShots>();
            if (upType == UpgradeType.Speed)
            {
                cubeToUpgrade.speed += currentUpgrade.modifier;
                upgrades.Remove(currentUpgrade);
                SpeedUpgrades = upgrades;
            } else if (upType == UpgradeType.Armor)
            {
                desObj.maxHealth += currentUpgrade.modifier;
                desObj.health = desObj.maxHealth;
                upgrades.Remove(currentUpgrade);
                ArmorUpgrades = upgrades;
            }

        }
    }
    */
}


[System.Serializable] public struct Upgrade
{
    public int Cost;
    public UpgradeType type;
    public float modifier;
    public GameObject upgradePrefab;
}

public enum UpgradeType {Armor, Speed}
