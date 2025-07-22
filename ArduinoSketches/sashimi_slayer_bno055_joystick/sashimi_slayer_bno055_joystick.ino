/*
v. 0.1.1
This variation allows the sword to act as a generic joystick. 
This is preferred over the serial comm variation.

To connect with the game, just plug the sword in and it should work immediately.
*/

// Libraries
#include <Wire.h>
#include <Adafruit_Sensor.h>
#include <Adafruit_BNO055.h>
#include <Joystick.h>
#include <math.h>
#include <utility/imumaths.h>

// Button state
int btnTop = 0;
int btnMid = 0;

int sliceBtn = 0;

imu::Quaternion q;

Joystick_ Joystick(JOYSTICK_DEFAULT_REPORT_ID,JOYSTICK_TYPE_JOYSTICK,
  32, 0,                  // Button Count, Hat Switch Count
  true, true, false,     // X and Y, but no Z Axis
  false, false, false,   // No Rx, Ry, or Rz
  false, false,          // No rudder or throttle
  false, false, false);  // No accelerator, brake, or steering

// Pin numbers
#define HAPTIC_IN_PIN 5
#define BTN_TOP_PIN 14
#define BTN_MID_PIN 15

#define SHEATHE_L_PIN 7
#define SHEATHE_R_PIN 8
#define LED_PIN 17

// Gamepad button mapping
#define SLICE_GAMEPAD 1
#define BLOCK_TOP_GAMEPAD 2
#define BLOCK_MID_GAMEPAD 3
#define AXIS_RANGE 1024

char inputBuffer;

Adafruit_BNO055 bno = Adafruit_BNO055(55);

// The vector of the IMU that points "up" (towards the handle of the sword)
const imu::Vector<3> upVector = imu::Vector<3>(1,0,0);

void setup_gyro()
{
  /* Initialise the sensor */
  if(!bno.begin())
  {
    /* There was a problem detecting the BNO055 ... check your connections */
    Serial.print("Ooops, no BNO055 detected ... Check your wiring or I2C ADDR!");
    while(1);
  }
  
  delay(1000);
    
  bno.setExtCrystalUse(true);
}

void setup() {  
  Serial.begin(9600);

  setup_gyro();

  pinMode(BTN_TOP_PIN, INPUT_PULLUP);
  pinMode(BTN_MID_PIN, INPUT_PULLUP);
  pinMode(SHEATHE_L_PIN, INPUT_PULLUP);
  pinMode(SHEATHE_R_PIN, INPUT_PULLUP);
  pinMode(LED_PIN, OUTPUT);
  pinMode(HAPTIC_IN_PIN, OUTPUT);

  Joystick.begin();
  Joystick.setXAxisRange(-AXIS_RANGE, AXIS_RANGE);
  Joystick.setYAxisRange(-AXIS_RANGE, AXIS_RANGE);
}

void loop() {
  // invert because of pullup
  bool newBtnTop = !digitalRead(BTN_TOP_PIN);
  bool newBtnMid = !digitalRead(BTN_MID_PIN);

  if (newBtnTop != btnTop)
  {
    btnTop = newBtnTop;
    Joystick.setButton(BLOCK_TOP_GAMEPAD, btnTop);
  }

  if (newBtnMid != btnMid)
  {
    btnMid = newBtnMid;
    Joystick.setButton(BLOCK_MID_GAMEPAD, btnMid);
  }

  // False when sheathe is in (switches are closed), true when sheathe is out (switches are open)
  int newSliceBtn = digitalRead(SHEATHE_L_PIN) && digitalRead(SHEATHE_R_PIN);
  
  if(newSliceBtn != sliceBtn)
  {
    sliceBtn = newSliceBtn;
    digitalWrite(HAPTIC_IN_PIN, sliceBtn);
    Joystick.setButton(SLICE_GAMEPAD, sliceBtn);
  }


  q = bno.getQuat();
  imu::Vector<3> orientUp = q.rotateVector(upVector);

  // Dot product wtih <0,1,0> to get the angle difference from "up"
  double cosTheta = orientUp.z();
  double sinTheta = sqrt(1 - cosTheta * cosTheta);

  // Subtract 90 degrees. We want to map sword aiming down to 90 degrees and horizontal sword to 0 degrees
  double temp = cosTheta;
  cosTheta = sinTheta;
  sinTheta = -temp;

  Joystick.setXAxis(cosTheta * AXIS_RANGE);
  Joystick.setYAxis(sinTheta * AXIS_RANGE);
}