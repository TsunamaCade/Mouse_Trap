using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Vector2 move;

    private Vector2 moveLeft;
    private bool canMoveLeft;
    private Vector2 moveRight;
    private bool canMoveRight;
    private Vector2 look;


    [Header("Player Control Variables")]
    [SerializeField] public CharacterController cc;
    [SerializeField] private Transform cam;
    public float damping;
    [SerializeField] private float dampingMove;

    [SerializeField] private float walkSpeed;
    [SerializeField] public float runSpeed;
    private float speed;

    [SerializeField] private float turnSmoothTime;
    float turnSmoothVelocity;

    [Header("Cover Variables")]
    public bool covered;
    [SerializeField] private Transform leftCoverDetect;
    [SerializeField] private Transform rightCoverDetect;
    [SerializeField] private float coverDetectDistance;
    [SerializeField] private LayerMask coverMask;

    public void OnMove(InputAction.CallbackContext context)
    {
        move = context.ReadValue<Vector2>();
    }
    public void OnLook(InputAction.CallbackContext context)
    {
        look = context.ReadValue<Vector2>();
    }
    public void OnRun(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            speed = runSpeed;
        }
        if(context.canceled)
        {
            speed = walkSpeed;
        }
    }
    public void OnExitCover(InputAction.CallbackContext context)
    {
        if(context.performed && covered == true)
        {
            covered = false;
        }
        else
        {
            return;
        }
    }

    void Update()
    {
        if(covered == false)        //If player isn't in cover, player can rotate 360 degrees around the y-axiz and can move onmi-directionally
        {
            Vector3 direction = new Vector3(move.x, 0f, move.y);
            Quaternion rotation = Quaternion.AngleAxis(cam.transform.eulerAngles.y, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * damping);

            if(direction.magnitude >= 0.1f)
            {
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * dampingMove);
                Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                cc.Move(moveDir.normalized * speed * Time.deltaTime);
            }
            if(direction.magnitude < 0.1f)
            {
                speed = walkSpeed;
            }
        }

        if(covered == true)         //If player IS in cover, player is locked forward, and can only move left and right
        {
            float coverWalkSpeed = 3f;
            Vector3 direction = transform.TransformDirection(new Vector3(move.x, 0f, 0f));
            Vector3 directionLeft = transform.TransformDirection(new Vector3(-1, 0f, 0f));
            Vector3 directionRight = transform.TransformDirection(new Vector3(1, 0f, 0f));

            if(direction.magnitude >= 0.1f && canMoveLeft == true && canMoveRight == true)
            {
                cc.Move(direction.normalized * coverWalkSpeed * Time.deltaTime);
            }
            else if(direction.magnitude >= 0.1f && canMoveLeft == false && canMoveRight == true)
            {
                cc.Move(directionRight.normalized * coverWalkSpeed * Time.deltaTime);
            }
            else if(direction.magnitude >= 0.1f && canMoveLeft == true && canMoveRight == false)
            {
                cc.Move(directionLeft.normalized * coverWalkSpeed * Time.deltaTime);
            }


            //These rays detect is the player is near the edge of the cover, and prevents them from moving further in that direction
            RaycastHit leftCoverHit;
            RaycastHit rightCoverHit;
            if(!Physics.Raycast(leftCoverDetect.position, leftCoverDetect.forward, out leftCoverHit, coverDetectDistance, coverMask))
            {
                Debug.DrawRay(leftCoverDetect.position, leftCoverDetect.forward, Color.blue);
                canMoveLeft = false;
            }
            else
            {
                Debug.DrawRay(leftCoverDetect.position, leftCoverDetect.forward, Color.black);
                canMoveLeft = true;
            }

            if(!Physics.Raycast(rightCoverDetect.position, rightCoverDetect.forward, out rightCoverHit, coverDetectDistance, coverMask))
            {
                Debug.DrawRay(rightCoverDetect.position, rightCoverDetect.forward, Color.blue);
                canMoveRight = false;
            }
            else
            {
                Debug.DrawRay(rightCoverDetect.position, rightCoverDetect.forward, Color.black);
                canMoveRight = true;
            }
        }
    }
}