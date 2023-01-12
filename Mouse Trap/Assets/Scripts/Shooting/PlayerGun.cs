using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGun : MonoBehaviour
{
    [Header("Ammo/Fire")]
    [SerializeField] private bool isAuto;
    [SerializeField] private bool isShootingAuto;
    [SerializeField] private float timeToShoot;
    [SerializeField] private bool canShoot = true;
    [SerializeField] private float totalAmmo;
    [SerializeField] private float currentAmmo;

    [Header("Reloading")]
    [SerializeField] private float reloadTime;
    [SerializeField] private bool isReloading = false;

    [Header("Bullet Spread")]
    [SerializeField] private float maxSpread;

    [Header("Misc")]
    [SerializeField] private Transform gunBarrel;
    [SerializeField] private LayerMask enemyMask;


    void Update()
    {
        if(currentAmmo <= 0f)
        {
            canShoot = false;
        }
    }

    public void OnShootAuto(InputAction.CallbackContext context)
    {
        if(isAuto == true)
        {
            if(currentAmmo <= 0f && isReloading == false && context.performed)
            {
                StartCoroutine(Reload());
            }

            if(canShoot == true && context.performed)
            {
                isShootingAuto = true;
                StartCoroutine(ShootAuto());
            }

            if(context.canceled)
            {
                isShootingAuto = false;
                StopCoroutine(ShootAuto());
            }
        }
        if(isAuto == false)
        {
            return;
        }
    }
    public void OnShootSemi(InputAction.CallbackContext context)
    {
        if(isAuto == false)
        {
            if(currentAmmo <= 0f && isReloading == false && context.performed)
            {
                StartCoroutine(Reload());
            }

            if(canShoot == true && context.performed)
            {
                StartCoroutine(ShootSemi());
            }
        }
        if(isAuto == true)
        {
            return;
        }
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if(currentAmmo < totalAmmo && isReloading == false)
        {
            if(context.performed)
            {
                StartCoroutine(Reload());
            }
        }
    }

    IEnumerator ShootAuto()
    {
        if(isAuto == true && isShootingAuto == true)
        {
            Vector3 dir = transform.forward + new Vector3(Random.Range(-maxSpread,maxSpread), Random.Range(-maxSpread,maxSpread), Random.Range(-maxSpread,maxSpread));
            Debug.Log("Shoot Auto");
            currentAmmo -= 1f;
            RaycastHit enemyHit;
            if(Physics.Raycast(gunBarrel.position, gunBarrel.forward.normalized, out enemyHit, Mathf.Infinity))
            {
                Debug.DrawRay(gunBarrel.position, dir * 50f, Color.black, 3f);
            }
            yield return new WaitForSeconds(timeToShoot);
            StartCoroutine(ShootAuto());
        }
        if(isAuto == false || isShootingAuto == false)
        {
            StopCoroutine(ShootAuto());
        }
    }

    IEnumerator ShootSemi()
    {
        if(isAuto == false && isShootingAuto == false)
        {
            canShoot = false;
            Vector3 dir = transform.forward + new Vector3(Random.Range(-maxSpread,maxSpread), Random.Range(-maxSpread,maxSpread), Random.Range(-maxSpread,maxSpread));
            Debug.Log("Shoot Semi");
            currentAmmo -= 1f;
            RaycastHit enemyHit;
            if(Physics.Raycast(gunBarrel.position, gunBarrel.forward.normalized, out enemyHit, Mathf.Infinity))
            {
                Debug.DrawRay(gunBarrel.position, dir * 50f, Color.black, 3f);
            }
            yield return new WaitForSeconds(timeToShoot);
            canShoot = true;
        }
        if(isAuto == true)
        {
            StopCoroutine(ShootSemi());
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        canShoot = false;
        yield return new WaitForSeconds(reloadTime);
        Debug.Log("Reloaded");
        currentAmmo = totalAmmo;
        isReloading = false;
        canShoot = true;
        yield return new WaitForSeconds(timeToShoot);
    }

    void OnDrawGizmos()
    {
        Debug.DrawRay(gunBarrel.position, gunBarrel.forward * 50f, Color.cyan);
    }
}
