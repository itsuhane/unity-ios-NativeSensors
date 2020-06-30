#import <CoreMotion/CoreMotion.h>

typedef void (*DeviceMotionCallback)(double t, double rx, double ry, double rz, double ax, double ay, double az);
static DeviceMotionCallback deviceMotionCallback = nullptr;

static CMMotionManager* sMotionManager  = nil;
static CMDeviceMotionHandler deviceMotionHandler = ^(CMDeviceMotion *deviceMotion, NSError *error) {
    if(error == nil && deviceMotion && deviceMotionCallback) {
        (*deviceMotionCallback)(deviceMotion.timestamp, deviceMotion.rotationRate.x, deviceMotion.rotationRate.y, deviceMotion.rotationRate.z, deviceMotion.userAcceleration.x, deviceMotion.userAcceleration.y, deviceMotion.userAcceleration.z);
    }
};

extern "C" void DeviceMotionStart(DeviceMotionCallback m) {
    if (!sMotionManager) {
        sMotionManager = [[CMMotionManager alloc] init];
    }
    if (sMotionManager.deviceMotionAvailable) {
        deviceMotionCallback = m;
        if (deviceMotionCallback != nullptr) {
            [sMotionManager startDeviceMotionUpdatesToQueue:[NSOperationQueue mainQueue] withHandler:deviceMotionHandler];
        }
    }
}

extern "C" void DeviceMotionStop() {
    if(sMotionManager != nil && deviceMotionCallback != nullptr) {
        [sMotionManager stopDeviceMotionUpdates];
        deviceMotionCallback = nullptr;
    }
}

typedef void (*HeadphoneMotionCallback)(double t, double rx, double ry, double rz, double ax, double ay, double az);
typedef void (*HeadphoneConnectCallback)();
typedef void (*HeadphoneDisconnectCallback)();

static HeadphoneMotionCallback headphoneMotionCallback = nullptr;
static HeadphoneConnectCallback headphoneConnectCallback = nullptr;
static HeadphoneDisconnectCallback headphoneDisconnectCallback = nullptr;

#if __IPHONE_OS_VERSION_MAX_ALLOWED >= 140000

API_AVAILABLE(ios(14.0))
static CMHeadphoneMotionManager *sHeadphoneMotionManager = nil;

API_AVAILABLE(ios(14.0))
static CMHeadphoneDeviceMotionHandler headphoneDeviceMotionHandler = ^(CMDeviceMotion *deviceMotion, NSError *error) {
    if(error == nil && deviceMotion && headphoneMotionCallback) {
        (*headphoneMotionCallback)(deviceMotion.timestamp, deviceMotion.rotationRate.x, deviceMotion.rotationRate.y, deviceMotion.rotationRate.z, deviceMotion.userAcceleration.x, deviceMotion.userAcceleration.y, deviceMotion.userAcceleration.z);
    }
};

extern "C" void HeadphoneMotionStart(HeadphoneMotionCallback m, HeadphoneConnectCallback c, HeadphoneDisconnectCallback d) {
    if (@available(iOS 14.0, *)) {
        NSLog(@"[NativeSensors] API available.");
        if (!sHeadphoneMotionManager) {
            sHeadphoneMotionManager = [[CMHeadphoneMotionManager alloc] init];
        }
        if (sHeadphoneMotionManager.deviceMotionAvailable) {
            headphoneMotionCallback = m;
            headphoneConnectCallback = c;
            headphoneDisconnectCallback = d;
            if (headphoneMotionCallback != nullptr) {
                [sHeadphoneMotionManager startDeviceMotionUpdatesToQueue:[NSOperationQueue mainQueue] withHandler:headphoneDeviceMotionHandler];
            }
        } else {
            NSLog(@"[NativeSensors] Device Motion Unavailable.");
        }
    } else {
        NSLog(@"[NativeSensors] API unavailable.");
    }
}

extern "C" void HeadphoneMotionStop() {
    if (@available(iOS 14.0, *)) {
        if(sHeadphoneMotionManager != nil && headphoneMotionCallback != nullptr) {
            [sHeadphoneMotionManager stopDeviceMotionUpdates];
            headphoneMotionCallback = nullptr;
            headphoneConnectCallback = nullptr;
            headphoneDisconnectCallback = nullptr;
        }
    }
}

#else

extern "C" void HeadphoneMotionStart(HeadphoneMotionCallback m, HeadphoneConnectCallback c, HeadphoneDisconnectCallback d) {
    NSLog(@"[NativeSensors] iOS version too low.");
}
extern "C" void HeadphoneMotionStop() {}

#endif
