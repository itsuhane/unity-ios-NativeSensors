using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using UnityEngine;
using AOT;

namespace NativeSensors
{
    using HeadphoneMotionData = DeviceMotionData;

    public interface HeadphoneMotionHandler
    {
        void OnHeadphoneMotion(ref HeadphoneMotionData motionData);
        void OnHeadphoneConnect();
        void OnHeadphoneDisconnect();
    }

    [DefaultExecutionOrder(-1000)]
    public class HeadphoneMotion : MonoBehaviour
    {
        private class HeadphoneMotionCore
        {
            private static HeadphoneMotionCore _Instance = null;
            private static object _Lock = new object();

            public static HeadphoneMotionCore Instance
            {
                get
                {
                    lock (_Lock)
                    {
                        if (_Instance == null) _Instance = new HeadphoneMotionCore();
                        return _Instance;
                    }
                }
            }

            private ConcurrentDictionary<HeadphoneMotion, HeadphoneMotionHandler> handlers = new ConcurrentDictionary<HeadphoneMotion, HeadphoneMotionHandler>();
            private bool started = false;

            public void RegisterHandler(HeadphoneMotion motion, HeadphoneMotionHandler handler)
            {
                handlers.TryAdd(motion, handler);
            }

            public void UnregisterHandler(HeadphoneMotion motion)
            {
                HeadphoneMotionHandler handler;
                handlers.TryRemove(motion, out handler);
            }

#if UNITY_IOS
            delegate void HeadphoneMotionDelegate(
                double t,
                double attx, double atty, double attz, double attw,
                double rrx, double rry, double rrz,
                double gx, double gy, double gz,
                double uax, double uay, double uaz,
                double mfx, double mfy, double mfz,
                int mfa, double heading, int loc
            );
            delegate void HeadphoneConnectDelegate();
            delegate void HeadphoneDisconnectDelegate();

            [MonoPInvokeCallback(typeof(HeadphoneMotionDelegate))]
            private static void OnHeadphoneMotion(
                double t,
                double attx, double atty, double attz, double attw,
                double rrx, double rry, double rrz,
                double gx, double gy, double gz,
                double uax, double uay, double uaz,
                double mfx, double mfy, double mfz,
                int mfa, double heading, int loc
            )
            {
                HeadphoneMotionData motionData;
                motionData.timestamp = t;
                motionData.attitude = new Quaternion((float)attx, (float)atty, (float)attz, (float)attw);
                motionData.rotationRate = new Vector3((float)rrx, (float)rry, (float)rrz);
                motionData.gravity = new Vector3((float)gx, (float)gy, (float)gz);
                motionData.userAcceleration = new Vector3((float)uax, (float)uay, (float)uaz);
                motionData.magneticField = new Vector3((float)mfx, (float)mfy, (float)mfz);
                motionData.magneticFieldAccuracy = (MagneticFieldCalibrationAccuracy)mfa;
                motionData.heading = heading;
                motionData.sensorLocation = (SensorLocation)loc;
                foreach (var h in Instance.handlers) h.Value.OnHeadphoneMotion(ref motionData);
            }

            [MonoPInvokeCallback(typeof(HeadphoneConnectDelegate))]
            private static void OnHeadphoneConnect()
            {
                foreach (var h in Instance.handlers) h.Value.OnHeadphoneConnect();
            }

            [MonoPInvokeCallback(typeof(HeadphoneDisconnectDelegate))]
            private static void OnHeadphoneDisconnect()
            {
                foreach (var h in Instance.handlers) h.Value.OnHeadphoneDisconnect();
            }

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
            private static extern bool HeadphoneMotionStart(HeadphoneMotionDelegate headphoneMotionDelegate, HeadphoneConnectDelegate headphoneConnectDelegate, HeadphoneDisconnectDelegate headphoneDisconnectDelegate);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
            private static extern void HeadphoneMotionStop();

            public void StartSensors()
            {
                lock (_Lock)
                {
                    if (!started)
                    {
                        started = HeadphoneMotionStart(OnHeadphoneMotion, OnHeadphoneConnect, OnHeadphoneDisconnect);
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
                        HeadphoneMotionStop();
                    }
                }
            }
#else
            void StartSensors() {
                Debug.Log("[NativeSensors] HeadphoneMotion requires iOS (>=14.0) device.");
            }
            void StopSensors() {}
#endif
            protected HeadphoneMotionCore() {}
        }

        public Component handler = null;

#if UNITY_EDITOR
        void OnValidate()
        {
            if (handler != null)
            {
                if (!(handler is HeadphoneMotionHandler))
                {
                    handler = null;
                }
            }
        }
#endif

        void OnEnable()
        {
            if (handler != null)
            {
                HeadphoneMotionCore.Instance.RegisterHandler(this, handler as HeadphoneMotionHandler);
            }
        }

        void OnDisable()
        {
            HeadphoneMotionCore.Instance.UnregisterHandler(this);
        }

        public void StartSensors()
        {
            HeadphoneMotionCore.Instance.StartSensors();
        }

        public void StopSensors()
        {
            HeadphoneMotionCore.Instance.StopSensors();
        }

    }
}
