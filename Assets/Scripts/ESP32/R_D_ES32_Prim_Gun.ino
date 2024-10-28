#include "I2Cdev.h"
#include "MPU6050_6Axis_MotionApps20.h"
#include "Wire.h"

MPU6050 mpu;

#define INTERRUPT_PIN 47  // W Set the interrupt pin
const int buttonTrigger = 10;
int buttonTriggerState = 0;


// ================================================================
// ===               INTERRUPT DETECTION ROUTINE                ===
// ================================================================

volatile bool mpuInterrupt = false;  // indicates whether MPU interrupt pin has gone high
void dmpDataReady() {
  mpuInterrupt = true;
}

// MPU control/status vars
bool dmpReady = false;   // set true if DMP init was successful
uint8_t devStatus;       // return status after each device operation (0 = success, !0 = error)
uint16_t packetSize;     // expected DMP packet size (default is 42 bytes)
uint16_t fifoCount;      // count of all bytes currently in FIFO
uint8_t fifoBuffer[64];  // FIFO storage buffer

// orientation/motion vars
Quaternion q;         // [w, x, y, z]         quaternion container
VectorInt16 aa;       // [x, y, z]            accel sensor measurements
VectorInt16 aaReal;   // [x, y, z]            gravity-free accel sensor measurements
VectorInt16 aaWorld;  // [x, y, z]            world-frame accel sensor measurements
VectorFloat gravity;  // [x, y, z]            gravity vector
float euler[3];       // [psi, theta, phi]    Euler angle container
float ypr[3];         // [yaw, pitch, roll]   yaw/pitch/roll container and gravity vector



// ================================================================
// ===                      INITIAL SETUP                       ===
// ================================================================

void setup() {
  //for MPU functionality
  Wire.begin(20, 21); // SDA:GPIO 20 and SCL:GPIO 21
  Wire.setClock(400000);  // 400kHz I2C clock. Comment this line if having compilation difficulties

  Serial.begin(115200);

  while (!Serial)
    ;  // wait for Leonardo enumeration, others continue immediately

  mpu.initialize();
  pinMode(INTERRUPT_PIN, INPUT);  // W Setup the interrupt pin as a input
  devStatus = mpu.dmpInitialize();
  mpu.setXGyroOffset(83);  // last set 18/10/2024 using uduino_zero script
  mpu.setYGyroOffset(26);  //
  mpu.setZGyroOffset(73);
  mpu.setZAccelOffset(1521);  // W

  if (devStatus == 0) {
    // W Calibration Time: generate offsets and calibrate our MPU6050
    mpu.CalibrateAccel(6);
    mpu.CalibrateGyro(6);
    mpu.PrintActiveOffsets();
    mpu.setDMPEnabled(true);
    // W enable Arduino interrupt detection
    digitalPinToInterrupt(INTERRUPT_PIN);
    attachInterrupt(digitalPinToInterrupt(INTERRUPT_PIN), dmpDataReady, RISING);
    // set our DMP Ready flag so the main loop() function knows it's okay to use it
    dmpReady = true;
    // get expected DMP packet size for later comparison
    packetSize = mpu.dmpGetFIFOPacketSize();
  } else {
    // Error
    Serial.println("Error!");
  }

  //for button functionality
  pinMode(buttonTrigger, INPUT_PULLUP);
}


// ================================================================
// ===                    MAIN PROGRAM LOOP                     ===
// ================================================================

void loop() {

  if (!dmpReady) {
    Serial.println("IMU not connected.");
    delay(10);
    return;
  }

  int mpuIntStatus = mpu.getIntStatus();
  fifoCount = mpu.getFIFOCount();

  if ((mpuIntStatus & 0x10) || fifoCount == 1024) {  // check if overflow
    mpu.resetFIFO();
  } else if (mpuIntStatus & 0x02) {
    while (fifoCount < packetSize) fifoCount = mpu.getFIFOCount();

    mpu.getFIFOBytes(fifoBuffer, packetSize);
    fifoCount -= packetSize;

    SendQuaternion();
    //SendEuler();
    //SendYawPitchRoll();
    //SendRealAccel();
    //SendWorldAccel();
  }


  // buttonTriggerState = digitalRead(buttonTrigger);
  //SendTriggerState()
}


// ================================================================
// ===                       MPU FUNCTIONS                      ===
// ================================================================

void SendQuaternion() {
  mpu.dmpGetQuaternion(&q, fifoBuffer);
  Serial.print("r/");
  Serial.print(q.w, 4);
  Serial.print("/");
  Serial.print(q.x, 4);
  Serial.print("/");
  Serial.print(q.y, 4);
  Serial.print("/");
  Serial.println(q.z, 4);
}

void SendEuler() {
  // display Euler angles in degrees
  mpu.dmpGetQuaternion(&q, fifoBuffer);
  mpu.dmpGetEuler(euler, &q);
  Serial.print(euler[0] * 180 / M_PI);
  Serial.print("/");
  Serial.print(euler[1] * 180 / M_PI);
  Serial.print("/");
  Serial.println(euler[2] * 180 / M_PI);
}

void SendYawPitchRoll() {
  // display Euler angles in degrees
  mpu.dmpGetQuaternion(&q, fifoBuffer);
  mpu.dmpGetGravity(&gravity, &q);
  mpu.dmpGetYawPitchRoll(ypr, &q, &gravity);
  Serial.print(ypr[0] * 180 / M_PI);
  Serial.print("/");
  Serial.print(ypr[1] * 180 / M_PI);
  Serial.print("/");
  Serial.println(ypr[2] * 180 / M_PI);
}

void SendRealAccel() {
  // display real acceleration, adjusted to remove gravity
  mpu.dmpGetQuaternion(&q, fifoBuffer);
  mpu.dmpGetAccel(&aa, fifoBuffer);
  mpu.dmpGetGravity(&gravity, &q);
  mpu.dmpGetLinearAccel(&aaReal, &aa, &gravity);
  Serial.print("a/");
  Serial.print(aaReal.x);
  Serial.print("/");
  Serial.print(aaReal.y);
  Serial.print("/");
  Serial.println(aaReal.z);
}

void SendWorldAccel() {
  // display initial world-frame acceleration, adjusted to remove gravity
  // and rotated based on known orientation from quaternion
  mpu.dmpGetQuaternion(&q, fifoBuffer);
  mpu.dmpGetAccel(&aa, fifoBuffer);
  mpu.dmpGetGravity(&gravity, &q);
  mpu.dmpGetLinearAccel(&aaReal, &aa, &gravity);
  mpu.dmpGetLinearAccelInWorld(&aaWorld, &aaReal, &q);
  Serial.print("a/");
  Serial.print(aaWorld.x);
  Serial.print("/");
  Serial.print(aaWorld.y);
  Serial.print("/");
  Serial.println(aaWorld.z);
}


//button functions
void SendTriggerState() {
  // Send the states to Unity in a single message
  String message = "tr/" + String(buttonTriggerState);
  Serial.println(message);
}