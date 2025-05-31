
using System;
using UnityEngine;
using UnityEngine.Serialization;

public sealed class PlayerController : MonoBehaviour
{
    private const float RaycastDistance = 0.15f;
    
    [SerializeField] private Transform[] RaycastsBackward  = null;
    [SerializeField] private Transform[] RaycastsDownward  = null;
    [SerializeField] private Transform[] RaycastsForward   = null;
    [SerializeField] private Transform[] RaycastsLeftward  = null;
    [SerializeField] private Transform[] RaycastsRightward = null;
    [SerializeField] private Transform[] RaycastsUpward    = null;
    
    [SerializeField] private float MovementSpeed                   = 3f;
    [SerializeField] private float RotationSpeedInDegreesPerSecond = 135f;
    [SerializeField] private float InitialJumpVelocity             = 100f;
    [SerializeField] private float TerminalFallingVelocity         = -200f;
    [SerializeField] private int   NumberOfJumps                   = 2;
    
    private float m_verticalVelocity       = 0f;
    private int   m_numberOfJumpsRemaining = 2;
    private bool  m_isAscending            = false;
    private bool  m_isDescending           = false;
    private bool  m_isGrounded             = false;
    private bool  m_canMoveForward         = false;
    private bool  m_canMoveBackward        = false;
    private bool  m_canMoveLeftward        = false;
    private bool  m_canMoveRightward       = false;
    
    private void Update()
    {
        this.HandleJump();
        
        this.HandleDetectionBackward();
        this.HandleDetectionCeiling();
        this.HandleDetectionForward();
        this.HandleDetectionLeftward();
        this.HandleDetectionRightward();
        this.HandleDetectionGround();
        
        this.HandleAscent();
        this.HandleDescent();
        
        this.HandleRotation();
        this.HandleMovement();
    }

    private bool CanJump()
    {
        return this.m_numberOfJumpsRemaining > 0;
    }
    
    private void HandleDetectionBackward()
    {
        this.m_canMoveBackward = true;
        foreach (var raycast in this.RaycastsBackward)
        {
            var hit = Physics.Raycast(
                origin:      raycast.position, 
                direction:   -this.transform.forward,
                maxDistance: PlayerController.RaycastDistance
            );

            if (hit is false)
            {
                continue;
            }
            
            this.m_canMoveBackward = false;
            break;
        }
    }
    
    private void HandleDetectionCeiling()
    {
        if (this.m_isDescending is true)
        {
            return;
        }
        
        foreach (var raycast in this.RaycastsUpward)
        {
            var hit = Physics.Raycast(
                origin:      raycast.position, 
                direction:   this.transform.up,
                maxDistance: PlayerController.RaycastDistance
            );
            if (hit is false)
            {
                continue;
            }

            this.m_verticalVelocity = 0f;
            this.m_isDescending = true;
            this.m_isAscending = false;
            break;
        }
    }

    private void HandleDetectionForward()
    {
        this.m_canMoveForward = true;
        foreach (var raycast in this.RaycastsForward)
        {
            var hit = Physics.Raycast(
                origin:      raycast.position, 
                direction:   this.transform.forward,
                maxDistance: PlayerController.RaycastDistance
            );

            if (hit is false)
            {
                continue;
            }
            
            this.m_canMoveForward = false;
            break;
        }
    }

    private void HandleDetectionGround()
    {
        if (this.m_isAscending is true)
        {
            return;
        }
        
        this.m_isGrounded = false;
        this.m_isDescending = true;
        foreach (var raycast in this.RaycastsDownward)
        {
            var hit = Physics.Raycast(
                origin:      raycast.position, 
                direction:   -this.transform.up,
                hitInfo:     out var hitInfo,
                maxDistance: PlayerController.RaycastDistance
            );
            if (
                hit is false || 
                hitInfo.collider.gameObject.layer != LayerMask.NameToLayer("Ground")
            )
            {
                continue;
            }
            
            this.m_isGrounded = true;
            this.m_numberOfJumpsRemaining = this.NumberOfJumps;
            this.m_verticalVelocity = 0f;
            this.m_isDescending = false;
            break;
        }
    }
    
    private void HandleDetectionLeftward()
    {
        this.m_canMoveLeftward = true;
        foreach (var raycast in this.RaycastsLeftward)
        {
            var hit = Physics.Raycast(
                origin:      raycast.position, 
                direction:   -this.transform.right,
                maxDistance: PlayerController.RaycastDistance
            );

            if (hit is false)
            {
                continue;
            }
            
            this.m_canMoveLeftward = false;
            break;
        }
    }
    
    private void HandleDetectionRightward()
    {
        this.m_canMoveRightward = true;
        foreach (var raycast in this.RaycastsRightward)
        {
            var hit = Physics.Raycast(
                origin:      raycast.position,
                direction:   this.transform.right,
                maxDistance: PlayerController.RaycastDistance
            );

            if (hit is false)
            {
                continue;
            }
            
            this.m_canMoveRightward = false;
            break;
        }
    }

    private void HandleJump()
    {
        if (this.CanJump() is false)
        {
            return;
        }
        
        var isPressedJump = Input.GetKeyDown(
            key: KeyCode.Space
        );
        if (isPressedJump is false)
        {
            return;
        }
        
        this.m_numberOfJumpsRemaining--;
        this.m_isAscending = true;
        this.m_isDescending = false;
        this.m_verticalVelocity = this.InitialJumpVelocity;
    }

    private void HandleAscent()
    {
        if (this.m_isAscending is false)
        {
            return;
        }
        
        this.transform.position += this.m_verticalVelocity * Time.deltaTime * Vector3.up;
        this.m_verticalVelocity += Physics.gravity.y * Time.deltaTime;

        if (this.m_verticalVelocity > 0)
        {
            return;
        }
        
        this.m_isAscending = false;
        this.m_isDescending = true;
    }

    private void HandleDescent()
    {
        if (this.m_isDescending is false)
        {
            return;
        }
        
        this.transform.position += this.m_verticalVelocity * Time.deltaTime * Vector3.up;

        if (this.m_verticalVelocity <= this.TerminalFallingVelocity)
        {
            return;
        }
        
        this.m_verticalVelocity += Physics.gravity.y * Time.deltaTime;
        if (this.m_verticalVelocity <= this.TerminalFallingVelocity)
        {
            this.m_verticalVelocity = this.TerminalFallingVelocity;
        }
    }
    
    private void HandleMovement()
    {
        var velocity = Vector3.zero;

        if (this.m_canMoveForward is true)
        {
            var isPressingForward = Input.GetKey(
                key: KeyCode.W
            );
            if (isPressingForward is true)
            {
                velocity.z += Time.deltaTime * this.MovementSpeed;
            }
        }
        
        if (this.m_canMoveBackward is true)
        {
            var isPressingBackward = Input.GetKey(
                key: KeyCode.S
            );
            if (isPressingBackward is true)
            {
                velocity.z -= Time.deltaTime * this.MovementSpeed;
            }
        }
        
        if (this.m_canMoveLeftward is true)
        {
            var isPressingLeftward = Input.GetKey(
                key: KeyCode.A
            );
            if (isPressingLeftward is true)
            {
                velocity.x -= Time.deltaTime * this.MovementSpeed;
            }
        }
        
        if (this.m_canMoveRightward is true)
        {
            var isPressingRightward = Input.GetKey(
                key: KeyCode.D
            );
            if (isPressingRightward is true)
            {
                velocity.x += Time.deltaTime * this.MovementSpeed;
            }
        }
        
        velocity = this.transform.TransformDirection(
            direction: velocity
        );

        this.transform.position += velocity;
    }

    private void HandleRotation()
    {
        var eulerAngles = this.transform.eulerAngles;

        var isPressedTurnLeft = Input.GetKey(
            key: KeyCode.Q
        );
        var isPressedTurnRight = Input.GetKey(
            key: KeyCode.E
        );
        
        if (isPressedTurnLeft is true)
        {
            eulerAngles.y += this.RotationSpeedInDegreesPerSecond * Time.deltaTime;
        }
        if (isPressedTurnRight is true)
        {
            eulerAngles.y -= this.RotationSpeedInDegreesPerSecond * Time.deltaTime;
        }
        
        this.transform.eulerAngles = eulerAngles;
    }
}















