#import <CoreMotion/CoreMotion.h>
#include <math.h>

static CMMotionManager* sMotionManager  = nil;

/* Accelerometer */

typedef void (*AccelerometerMotionCallback)(
    double t,
    double ax, double ay, double az
);
static AccelerometerMotionCallback accelerometerMotionCallback = nullptr;

static CMAccelerometerHandler accelerometerHandler = ^(CMAccelerometerData *accelerometerData, NSError *error) {
    if (error == nil && accelerometerData && accelerometerMotionCallback) {
        (*accelerometerMotionCallback)(
            accelerometerData.timestamp,
            accelerometerData.acceleration.x, accelerometerData.acceleration.y, accelerometerData.acceleration.z
        );
    }
};

extern "C" bool AccelerometerMotionStart(double interval, AccelerometerMotionCallback g) {
    accelerometerMotionCallback = g;
    if (!sMotionManager) {
        sMotionManager = [[CMMotionManager alloc] init];
    }
    if (sMotionManager.accelerometerAvailable) {
        sMotionManager.accelerometerUpdateInterval = interval;
        [sMotionManager startAccelerometerUpdatesToQueue:[NSOperationQueue mainQueue] withHandler:accelerometerHandler];
        NSLog(@"[NativeSensors] AccelerometerMotion updating with interval %.3f.", interval);
        return true;
    } else {
        NSLog(@"[NativeSensors] AccelerometerMotion unavailable. Did you grant motion usage permission?");
    }
    (*accelerometerMotionCallback)(
        0,
        NAN, 0, NAN
    );
    accelerometerMotionCallback = nullptr;
    return false;
}

extern "C" void AccelerometerMotionStop() {
    if (sMotionManager != nil && accelerometerMotionCallback != nullptr) {
        [sMotionManager stopAccelerometerUpdates];
        accelerometerMotionCallback = nullptr;
    }
}

/* GyroscopeMotion */

typedef void (*GyroscopeMotionCallback)(
    double t,
    double rrx, double rry, double rrz
);
static GyroscopeMotionCallback gyroscopeMotionCallback = nullptr;

static CMGyroHandler gyroHandler = ^(CMGyroData *gyroData, NSError *error) {
    if (error == nil && gyroData && gyroscopeMotionCallback) {
        (*gyroscopeMotionCallback)(
            gyroData.timestamp,
            gyroData.rotationRate.x, gyroData.rotationRate.y, gyroData.rotationRate.z
        );
    }
};

extern "C" bool GyroscopeMotionStart(double interval, GyroscopeMotionCallback g) {
    gyroscopeMotionCallback = g;
    if (!sMotionManager) {
        sMotionManager = [[CMMotionManager alloc] init];
    }
    if (sMotionManager.gyroAvailable) {
        sMotionManager.gyroUpdateInterval = interval;
        [sMotionManager startGyroUpdatesToQueue:[NSOperationQueue mainQueue] withHandler:gyroHandler];
        NSLog(@"[NativeSensors] GyroscopeMotion updating with interval %.3f.", interval);
        return true;
    } else {
        NSLog(@"[NativeSensors] GyroscopeMotion unavailable. Did you grant motion usage permission?");
    }
    (*gyroscopeMotionCallback)(
        0,
        NAN, 0, NAN
    );
    gyroscopeMotionCallback = nullptr;
    return false;
}

extern "C" void GyroscopeMotionStop() {
    if (sMotionManager != nil && gyroscopeMotionCallback != nullptr) {
        [sMotionManager stopGyroUpdates];
        gyroscopeMotionCallback = nullptr;
    }
}

/* Magnetometer */

typedef void (*MagnetometerMotionCallback)(
    double t,
    double mfx, double mfy, double mfz
);
static MagnetometerMotionCallback magnetometerMotionCallback = nullptr;

static CMMagnetometerHandler magnetometerHandler = ^(CMMagnetometerData *magnetometerData, NSError *error) {
    if (error == nil && magnetometerData && magnetometerMotionCallback) {
        (*magnetometerMotionCallback)(
            magnetometerData.timestamp,
            magnetometerData.magneticField.x, magnetometerData.magneticField.y, magnetometerData.magneticField.z
        );
    }
};

extern "C" bool MagnetometerMotionStart(double interval, MagnetometerMotionCallback g) {
    magnetometerMotionCallback = g;
    if (!sMotionManager) {
        sMotionManager = [[CMMotionManager alloc] init];
    }
    if (sMotionManager.magnetometerAvailable) {
        sMotionManager.magnetometerUpdateInterval = interval;
        [sMotionManager startMagnetometerUpdatesToQueue:[NSOperationQueue mainQueue] withHandler:magnetometerHandler];
        NSLog(@"[NativeSensors] MagnetometerMotion updating with interval %.3f.", interval);
        return true;
    } else {
        NSLog(@"[NativeSensors] MagnetometerMotion unavailable. Did you grant motion usage permission?");
    }
    (*magnetometerMotionCallback)(
        0,
        NAN, 0, NAN
    );
    magnetometerMotionCallback = nullptr;
    return false;
}

extern "C" void MagnetometerMotionStop() {
    if (sMotionManager != nil && magnetometerMotionCallback != nullptr) {
        [sMotionManager stopMagnetometerUpdates];
        magnetometerMotionCallback = nullptr;
    }
}

/* DeviceMotion */

typedef void (*DeviceMotionCallback)(
    double t,
    double attx, double atty, double attz, double attw,
    double rrx, double rry, double rrz,
    double gx, double gy, double gz,
    double uax, double uay, double uaz,
    double mfx, double mfy, double mfz,
    int mfa, double heading, int loc
);
static DeviceMotionCallback deviceMotionCallback = nullptr;

static CMDeviceMotionHandler deviceMotionHandler = ^(CMDeviceMotion *deviceMotion, NSError *error) {
    if (error == nil && deviceMotion && deviceMotionCallback) {
        double heading;
        if (@available(iOS 11.0, *)) {
            heading = deviceMotion.heading;
        } else {
            heading = NAN;
        }

        int sensorLocation;
        if (@available(iOS 14.0, *)) {
            sensorLocation =
                deviceMotion.sensorLocation == CMDeviceMotionSensorLocationHeadphoneLeft ? 1 :
                deviceMotion.sensorLocation == CMDeviceMotionSensorLocationHeadphoneRight ? 2 :
                /* else */ 0;
        } else {
            sensorLocation = 0;
        }

        (*deviceMotionCallback)(
            deviceMotion.timestamp,
            deviceMotion.attitude.quaternion.x, deviceMotion.attitude.quaternion.y, deviceMotion.attitude.quaternion.z, deviceMotion.attitude.quaternion.w,
            deviceMotion.rotationRate.x, deviceMotion.rotationRate.y, deviceMotion.rotationRate.z,
            deviceMotion.gravity.x, deviceMotion.gravity.y, deviceMotion.gravity.z,
            deviceMotion.userAcceleration.x, deviceMotion.userAcceleration.y, deviceMotion.userAcceleration.z,
            deviceMotion.magneticField.field.x, deviceMotion.magneticField.field.y, deviceMotion.magneticField.field.z,
            deviceMotion.magneticField.accuracy == CMMagneticFieldCalibrationAccuracyLow ? 0 :
            deviceMotion.magneticField.accuracy == CMMagneticFieldCalibrationAccuracyMedium ? 1 :
            deviceMotion.magneticField.accuracy == CMMagneticFieldCalibrationAccuracyHigh ? 2 :
            /* else */ -1,
            heading, sensorLocation
        );
    }
};

extern "C" bool DeviceMotionStart(double interval, int referenceFrame, DeviceMotionCallback m) {
    deviceMotionCallback = m;
    if (!sMotionManager) {
        sMotionManager = [[CMMotionManager alloc] init];
    }
    if (sMotionManager.deviceMotionAvailable) {
        sMotionManager.deviceMotionUpdateInterval = interval;
        CMAttitudeReferenceFrame frame =
            referenceFrame == 1 ? CMAttitudeReferenceFrameXArbitraryCorrectedZVertical :
            referenceFrame == 2 ? CMAttitudeReferenceFrameXMagneticNorthZVertical :
            referenceFrame == 3 ? CMAttitudeReferenceFrameXTrueNorthZVertical :
            /* else */ CMAttitudeReferenceFrameXArbitraryZVertical;
        [sMotionManager startDeviceMotionUpdatesUsingReferenceFrame:frame toQueue:[NSOperationQueue mainQueue] withHandler:deviceMotionHandler];
        NSLog(@"[NativeSensors] DeviceMotion updating with interval %.3f.", interval);
        return true;
    } else {
        NSLog(@"[NativeSensors] DeviceMotion unavailable. Did you grant motion usage permission?");
    }
    (*deviceMotionCallback)(
        0,
        NAN, 0, NAN, 0,
        NAN, 0, NAN,
        0, NAN, 0,
        NAN, 0, NAN,
        0, NAN, 0,
        -1, NAN, 0
    );
    deviceMotionCallback = nullptr;
    return false;
}

extern "C" void DeviceMotionStop() {
    if (sMotionManager != nil && deviceMotionCallback != nullptr) {
        [sMotionManager stopDeviceMotionUpdates];
        deviceMotionCallback = nullptr;
    }
}

/* HeadphoneMotion */

typedef void (*HeadphoneMotionCallback)(
    double t,
    double attx, double atty, double attz, double attw,
    double rrx, double rry, double rrz,
    double gx, double gy, double gz,
    double uax, double uay, double uaz,
    double mfx, double mfy, double mfz,
    int mfa, double heading, int loc
);
typedef void (*HeadphoneConnectCallback)();
typedef void (*HeadphoneDisconnectCallback)();

static HeadphoneMotionCallback headphoneMotionCallback = nullptr;
static HeadphoneConnectCallback headphoneConnectCallback = nullptr;
static HeadphoneDisconnectCallback headphoneDisconnectCallback = nullptr;

#if __IPHONE_OS_VERSION_MAX_ALLOWED >= 140000

@interface HeadphoneMotionDelegate : NSObject <CMHeadphoneMotionManagerDelegate>
- (void)headphoneMotionManagerDidConnect:(CMHeadphoneMotionManager *)manager API_AVAILABLE(ios(14.0));
- (void)headphoneMotionManagerDidDisonnect:(CMHeadphoneMotionManager *)manager API_AVAILABLE(ios(14.0));
@end

@implementation HeadphoneMotionDelegate

- (void)headphoneMotionManagerDidConnect:(CMHeadphoneMotionManager *)manager {
    if (headphoneConnectCallback) headphoneConnectCallback();
}

- (void)headphoneMotionManagerDidDisonnect:(CMHeadphoneMotionManager *)manager {
    if (headphoneDisconnectCallback) headphoneDisconnectCallback();
}

@end

API_AVAILABLE(ios(14.0))
static CMHeadphoneMotionManager *sHeadphoneMotionManager = nil;
static HeadphoneMotionDelegate *sHeadphoneMotionDelegate = nil;

static CMHeadphoneDeviceMotionHandler headphoneDeviceMotionHandler = ^(CMDeviceMotion *deviceMotion, NSError *error) {
    if (error == nil && deviceMotion && headphoneMotionCallback) {
        double heading;
        if (@available(iOS 11.0, *)) {
            heading = deviceMotion.heading;
        } else {
            heading = NAN;
        }

        int sensorLocation;
        if (@available(iOS 14.0, *)) {
            sensorLocation =
                deviceMotion.sensorLocation == CMDeviceMotionSensorLocationHeadphoneLeft ? 1 :
                deviceMotion.sensorLocation == CMDeviceMotionSensorLocationHeadphoneRight ? 2 :
                /* else */ 0;
        } else {
            sensorLocation = 0;
        }

        (*headphoneMotionCallback)(
            deviceMotion.timestamp,
            deviceMotion.attitude.quaternion.x, deviceMotion.attitude.quaternion.y, deviceMotion.attitude.quaternion.z, deviceMotion.attitude.quaternion.w,
            deviceMotion.rotationRate.x, deviceMotion.rotationRate.y, deviceMotion.rotationRate.z,
            deviceMotion.gravity.x, deviceMotion.gravity.y, deviceMotion.gravity.z,
            deviceMotion.userAcceleration.x, deviceMotion.userAcceleration.y, deviceMotion.userAcceleration.z,
            deviceMotion.magneticField.field.x, deviceMotion.magneticField.field.y, deviceMotion.magneticField.field.z,
            deviceMotion.magneticField.accuracy == CMMagneticFieldCalibrationAccuracyLow ? 0 :
            deviceMotion.magneticField.accuracy == CMMagneticFieldCalibrationAccuracyMedium ? 1 :
            deviceMotion.magneticField.accuracy == CMMagneticFieldCalibrationAccuracyHigh ? 2 :
            /* else */ -1,
            heading, sensorLocation
        );
    } else {
        NSLog(@"[NativeSensors] Error: %@ %@", error, [error userInfo]);
    }
};

extern "C" bool HeadphoneMotionStart(HeadphoneMotionCallback m, HeadphoneConnectCallback c, HeadphoneDisconnectCallback d) {
    headphoneMotionCallback = m;
    headphoneConnectCallback = c;
    headphoneDisconnectCallback = d;
    if (@available(iOS 14.0, *)) {
        NSLog(@"[NativeSensors] API available.");
        if (!sHeadphoneMotionDelegate) {
            sHeadphoneMotionDelegate = [[HeadphoneMotionDelegate alloc] init];
        }
        if (!sHeadphoneMotionManager) {
            sHeadphoneMotionManager = [[CMHeadphoneMotionManager alloc] init];
            sHeadphoneMotionManager.delegate = sHeadphoneMotionDelegate;
        }
        if (sHeadphoneMotionManager.deviceMotionAvailable) {
            [sHeadphoneMotionManager startDeviceMotionUpdatesToQueue:[NSOperationQueue mainQueue] withHandler:headphoneDeviceMotionHandler];
            return true;
        } else {
            NSLog(@"[NativeSensors] HeadphoneMotion unavailable. Your device may not support headphone motion.");
        }
    } else {
        NSLog(@"[NativeSensors] (HeadphoneMotion) API unavailable. Either the deployment target or the iOS on your phone has version below 14.0.");
    }
    (*headphoneMotionCallback)(
        0,
        NAN, 0, NAN, 0,
        NAN, 0, NAN,
        0, NAN, 0,
        NAN, 0, NAN,
        0, NAN, 0,
        -1, NAN, 0
    );
    headphoneMotionCallback = nullptr;
    headphoneConnectCallback = nullptr;
    headphoneDisconnectCallback = nullptr;
    return false;
}

extern "C" void HeadphoneMotionStop() {
    if (@available(iOS 14.0, *)) {
        if (sHeadphoneMotionManager != nil && headphoneMotionCallback != nullptr) {
            [sHeadphoneMotionManager stopDeviceMotionUpdates];
            headphoneMotionCallback = nullptr;
            headphoneConnectCallback = nullptr;
            headphoneDisconnectCallback = nullptr;
        }
    }
}

#else

extern "C" bool HeadphoneMotionStart(HeadphoneMotionCallback m, HeadphoneConnectCallback c, HeadphoneDisconnectCallback d) {
    NSLog(@"[NativeSensors] (HeadphoneMotion) SDK version too low. Make sure your SDK supports iOS >= 14.0.");
    headphoneMotionCallback = m;
    headphoneConnectCallback = c;
    headphoneDisconnectCallback = d;
    (*headphoneMotionCallback)(
        0,
        NAN, 0, NAN, 0,
        NAN, 0, NAN,
        0, NAN, 0,
        NAN, 0, NAN,
        0, NAN, 0,
        -1, NAN, 0
    );
    headphoneMotionCallback = nullptr;
    headphoneConnectCallback = nullptr;
    headphoneDisconnectCallback = nullptr;
    return false;
}
extern "C" void HeadphoneMotionStop() {}

#endif
