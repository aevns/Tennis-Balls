using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropRecorder : MonoBehaviour
{

    public int physicsStepsPerFrame = 8;
    public int recordingFrameRate = 60;
    public float recordingDuration = 3;

    public GameObject savingMessage;

    public bool recording { get; private set; }
    public bool ready { get; private set; } 

    private int frame;
    private int maxFrames
    {
        get { return (int)(Time.captureFramerate * recordingDuration); }
    }

    private DropData data;

    public void StartRecording(DropBall ball, List<Camera> dropCams)
    {
        recording = true;
        ready = false;
        frame = 0;

        BallData ballData = new BallData(ball);
        List<CameraData> camerasData = new List<CameraData>(dropCams.Count);
        for (int i = 0; i < dropCams.Count; i++)
        {
            camerasData.Add(new CameraData(dropCams[i]));
            camerasData[i].cameraNumber = i;
        }

        data = new DropData(ballData, camerasData);
    }

    public IEnumerator EndRecording()
    {
        savingMessage.SetActive(true);
        recording = false;
        yield return null;

        data.SaveData();
        data = null;
        ready = true;
        savingMessage.SetActive(false);
    }

    private IEnumerator Start()
    {
        // Using a constant framerate for camera recording.
        // Does not relate to realtime playback rate; current recording method causes realtime slowdown.
        Time.captureFramerate = recordingFrameRate;
        Time.fixedDeltaTime = Time.captureDeltaTime / physicsStepsPerFrame;
        recording = false;
        ready = true;
        savingMessage.SetActive(false);

        while (true)
        {
            if (recording && frame >= maxFrames)
            {
                yield return EndRecording();
            }
            yield return null;
        }
    }

    private void Update()
    {
        if (recording)
        {
            data.ballData.SavePoint();
            for (int i = 0; i < data.camerasData.Count; i++)
            {
                data.camerasData[i].SaveFrame();
            }
            frame++;
        }
    }
}