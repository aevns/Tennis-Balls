using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

using System.Collections;
using System.Collections.Generic;

public class DropData
{
    public string recordName;
    public BallData ballData;
    public List<CameraData> camerasData;
    
    public DropData()
    {
        recordName = "Recording_" + Convert.ToString(new System.Random().Next(), 16);
    }
    public DropData(BallData ball, List<CameraData> camerasData)
    {
        recordName = "Recording_" + Convert.ToString(new System.Random().Next(), 16);
        this.ballData = ball;
        this.camerasData = camerasData;
    }

    public string CreateSaveDirectory()
    {
        string saveDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + "/BallDrop/" + recordName;
        System.IO.Directory.CreateDirectory(saveDirectory);
        for (int i = 0; i < camerasData.Count; i++)
        {
            System.IO.Directory.CreateDirectory(saveDirectory + "/camera_" + i);
        }
        return saveDirectory;
    }

    public void SaveData()
    {
        string saveDirectory = CreateSaveDirectory();
        XmlSerializer serializer = new XmlSerializer(typeof(DropData));
        TextWriter writer = new StreamWriter(saveDirectory + "/data.xml");
        serializer.Serialize(writer, this);
        writer.Close();
        for (int i = 0; i < camerasData.Count; i++)
        {
            camerasData[i].SaveVideo(saveDirectory + "/camera_" + i);
        }
    }
}