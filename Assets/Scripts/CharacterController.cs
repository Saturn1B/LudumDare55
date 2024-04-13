using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float speed;
    [SerializeField] private float jumpForce;

    void Update()
    {
        Move();
        Jump();
    }

	private void Move()
	{
        float input = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(input * speed, rb.velocity.y);
	}

    private void Jump()
    {
		if (Input.GetKeyDown(KeyCode.Space))
		{
            rb.AddForce(Vector2.up * jumpForce);
		}
    }
}
