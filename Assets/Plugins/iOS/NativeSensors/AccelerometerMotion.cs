using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using UnityEngine;
using AOT;

namespace NativeSensors
{
    public interface AccelerometerMotionHandler
    {
        void OnAccelerometerMotion(double t, Vector3 acceleration);
    }

    [DefaultExecutionOrder(-1000)]
    public class AccelerometerMotion : MonoBehaviour
    {
        private class AccelerometerMotionCore
        {
            private static AccelerometerMotionCore _Instance = null;
            private static object _Lock = new object();

            public static AccelerometerMotionCore Instance
            {
                get
                {
                    lock (_Lock)
                    {
                        if (_Instance == null) _Instance = new AccelerometerMotionCore();
                        return _Instance;
                    }
                }
            }

            private ConcurrentDictionary<AccelerometerMotion, AccelerometerMotionHandler> handlers = new ConcurrentDictionary<AccelerometerMotion, AccelerometerMotionHandler>();
            private bool started = false;

            public void RegisterHandler(AccelerometerMotion motion, AccelerometerMotionHandler handler)
            {
                handlers.TryAdd(motion, handler);
            }

            public void UnregisterHandler(AccelerometerMotion motion)
            {
                AccelerometerMotionHandler handler;
                handlers.TryRemove(motion, out handler);
            }

#if UNITY_IOS
            delegate void AccelerometerMotionDelegate(
                double t,
                double ax, double ay, double az
            );

            [MonoPInvokeCallback(typeof(AccelerometerMotionDelegate))]
            private static void OnAccelerometerMotion(
                double t,
                double ax, double ay, double az
            )
            {
                foreach (var h in Instance.handlers) h.Value.OnAccelerometerMotion(
                    t,
                    new Vector3((float)ax, (float)ay, (float)az)
                );
            }

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
            private static extern bool AccelerometerMotionStart(double interval, AccelerometerMotionDelegate accelerometerMotionDelegate);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
            private static extern void AccelerometerMotionStop();

            public void StartSensors(int frequency)
            {
                lock (_Lock)
                {
                    if (!started)
                    {
                        double interval = 1.0/(double)frequency;
                        started = AccelerometerMotionStart(interval, OnAccelerometerMotion);
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
                        AccelerometerMotionStop();
                    }
                }
            }
#else
            void StartSensors(int frequency)
            {
                Debug.Log("[NativeSensors] AccelerometerMotion requires iOS device.");
            }
            void StopSensors() {}
#endif

            protected AccelerometerMotionCore() {}
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
                if (!(handler is AccelerometerMotionHandler))
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
                AccelerometerMotionCore.Instance.RegisterHandler(this, handler as AccelerometerMotionHandler);
            }
        }

        void OnDisable()
        {
            AccelerometerMotionCore.Instance.UnregisterHandler(this);
        }

        public void StartSensors()
        {
            AccelerometerMotionCore.Instance.StartSensors(frequency);
        }

        public void StopSensors()
        {
            AccelerometerMotionCore.Instance.StopSensors();
        }
    }
}
