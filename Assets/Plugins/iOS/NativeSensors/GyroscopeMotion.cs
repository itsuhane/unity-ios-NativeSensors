using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using UnityEngine;
using AOT;

namespace NativeSensors
{
    public interface GyroscopeMotionHandler
    {
        void OnGyroscopeMotion(double t, Vector3 rotationRate);
    }

    [DefaultExecutionOrder(-1000)]
    public class GyroscopeMotion : MonoBehaviour
    {
        private class GyroscopeMotionCore
        {
            private static GyroscopeMotionCore _Instance = null;
            private static object _Lock = new object();

            public static GyroscopeMotionCore Instance
            {
                get
                {
                    lock (_Lock)
                    {
                        if (_Instance == null) _Instance = new GyroscopeMotionCore();
                        return _Instance;
                    }
                }
            }

            private ConcurrentDictionary<GyroscopeMotion, GyroscopeMotionHandler> handlers = new ConcurrentDictionary<GyroscopeMotion, GyroscopeMotionHandler>();
            private bool started = false;

            public void RegisterHandler(GyroscopeMotion motion, GyroscopeMotionHandler handler)
            {
                handlers.TryAdd(motion, handler);
            }

            public void UnregisterHandler(GyroscopeMotion motion)
            {
                GyroscopeMotionHandler handler;
                handlers.TryRemove(motion, out handler);
            }

#if UNITY_IOS
            delegate void GyroscopeMotionDelegate(
                double t,
                double rrx, double rry, double rrz
            );

            [MonoPInvokeCallback(typeof(GyroscopeMotionDelegate))]
            private static void OnGyroscopeMotion(
                double t,
                double rrx, double rry, double rrz
            )
            {
                foreach (var h in Instance.handlers) h.Value.OnGyroscopeMotion(
                    t,
                    new Vector3((float)rrx, (float)rry, (float)rrz)
                );
            }

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
            private static extern bool GyroscopeMotionStart(double interval, GyroscopeMotionDelegate gyroscopeMotionDelegate);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
            private static extern void GyroscopeMotionStop();

            public void StartSensors(int frequency)
            {
                lock (_Lock)
                {
                    if (!started)
                    {
                        double interval = 1.0/(double)frequency;
                        started = GyroscopeMotionStart(interval, OnGyroscopeMotion);
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
                        GyroscopeMotionStop();
                    }
                }
            }
#else
            void StartSensors(int frequency)
            {
                Debug.Log("[NativeSensors] GyroscopeMotion requires iOS device.");
            }
            void StopSensors() {}
#endif

            protected GyroscopeMotionCore() {}
        }

        public static int frequency = 100;
        [SerializeField][HideInInspector]
        private int _frequency = frequency;
        public Component handler = null;

#if UNITY_EDITOR
        void OnValidate()
        {
            if (handler != null)
            {
                if (!(handler is GyroscopeMotionHandler))
                {
                    handler = null;
                }
            }
        }
#endif

        void Awake()
        {
            frequency = _frequency;
        }

        void OnEnable()
        {
            if (handler != null)
            {
                GyroscopeMotionCore.Instance.RegisterHandler(this, handler as GyroscopeMotionHandler);
            }
        }

        void OnDisable()
        {
            GyroscopeMotionCore.Instance.UnregisterHandler(this);
        }

        public void StartSensors()
        {
            GyroscopeMotionCore.Instance.StartSensors(frequency);
        }

        public void StopSensors()
        {
            GyroscopeMotionCore.Instance.StopSensors();
        }
    }
}
