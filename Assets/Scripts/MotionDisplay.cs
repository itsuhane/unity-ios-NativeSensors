using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NativeSensors;

public class MotionDisplay : MonoBehaviour, AccelerometerMotionHandler, GyroscopeMotionHandler, MagnetometerMotionHandler, DeviceMotionHandler, HeadphoneMotionHandler
{
    public Text AMTimestampText;
    public Text AMAccelerationText;

    public Text GMTimestampText;
    public Text GMRotationRateText;

    public Text MMTimestampText;
    public Text MMMagneticFieldText;

    public Text DMTimestampText;
    public Text DMRotationRateText;
    public Text DMUserAccelerationText;

    public Text HMTimestampText;
    public Text HMRotationRateText;
    public Text HMUserAccelerationText;

    private AccelerometerMotion accelerometerMotion;
    private GyroscopeMotion gyroscopeMotion;
    private MagnetometerMotion magnetometerMotion;
    private DeviceMotion deviceMotion;
    private HeadphoneMotion headphoneMotion;

    public void OnAccelerometerMotion(double timestamp, Vector3 acceleration)
    {
        AMTimestampText.text = string.Format("T: {0,12:#######0.00}", timestamp);
        AMAccelerationText.text = string.Format("R: ({0,8:#0.000}, {1,8:#0.000}, {2,8:#0.000})", acceleration.x, acceleration.y, acceleration.z);
    }

    public void OnGyroscopeMotion(double timestamp, Vector3 rotationRate)
    {
        GMTimestampText.text = string.Format("T: {0,12:#######0.00}", timestamp);
        GMRotationRateText.text = string.Format("R: ({0,8:#0.000}, {1,8:#0.000}, {2,8:#0.000})", rotationRate.x, rotationRate.y, rotationRate.z);
    }

    public void OnMagnetometerMotion(double timestamp, Vector3 magneticField)
    {
        MMTimestampText.text = string.Format("T: {0,12:#######0.00}", timestamp);
        MMMagneticFieldText.text = string.Format("R: ({0,8:#0.000}, {1,8:#0.000}, {2,8:#0.000})", magneticField.x, magneticField.y, magneticField.z);
    }

    public void OnDeviceMotion(ref DeviceMotionData motionData)
    {
        DMTimestampText.text = string.Format("T: {0,12:#######0.00}", motionData.timestamp);
        DMRotationRateText.text = string.Format("R: ({0,8:#0.000}, {1,8:#0.000}, {2,8:#0.000})", motionData.rotationRate.x, motionData.rotationRate.y, motionData.rotationRate.z);
        DMUserAccelerationText.text = string.Format("A: ({0,8:#0.000}, {1,8:#0.000}, {2,8:#0.000})", motionData.userAcceleration.x, motionData.userAcceleration.y, motionData.userAcceleration.z);
    }

    public void OnHeadphoneMotion(ref DeviceMotionData motionData)
    {
        HMTimestampText.text = string.Format("T: {0,12:#######0.00}", motionData.timestamp);
        HMRotationRateText.text = string.Format("R: ({0,8:#0.000}, {1,8:#0.000}, {2,8:#0.000})", motionData.rotationRate.x, motionData.rotationRate.y, motionData.rotationRate.z);
        HMUserAccelerationText.text = string.Format("A: ({0,8:#0.000}, {1,8:#0.000}, {2,8:#0.000})", motionData.userAcceleration.x, motionData.userAcceleration.y, motionData.userAcceleration.z);
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
        AMTimestampText.text = "";
        AMAccelerationText.text = "";

        GMTimestampText.text = "";
        GMRotationRateText.text = "";

        MMTimestampText.text = "";
        MMMagneticFieldText.text = "";

        DMTimestampText.text = "";
        DMRotationRateText.text = "";
        DMUserAccelerationText.text = "";

        HMTimestampText.text = "";
        HMRotationRateText.text = "";
        HMUserAccelerationText.text = "";

        accelerometerMotion = GetComponent<AccelerometerMotion>();
        gyroscopeMotion = GetComponent<GyroscopeMotion>();
        magnetometerMotion = GetComponent<MagnetometerMotion>();
        deviceMotion = GetComponent<DeviceMotion>();
        headphoneMotion = GetComponent<HeadphoneMotion>();
    }

    void OnEnable()
    {
        accelerometerMotion.StartSensors();
        gyroscopeMotion.StartSensors();
        magnetometerMotion.StartSensors();
        deviceMotion.StartSensors();
        headphoneMotion.StartSensors();
    }

    void OnDisable()
    {
        accelerometerMotion.StopSensors();
        gyroscopeMotion.StopSensors();
        magnetometerMotion.StopSensors();
        deviceMotion.StopSensors();
        headphoneMotion.StopSensors();
    }
}
