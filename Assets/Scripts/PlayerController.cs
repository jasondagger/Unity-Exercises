
using System;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Transform[] CeilingRaycasts                 = null;
    [SerializeField] private Transform[] GroundRaycasts                  = null;
    [SerializeField] private float       MovementSpeed                   = 3f;
    [SerializeField] private float       RotationSpeedInDegreesPerSecond = 135f;
    [SerializeField] private float       InitialJumpVelocity             = 100f;
    [SerializeField] private float       TerminalFallingVelocity         = -200f;
    [SerializeField] private int         NumberOfJumps                   = 2;
    
    private int   m_numberOfJumpsRemaining = 2;
    private bool  m_isAscending            = false;
    private bool  m_isDescending           = false;
    private bool  m_isGrounded             = false;
    private float m_verticalVelocity       = 0f;
    
    private void Update()
    {
        this.HandleJump();
        
        this.HandleDetectionCeiling();
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
    
    private void HandleDetectionCeiling()
    {
        if (this.m_isDescending is true)
        {
            return;
        }
        
        foreach (var ceilingRaycast in this.CeilingRaycasts)
        {
            var hit = Physics.Raycast(
                origin:      ceilingRaycast.position, 
                direction:   Vector3.up,
                maxDistance: 0.15f
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

    private void HandleDetectionGround()
    {
        if (this.m_isAscending is true)
        {
            return;
        }
        
        this.m_isGrounded = false;
        this.m_isDescending = true;
        foreach (var groundRaycast in this.GroundRaycasts)
        {
            var hit = Physics.Raycast(
                origin:      groundRaycast.position, 
                direction:   Vector3.down,
                hitInfo:     out var hitInfo,
                maxDistance: 0.15f
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

        var isPressingForward = Input.GetKey(
            key: KeyCode.W
        );
        var isPressingBackward = Input.GetKey(
            key: KeyCode.S
        );
        var isPressingLeftward = Input.GetKey(
            key: KeyCode.A
        );
        var isPressingRightward = Input.GetKey(
            key: KeyCode.D
        );
        
        if (isPressingForward is true)
        {
            velocity.z += Time.deltaTime * this.MovementSpeed;
        }
        if (isPressingBackward is true)
        {
            velocity.z -= Time.deltaTime * this.MovementSpeed;
        }
        if (isPressingLeftward is true)
        {
            velocity.x -= Time.deltaTime * this.MovementSpeed;
        }
        if (isPressingRightward is true)
        {
            velocity.x += Time.deltaTime * this.MovementSpeed;
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

    private bool IsGrounded()
    {
        return this.m_isGrounded;
    }
}















