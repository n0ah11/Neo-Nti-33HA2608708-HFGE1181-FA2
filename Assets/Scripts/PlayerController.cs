using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public float fallMultiplier = 2.5f;
    public bool canDoubleJump = true;
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundMask;

    private Rigidbody2D rb;
    private bool isGrounded = false;
    private Animator anim;
    private float moveInputX;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        CheckIfGrounded();
        Jump();
        
    }

    private void FixedUpdate()
    {
        HandleLinearVelocity();
    }

    public void Move()
    {
        moveInputX = Input.GetAxis("Horizontal");

        if (moveInputX > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
            anim.SetBool("isMoving", true);
        }
        else if (moveInputX < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
            anim.SetBool("isMoving", true);
        }
        else
        {
            anim.SetBool("isMoving", false);
        }


    }

    public void HandleLinearVelocity()
    {
        rb.linearVelocity = new Vector2(moveInputX * moveSpeed, rb.linearVelocity.y);

        if (rb.linearVelocity.y < 0)
        {
            // Increase downward velocity to make fall snappier
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
    }
    
    public void CheckIfGrounded()
    { 
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);
        
        if (isGrounded)
        {
            anim.SetBool("isGrounded", true);
            canDoubleJump = true;
        }
        else
        {
            anim.SetBool("isGrounded", false);
        }
    }

    public void Jump()
    {
        
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            anim.SetTrigger("Jump");
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
        else if (Input.GetButtonDown("Jump") && canDoubleJump)
        {
            anim.SetTrigger("Jump");
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            canDoubleJump = false;
        }
    }

    public void Knockback(float force)
    {
        anim.SetTrigger("Jump");
        rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        
    }

}
