﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CastleRoom_Ambush : CastleRoom
{

    private int selectedAmbush;

    [Header("Stats")]
    public Stat useCooldown;
    public Stat damageAmount;

    [Header("Volley")]
    public int volleyRows;
    public int volleyColumns;
    public float volleyDistOffset;
    public float volleySpawnDelay;
    public float randomVolleySpawnOffset;

    [Header("Meteor")]
    public int meteorRows;
    public int meteorColumns;
    public float meteorDistOffset;
    public float meteorSpawnDelay;
    public float randomMeteorSpawnOffset;

    [Header("Spear")]
    public int spearRows;
    public int spearColumns;
    public float spearDistOffset;
    public float spearSpawnDelay;
    public float randomSpearSpawnOffset;

    [Header("Ambush Room Setup")]
    public Image cooldownFill;
    public TextMeshProUGUI descriptionText;
    private Transform cameraTarget;
    public float camZoomSpeed;
    private CameraManager mainCamManager;
    private Camera mainCam;

    [Header("Upgrades")]
    public TextMeshProUGUI nextLevelExtraUpgradeText;
    public GameObject nextLevelExtraUpgradePanel;
    public GameObject meteorStrikeButton;
    public GameObject spearRainButton;

    private GameObject[] ambushSpawns;

    private float currentCooldown = 0.95f;

    public override void Awake()
    {
        base.Awake();

        cameraTarget = GameObject.FindWithTag("CameraTarget").transform;
        mainCam = Camera.main;
        mainCamManager = mainCam.GetComponent<CameraManager>();

        ambushSpawns = GameObject.FindGameObjectsWithTag("AmbushSpawn");
    }

    public override void SetupUI()
    {
        base.SetupUI();

        descriptionText.text = "Ambushes the enemies with a strike from above.\n Deals " + "<color=green>" + damageAmount.currentValue + "</color> damage.";

        if (myLevel < myMaxLevel)
        {
            upgradeStatsText.text = "Damage amount: " + damageAmount.currentValue + " (<color=green>" + CastleUpgradeManager.instance.CheckPositiveOrNegative(damageAmount.increaseValue) + "</color>)" + "\n" +
                                    "Cooldown: " + useCooldown.currentValue + " (<color=green>" + CastleUpgradeManager.instance.CheckPositiveOrNegative(useCooldown.increaseValue) + "</color>)";
        }
        else
        {
            upgradeStatsText.text = "Damage amount: " + damageAmount.currentValue + "\n" +
                                    "Cooldown: " + useCooldown.currentValue;
        }

        if (myLevel == 4)
        {
            nextLevelExtraUpgradePanel.SetActive(true);
            nextLevelExtraUpgradeText.text = "Next level will unlock the ambush: meteor strike";
        }
        else if (myLevel == 9)
        {
            nextLevelExtraUpgradePanel.SetActive(true);
            nextLevelExtraUpgradeText.text = "Next level will unlock the ambush: spear rain";
        }
        else
        {
            nextLevelExtraUpgradePanel.SetActive(false);
            nextLevelExtraUpgradeText.text = string.Empty;
        }

        if (myLevel >= 5)
        {
            meteorStrikeButton.SetActive(true);
        }

        if (myLevel >= 10)
        {
            spearRainButton.SetActive(true);
        }
    }

    public override void Update()
    {
        base.Update();

        if (currentCooldown < 1)
        {
            currentCooldown += 1 / useCooldown.currentValue * Time.deltaTime;
            cooldownFill.fillAmount = currentCooldown;
        }
        else
        {
            currentCooldown = 1;
        }
    }

    public override void UseRoom()
    {
        base.UseRoom();

        if (currentCooldown < 1)
        {
            return;
        }

        currentCooldown = 0;

        switch (selectedAmbush)
        {
            case 0:

                StartCoroutine(ProjectileRain("ambush volley", volleyRows, volleyColumns, volleyDistOffset, volleySpawnDelay, randomVolleySpawnOffset, false, 2));
                break;

            case 1:

                StartCoroutine(ProjectileRain("ambush meteor", meteorRows, meteorColumns, meteorDistOffset, meteorSpawnDelay, randomMeteorSpawnOffset, true, 4));
                break;

            case 2:

                StartCoroutine(ProjectileRain("ambush spear", spearRows, spearColumns, spearDistOffset, spearSpawnDelay, randomSpearSpawnOffset, false, 2));
                break;
        }

        StopUsingButton();
    }

    private IEnumerator ProjectileRain(string projectile, int rainRows, int rainColumns, float rainDistOffset, float rainSpawnDelay, float rainSpawnOffset, bool addTorque, float screenShakeAmount)
    {
        Vector3 zoomTo = new Vector3(0, 15, -25);
        mainCamManager.canMove = false;

        while (Vector3.Distance(cameraTarget.position, zoomTo) > 0.1f)
        {
            cameraTarget.position = Vector3.Lerp(cameraTarget.position, zoomTo, Time.deltaTime * camZoomSpeed);
            mainCam.fieldOfView = Mathf.Lerp(mainCam.fieldOfView, 60, Time.deltaTime * (camZoomSpeed * 5));
            yield return null;
        }

        mainCam.GetComponent<CameraShake>().Shake(rainSpawnDelay * rainRows + 5, screenShakeAmount, screenShakeAmount, 0, 0, 1);

        int toSpawnLeft = rainRows;
        int toSpawnRight = rainRows;

        for (int i = 0; i < rainRows; i++)
        {
            for (int ii = 0; ii < ambushSpawns.Length; ii++)
            {
                Vector3 spawnPos = new Vector3();

                if (ambushSpawns[ii].transform.position.x < 0)
                {
                    spawnPos = new Vector3(ambushSpawns[ii].transform.position.x - (rainDistOffset * toSpawnLeft + GetRandomOffset(rainSpawnOffset)), ambushSpawns[ii].transform.position.y + GetRandomOffset(rainSpawnOffset), ambushSpawns[ii].transform.position.z);
                    toSpawnLeft--;
                }
                else
                {
                    spawnPos = new Vector3(ambushSpawns[ii].transform.position.x + (rainDistOffset * toSpawnRight + GetRandomOffset(rainSpawnOffset)), ambushSpawns[ii].transform.position.y + GetRandomOffset(rainSpawnOffset), ambushSpawns[ii].transform.position.z);
                    toSpawnRight--;
                }

                GameObject newProjectile = SpawnProjectile(projectile, spawnPos, ambushSpawns[ii].transform.rotation, addTorque);

                Vector3 columnSpawnPos = new Vector3();
                int columns = rainColumns;
                for (int iii = 0; iii < rainColumns; iii++)
                {
                    if (ambushSpawns[ii].transform.position.x < 0)
                    {
                        columnSpawnPos = new Vector3(newProjectile.transform.position.x + (0.2f * columns + GetRandomOffset(rainSpawnOffset)), newProjectile.transform.position.y + (2f * columns + GetRandomOffset(rainSpawnOffset)), newProjectile.transform.position.z);
                    }
                    else
                    {
                        columnSpawnPos = new Vector3(newProjectile.transform.position.x - (0.2f * columns + GetRandomOffset(rainSpawnOffset)), newProjectile.transform.position.y + (2f * columns + GetRandomOffset(rainSpawnOffset)), newProjectile.transform.position.z);
                    }

                    SpawnProjectile(projectile, columnSpawnPos, ambushSpawns[ii].transform.rotation, addTorque);

                    columns--;
                }

                yield return new WaitForSeconds(rainSpawnDelay);
            }
        }

        yield return new WaitForSeconds(4);

        zoomTo = new Vector3(0, 2, 0);

        while (Vector3.Distance(cameraTarget.position, zoomTo) > 0.1f)
        {
            cameraTarget.position = Vector3.Lerp(cameraTarget.position, zoomTo, Time.deltaTime * camZoomSpeed);
            yield return null;
        }

        cameraTarget.position = zoomTo;
        mainCamManager.canMove = true;
    }

    private GameObject SpawnProjectile(string projectile, Vector3 position, Quaternion rotation, bool addTorque)
    {
        GameObject newProjectile = ObjectPooler.instance.GrabFromPool(projectile, position, rotation);
        newProjectile.GetComponent<Rigidbody>().AddForce(newProjectile.transform.forward * 150);
        newProjectile.GetComponent<Projectile>().myDamage = damageAmount.currentValue;

        //if (addTorque)
        //{
        //    newProjectile.GetComponent<Rigidbody>().AddTorque(newProjectile.transform.right * 150);
        //}

        return newProjectile;
    }

    private float GetRandomOffset(float variable)
    {
        float random = Random.Range(-variable, variable);
        return random;
    }

    public void SetSelectedAmbush(int i)
    {
        selectedAmbush = i;
    }

    public override void Upgrade()
    {
        if (!ResourceManager.instance.HasEnoughGold((int)myUpgradeCost.currentValue) || myLevel >= myMaxLevel)
        {
            return;
        }

        base.Upgrade();

        useCooldown.currentValue += useCooldown.increaseValue;
        damageAmount.currentValue += damageAmount.increaseValue;

        SetupUI();
    }
}
