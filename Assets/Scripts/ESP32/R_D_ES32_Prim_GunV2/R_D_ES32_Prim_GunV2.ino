#include "I2Cdev.h"
//#include "USB.h"
#include "MPU6050_6Axis_MotionApps20.h"
#include "Wire.h"
#include <esp_now.h>
#include <WiFi.h>
#include <SPI.h>
//#include <MFRC522.h>
#include <Adafruit_GFX.h>
#include <Adafruit_SH110X.h>
MPU6050 mpu;

#define INTERRUPT_PIN 47  // W Set the interrupt pin
const int trigger_pin = 4;
int lastTriggerState = 1;

//ESP now 
//uint8_t broadcastAddress[] = {0xDC, 0xDA, 0x0C, 0x63, 0xCC, 0x9C}; // send to esp32s3 divice 2
uint8_t broadcastAddress[] = {0x24, 0xEC, 0x4A, 0x01, 0x32, 0xA0}; // send to esp32s3 device 3 (24:ec:4a:01:32:a0)
String success;

esp_now_peer_info_t peerInfo;

//-------------OLED------------
#define SCREEN_WIDTH 128
#define SCREEN_HEIGHT 64
#define OLED_RESET -1
#define oled_i2c_Address 0x3c

Adafruit_SH1106G display(SCREEN_WIDTH, SCREEN_HEIGHT, &Wire, OLED_RESET);
int magCapacity = 10;
int bulletsLeft = 0;

#define bulletIcon_width 30
#define bulletIcon_height 29
const unsigned char bulletIcon[] PROGMEM = {
	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x20, 
	0x00, 0x00, 0x17, 0x80, 0x00, 0x00, 0x2e, 0x00, 0x00, 0x00, 0xfc, 0x00, 0x00, 0x01, 0x18, 0x20, 
	0x00, 0x02, 0xc8, 0x00, 0x00, 0x05, 0xe0, 0x40, 0x00, 0x0b, 0xc0, 0x00, 0x00, 0x07, 0x81, 0x80, 
	0x00, 0x0f, 0x03, 0x00, 0x00, 0x1e, 0x02, 0x00, 0x00, 0x3c, 0x04, 0x00, 0x00, 0x78, 0x08, 0x00, 
	0x00, 0xf0, 0x10, 0x00, 0x05, 0xe0, 0x20, 0x00, 0x07, 0x80, 0x40, 0x00, 0x1f, 0x80, 0x00, 0x00, 
	0x07, 0x80, 0x00, 0x00, 0x03, 0xc0, 0x00, 0x00, 0x00, 0xe0, 0x00, 0x00, 0x00, 0x70, 0x00, 0x00, 
	0x00, 0x40, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
	0x00, 0x00, 0x00, 0x00
};


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

  //ESP now
  WiFi.mode(WIFI_STA);

  if (esp_now_init() != ESP_OK) {
    Serial.println("Error initializing ESP-NOW");
    return;
  }
  esp_now_register_send_cb(OnDataSent); // set wich func to execute on data sent event
 
  memcpy(peerInfo.peer_addr, broadcastAddress, 6);
  peerInfo.channel = 0; 
  peerInfo.encrypt = false;
       
  if (esp_now_add_peer(&peerInfo) != ESP_OK){
    Serial.println("Failed to add peer");
    return;
  }
  esp_now_register_recv_cb(OnDataRecv); // set wich func to execute on data recieve event

  //for button functionality
  pinMode(trigger_pin, INPUT_PULLUP);

  //-------------------OLED-----------------
  if (display.begin(oled_i2c_Address, true)) {  // Try to initialize the display
    display.setRotation(2);
    Serial.println("OLED display initialized successfully.");
    display.clearDisplay();            // Clear any previous data from the display buffer
    initializingAnimation();           
    updateDisplay();
  } else {
    Serial.println("Failed to initialize OLED display.");
    // Handle the failure case, like displaying an error or retrying
  }
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

    //SendQuaternion();
    SendQuaternionEspNow();
    //SendEuler();
    //SendYawPitchRoll();
    //SendRealAccel();
    //SendWorldAccel();
  }

  int triggerState = digitalRead(trigger_pin);
  if (triggerState == 0 || (lastTriggerState == 0 && triggerState == 1)) {
    lastTriggerState = triggerState;
    SendTriggerStateEspNow(triggerState);
  }
}

// ================================================================
// ===                     OLED FUNCTIONS                       ===
// ================================================================


void initializingAnimation() {
  display.setTextSize(1);
  display.setTextColor(SH110X_WHITE);
  display.setCursor(0, 0);
  display.print("Initializing...");

  // Animation loop
  for (int i = 0; i < SCREEN_WIDTH; i += 4) {
    display.drawLine(0, SCREEN_HEIGHT - 1, i, 0, SH110X_WHITE);
    display.display();
    delay(50);
  }

  display.clearDisplay();
  display.setCursor(0, 0);
  display.println("Ready to Fire!");
  display.display();
  delay(1000);
  display.clearDisplay();
}

void updateDisplay() {
  display.clearDisplay();

  int bulletX = 6; 
  int bulletY = (SCREEN_HEIGHT - 6) - 4; 
  int bulletWidth = 3;
  int bulletHeight = 10;

  // Display bullets left with icons
  for (int i = 0; i < bulletsLeft; i++) {
    display.fillRoundRect(bulletX + (i*4), bulletY, bulletWidth, bulletHeight, 1, SH110X_WHITE);
    // display.drawBitmap(bulletX, bulletY - (i * 10), bulletIcon, 8, 8, SH110X_WHITE);
  }

  //Display bullet
  display.drawBitmap(10, (SCREEN_HEIGHT/2)-14, bulletIcon, bulletIcon_width, bulletIcon_height, SH110X_WHITE);

  // Display bullet count as a number below icons
  display.setTextSize(3);
  display.setTextColor(SH110X_WHITE);
  if(bulletsLeft > 9){
    display.setCursor((SCREEN_WIDTH - 30) / 2, (SCREEN_HEIGHT/2)-10);
  }else{
    display.setCursor((SCREEN_WIDTH - 15) / 2, (SCREEN_HEIGHT/2)-10);
  }
  display.print(bulletsLeft);
  display.setTextSize(1);
  display.print("/" + String(magCapacity));

  display.display();
}

void reload(const uint8_t *incomingData, int len) {
  String dataStr = "";
  for (int i = 0; i < len; i++) {
    dataStr += (char)incomingData[i];
  }
  // Split dataStr by '/' and store in an array
  String values[3];  // Array to hold split parts: ["b", "15", "10"]
  splitString(dataStr, '/', values, 3);

  if(values[0] == "b"){
    bulletsLeft = values[2].toInt();
    Serial.print("Parsed bulletsLeft: ");
    Serial.println(bulletsLeft);
    updateDisplay();
  }
  else if (values[0] == "rb") {  // Check that it's the correct type
    magCapacity = values[1].toInt();
    bulletsLeft = values[2].toInt();

    Serial.print("Parsed magCapacity: ");
    Serial.println(magCapacity);
    Serial.print("Parsed bulletsLeft: ");
    Serial.println(bulletsLeft);
    reloadingAnimation();
    updateDisplay();
  } else {
    Serial.println("Error: Unsupported data type");
  }  
}

// Function to split a string by a delimiter and store results in an array
void splitString(String str, char delimiter, String* result, int maxParts) {
  int start = 0;
  int partIndex = 0;
  
  for (int i = 0; i < str.length() && partIndex < maxParts; i++) {
    if (str[i] == delimiter) {
      result[partIndex++] = str.substring(start, i);
      start = i + 1;
    }
  }
  result[partIndex] = str.substring(start);
}

void reloadingAnimation() {
  display.clearDisplay();
  display.setTextSize(1);
  display.setCursor(0, 0);
  display.println("Reloading...");

  // Simple reloading bar animation
  for (int i = 0; i <= SCREEN_WIDTH; i += 8) {
    display.fillRect(0, SCREEN_HEIGHT - 8, i, 8, SH110X_WHITE);
    display.display();
    delay(100);
  }

  display.clearDisplay();
  display.setCursor(0, 0);
  display.println("Reload Complete!");
  display.display();
  delay(1000);
  display.clearDisplay();
}


// ================================================================
// ===                   TRIGGER FUNCTIONS                      ===
// ================================================================

// send button state via ESP-NOW
void SendTriggerStateEspNow(int State) {
  String message = "tr/" + String(State);
  esp_err_t result = esp_now_send(broadcastAddress, (uint8_t *) message.c_str(), message.length() + 1);

  if (result == ESP_OK) {
    Serial.println("Trigger state sent successfully");
  } else {
    Serial.println("Error sending trigger state");
  }
}


// ================================================================
// ===                   ESP now functions                      ===
// ================================================================

void OnDataSent(const uint8_t *mac_addr, esp_now_send_status_t status) {
  Serial.print("\r\nDelivery Status: ");
  Serial.println(status == ESP_NOW_SEND_SUCCESS ? "Deliverd Successfully" : "Delivery Fail");
  if (status ==0){
    success = "Delivery Success :)";
  }
  else{
    success = "Delivery Fail :(";
  }
}

void OnDataRecv(const esp_now_recv_info* recv_info, const uint8_t *incomingData, int len) {
  Serial.print("\r\nDataReceived: ");

  // Print data in hexadecimal format
  Serial.print("Hex data: ");
  for (int i = 0; i < len; i++) {
    Serial.print(incomingData[i], HEX);
    Serial.print(" ");
  }
  
  // Print data as characters
  Serial.print("\nASCII data: ");
  for (int i = 0; i < len; i++) {
    Serial.print((char)incomingData[i]);
  }
  Serial.println();

  reload(incomingData, len);
}


// ================================================================
// ===                       MPU FUNCTIONS                      ===
// ================================================================



void SendQuaternionEspNow() {
  mpu.dmpGetQuaternion(&q, fifoBuffer);

  String message = "r/" + String(q.w, 4) 
                  + "/" + String(q.x, 4) 
                  + "/" + String(q.y, 4) 
                  + "/" + String(q.z, 4);
  esp_err_t result = esp_now_send(broadcastAddress, (uint8_t *) message.c_str(), message.length() + 1); // Send the message
  
  if (result == ESP_OK) {
    Serial.println("Quaternion data sent successfully");
  } else {
    Serial.println("Error sending quaternion data");
  }
}

// void SendQuaternion() {
//   mpu.dmpGetQuaternion(&q, fifoBuffer);
//   Serial.print("r/");
//   Serial.print(q.w, 4);
//   Serial.print("/");
//   Serial.print(q.x, 4);
//   Serial.print("/");
//   Serial.print(q.y, 4);
//   Serial.print("/");
//   Serial.println(q.z, 4);
// }

// void SendEuler() {
//   // display Euler angles in degrees
//   mpu.dmpGetQuaternion(&q, fifoBuffer);
//   mpu.dmpGetEuler(euler, &q);
//   Serial.print(euler[0] * 180 / M_PI);
//   Serial.print("/");
//   Serial.print(euler[1] * 180 / M_PI);
//   Serial.print("/");
//   Serial.println(euler[2] * 180 / M_PI);
// }

// void SendYawPitchRoll() {
//   // display Euler angles in degrees
//   mpu.dmpGetQuaternion(&q, fifoBuffer);
//   mpu.dmpGetGravity(&gravity, &q);
//   mpu.dmpGetYawPitchRoll(ypr, &q, &gravity);
//   Serial.print(ypr[0] * 180 / M_PI);
//   Serial.print("/");
//   Serial.print(ypr[1] * 180 / M_PI);
//   Serial.print("/");
//   Serial.println(ypr[2] * 180 / M_PI);
// }

// void SendRealAccel() {
//   // display real acceleration, adjusted to remove gravity
//   mpu.dmpGetQuaternion(&q, fifoBuffer);
//   mpu.dmpGetAccel(&aa, fifoBuffer);
//   mpu.dmpGetGravity(&gravity, &q);
//   mpu.dmpGetLinearAccel(&aaReal, &aa, &gravity);
//   Serial.print("a/");
//   Serial.print(aaReal.x);
//   Serial.print("/");
//   Serial.print(aaReal.y);
//   Serial.print("/");
//   Serial.println(aaReal.z);
// }

// void SendWorldAccel() {
//   // display initial world-frame acceleration, adjusted to remove gravity
//   // and rotated based on known orientation from quaternion
//   mpu.dmpGetQuaternion(&q, fifoBuffer);
//   mpu.dmpGetAccel(&aa, fifoBuffer);
//   mpu.dmpGetGravity(&gravity, &q);
//   mpu.dmpGetLinearAccel(&aaReal, &aa, &gravity);
//   mpu.dmpGetLinearAccelInWorld(&aaWorld, &aaReal, &q);
//   Serial.print("a/");
//   Serial.print(aaWorld.x);
//   Serial.print("/");
//   Serial.print(aaWorld.y);
//   Serial.print("/");
//   Serial.println(aaWorld.z);
// }
