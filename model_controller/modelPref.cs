using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class modelPref
{
    public float max_rotation_angle;

    public float ear_max_threshold;
    public float ear_min_threshold;


    public float mar_max_threshold;
    public float mar_min_threshold;

    public modelPref(float max_rotation_angle = 45.0f,
                         float ear_max_threshold = 0.42f,
                         float ear_min_threshold = 0.34f,
                         float mar_max_threshold = 0.8f,
                         float mar_min_threshold = 0.0f)
    {
        this.max_rotation_angle = max_rotation_angle;
        this.ear_max_threshold = ear_max_threshold;
        this.ear_min_threshold = ear_min_threshold;
        this.mar_max_threshold = mar_max_threshold;
        this.mar_min_threshold = mar_min_threshold;
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public void LoadFromJson(string a_Json)
    {
        JsonUtility.FromJsonOverwrite(a_Json, this);
    }

}

public interface ISaveable
{
    void PopulateSaveData(modelPref a_SaveData);
    void LoadFromSaveData(modelPref a_SaveData);
}