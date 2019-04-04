using System.Runtime.InteropServices;
using UnityEngine;
using AOT;

public class NativeSensors : MonoBehaviour
{
#if UNITY_IOS
    delegate void GyroscopeDelegate(double t, double x, double y, double z);
    delegate void AccelerometerDelegate(double t, double x, double y, double z);

    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern void NativeSensorsStart();

    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern void NativeSensorsStop();

    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern void NativeSensorsSetDelegate(GyroscopeDelegate gyroscopeDelegate, AccelerometerDelegate accelerometerDelegate);

    void Start()
    {
        NativeSensorsSetDelegate(OnGyroscope, OnAccelerometer);
        NativeSensorsStart();
    }

    void OnDestroy()
    {
        NativeSensorsStop();
    }

    [MonoPInvokeCallback(typeof(GyroscopeDelegate))]
    static void OnGyroscope(double t, double x, double y, double z)
    {
        Debug.LogFormat("GYR:{0} {1} {2} {3}", t, x, y, z);
    }

    [MonoPInvokeCallback(typeof(AccelerometerDelegate))]
    static void OnAccelerometer(double t, double x, double y, double z)
    {
        Debug.LogFormat("ACC:{0} {1} {2} {3}", t, x, y, z);
    }
#endif
}
