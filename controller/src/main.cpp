#include <Arduino.h>
#include "LedControl.h"
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

// Sensor Smoother
Smoothed<float> tsSensor;

// LEDs
const int LED_PINS[2] { DD2, DD3};

int led_State[2];

// Utils
float mapFloat(float value, float fromLow, float fromHigh, float toLow, float toHigh)
{
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
  lc.setChar(0, 6, '0', 0);
  lc.setChar(0, 5, 'L', 0);
  lc.setChar(0, 4, 'A', 0);
  lc.setChar(0, 3, 'b', 0);
  lc.setChar(0, 2, 'b', 0);
  lc.setChar(0, 1, '0', 0);
  delay(1000);
  lc.clearDisplay(0);

  // -
  pinMode(LED_PINS[0], OUTPUT);
  pinMode(LED_PINS[1], OUTPUT);
  digitalWrite(LED_PINS[0], HIGH);
  digitalWrite(LED_PINS[1], HIGH);

  // Initial Values
  mainDisplayValues[0] = 100;
  mainDisplayValues[1] = 100;
  mainDisplayValues[2] = 100;
  mainDisplayValues[3] = 100;

  // - LEDS
  led_State[0] = 0;
  led_State[1] = 0;

  // Begins the Serial Comm
  Serial.begin(19200);
  Serial.println("OLABBR");

  tsSensor.begin(SMOOTHED_AVERAGE, 10);
}

void readThermalSensor()
{
  int value = analogRead(TS_PIN);
  float voltage = mapFloat(value, 0, 1023, 0, 5);
  float temp = mapFloat(voltage, TS_MIN_VOLT, TS_MAX_VOLT, TS_MIN_VALUE, TS_MAX_VALUE);

  tsSensor.add(temp);

  mainDisplayValues[0] = tsSensor.get();
}

void writeDisplayValue(int index)
{
  int value = mainDisplayValues[index];
  // Converts to 2 Digits
  char firstDigit = '0';
  char secDigit = '0';

  if (value > 99 || value < 0)
  {
    firstDigit = '-';
    secDigit = '-';
  }
  else
  {
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

void writeLedState(int lIndex) {
  digitalWrite(LED_PINS[lIndex], led_State[lIndex] == 1 ? HIGH : LOW);
}

void doCommand(String cmd)
{
  String name = cmd.substring(0, 1);

  if (name.equals("S"))   // Set a Value in the Display
  { 
  
    int vIndex = cmd.substring(1,2).toInt();
    int vValue = cmd.substring(2).toInt();

    mainDisplayValues[vIndex] = vValue;
    Serial.print("+");
    Serial.println(cmd);
  }
  else if(name.equals("L")) { // Set Led State
    int vIndex = cmd.substring(1,2).toInt();
    int vValue = cmd.substring(2).toInt();

    led_State[vIndex] = vValue;

    Serial.print("+");
    Serial.println(cmd);
  }
  else {
    Serial.print("-");
    Serial.println(cmd);
  }
}

void loop()
{
  // Check for Command
  if (Serial.available() > 0)
  {
    String cmd = Serial.readStringUntil('\n');
    doCommand(cmd);
  }

  // Updates the Display
  writeDisplayValue(0);
  writeDisplayValue(1);
  writeDisplayValue(2);
  writeDisplayValue(3);
  delay(100);

  writeLedState(0);
  writeLedState(1);
  delay(100);

  readThermalSensor();
}