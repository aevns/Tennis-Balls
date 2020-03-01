using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraData
{
    private Camera camera;
    private Texture2D writeTexture;
    private List<RenderTexture> textureBuffer;

    public int cameraNumber;

    public Vector3 position
    {
        get { return camera.transform.position; }
        set { }
    }

    public Quaternion rotation
    {
        get { return camera.transform.rotation; }
        set { }
    }

    public float focalLength
    {
        get { return camera.focalLength; }
        set { }
    }

    public Matrix4x4 extrinsicMatrix
    {
        get { return GetExtrinsicParameters(); }
        set { }
    }

    public Matrix4x4 intrinsicMatrix
    {
        get { return GetIntrinsicParameters(); }
        set { }
    }

    private Matrix4x4 GetExtrinsicParameters()
    {
        Quaternion worldToCameraRotation = Quaternion.Inverse(camera.transform.rotation);
        Vector4 t = worldToCameraRotation * -camera.transform.position;
        Matrix4x4 mat = Matrix4x4.Rotate(worldToCameraRotation);
        mat[0, 3] = t[0];
        mat[1, 3] = t[1];
        mat[2, 3] = t[2];
        mat[3, 3] = 1;
        return mat;
    }

    private Matrix4x4 GetIntrinsicParameters()
    {
        Vector2 shift = camera.GetGateFittedLensShift();

        float unitLengthGateHeight = 2 * Mathf.Tan(camera.GetGateFittedFieldOfView() * 0.5f * Mathf.Deg2Rad);
        float widthInPixels = camera.targetTexture.width;
        float heightInPixels = camera.targetTexture.height;

        Matrix4x4 intrinsicParameters = new Matrix4x4();
        intrinsicParameters.m00 = widthInPixels / (unitLengthGateHeight * camera.aspect);
        intrinsicParameters.m11 = heightInPixels / unitLengthGateHeight;
        // For these examples, camera skew at m01 is 0
        intrinsicParameters.m02 = -shift.x * widthInPixels;
        intrinsicParameters.m12 = -shift.y * heightInPixels;
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
        RenderTexture.active = null;
        for (int i = 0; i < textureBuffer.Count; i++)
        {
            textureBuffer[i].Release();
        }
    }
}