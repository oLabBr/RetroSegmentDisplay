#include <Arduino.h>
#include "LedControl.h"
#include "StringSplitter.h"
#include "Smoothed.h"

// MAIN DISPLAY
LedControl lc = LedControl(12, 11, 10, 1);
int mainDisplayValues[4];

// THERMAL-SENSOR CONFIG
#define TS_MIN_VALUE 10
#define TS_MIN_VOLT 4.756
#define TS_MAX_VALUE 125
#define TS_MAX_VOLT 1.154

const int TS_PIN = A1;
Smoothed <float> tsSensor;


float mapFloat(float value, float fromLow, float fromHigh, float toLow, float toHigh) {
  return (value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow) + toLow; 
}

// - 
void setup()
{
  // - Enables the DISPLAY
  lc.shutdown(0, false);
  lc.setIntensity(0, 6);
  lc.clearDisplay(0);
  delay(100);
  lc.setChar(0,6,'0', 0);
  lc.setChar(0,5,'L', 0);
  lc.setChar(0,4,'A', 0);
  lc.setChar(0,3,'b', 0);
  lc.setChar(0,2,'b', 0);
  lc.setChar(0,1,'0', 0);
  delay(1000);
  lc.clearDisplay(0);

  // Initial Values
  mainDisplayValues[0] = 10;
  mainDisplayValues[1] = 22;
  mainDisplayValues[2] = 100;
  mainDisplayValues[3] = 7;

  // Begins the Serial Comm
  Serial.begin(9600);
  Serial.println("OLABBR");

  tsSensor.begin(SMOOTHED_AVERAGE, 10);
}

void readThermalSensor() {
  int value = analogRead(TS_PIN);
  float voltage = mapFloat(value, 0, 1023, 0, 5);
  float temp = mapFloat(voltage, TS_MIN_VOLT, TS_MAX_VOLT, TS_MIN_VALUE, TS_MAX_VALUE);

  tsSensor.add(temp);

  mainDisplayValues[0] = tsSensor.get();
}

void writeDisplayValue(int index)
{
  int value = mainDisplayValues[index];
  if (value > 99)
  {
    value = 99;
  }
  else if (value < 0)
  {
    value = 0;
  }

  // Converts to 2 Digits
  char firstDigit = '0';
  char secDigit = '0';
  String valueStr = String(value);

  if (value > 9)
  {
    firstDigit = valueStr.charAt(1);
    secDigit = valueStr.charAt(0);
  }
  else
  {
    firstDigit = valueStr.charAt(0);
  }

  // Writes
  switch (index)
  {
  case 3:
    lc.setChar(0, 0, firstDigit, 0);
    lc.setChar(0, 1, secDigit, 0);
    break;

  case 2:
    lc.setChar(0, 2, firstDigit, 0);
    lc.setChar(0, 3, secDigit, 0);
    break;

  case 1:
    lc.setChar(0, 4, firstDigit, 0);
    lc.setChar(0, 5, secDigit, 0);
    break;

  case 0:
    lc.setChar(0, 6, firstDigit, 0);
    lc.setChar(0, 7, secDigit, 0);
    break;

  default:
    break;
  }
}

void doCommand(String cmd) {
  StringSplitter *sp = new StringSplitter(cmd, ':', 3);
  String name = sp->getItemAtIndex(0);

  if(name.equals("S")) { // Set a Value in the Display
      int vIndex = sp->getItemAtIndex(1).toInt();
      int vValue = sp->getItemAtIndex(2).toInt();

      mainDisplayValues[vIndex] = vValue;      
  }
}

void loop()
{
  // Check for Command
  if(Serial.available() > 0) {
    String cmd = Serial.readStringUntil('\n');
    doCommand(cmd);
  }

  // Updates the Display
  writeDisplayValue(0);
  writeDisplayValue(1);
  writeDisplayValue(2);
  writeDisplayValue(3);
  delay(250);

  readThermalSensor();
}