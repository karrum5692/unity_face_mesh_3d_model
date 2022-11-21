using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Photon;
using Photon.Pun;


public class FaceMesh : MonoBehaviourPun, IPunObservable
{
    private Animator anim;

    public PhotonView pv;

    // for blinking stuff
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

    public Transform neck;
    public Quaternion neck_quat;

    public string clientMessage;

    Thread receiveThread;
    TcpClient client;
    TcpListener listener;
    int port = 9999;

    public float roll = 0, pitch = 0, yaw = 0;
    //private float x_ratio_left = 0, y_ratio_left = 0, x_ratio_right = 0, y_ratio_right = 0;
    public float ear_left = 0, ear_right = 0;
    public float mar = 0;

    public float smileParamete = 0;

    //public static FaceMesh instance = null;

    public float rpc_roll = 0, rpc_pitch = 0, rpc_yaw = 0;
    public float rpc_ear_left = 0, rpc_ear_right = 0;
    public float rpc_mar = 0;





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
                            clientMessage = Encoding.ASCII.GetString(incommingData);
                            Debug.Log(clientMessage);
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
        //print(string.Format("Roll: {0:F}; Pitch: {1:F}; Yaw: {2:F}", roll, pitch, yaw));
        //print(string.Format("Left eye: {0:F}, {1:F}; Right eye: {2:F}, {3:F}",
        //    x_ratio_left, y_ratio_left, x_ratio_right, y_ratio_right));


        if (pv.IsMine)
        {
            HeadRotation();
            EyeBlinking();
            MouthMoving();
            SmileGesture();
        }
        else
        {

            RPC_HeadRotation();
            RPC_EyeBlinking();
            RPC_MouthMoving();
            print($"roll: {rpc_roll} \r\n pitch: {rpc_pitch} \r\n yaw: {rpc_yaw} \r\n ear_left: {rpc_ear_left} \r\n ear_right: {rpc_ear_right} \r\n mar : {rpc_mar}");
        }



    }





    void HeadRotation()
    {

        var rot = transform.rotation.eulerAngles;


        // clamp the angles to prevent unnatural movement
        float pitch_clamp = Mathf.Clamp(pitch, -max_rotation_angle, max_rotation_angle);
        float yaw_clamp = Mathf.Clamp(yaw, -max_rotation_angle, max_rotation_angle);
        float roll_clamp = Mathf.Clamp(roll, -max_rotation_angle, max_rotation_angle);

        // do rotation at neck to control the movement of head
        neck.rotation = Quaternion.Euler(-pitch_clamp + 30, rot.y - yaw_clamp, -roll_clamp) * neck_quat;

    }


    void EyeBlinking()
    {

        float eyes_left = ear_left;
        float eyes_right = ear_right;

 
        eyes_left = Mathf.Clamp(eyes_left, ear_min_threshold, ear_max_threshold);
        float xl = Mathf.Abs((eyes_left - ear_min_threshold) / (ear_max_threshold - ear_min_threshold) - 1);

        eyes_right = Mathf.Clamp(eyes_right, ear_min_threshold, ear_max_threshold);
        float xr = Mathf.Abs((eyes_right - ear_min_threshold) / (ear_max_threshold - ear_min_threshold) - 1);

        float yl = 90 * Mathf.Pow(xl, 2) - 5 * xl;
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
        // enlarge it to [0, 100]
        ratio = ratio * 100 / (mar_max_threshold - mar_min_threshold);
        SetMouth(ratio);
    }



    void SetMouth(float ratio)
    {
        ref_main_face.SetBlendShapeWeight(5, ratio);
    }

    //void OnApplicationQuit()
    //{
    //    // close the thread when the application quits
    //    receiveThread.Abort();
    //}

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


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //roll = 0, pitch = 0, yaw = 0;
            //ear_left = 0, ear_right = 0;
            //mar = 0
            stream.SendNext(roll);
            stream.SendNext(pitch);
            stream.SendNext(yaw);
            stream.SendNext(ear_left);
            stream.SendNext(ear_right);
            stream.SendNext(mar);
        }
        else if (stream.IsReading)
        {
            rpc_roll = (float)stream.ReceiveNext();
            rpc_pitch = (float)stream.ReceiveNext();
            rpc_yaw = (float)stream.ReceiveNext();
            rpc_ear_left = (float)stream.ReceiveNext();
            rpc_ear_right = (float)stream.ReceiveNext();
            rpc_mar = (float)stream.ReceiveNext();
        }
    }





    public void SmileGesture()
    {
        if (Input.GetKey(KeyCode.F1))
        {
            //ref_main_face.SetBlendShapeWeight(34, 100); //���� �Բ���

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
                ref_main_face.SetBlendShapeWeight(36, smileParamete / 3);
                ref_main_face.SetBlendShapeWeight(37, smileParamete / 3);
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






    void RPC_HeadRotation()
    {

        var rot = transform.rotation.eulerAngles;

        float pitch_clamp = Mathf.Clamp(rpc_pitch, -max_rotation_angle, max_rotation_angle);
        float yaw_clamp = Mathf.Clamp(rpc_yaw, -max_rotation_angle, max_rotation_angle);
        float roll_clamp = Mathf.Clamp(rpc_roll, -max_rotation_angle, max_rotation_angle);


        neck.rotation = Quaternion.Euler(-pitch_clamp + 30, rot.y - yaw_clamp, -roll_clamp) * neck_quat;

    }


    void RPC_EyeBlinking()
    {
        float eyes_left = rpc_ear_left;
        float eyes_right = rpc_ear_right;

        eyes_left = Mathf.Clamp(eyes_left, ear_min_threshold, ear_max_threshold);
        float xl = Mathf.Abs((eyes_left - ear_min_threshold) / (ear_max_threshold - ear_min_threshold) - 1);

        eyes_right = Mathf.Clamp(eyes_right, ear_min_threshold, ear_max_threshold);
        float xr = Mathf.Abs((eyes_right - ear_min_threshold) / (ear_max_threshold - ear_min_threshold) - 1);

        float yl = 90 * Mathf.Pow(xl, 2) - 5 * xl;
        float yr = 90 * Mathf.Pow(xr, 2) - 5 * xr;

        RPC_SetEyes_Left(yl);
        RPC_SetEyes_Right(yr);
    }


    void RPC_SetEyes_Left(float ratio)
    {
        ref_main_face.SetBlendShapeWeight(1, ratio);
    }


    void RPC_SetEyes_Right(float ratio)
    {
        ref_main_face.SetBlendShapeWeight(2, ratio);
    }


    void RPC_MouthMoving()
    {
        float mar_clamped = Mathf.Clamp(rpc_mar, mar_min_threshold, mar_max_threshold);
        float ratio = (mar_clamped - mar_min_threshold) / (mar_max_threshold - mar_min_threshold);
        ratio = ratio * 100 / (mar_max_threshold - mar_min_threshold);
        RPC_SetMouth(ratio);
    }




    void RPC_SetMouth(float ratio)
    {
        ref_main_face.SetBlendShapeWeight(5, ratio);
    }





















}
