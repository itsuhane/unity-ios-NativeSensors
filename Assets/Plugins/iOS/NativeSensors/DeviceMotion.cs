using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using UnityEngine;
using AOT;

namespace NativeSensors
{
    public enum MagneticFieldCalibrationAccuracy
    {
        Uncalibrated = -1,
        Low = 0,
        Medium = 1,
        High = 2
    }

    public enum SensorLocation
    {
        Default = 0,
        HeadphoneLeft = 1,
        HeadphoneRight = 2
    }

    public enum AttitudeReferenceFrameX
    {
        Arbitrary = 0,
        ArbitraryCorrected = 1,
        MagneticNorth = 2,
        TrueNorth = 3
    }

    public struct DeviceMotionData
    {
        public double timestamp;
        public Quaternion attitude;
        public Vector3 rotationRate;
        public Vector3 gravity;
        public Vector3 userAcceleration;
        public Vector3 magneticField;
        public MagneticFieldCalibrationAccuracy magneticFieldAccuracy;
        public double heading;
        public SensorLocation sensorLocation;
    }

    public interface DeviceMotionHandler
    {
        void OnDeviceMotion(ref DeviceMotionData motionData);
    }

    [DefaultExecutionOrder(-1000)]
    public class DeviceMotion : MonoBehaviour
    {
        private class DeviceMotionCore
        {
            private static DeviceMotionCore _Instance = null;
            private static object _Lock = new object();

            public static DeviceMotionCore Instance
            {
                get
                {
                    lock (_Lock)
                    {
                        if (_Instance == null) _Instance = new DeviceMotionCore();
                        return _Instance;
                    }
                }
            }

            private ConcurrentDictionary<DeviceMotion, DeviceMotionHandler> handlers = new ConcurrentDictionary<DeviceMotion, DeviceMotionHandler>();
            private bool started = false;

            public void RegisterHandler(DeviceMotion motion, DeviceMotionHandler handler)
            {
                handlers.TryAdd(motion, handler);
            }

            public void UnregisterHandler(DeviceMotion motion)
            {
                DeviceMotionHandler handler;
                handlers.TryRemove(motion, out handler);
            }

#if UNITY_IOS
            delegate void DeviceMotionDelegate(
                double t,
                double attx, double atty, double attz, double attw,
                double rrx, double rry, double rrz,
                double gx, double gy, double gz,
                double uax, double uay, double uaz,
                double mfx, double mfy, double mfz,
                int mfa, double heading, int loc
            );

            [MonoPInvokeCallback(typeof(DeviceMotionDelegate))]
            private static void OnDeviceMotion(
                double t,
                double attx, double atty, double attz, double attw,
                double rrx, double rry, double rrz,
                double gx, double gy, double gz,
                double uax, double uay, double uaz,
                double mfx, double mfy, double mfz,
                int mfa, double heading, int loc
            )
            {
                DeviceMotionData motionData;
                motionData.timestamp = t;
                motionData.attitude = new Quaternion((float)attx, (float)atty, (float)attz, (float)attw);
                motionData.rotationRate = new Vector3((float)rrx, (float)rry, (float)rrz);
                motionData.gravity = new Vector3((float)gx, (float)gy, (float)gz);
                motionData.userAcceleration = new Vector3((float)uax, (float)uay, (float)uaz);
                motionData.magneticField = new Vector3((float)mfx, (float)mfy, (float)mfz);
                motionData.magneticFieldAccuracy = (MagneticFieldCalibrationAccuracy)mfa;
                motionData.heading = heading;
                motionData.sensorLocation = (SensorLocation)loc;
                foreach (var h in Instance.handlers) h.Value.OnDeviceMotion(ref motionData);
            }

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DeviceMotionStart(double interval, int referenceFrame, DeviceMotionDelegate deviceMotionDelegate);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
            private static extern void DeviceMotionStop();

            public void StartSensors(int frequency, AttitudeReferenceFrameX referenceFrame)
            {
                lock (_Lock)
                {
                    if (!started)
                    {
                        double interval = 1.0/(double)frequency;
                        started = DeviceMotionStart(interval, (int)referenceFrame, OnDeviceMotion);
                    }
                }
            }

            public void StopSensors()
            {
                lock (_Lock)
                {
                    if (started)
                    {
                        started = false;
                        DeviceMotionStop();
                    }
                }
            }
#else
            void StartSensors(int frequency, AttitudeReferenceFrameX referenceFrame)
            {
                Debug.Log("[NativeSensors] DeviceMotion requires iOS device.");
            }
            void StopSensors() {}
#endif

            protected DeviceMotionCore() {}
        }

        public static int frequency = 100;
        [SerializeField][HideInInspector]
        private int _frequency = frequency;

        public static AttitudeReferenceFrameX attitudeReferenceX = AttitudeReferenceFrameX.Arbitrary;
        [SerializeField][HideInInspector]
        private AttitudeReferenceFrameX _attitudeReferenceX = attitudeReferenceX;

        public Component handler = null;

#if UNITY_EDITOR
        void OnValidate()
        {
            if (handler != null)
            {
                if (!(handler is DeviceMotionHandler))
                {
                    handler = null;
                }
            }
        }
#endif

        void Awake()
        {
            frequency = _frequency;
            attitudeReferenceX = _attitudeReferenceX;
        }

        void OnEnable()
        {
            if (handler != null)
            {
                DeviceMotionCore.Instance.RegisterHandler(this, handler as DeviceMotionHandler);
            }
        }

        void OnDisable()
        {
            DeviceMotionCore.Instance.UnregisterHandler(this);
        }

        public void StartSensors()
        {
            DeviceMotionCore.Instance.StartSensors(frequency, attitudeReferenceX);
        }

        public void StopSensors()
        {
            DeviceMotionCore.Instance.StopSensors();
        }
    }
}
