using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallData
{
    private DropBall ball;

    public Vector3 initialVelocity { get { return ball.initialVelocity; } set { } }
    public float radius { get { return ball.radius; } set { } }
    public float mass { get { return ball.mass; } set { } }
    public float airDensity { get { return ball.airDensity; } set { } }
    public float dragCoefficient { get { return ball.dragCoefficient; } set { } }
    public float coefficientOfRestitution { get { return ball.coefficientOfRestitution; } set { } }

    public List<float> times;
    public List<Vector3> positions;

    public BallData()
    {
        times = new List<float>(500);
        positions = new List<Vector3>(500);
    }
    public BallData(DropBall ball)
    {
        times = new List<float>(500);
        positions = new List<Vector3>(500);
        this.ball = ball;
    }

    public void SavePoint()
    {
        times.Add(Time.time);
        positions.Add(ball.transform.position);
    }
}