#import <CoreMotion/CoreMotion.h>

#define GRAVITY_FACTOR (-9.80665)

typedef void (*GyroscopeCallback)(double t, double x, double y, double z);
typedef void (*AccelerometerCallback)(double t, double x, double y, double z);

static GyroscopeCallback gyroscopeCallback = nullptr;
static AccelerometerCallback accelerometerCallback = nullptr;
static CMMotionManager* sMotionManager  = nil;

static CMGyroHandler gyroscopeHandler = ^(CMGyroData *gyroscopeData, NSError *error) {
    if(error == nil && gyroscopeData && gyroscopeCallback) {
        (*gyroscopeCallback)(gyroscopeData.timestamp, gyroscopeData.rotationRate.x, gyroscopeData.rotationRate.y, gyroscopeData.rotationRate.z);
    }
};

static CMAccelerometerHandler accelerometerHandler = ^(CMAccelerometerData *accelerometerData, NSError *error) {
    if(error == nil && accelerometerData && accelerometerCallback) {
        (*accelerometerCallback)(accelerometerData.timestamp, GRAVITY_FACTOR*accelerometerData.acceleration.x, GRAVITY_FACTOR*accelerometerData.acceleration.y, GRAVITY_FACTOR*accelerometerData.acceleration.z);
    }
};

extern "C" void NativeSensorsSetDelegate(GyroscopeCallback g, AccelerometerCallback a) {
    gyroscopeCallback = g;
    accelerometerCallback = a;
}

extern "C" void NativeSensorsStart()
{
    if (!sMotionManager)
        sMotionManager = [[CMMotionManager alloc] init];

    if (sMotionManager.gyroAvailable) {
        [sMotionManager setGyroUpdateInterval: 1.0];
        [sMotionManager startGyroUpdatesToQueue:[NSOperationQueue mainQueue] withHandler:gyroscopeHandler];
    }

    if (sMotionManager.accelerometerAvailable)
    {
        [sMotionManager setAccelerometerUpdateInterval: 1.0];
        [sMotionManager startAccelerometerUpdatesToQueue:[NSOperationQueue mainQueue] withHandler:accelerometerHandler];
    }
}

extern "C" void NativeSensorsStop() {
    if (sMotionManager != nil) {
        [sMotionManager stopGyroUpdates];
        [sMotionManager stopAccelerometerUpdates];
    }
}
