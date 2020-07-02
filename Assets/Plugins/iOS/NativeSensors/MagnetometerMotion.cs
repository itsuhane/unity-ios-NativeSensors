using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using UnityEngine;
using AOT;

namespace NativeSensors
{
    public interface MagnetometerMotionHandler
    {
        void OnMagnetometerMotion(double t, Vector3 magneticField);
    }

    [DefaultExecutionOrder(-1000)]
    public class MagnetometerMotion : MonoBehaviour
    {
        private class MagnetometerMotionCore
        {
            private static MagnetometerMotionCore _Instance = null;
            private static object _Lock = new object();

            public static MagnetometerMotionCore Instance
            {
                get
                {
                    lock (_Lock)
                    {
                        if (_Instance == null) _Instance = new MagnetometerMotionCore();
                        return _Instance;
                    }
                }
            }

            private ConcurrentDictionary<MagnetometerMotion, MagnetometerMotionHandler> handlers = new ConcurrentDictionary<MagnetometerMotion, MagnetometerMotionHandler>();
            private bool started = false;

            public void RegisterHandler(MagnetometerMotion motion, MagnetometerMotionHandler handler)
            {
                handlers.TryAdd(motion, handler);
            }

            public void UnregisterHandler(MagnetometerMotion motion)
            {
                MagnetometerMotionHandler handler;
                handlers.TryRemove(motion, out handler);
            }

#if UNITY_IOS
            delegate void MagnetometerMotionDelegate(
                double t,
                double mfx, double mfy, double mfz
            );

            [MonoPInvokeCallback(typeof(MagnetometerMotionDelegate))]
            private static void OnMagnetometerMotion(
                double t,
                double mfx, double mfy, double mfz
            )
            {
                foreach (var h in Instance.handlers) h.Value.OnMagnetometerMotion(
                    t,
                    new Vector3((float)mfx, (float)mfy, (float)mfz)
                );
            }

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
            private static extern bool MagnetometerMotionStart(double interval, MagnetometerMotionDelegate magnetometerMotionDelegate);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
            private static extern void MagnetometerMotionStop();

            public void StartSensors(int frequency)
            {
                lock (_Lock)
                {
                    if (!started)
                    {
                        double interval = 1.0/(double)frequency;
                        started = MagnetometerMotionStart(interval, OnMagnetometerMotion);
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
                        MagnetometerMotionStop();
                    }
                }
            }
#else
            void StartSensors(int frequency)
            {
                Debug.Log("[NativeSensors] MagnetometerMotion requires iOS device.");
            }
            void StopSensors() {}
#endif

            protected MagnetometerMotionCore() {}
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
                if (!(handler is MagnetometerMotionHandler))
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
                MagnetometerMotionCore.Instance.RegisterHandler(this, handler as MagnetometerMotionHandler);
            }
        }

        void OnDisable()
        {
            MagnetometerMotionCore.Instance.UnregisterHandler(this);
        }

        public void StartSensors()
        {
            MagnetometerMotionCore.Instance.StartSensors(frequency);
        }

        public void StopSensors()
        {
            MagnetometerMotionCore.Instance.StopSensors();
        }
    }
}
