using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

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
    [SerializeField] private float spread;
    [SerializeField] private float maxSpread;
    [SerializeField] private float minSpread;

    [Header("Gun Rotation")]
    [SerializeField] private Transform cam;
    [SerializeField] private Transform gun;
    [SerializeField] private float rotationPercent;
    [SerializeField] private float damping;
    [SerializeField] private bool canRotate;

    [Header("Aiming")]
    [SerializeField] private CinemachineFreeLook aimFOV;
    [SerializeField] private float fovNormal;
    [SerializeField] private float fovAiming;
    [SerializeField] private bool isAiming;

    [Header("Misc")]
    [SerializeField] private Transform gunBarrel;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private PlayerMovement pm;

    void Awake()
    {
        aimFOV.m_Lens.FieldOfView = fovNormal;
    }


    void Update()
    {
        if(isReloading == true)
        {   
            canShoot = false;
        }

        if(pm.covered == false)
        {
            if(currentAmmo <= 0f)           //If ammo is empty, you can not shoot;
            {
                currentAmmo = 0f;
                canShoot = false;
            }

            gun.transform.localEulerAngles = new Vector3(cam.rotation.x * rotationPercent, 0f, 0f);         //Rotate gun up and down

            if(isAiming == true)            //If aiming, zoom FOV, decrease bullet spread, and rotate player faster
            {
                aimFOV.m_Lens.FieldOfView = fovAiming;
                pm.damping = 10f;
                spread = minSpread;
            }
            else                             //If not aiming, reset to normal
            {
                aimFOV.m_Lens.FieldOfView = fovNormal;
                pm.damping = 2f;
                spread = maxSpread;
            }
        }
        else if(pm.covered == true && isAiming == true)
        {
            if(currentAmmo <= 0f)           //If ammo is empty, you can not shoot;
            {
                currentAmmo = 0f;
                canShoot = false;
            }

            gun.transform.localEulerAngles = new Vector3(cam.rotation.eulerAngles.x, 0f, 0f);         //Rotate gun completely around
            Quaternion rotation = Quaternion.AngleAxis(cam.transform.eulerAngles.y, Vector3.up);
            pm.transform.rotation = Quaternion.Slerp(pm.transform.rotation, rotation, Time.deltaTime * damping);

            if(isAiming == true)            //If aiming, zoom FOV, decrease bullet spread, and rotate player faster
            {
                aimFOV.m_Lens.FieldOfView = fovAiming;
                pm.damping = 10f;
                spread = minSpread;
            }
            else                             //If not aiming, reset to normal
            {
                aimFOV.m_Lens.FieldOfView = fovNormal;
                pm.damping = 2f;
                spread = maxSpread;
            }
        }
        else if(pm.covered == true && isAiming == false)
        {
            aimFOV.m_Lens.FieldOfView = fovNormal;
            pm.damping = 2f;
            spread = maxSpread;
            gun.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
            return;
        }
    }

    public void OnAimDown(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            isAiming = true;
        }
        if(context.canceled)
        {
            isAiming = false;
        }
    }
    public void OnShootAuto(InputAction.CallbackContext context)
    {
        if(isReloading == false)
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
        else
        {
            return;
        }
    }
    public void OnShootSemi(InputAction.CallbackContext context)
    {
        if(isReloading == false)
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
        else
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
        if(isAuto == true && isShootingAuto == true && currentAmmo > 0)
        {
            Vector3 dir = transform.forward + new Vector3(Random.Range(-spread, spread), Random.Range(-spread, spread), Random.Range(-spread, spread));  //Spreads bullets across maxSpread randomly
            Debug.Log("Shoot Auto");
            currentAmmo -= 1f;
            RaycastHit enemyHit;
            if(Physics.Raycast(gunBarrel.position, gunBarrel.forward.normalized, out enemyHit, Mathf.Infinity))     //If bullet hits enemy, enemy takes damage
            {
                Debug.DrawRay(gunBarrel.position, dir * 50f, Color.black, 3f);
            }
            yield return new WaitForSeconds(timeToShoot);
            StartCoroutine(ShootAuto());        //Repeat for full auto
        }
        if(isAuto == false || isShootingAuto == false || currentAmmo <= 0)
        {
            StopCoroutine(ShootAuto());
        }
    }

    IEnumerator ShootSemi()
    {
        if(isAuto == false && isShootingAuto == false)
        {
            canShoot = false;
            Vector3 dir = transform.forward + new Vector3(Random.Range(-spread, spread), Random.Range(-spread, spread), Random.Range(-spread, spread));    //Spreads bullets across maxSpread randomly
            Debug.Log("Shoot Semi");
            currentAmmo -= 1f;
            RaycastHit enemyHit;
            if(Physics.Raycast(gunBarrel.position, gunBarrel.forward.normalized, out enemyHit, Mathf.Infinity))     //If bullet hits enemy, enemy takes damage
            {
                Debug.DrawRay(gunBarrel.position, dir * 50f, Color.black, 3f);
            }
            yield return new WaitForSeconds(timeToShoot);
            canShoot = true;    //Stops from being full auto
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
