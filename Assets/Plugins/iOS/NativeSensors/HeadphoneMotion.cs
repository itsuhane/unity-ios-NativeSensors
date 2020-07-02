using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using UnityEngine;
using AOT;

namespace NativeSensors
{
    public interface HeadphoneMotionHandler
    {
        void OnHeadphoneMotion(double t, Vector3 rotationRate, Vector3 userAcceleration);
        void OnHeadphoneConnect();
        void OnHeadphoneDisconnect();
    }

    public class HeadphoneMotion : MonoBehaviour
    {
        private class HeadphoneMotionCore
        {
            private static HeadphoneMotionCore _Instance = null;
            private static object _Lock = new object();
            private static bool _Started = false;

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
            public ConcurrentDictionary<HeadphoneMotion, HeadphoneMotionHandler> handlers = new ConcurrentDictionary<HeadphoneMotion, HeadphoneMotionHandler>();

#if UNITY_IOS
            delegate void HeadphoneMotionDelegate(double t, double rx, double ry, double rz, double ax, double ay, double az);
            delegate void HeadphoneConnectDelegate();
            delegate void HeadphoneDisconnectDelegate();

            [MonoPInvokeCallback(typeof(HeadphoneMotionDelegate))]
            private static void OnHeadphoneMotion(double t, double rx, double ry, double rz, double ax, double ay, double az)
            {
                foreach (var h in Instance.handlers) h.Value.OnHeadphoneMotion(t, new Vector3((float)rx, (float)ry, (float)rz), new Vector3((float)ax, (float)ay, (float)az));
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

            public void RegisterHandler(HeadphoneMotion motion, HeadphoneMotionHandler handler)
            {
                handlers.TryAdd(motion, handler);
            }

            public void UnregisterHandler(HeadphoneMotion motion)
            {
                HeadphoneMotionHandler handler;
                handlers.TryRemove(motion, out handler);
            }

            public void StartSensors()
            {
                lock (_Lock)
                {
                    if (!_Started)
                    {
                        _Started = HeadphoneMotionStart(OnHeadphoneMotion, OnHeadphoneConnect, OnHeadphoneDisconnect);
                    }
                }
            }

            public void StopSensors()
            {
                lock (_Lock)
                {
                    if (_Started)
                    {
                        _Started = false;
                        HeadphoneMotionStop();
                    }
                }
            }
#else
            void StartSensors() {} // Nothing we can do.
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
                if (!(handler is DeviceMotionHandler))
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
