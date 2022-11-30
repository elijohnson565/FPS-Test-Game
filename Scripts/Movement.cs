using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object;
public class Movement : NetworkBehaviour
{
    [SerializeField] private Transform groundCheck;

    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask wallMask;

    [SerializeField] float speed;

   
    [SerializeField] private float normalGravity;

    [SerializeField] private bool isGrounded;
    [SerializeField] private float jumpForce;

    private Rigidbody body;

    private float vertical;
    private float horizontal;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!base.IsOwner)
        {
            gameObject.GetComponent<Movement>().enabled = false;
        }
    }

    void Start()
    {
        body = gameObject.GetComponent<Rigidbody>();  
    }

    void IncreaseSpeed(float speedIncrease)
    {
        speed += speedIncrease;
    }

    void DecreaseSpeed(float speedDecrease)
    {
        speed -= speedDecrease * Time.deltaTime;
    }

    // Update is called once per frame
    void Update()
    {

        vertical = Input.GetAxisRaw("Vertical");
        horizontal = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyUp(KeyCode.Space))
        {
            body.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
          
    }

    void FixedUpdate()
    {
        if (!isGrounded)
        {
            ApplyGravity();
        }

        body.AddForce((transform.forward * vertical) * speed);
        body.AddForce((transform.right * horizontal) * speed);

        CheckGround();
    }



    void CheckGround()
    {
      
        isGrounded = Physics.CheckSphere(groundCheck.position, 0.50f, groundMask);

    }


    void ApplyGravity()
    {
        
        body.AddForce(-transform.up * normalGravity, ForceMode.Force); 
    }

}
