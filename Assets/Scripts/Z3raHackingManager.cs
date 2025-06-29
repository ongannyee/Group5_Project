using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Z3raHackingManager : MonoBehaviour
{
    //public Text overridePointText;                   // UI Text to display current OP
    public int currentOP = 300;                      // Starting OP

    public GameObject kieran;
    public GameObject DASH;
    public DASHController dashController;            // Reference to DASH for thermal sweep

    public float jamCommsDuration = 10f;
    public float thermalSweepDuration = 3f;
    public float routerPingDuration = 15f;
    public float flickerDuration = 3f;
    public float powerSurgeDuration = 10f;

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
        if (!HasEnoughOP(150) || unlockedAbilities[0]==false) return;
        SpendOP(150);
        StartCoroutine(JamCommsRoutine());
        Debug.Log("Jam Guard Communication");
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

    public void TryThermalSweep()
    {
        if (!HasEnoughOP(100) || unlockedAbilities[1]==false) return;
        SpendOP(100);
        StartCoroutine(ThermalSweepRoutine());
        Debug.Log("Thermal Sweep");
    }

    IEnumerator ThermalSweepRoutine()
    {
        dashController.EnableThermalSweep(thermalSweepDuration);
        yield return new WaitForSeconds(thermalSweepDuration);
    }

    public void TryRouterPing()
    {
        if (!HasEnoughOP(50) || unlockedAbilities[2]==false) return;
        SpendOP(50);
        Router[] routers = FindObjectsOfType<Router>();
        foreach (var router in routers)
        {
            if(router.isActivated)
            {
                Debug.Log("Ping progress");
                router.RevealEnemies(routerPingDuration); 
            }
        }
        Debug.Log("Router Ping");
    }

    public void TryFlickerSurveillance()
    {
        if (!HasEnoughOP(150) || unlockedAbilities[3]==false) return;
        SpendOP(150);
        CCTV[] cctvs = FindObjectsOfType<CCTV>();
        foreach (var cam in cctvs)
        {
            cam.Flicker(flickerDuration); // You will implement this on CCTV
        }
        Debug.Log("Flicker Surveillance");
    }

    public void TryPowerSurge()
    {
        if (!HasEnoughOP(450) || unlockedAbilities[4]==false) return;
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
        Debug.Log("Power Surge end");
    }

    public void TryHackDoor()
    {
        if (!HasEnoughOP(150) || unlockedAbilities[5]==false) return;
        OfficeDoor[] doors = FindObjectsOfType<OfficeDoor>();
        
        foreach (var door in doors)
        {
            Debug.Log(door);
            if (Vector2.Distance(kieran.transform.position, door.transform.position) < 3f)
            {
                Debug.Log("Unlocking Door");
                SpendOP(150);
                door.UnlockInstantly();
                break;
            }
            else
            {
                Debug.Log("No doors found");
                return;
            }
        }
        Debug.Log("Hack Door");
    }

    // For override item +points
    public void AddOverridePoints(int amount)
    {
        currentOP += amount;
        Debug.Log($"Gained {amount} Override Points! Total OP: {currentOP}");
    }

}
