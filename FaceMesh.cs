using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class FaceMesh: MonoBehaviour
{
    private Animator anim;

    public SkinnedMeshRenderer ref_main_face;

    public float max_rotation_angle = 45.0f;

    public float ear_max_threshold = 0.38f;
    public float ear_min_threshold = 0.30f;

    [HideInInspector]
    public float eye_ratio_close = 70.0f;
    [HideInInspector]
    public float eye_ratio_half_close = 35.0f;
    [HideInInspector]
    public float eye_ratio_open = 0.0f;

    public float mar_max_threshold = 1.0f;
    public float mar_min_threshold = 0.0f;

    private Transform neck;
    private Quaternion neck_quat;

    Thread receiveThread;
    TcpClient client;
    TcpListener listener;
    int port = 9999;

    private float roll = 0, pitch = 0, yaw = 0;
    private float x_ratio_left = 0, y_ratio_left = 0, x_ratio_right = 0, y_ratio_right = 0;
    private float ear_left = 0, ear_right = 0;
    private float mar = 0;

    private float smileParamete = 0;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();

        neck = anim.GetBoneTransform(HumanBodyBones.Neck);
        neck_quat = Quaternion.Euler(0, 90, -90);
        SetEyes_Left(eye_ratio_open);
        SetEyes_Right(eye_ratio_open);

        InitTCP();
    }

    private void InitTCP()
    {
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    private void ReceiveData()
    {
        try
        {
            listener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
            listener.Start();
            Byte[] bytes = new Byte[1024];

            while (true)
            {
                using (client = listener.AcceptTcpClient())
                {
                    using (NetworkStream stream = client.GetStream())
                    {
                        int length;
                        while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            var incommingData = new byte[length];
                            Array.Copy(bytes, 0, incommingData, 0, length);
                            string clientMessage = Encoding.ASCII.GetString(incommingData);
                            string[] res = clientMessage.Split(' ');

                            roll = float.Parse(res[0]);
                            pitch = float.Parse(res[1]);
                            yaw = float.Parse(res[2]);
                            ear_left = float.Parse(res[3]);
                            ear_right = float.Parse(res[4]);
                            mar = float.Parse(res[9]);
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            print(e.ToString());
        }
    }

    // Update is called once per frame
    void Update()
    {
        HeadRotation();
        EyeBlinking();
        MouthMoving();
        SmileGesture();
    }

    void HeadRotation()
    {

        var rot = transform.rotation.eulerAngles;

        float pitch_clamp = Mathf.Clamp(pitch, -max_rotation_angle, max_rotation_angle);
        float yaw_clamp = Mathf.Clamp(yaw, -max_rotation_angle, max_rotation_angle);
        float roll_clamp = Mathf.Clamp(roll, -max_rotation_angle, max_rotation_angle);

        neck.rotation = Quaternion.Euler(-pitch_clamp+30 ,  rot.y + yaw_clamp, -roll_clamp) * neck_quat;

    }
    void EyeBlinking()
    {
        float eyes_left = ear_left;
        float eyes_right = ear_right;

        eyes_left = Mathf.Clamp(eyes_left, ear_min_threshold, ear_max_threshold);
        float xl = Mathf.Abs((eyes_left - ear_min_threshold) / (ear_max_threshold - ear_min_threshold) - 1);

        eyes_right = Mathf.Clamp(eyes_right, ear_min_threshold, ear_max_threshold);
        float xr = Mathf.Abs((eyes_right - ear_min_threshold) / (ear_max_threshold - ear_min_threshold) - 1);

        float yl = 90 * Mathf.Pow(xl, 2) - 5 * xl ;
        float yr = 90 * Mathf.Pow(xr, 2) - 5 * xr;

        SetEyes_Left(yl);
        SetEyes_Right(yr);
    }

    void SetEyes_Left(float ratio)
    {
        ref_main_face.SetBlendShapeWeight(1, ratio);
    }

    void SetEyes_Right(float ratio)
    {
        ref_main_face.SetBlendShapeWeight(2, ratio);
    }

    void MouthMoving()
    {
        float mar_clamped = Mathf.Clamp(mar, mar_min_threshold, mar_max_threshold);
        float ratio = (mar_clamped - mar_min_threshold) / (mar_max_threshold - mar_min_threshold);

        ratio = ratio * 100 / (mar_max_threshold - mar_min_threshold);
        SetMouth(ratio);
    }

    void SetMouth(float ratio)
    {
        ref_main_face.SetBlendShapeWeight(5, ratio);
    }


    public void PopulateSaveData(modelPref modelPref)
    {
        modelPref.max_rotation_angle = max_rotation_angle;
        modelPref.ear_max_threshold = ear_max_threshold;
        modelPref.ear_min_threshold = ear_min_threshold;
        modelPref.mar_max_threshold = mar_max_threshold;
        modelPref.mar_min_threshold = mar_min_threshold;
    }

    public void LoadFromSaveData(modelPref modelPref)
    {
        max_rotation_angle = modelPref.max_rotation_angle;
        ear_max_threshold = modelPref.ear_max_threshold;
        ear_min_threshold = modelPref.ear_min_threshold;
        mar_max_threshold = modelPref.mar_max_threshold;
        mar_min_threshold = modelPref.mar_min_threshold;
    }


    public void SmileGesture()
    {
        if (Input.GetKey(KeyCode.F1))
        {
            for (int i = 0; i <= 50; i++)
            {
                ref_main_face.SetBlendShapeWeight(i, 0);
            }
                ref_main_face.SetBlendShapeWeight(0, 100);

        }


        if (Input.GetKey(KeyCode.F2))
        {
            for (int i = 0; i <= 50; i++)
            {
                ref_main_face.SetBlendShapeWeight(i, 0);
            }
            ref_main_face.SetBlendShapeWeight(4, 100);
            ref_main_face.SetBlendShapeWeight(8, 100);
        }



        if (Input.GetKey(KeyCode.F3))
        {
            for (int i = 0; i <= 50; i++)
            {
                ref_main_face.SetBlendShapeWeight(i, 0);
            }
            ref_main_face.SetBlendShapeWeight(4, 100);
            ref_main_face.SetBlendShapeWeight(6, 100);
            ref_main_face.SetBlendShapeWeight(7, 100);
            ref_main_face.SetBlendShapeWeight(40, 100);
            ref_main_face.SetBlendShapeWeight(41, 100);
        }

        if (Input.GetKey(KeyCode.F4))
        {
            for (int i = 0; i <= 50; i++)
            {
                ref_main_face.SetBlendShapeWeight(i, 0);
            }
            ref_main_face.SetBlendShapeWeight(9, 100);
            ref_main_face.SetBlendShapeWeight(13, 100);
            ref_main_face.SetBlendShapeWeight(14, 100);
            ref_main_face.SetBlendShapeWeight(15, 100);
            ref_main_face.SetBlendShapeWeight(16, 100);
        }



        if (Input.GetKey(KeyCode.R))
        {
            if (smileParamete <= 100)
            {
                smileParamete += Mathf.Lerp(0, 100, Time.deltaTime * 1f);
                ref_main_face.SetBlendShapeWeight(0, smileParamete);
                ref_main_face.SetBlendShapeWeight(36, smileParamete/3);
                ref_main_face.SetBlendShapeWeight(37, smileParamete/3);
            }
            print(smileParamete);
        }


        if (Input.GetKey(KeyCode.Escape))
        {
            for (int i = 0; i <= 50; i++)
            {
                ref_main_face.SetBlendShapeWeight(i, 0);
                smileParamete = 0;
            }
        }


    }

}
