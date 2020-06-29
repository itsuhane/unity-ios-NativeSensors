using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NativeSensors;

public class MotionDisplay : MonoBehaviour, DeviceMotionHandler, HeadphoneMotionHandler
{
    public Text DMTimestampText;
    public Text DMRotationRateText;
    public Text DMUserAccelerationText;
    public Text HMTimestampText;
    public Text HMRotationRateText;
    public Text HMUserAccelerationText;

    private DeviceMotion deviceMotion;
    private HeadphoneMotion headphoneMotion;

    public void OnDeviceMotion(double t, Vector3 rotationRate, Vector3 userAcceleration)
    {
        DMTimestampText.text = string.Format("T: {0,12:#######.00}", t);
        DMRotationRateText.text = string.Format("R: ({0,8:#0.000}, {1,8:#0.000}, {2,8:#0.000})", rotationRate.x, rotationRate.y, rotationRate.z);
        DMUserAccelerationText.text = string.Format("A: ({0,8:#0.000}, {1,8:#0.000}, {2,8:#0.000})", userAcceleration.x, userAcceleration.y, userAcceleration.z);
    }

    public void OnHeadphoneMotion(double t, Vector3 rotationRate, Vector3 userAcceleration)
    {
        HMTimestampText.text = string.Format("T: {0,12:#######.00}", t);
        HMRotationRateText.text = string.Format("R: ({0,8:#0.000}, {1,8:#0.000}, {2,8:#0.000})", rotationRate.x, rotationRate.y, rotationRate.z);
        HMUserAccelerationText.text = string.Format("A: ({0,8:#0.000}, {1,8:#0.000}, {2,8:#0.000})", userAcceleration.x, userAcceleration.y, userAcceleration.z);
    }

    public void OnHeadphoneConnect()
    {
        headphoneMotion.StartSensors();
    }

    public void OnHeadphoneDisconnect()
    {
        headphoneMotion.StopSensors();
    }

    void Awake()
    {
        DMTimestampText.text = "";
        DMRotationRateText.text = "";
        DMUserAccelerationText.text = "";
        HMTimestampText.text = "";
        HMRotationRateText.text = "";
        HMUserAccelerationText.text = "";
        deviceMotion = GetComponent<DeviceMotion>();
        headphoneMotion = GetComponent<HeadphoneMotion>();
        deviceMotion.deviceMotionHandler = this;
        headphoneMotion.headphoneMotionHandler = this;
    }

    void OnEnable()
    {
        deviceMotion.StartSensors();
        headphoneMotion.StartSensors();
    }

    void OnDisable()
    {
        deviceMotion.StopSensors();
        headphoneMotion.StopSensors();
    }
}
