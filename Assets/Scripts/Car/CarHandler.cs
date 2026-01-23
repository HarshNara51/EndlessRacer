using UnityEngine;

public class CarHandler : MonoBehaviour
{
    [SerializeField]
    Rigidbody rb;

    //Multipliers
    float accelerationMultiplier = 3;

    //Input
    Vector2 input = Vector2.zero;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        Accelerate();
    }

    void Accelerate()
    {
        rb.linearDamping = 0;

        rb.AddForce(rb.transform.forward * accelerationMultiplier * input.y);
    }

    void Brake()
    {
        if (rb.linearVelocity.z<=0)
        return;
    }
}
