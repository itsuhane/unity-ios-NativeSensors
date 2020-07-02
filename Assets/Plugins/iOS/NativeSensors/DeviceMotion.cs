using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using UnityEngine;
using AOT;

namespace NativeSensors
{
    public interface DeviceMotionHandler
    {
        void OnDeviceMotion(double t, Vector3 rotationRate, Vector3 userAcceleration);
    }

    public class DeviceMotion : MonoBehaviour
    {
        private class DeviceMotionCore
        {
            private static DeviceMotionCore _Instance = null;
            private static object _Lock = new object();
            private static bool _Started = false;

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

            public ConcurrentDictionary<DeviceMotion, DeviceMotionHandler> handlers = new ConcurrentDictionary<DeviceMotion, DeviceMotionHandler>();

#if UNITY_IOS
            delegate void DeviceMotionDelegate(double t, double rx, double ry, double rz, double ax, double ay, double az);

            [MonoPInvokeCallback(typeof(DeviceMotionDelegate))]
            private static void OnDeviceMotion(double t, double rx, double ry, double rz, double ax, double ay, double az)
            {
                foreach (var h in Instance.handlers) h.Value.OnDeviceMotion(t, new Vector3((float)rx, (float)ry, (float)rz), new Vector3((float)ax, (float)ay, (float)az));
            }

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DeviceMotionStart(DeviceMotionDelegate deviceMotionDelegate);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
            private static extern void DeviceMotionStop();

            public void RegisterHandler(DeviceMotion motion, DeviceMotionHandler handler)
            {
                handlers.TryAdd(motion, handler);
            }

            public void UnregisterHandler(DeviceMotion motion)
            {
                DeviceMotionHandler handler;
                handlers.TryRemove(motion, out handler);
            }

            public void StartSensors()
            {
                lock (_Lock)
                {
                    if (!_Started)
                    {
                        _Started = DeviceMotionStart(OnDeviceMotion);
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
                        DeviceMotionStop();
                    }
                }
            }
#else
            void StartSensors() {} // Nothing we can do.
            void StopSensors() {}
#endif
            protected DeviceMotionCore() {}
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
                DeviceMotionCore.Instance.RegisterHandler(this, handler as DeviceMotionHandler);
            }
        }

        void OnDisable()
        {
            DeviceMotionCore.Instance.UnregisterHandler(this);
        }

        public void StartSensors()
        {
            DeviceMotionCore.Instance.StartSensors();
        }

        public void StopSensors()
        {
            DeviceMotionCore.Instance.StopSensors();
        }

    }
}
