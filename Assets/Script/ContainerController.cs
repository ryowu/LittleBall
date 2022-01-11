using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
        //rb.velocity = new Vector2(rb.velocity.x * -1f, rb.velocity.y);
    }
}
