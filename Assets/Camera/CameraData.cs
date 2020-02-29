using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraData
{
    public struct IntrinsicMatrix
    {
        public float
            m00, m01, m02, m03, m04,
            m10, m11, m12, m13, m14,
            m20, m21, m22, m23, m24;
    }

    private Camera camera;
    private Texture2D writeTexture;
    private List<RenderTexture> textureBuffer;

    public int cameraNumber;

    public Vector3 position
    {
        get { return camera.transform.position; }
        set { }
    }
    public Vector3 forward
    {
        get { return camera.transform.forward; }
        set { }
    }
    public Vector3 up
    {
        get { return camera.transform.up; }
        set { }
    }
    public Quaternion rotation
    {
        get { return camera.transform.rotation; }
        set { }
    }
    public Matrix4x4 localToWorldMatrix
    {
        get { return camera.transform.localToWorldMatrix; }
        set { }
    }

    public float focalLength
    {
        get { return camera.focalLength; }
        set { }
    }
    public IntrinsicMatrix intrinsicMatrix
    {
        get { return GetIntrinsicParameters(); }
        set { }
    }

    private IntrinsicMatrix GetIntrinsicParameters()
    {
        float focalLength = camera.focalLength;
        float xScale = camera.pixelWidth;
        float yScale = camera.pixelHeight;
        float xPoint = xScale * camera.lensShift.x;
        float yPoint = yScale * camera.lensShift.y;

        IntrinsicMatrix intrinsicParameters = new IntrinsicMatrix();
        intrinsicParameters.m00 = focalLength * xScale;
        intrinsicParameters.m11 = focalLength * yScale;
        // For this example, camera skew is 0
        intrinsicParameters.m02 = xPoint;
        intrinsicParameters.m12 = yPoint;
        intrinsicParameters.m22 = 1;

        return intrinsicParameters;
    }

    public CameraData()
    {
        textureBuffer = new List<RenderTexture>(500);
    }
    public CameraData(Camera camera)
    {
        textureBuffer = new List<RenderTexture>(500);
        RenderTexture tex = camera.targetTexture;
        writeTexture = new Texture2D(tex.width, tex.height, TextureFormat.RGB24, false);
        this.camera = camera;
    }

    public void SaveFrame()
    {
        camera.Render();
        RenderTexture tex = new RenderTexture(camera.targetTexture);
        Graphics.CopyTexture(camera.targetTexture, tex);
        textureBuffer.Add(tex);
    }

    public void SaveVideo(string directory)
    {
        int width = textureBuffer[0].width;
        int height = textureBuffer[0].height;

        for (int i = 0; i < textureBuffer.Count; i++)
        {
            //Could be replaced with the new AsyncGPUReadbackRequest, but we would need to wait before recording again regardless.
            RenderTexture.active = textureBuffer[i];
            writeTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            byte[] bytes = writeTexture.EncodeToPNG();
            System.IO.File.WriteAllBytes(directory + "/frame_" + string.Format("{0:D3}", i) + ".png", bytes);
        }
    }
}