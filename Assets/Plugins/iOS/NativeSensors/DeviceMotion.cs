using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor;
using AOT;

namespace NativeSensors
{
    public interface DeviceMotionHandler
    {
        void OnDeviceMotion(double t, Vector3 rotationRate, Vector3 userAcceleration);
    }

    public class DeviceMotion : Singleton<DeviceMotion>
    {
        public DeviceMotionHandler deviceMotionHandler;
#if UNITY_IOS
        delegate void DeviceMotionDelegate(double t, double rx, double ry, double rz, double ax, double ay, double az);

        [MonoPInvokeCallback(typeof(DeviceMotionDelegate))]
        private static void OnDeviceMotion(double t, double rx, double ry, double rz, double ax, double ay, double az)
        {
            Instance.deviceMotionHandler.OnDeviceMotion(t, new Vector3((float)rx, (float)ry, (float)rz), new Vector3((float)ax, (float)ay, (float)az));
        }

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void DeviceMotionStart(DeviceMotionDelegate deviceMotionDelegate);

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void DeviceMotionStop();

        public void StartSensors()
        {
            if (deviceMotionHandler != null)
            {
                DeviceMotionStart(OnDeviceMotion);
            }
        }

        public void StopSensors()
        {
            DeviceMotionStop();
        }

        void OnDisable()
        {
            DeviceMotionStop();
        }
#else
        void StartSensors() {} // Nothing we can do.
        void StopSensors() {}
#endif
        protected DeviceMotion() {}
    }
}
