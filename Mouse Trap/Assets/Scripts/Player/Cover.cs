using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Cover : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private PlayerMovement pm;
    [SerializeField] private CharacterController cc;
    [SerializeField] private Transform coverDetection;
    [SerializeField] private Transform coverEnter;
    [SerializeField] private float distanceCanCover;
    [SerializeField] private float distanceToCover;
    [SerializeField] LayerMask coverMask;
    [SerializeField] private bool canCover = false;
    private Vector3 moveDirection;
    private Quaternion moveRotation;
    [SerializeField] private bool isCovering;
    [SerializeField] private bool hasCovered;

    void Update()
    {
        RaycastHit hitGoToCover;
        RaycastHit hitEnterCover;
        Debug.DrawRay(coverDetection.position, coverDetection.forward * distanceCanCover, Color.yellow);
        if(Physics.Raycast(coverDetection.position, coverDetection.forward, out hitGoToCover, distanceCanCover, coverMask))
        {
            Debug.DrawRay(coverDetection.position, coverDetection.forward * distanceCanCover, Color.red);
            canCover = true;
            moveDirection = (hitGoToCover.transform.position - player.position).normalized;
            moveRotation = (hitGoToCover.transform.rotation).normalized;
        }
        else
        {
            if(isCovering == false)
            {
                Debug.DrawRay(coverDetection.position, coverDetection.forward * distanceCanCover, Color.yellow);
                canCover = false;
                moveDirection = Vector3.zero;
            }
        }

        if(Physics.Raycast(coverEnter.position, coverEnter.forward, out hitEnterCover, distanceToCover, coverMask))
        {
            Debug.DrawRay(coverEnter.position, coverEnter.forward * distanceToCover, Color.green);
            hasCovered = true;
        }
        else
        {
            Debug.DrawRay(coverEnter.position, coverEnter.forward * distanceToCover, Color.blue);
            hasCovered = false;
        }
    }

    public void OnCover(InputAction.CallbackContext context)
    {
        if(context.performed && canCover == true && isCovering == false)
        {
            InvokeRepeating("TakeCover", 0, 0.01f);
        }
        if(canCover == false)
        {
            return;
        }
    }
    public void OnExitCover(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            canCover = true;
            isCovering = false;
        }
        else
        {
            return;
        }
    }

    void TakeCover()
    {
        if(canCover == true)
        {
            isCovering = true;
            pm.enabled = false;
            cc.Move(moveDirection * 10f * Time.deltaTime);
            player.rotation = Quaternion.Slerp(player.rotation, moveRotation, 1f);
            if(hasCovered == true)
            {
                CancelInvoke();
                cc.Move(new Vector3(0f, 0f, 0f));
                pm.enabled = true;
                pm.covered = true;
            }
        }
        if(canCover == false)
        {
            CancelInvoke();
            return;
        }
    }
}
