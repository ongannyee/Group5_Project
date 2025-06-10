using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Z3raHackingManager : MonoBehaviour
{
    //public Text overridePointText;                   // UI Text to display current OP
    public int currentOP = 300;                      // Starting OP

    public DASHController dashController;            // Reference to DASH for thermal sweep
    public float flickerDuration = 3f;
    public float powerSurgeDuration = 10f;
    public float thermalSweepDuration = 3f;
    public float jamCommsDuration = 3f;

    // Unlock flags for abilities (can be used to unlock later during progression)
    public bool[] unlockedAbilities = new bool[6] { true, true, true, true, true, true }; 

    void SpendOP(int amount)
    {
        currentOP -= amount;
        //overridePointText.text = "OP: " + currentOP;
    }

    bool HasEnoughOP(int cost)
    {
        return currentOP >= cost;
    }

    public void TryJamComms()
    {
        if (!HasEnoughOP(150)) return;
        SpendOP(150);
        StartCoroutine(JamCommsRoutine());
    }

    IEnumerator JamCommsRoutine()
    {
        EnemyMovement[] guards = FindObjectsOfType<EnemyMovement>();
        foreach (var guard in guards)
        {
            guard.DisableComms(jamCommsDuration);
        }
        yield return new WaitForSeconds(jamCommsDuration);
    }

    public  void TryThermalSweep()
    {
        if (!HasEnoughOP(100)) return;
        SpendOP(100);
        StartCoroutine(ThermalSweepRoutine());
    }

    IEnumerator ThermalSweepRoutine()
    {
        dashController.EnableThermalSweep(thermalSweepDuration);
        yield return new WaitForSeconds(thermalSweepDuration);
    }

    public void TryRouterPing()
    {
        if (!HasEnoughOP(50)) return;
        SpendOP(50);
        Router[] routers = FindObjectsOfType<Router>();
        foreach (var router in routers)
        {
            router.RevealEnemies(); 
        }
    }

    public void TryFlickerSurveillance()
    {
        if (!HasEnoughOP(150)) return;
        SpendOP(150);
        CCTV[] cctvs = FindObjectsOfType<CCTV>();
        foreach (var cam in cctvs)
        {
            cam.Flicker(flickerDuration); // You will implement this on CCTV
        }
    }

    public void TryPowerSurge()
    {
        if (!HasEnoughOP(450)) return;
        SpendOP(450);
        CCTV[] cctvs = FindObjectsOfType<CCTV>();
        foreach (var cam in cctvs)
        {
            cam.Flicker(powerSurgeDuration);
        }
        EnemyMovement[] guards = FindObjectsOfType<EnemyMovement>();
        foreach (var guard in guards)
        {
            guard.ReduceVisionTemporarily(powerSurgeDuration);
        }
    }

    public void TryHackDoor()
    {
        if (!HasEnoughOP(150)) return;
        SpendOP(150);
        OfficeDoor[] doors = FindObjectsOfType<OfficeDoor>();
        foreach (var door in doors)
        {
            if (Vector2.Distance(transform.position, door.transform.position) < 3f)
            {
                door.UnlockInstantly();
                break;
            }
        }
    }
}
