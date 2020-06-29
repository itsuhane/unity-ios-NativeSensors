using System.Runtime.InteropServices;
using UnityEngine;
using AOT;

namespace NativeSensors {
    public interface HeadphoneMotionHandler
    {
        void OnHeadphoneMotion(double t, Vector3 rotationRate, Vector3 userAcceleration);
        void OnHeadphoneConnect();
        void OnHeadphoneDisconnect();
    }

    public class HeadphoneMotion : Singleton<HeadphoneMotion>
    {
        public HeadphoneMotionHandler headphoneMotionHandler;
#if UNITY_IOS
        delegate void HeadphoneMotionDelegate(double t, double rx, double ry, double rz, double ax, double ay, double az);
        delegate void HeadphoneConnectDelegate();
        delegate void HeadphoneDisconnectDelegate();

        [MonoPInvokeCallback(typeof(HeadphoneMotionDelegate))]
        private static void OnHeadphoneMotion(double t, double rx, double ry, double rz, double ax, double ay, double az)
        {
            Instance.headphoneMotionHandler.OnHeadphoneMotion(t, new Vector3((float)rx, (float)ry, (float)rz), new Vector3((float)ax, (float)ay, (float)az));
        }

        [MonoPInvokeCallback(typeof(HeadphoneConnectDelegate))]
        private static void OnHeadphoneConnect()
        {
            Instance.headphoneMotionHandler.OnHeadphoneConnect();
        }

        [MonoPInvokeCallback(typeof(HeadphoneDisconnectDelegate))]
        private static void OnHeadphoneDisconnect()
        {
            Instance.headphoneMotionHandler.OnHeadphoneDisconnect();
        }

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void HeadphoneMotionStart(HeadphoneMotionDelegate headphoneMotionDelegate, HeadphoneConnectDelegate headphoneConnectDelegate, HeadphoneDisconnectDelegate headphoneDisconnectDelegate);

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void HeadphoneMotionStop();

        public void StartSensors()
        {
            if (headphoneMotionHandler != null)
            {
                HeadphoneMotionStart(OnHeadphoneMotion, OnHeadphoneConnect, OnHeadphoneDisconnect);
            }
        }

        public void StopSensors()
        {
            HeadphoneMotionStop();
        }

        void OnDisable()
        {
            HeadphoneMotionStop();
        }
#else
        void StartSensors() {} // Nothing we can do.
        void StopSensors() {}
#endif
        protected HeadphoneMotion() {}
    }
}

// public abstract class GyroscopeHandler : MonoBehaviour
// {
//     public abstract void OnGyroscope(double t, Vector3 rotationRate);
// }

// public abstract class AccelerometerHandler : MonoBehaviour
// {
//     public abstract void OnAccelerometer(double t, Vector3 acceleration);
// }

// public abstract class HeadphoneMotionHandler : MonoBehaviour
// {
//     public abstract void OnHeadphoneMotion(double t, Vector3 rotationRate, Vector3 userAcceleration);
// }

// public class NativeSensors : MonoBehaviour
// {
//     public GyroscopeHandler gyroscopeHandler;
//     public AccelerometerHandler accelerometerHandler;
//     public HeadphoneMotionHandler headphoneMotionHandler;

//     private static bool _Destroying = false;
//     private static object _LockObject = new object();
//     private static NativeSensors _Instance;

//     public static NativeSensors Instance
//     {
//         get
//         {
//             if (_Destroying)
//             {
//                 return null;
//             }
//             lock(_LockObject)
//             {
//                 if (_Instance == null)
//                 {
//                     _Instance = (NativeSensors)FindObjectOfType(typeof(NativeSensors));
//                     if (_Instance == null)
//                     {
//                         GameObject obj = new GameObject();
//                         obj.name = "Native Sensors (Auto Created)";
//                         _Instance = obj.AddComponent<NativeSensors>();
//                         DontDestroyOnLoad(obj);
//                     }
//                 }
//                 return _Instance;
//             }
//         }
//     }

// #if UNITY_IOS
//     delegate void GyroscopeDelegate(double t, double x, double y, double z);
//     delegate void AccelerometerDelegate(double t, double x, double y, double z);
//     delegate void HeadphoneMotionDelegate(double t, double rx, double ry, double rz, double ax, double ay, double az);

//     [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
//     private static extern void NativeSensorsStart();

//     [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
//     private static extern void NativeSensorsStop();

//     [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
//     private static extern void NativeSensorsSetDelegate(GyroscopeDelegate gyroscopeDelegate, AccelerometerDelegate accelerometerDelegate, HeadphoneMotionDelegate headphoneMotionDelegate);

//     void Start()
//     {
//         GyroscopeDelegate gyroscopeDelegate = NativeSensors.OnGyroscope;
//         AccelerometerDelegate accelerometerDelegate = NativeSensors.OnAccelerometer;
//         HeadphoneMotionDelegate headphoneMotionDelegate = NativeSensors.OnHeadphoneMotion;
//         if (gyroscopeHandler == null) gyroscopeDelegate = null;
//         if (accelerometerHandler == null) accelerometerDelegate = null;
//         if (headphoneMotionHandler == null) headphoneMotionDelegate = null;
//         NativeSensorsSetDelegate(gyroscopeDelegate, accelerometerDelegate, headphoneMotionDelegate);
//         NativeSensorsStart();
//     }

//     void OnDestroy()
//     {
//         NativeSensorsStop();
//         _Destroying = true;
//     }

//     void OnApplicationQuit()
//     {
//         NativeSensorsStop();
//         _Destroying = true;
//     }

//     [MonoPInvokeCallback(typeof(GyroscopeDelegate))]
//     static void OnGyroscope(double t, double x, double y, double z)
//     {
//         Instance.gyroscopeHandler.OnGyroscope(t, new Vector3((float)x, (float)y, (float)z));
//     }

//     [MonoPInvokeCallback(typeof(AccelerometerDelegate))]
//     static void OnAccelerometer(double t, double x, double y, double z)
//     {
//         Instance.accelerometerHandler.OnAccelerometer(t, new Vector3((float)x, (float)y, (float)z));
//     }

//     [MonoPInvokeCallback(typeof(HeadphoneMotionDelegate))]
//     static void OnHeadphoneMotion(double t, double rx, double ry, double rz, double ax, double ay, double az)
//     {
//         Instance.headphoneMotionHandler.OnHeadphoneMotion(t, new Vector3((float)rx, (float)ry, (float)rz), new Vector3((float)ax, (float)ay, (float)az));
//     }
// #endif
// }
