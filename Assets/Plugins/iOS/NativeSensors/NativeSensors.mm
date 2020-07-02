#import <CoreMotion/CoreMotion.h>

typedef void (*DeviceMotionCallback)(double t, double rx, double ry, double rz, double ax, double ay, double az);
static DeviceMotionCallback deviceMotionCallback = nullptr;

static CMMotionManager* sMotionManager  = nil;
static CMDeviceMotionHandler deviceMotionHandler = ^(CMDeviceMotion *deviceMotion, NSError *error) {
    if(error == nil && deviceMotion && deviceMotionCallback) {
        (*deviceMotionCallback)(deviceMotion.timestamp, deviceMotion.rotationRate.x, deviceMotion.rotationRate.y, deviceMotion.rotationRate.z, deviceMotion.userAcceleration.x, deviceMotion.userAcceleration.y, deviceMotion.userAcceleration.z);
    }
};

extern "C" bool DeviceMotionStart(DeviceMotionCallback m) {
    if (!sMotionManager) {
        sMotionManager = [[CMMotionManager alloc] init];
    }
    if (sMotionManager.deviceMotionAvailable) {
        deviceMotionCallback = m;
        if (deviceMotionCallback != nullptr) {
            [sMotionManager startDeviceMotionUpdatesToQueue:[NSOperationQueue mainQueue] withHandler:deviceMotionHandler];
            return true;
        }
    }
    return false;
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
    if(error == nil && deviceMotion && headphoneMotionCallback) {
        (*headphoneMotionCallback)(deviceMotion.timestamp, deviceMotion.rotationRate.x, deviceMotion.rotationRate.y, deviceMotion.rotationRate.z, deviceMotion.userAcceleration.x, deviceMotion.userAcceleration.y, deviceMotion.userAcceleration.z);
    } else {
        NSLog(@"[NativeSensors] Error: %@ %@", error, [error userInfo]);
    }
};

extern "C" bool HeadphoneMotionStart(HeadphoneMotionCallback m, HeadphoneConnectCallback c, HeadphoneDisconnectCallback d) {
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
            headphoneMotionCallback = m;
            headphoneConnectCallback = c;
            headphoneDisconnectCallback = d;
            if (headphoneMotionCallback != nullptr) {
                [sHeadphoneMotionManager startDeviceMotionUpdatesToQueue:[NSOperationQueue mainQueue] withHandler:headphoneDeviceMotionHandler];
                return true;
            } else {
                NSLog(@"[NativeSensors] No handler is set.");
            }
        } else {
            NSLog(@"[NativeSensors] Device Motion unavailable. Your device may not support headphone motion.");
        }
    } else {
        NSLog(@"[NativeSensors] API unavailable. Either the deployment target or the iOS on your phone has version below 14.0.");
    }
    return false;
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
    NSLog(@"[NativeSensors] SDK version too low. Make sure your SDK supports iOS >= 14.0.");
}
extern "C" void HeadphoneMotionStop() {}

#endif
