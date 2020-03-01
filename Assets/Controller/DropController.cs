using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DropRecorder))]
public class DropController : MonoBehaviour
{
    public int cameraCount = 2;
    public float meanCameraDistance = 5f;
    public float minCameraDistance = 1f;
    public float meanTargetTime = 0.5f;

    public float meanCameraHeight = 1.5f;
    public float minCameraHeight = 0.25f;

    public float meanCameraOffset = 0.25f;
    public float meanCameraTilt = 0.5f;

    public float meanCameraFocalLength = 30f;
    public float minCameraFocalLength = 5f;

    public float meanCameraLensShift = 0.2f;

    public float meanBallHeight = 1.0f;
    public float meanBallVelocity = 4.0f;
    public float minBallEnergy = 2.0f;

    public bool recordData = true;

    public DropBall ballPrefab;
    public Camera cameraPrefab;

    private List<Camera> cameras;
    private DropBall ball;

    private DropRecorder recorder { get { return GetComponent<DropRecorder>(); } }

    private void PlaceBall()
    {
        Vector3 spawnPoint = new Vector3(), spawnVelocity = new Vector3();

        for (int attempt = 0; attempt < 25; attempt++)
        {
            spawnPoint = new Vector3(Random.value * 16 - 8, Random.value * meanBallHeight * 2.0f, Random.value * 16 - 8);
            spawnVelocity = Random.insideUnitSphere * meanBallVelocity * 4.0f / 3.0f;

            // Ensure the ball has enough total energy to warrant observation; could work backwards from this limitation instead, not a priority
            if ((spawnVelocity.sqrMagnitude / 2 - (spawnPoint.y - ballPrefab.radius) * Physics.gravity.y) * ballPrefab.mass > minBallEnergy)
                break;
        }

        if (ball != null)
        {
            ball.gameObject.SetActive(false);
            Destroy(ball.gameObject);
        }
        ball = Instantiate(ballPrefab, spawnPoint, Quaternion.identity);
        ball.Initialize(spawnVelocity);
    }

    private void PlaceCameras()
    {
        if (cameras == null) { cameras = new List<Camera>(cameraCount); }

        for (int i = 0; i < cameraCount; i++)
        {
            if (cameras.Count <= i)
                cameras.Add(Instantiate(cameraPrefab));
            cameras[i].forceIntoRenderTexture = true;
            PlaceCamera(cameras[i]);
        }
    }

    private void PlaceCamera(Camera camera)
    {
        // set the target position
        float targetTime = meanTargetTime * Random.value * 2;
        Vector3 targetPosition = ball.GetApproximateBallPosition(targetTime);

        // set the camera's position
        Vector3 cameraPosition = targetPosition + new Vector3(Random.value * 2 - 1, 0, Random.value * 2 - 1).normalized * (minCameraDistance + Random.value * 2 * (meanCameraDistance - minCameraDistance));
        cameraPosition.y = minCameraHeight + Random.value * 2 * (meanCameraHeight - minCameraHeight);

        // set the camera's view direction
        Vector3 cameraForward = (targetPosition - cameraPosition).normalized;
        Quaternion cameraTilt = Quaternion.AngleAxis(meanCameraTilt * (Random.value * 2 - 1), cameraForward);
        Vector3 cameraUp = Vector3.Cross(cameraForward, Vector3.Cross(Vector3.up, cameraForward)).normalized;
        Quaternion cameraRotation = Quaternion.LookRotation(cameraForward, cameraTilt * cameraUp);

        // apply changes to camera
        camera.transform.position = cameraPosition;
        camera.transform.rotation = cameraRotation;
        camera.focalLength = minCameraFocalLength - Mathf.Log(1 - Random.value) * (meanCameraFocalLength - minCameraFocalLength);
        camera.lensShift = Random.insideUnitCircle.normalized * Random.value * meanCameraLensShift * 2;

        // apply camera targeting offset
        camera.transform.rotation = Quaternion.AngleAxis(camera.fieldOfView * meanCameraOffset * (2 * Random.value - 1), camera.transform.up) * camera.transform.rotation;
        camera.transform.rotation = Quaternion.AngleAxis(camera.fieldOfView * meanCameraOffset * (2 * Random.value - 1), camera.transform.right) * camera.transform.rotation;

#if UNITY_EDITOR
        Debug.DrawRay(cameraPosition, camera.transform.forward * (cameraPosition - targetPosition).magnitude, Color.green, 10.0f);
        Debug.DrawLine(targetPosition, cameraPosition, Color.blue, 10.0f);
#endif
    }

private void Update()
{
    if (recorder.ready && Input.GetKeyDown(KeyCode.Space))
    {
        PlaceBall();
        PlaceCameras();
        if (recordData)
            recorder.StartRecording(ball, cameras);
    }
}
}