using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DropRecorder))]
public class DropController : MonoBehaviour
{
    public int cameraCount = 2;
    public float meanCameraDistance = 5f;
    public float minCameraDistance = 1f;

    public float meanCameraHeight = 1.5f;
    public float minCameraHeight = 0.25f;

    public float meanCameraTilt = 0.5f;

    public float meanCameraFocalLength = 30f;
    public float minCameraFocalLength = 5f;

    public float meanCameraLensShift = 0.2f;

    public float meanTargetDistance = 2f;
    public float meanTargetTime = 0.5f;

    public float meanBallHeight = 1.0f;
    public float meanBallVelocity = 4.0f;

    public bool recordData = true;

    public DropBall ballPrefab;
    public Camera cameraPrefab;

    private List<Camera> cameras;
    private DropBall ball;

    private DropRecorder recorder { get { return GetComponent<DropRecorder>(); } }

    private void PlaceBall()
    {
        if (ball != null)
        {
            ball.gameObject.SetActive(false);
            Destroy(ball.gameObject);
        }

        Vector3 spawnPoint = Vector3.up * Random.value * meanBallHeight * 2;
        Vector3 spawnVelocity = Random.insideUnitSphere.normalized * Random.value * meanBallVelocity * 2;

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
            PlaceCamera(cameras[i]);
            cameras[i].forceIntoRenderTexture = true;
        }
    }

    private void PlaceCamera(Camera camera)
    {
        // decide when the ball should be in view
        float targetTime = meanTargetTime * Random.value * 2;
        Vector3 expectedBallPosition = ball.GetApproximateBallPosition(targetTime);

        for (int attempt = 0; attempt < 50; attempt++)
        {
            // set the target position
            Vector3 targetPosition = expectedBallPosition + Random.insideUnitSphere * meanTargetDistance * 2;

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

            // Do not accept camera placement unless the origin is located in it
            Vector3 p = camera.WorldToViewportPoint(expectedBallPosition);
            if (p.x > 0 && p.x < 1 && p.y > 0 && p.y < 1 && p.z > 0)
                return;
        }
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