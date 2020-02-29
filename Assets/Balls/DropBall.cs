using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class DropBall : MonoBehaviour
{
    public float radius = 0.0329f;
    public float mass = 0.057f;
    public float airDensity = 1.1455f;
    public float dragCoefficient = 0.5f;
    public float coefficientOfRestitution = 0.4f;

    public Vector3 initialVelocity { get; private set; }
    private float crossSectionArea
    {
        get { return Mathf.PI * radius * radius; }
    }
    private float drag
    {
        get { return airDensity * crossSectionArea * dragCoefficient / (2 * mass); }
    }
    private Vector3 velocity
    {
        get { return GetComponent<Rigidbody>().velocity; }
        set { GetComponent<Rigidbody>().velocity = value; }
    }
    private Vector3 gravity
    {
        get { return Physics.gravity; }
    }

    public void Initialize(Vector3 initialVelocity)
    {
        SphereCollider collider = GetComponent<SphereCollider>();
        collider.radius = radius / (2 * transform.localScale.x);

        PhysicMaterial material = collider.material ?? (collider.sharedMaterial = new PhysicMaterial());
        material.bounceCombine = PhysicMaterialCombine.Minimum;
        material.bounciness = coefficientOfRestitution;

        // Using our own gravity and drag
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        GetComponent<Rigidbody>().useGravity = false;
        GetComponent<Rigidbody>().mass = mass;
        GetComponent<Rigidbody>().drag = 0;
        GetComponent<Rigidbody>().angularDrag = 0;

        this.initialVelocity = initialVelocity;
        velocity = initialVelocity - Acceleration(initialVelocity, Time.fixedDeltaTime) / 2;
    }

    public Vector3 GetApproximateBallPosition(float time)
    {
        return GetApproximateBallPosition(time, transform.position, initialVelocity);
    }

    /// <summary>
    /// Get the ball's position, ignoring nonlinear effects
    /// </summary>
    /// <param name="time"></param>
    /// <param name="p0"></param>
    /// <param name="v0"></param>
    /// <returns></returns>
    private Vector3 GetApproximateBallPosition(float time, Vector3 p0, Vector3 v0)
    {
        Vector3 p1 = p0 + v0 * time + Physics.gravity * time * time / 2;

        if (p1.y > radius)
            return p1;

        float tGrounded = (-v0.y - Mathf.Sqrt(v0.y * v0.y - 2 * (p0.y - radius) * Physics.gravity.y)) / Physics.gravity.y;

        if (tGrounded < 0.01f || double.IsNaN(tGrounded) || double.IsInfinity(tGrounded))
            return p0;

        Vector3 vBounce = v0 + Physics.gravity * tGrounded;
        vBounce.y *= -coefficientOfRestitution;
        return GetApproximateBallPosition(
            time - tGrounded,
            p0 + v0 * tGrounded + Physics.gravity * tGrounded * tGrounded / 2,
            vBounce);
    }

    /// <summary>
    /// Apply acceleration due to gravity and drag. No acceleration due to lift as yet.
    /// </summary>
    /// <param name="v"></param>
    /// <param name="dt"></param>
    /// <returns></returns>
    private Vector3 Acceleration(Vector3 v, float dt)
    {
        Vector3 dv = gravity * dt / 2;
        dv -= drag * v * v.magnitude * dt;
        dv += gravity * dt / 2;
        return dv;
    }

    /// <summary>
    /// Applying acceleration in the physics update.
    /// The physics engine uses the semi-explicit euler method, which isn't ideal.
    /// We'll just switch it to leapfrog integration instead.
    /// </summary>
    private void FixedUpdate()
    {
#if UNITY_EDITOR
        Debug.DrawLine(transform.position, transform.position - velocity * Time.fixedDeltaTime, Color.red, 20.0f);
#endif
        velocity += Acceleration(velocity, Time.fixedDeltaTime);
    }
}