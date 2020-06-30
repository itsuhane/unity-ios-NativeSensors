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
