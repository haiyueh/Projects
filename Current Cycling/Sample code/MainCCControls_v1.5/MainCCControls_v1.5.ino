#include "TimerOne.h"
#include <string.h>
#include <avr/wdt.h>

//Definitions
#define IDN_RESPONSE "ARDUINO CC Controls v1.5"
#define new_max(x,y) ((x) >= (y)) ? (x) : (y)
#define RELAY_PIN 14
#define MAX_SECONDS_BEFORE_FAIL_HEART_BEAT 30
#define TERMINATOR '\r'
#define IGNORE_CHAR '\n'
#define TEMP_SENSOR_VOLTAGE 4.9
#define SMOKE_SENSOR_PIN 15


//==================================
//Global Declarations
//==================================

//Heart beat seconds
int intHeartBeatSeconds = 0;

//Target fan speed
int TargetFanPWMSpeeds[8] = {0};

//Temperature Data
int TempData[16] = {1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16};

//Serial buffer
String txtSerialBuffer;

//Serial write state machine
bool NextDataPacketIsWriteToArduinoData = false;

//Incoming data from PC initial values
int intBiasCurrentStatus = 0;
int intBiasCurrentOnTemp = 85;
int intBiasCurrentOffTemp = 25;
int intOverloadTemp = 100;

bool bolHeartBeatGood = false;
bool bolOverTempTrip = true;
bool bolSmokeSensorLockout = false;
bool bolPauseFans = false;

void watchdogSetup(void)
{
cli();  // disable all interrupts
wdt_reset(); // reset the WDT timer
/*
WDTCSR configuration:
WDIE = 1: Interrupt Enable
WDE = 1 :Reset Enable
WDP3 = 0 :For 2000ms Time-out
WDP2 = 1 :For 2000ms Time-out
WDP1 = 1 :For 2000ms Time-out
WDP0 = 1 :For 2000ms Time-out
*/
// Enter Watchdog Configuration mode:
WDTCSR |= (1<<WDCE) | (1<<WDE);
// Set Watchdog settings:
WDTCSR = (1<<WDIE) | (1<<WDE) | (0<<WDP3) | (1<<WDP2) | (1<<WDP1) | (1<<WDP0);
sei();
}
//======================================================================
//Function: ReadTemps
//Inputs: none
//Outputs: none
//Description: Reads the temperatures from the sensors
//             424mV ofset at 6.25mV/deg. C
//======================================================================
void ReadTemps(){
  TempData[0] = (int)((analogRead(A0)  / 1024.0 * TEMP_SENSOR_VOLTAGE - .424) * 160);
  TempData[1] = (int)((analogRead(A1)  / 1024.0 * TEMP_SENSOR_VOLTAGE - .424) * 160);
  TempData[2] = (int)((analogRead(A2)  / 1024.0 * TEMP_SENSOR_VOLTAGE - .424) * 160);
  TempData[3] = (int)((analogRead(A3)  / 1024.0 * TEMP_SENSOR_VOLTAGE - .424) * 160);
  TempData[4] = (int)((analogRead(A4)  / 1024.0 * TEMP_SENSOR_VOLTAGE - .424) * 160);
  TempData[5] = (int)((analogRead(A5)  / 1024.0 * TEMP_SENSOR_VOLTAGE - .424) * 160);
  TempData[6] = (int)((analogRead(A6)  / 1024.0 * TEMP_SENSOR_VOLTAGE - .424) * 160);
  TempData[7] = (int)((analogRead(A7)  / 1024.0 * TEMP_SENSOR_VOLTAGE - .424) * 160);
  TempData[8] = (int)((analogRead(A8)  / 1024.0 * TEMP_SENSOR_VOLTAGE - .424) * 160);
  TempData[9] = (int)((analogRead(A9)  / 1024.0 * TEMP_SENSOR_VOLTAGE - .424) * 160);
  TempData[10] = (int)((analogRead(A10)  / 1024.0 * TEMP_SENSOR_VOLTAGE - .424) * 160);
  TempData[11] = (int)((analogRead(A11)  / 1024.0 * TEMP_SENSOR_VOLTAGE - .424) * 160);
  TempData[12] = (int)((analogRead(A12)  / 1024.0 * TEMP_SENSOR_VOLTAGE - .424) * 160);
  TempData[13] = (int)((analogRead(A13)  / 1024.0 * TEMP_SENSOR_VOLTAGE - .424) * 160);
  TempData[14] = (int)((analogRead(A14)  / 1024.0 * TEMP_SENSOR_VOLTAGE - .424) * 160);
  TempData[15] = (int)((analogRead(A15)  / 1024.0 * TEMP_SENSOR_VOLTAGE - .424) * 160);
}

//======================================================================
//Function: WriteSerialData
//Inputs: txtInput
//Outputs: none
//Description: writes data to Arduino
//======================================================================
void WriteSerialData(String txtInput){ 
  char *strBiasCurrentStatus;
  char *strBiasCurrentOnTemp;
  char *strBiasCurrentOffTemp;
  char *strOvertemp;
  char toParse[64];

  //Parses the incoming data
  txtInput.toCharArray(toParse, 64);
  strBiasCurrentStatus = strtok(toParse, ",");
  strBiasCurrentOnTemp = strtok(NULL, ",");
  strBiasCurrentOffTemp = strtok(NULL, ",");
  strOvertemp = strtok(NULL, ",");

  //Stores the data into global variables and converts
  intBiasCurrentStatus = atoi(strBiasCurrentStatus);
  intBiasCurrentOnTemp = atoi(strBiasCurrentOnTemp);
  intBiasCurrentOffTemp = atoi(strBiasCurrentOffTemp);
  intOverloadTemp = atoi(strOvertemp);
}

//======================================================================
//Function: SendSerialData
//Inputs: none
//Outputs: none
//Description: sends serial data to the PC
//======================================================================
void SendSerialData(){ 
  int i;

  //Prints the temperature data
  for (i=0; i<16 ; i++){
    Serial.print(String(TempData[i]));
    Serial.print(",");
  }

  //Prints the target fan speeds
  for (i=0; i<8 ; i++){
    Serial.print(String(TargetFanPWMSpeeds[i]));
    Serial.print(",");
  }

  //Prints binary current bias status in Arduino
  Serial.print(String(intBiasCurrentStatus));
  Serial.print(",");

  //Prints last line uses Serial.println
  if (bolSmokeSensorLockout == false){
    //Sends a 0 for smoke sensor lockout (this means no smoke detected)
    Serial.println("0");
  }
  else{
    //Sends a 1 for smoke sensor lockout (this means smoke detected)
    Serial.println("1");
  }
 }

//======================================================================
//Function: ParseSerialData
//Inputs: none
//Outputs: none
//Description: Parses the serial data from the PC
//======================================================================
void ParseSerialData(String txtParse){
  //PC sent data packet to write to Arduino
  if (NextDataPacketIsWriteToArduinoData == true){
    //Resets the write data flag
    NextDataPacketIsWriteToArduinoData = false;

    //Parses the data from the PC
    WriteSerialData(txtParse);

    //Resets the heart beat counter
    intHeartBeatSeconds = 0;
  }
  
  //PC sent IDN
  if (txtParse == "IDN?"){
    //Sends back the IDN response
    Serial.println(IDN_RESPONSE); 
  }

  //PC sent the READ command
  if (txtParse ==  "READ_TO_PC"){
    //Sends back all the serial data
    SendSerialData();

    //Resets the heart beat counter
    intHeartBeatSeconds = 0;
  }

  //PC sent the WRITE_TO_ARDUINO command
  if (txtParse == "WRITE_TO_ARDUINO"){
    NextDataPacketIsWriteToArduinoData = true;

    //Resets the heart beat counter
    intHeartBeatSeconds = 0;
  }

  //PC sent the START command
  if (txtParse == "START"){
    //Resets the smoke sensor lockout
    bolSmokeSensorLockout = false;
  }

  //PC sent the "pause fans" command
  if (txtParse == "FAN_PAUSE"){
    //Sets the pause fan flag to true
    bolPauseFans = true;
  }

  //PC sent the "resume fans" command
  if (txtParse == "FAN_RESUME"){
    //Sets the pause fan flag to true
    bolPauseFans = false;
  }  
}


//======================================================================
//Function: ReceiveSerialData
//Inputs: none
//Outputs: none
//Description: receives data from PC. 
//======================================================================
void ReceiveSerialData(){ 
    char incomingByte = 0;
    
    //Reads the serial data
    if (Serial.available() > 0) {
      //Read the incoming byte:
      incomingByte = Serial.read();

      //Checks for new line
      if (incomingByte == TERMINATOR){
        //Parses the serial data since we got a complete line of text
        ParseSerialData(txtSerialBuffer);

        //Clears the serial buffer
        txtSerialBuffer = "";
      }
      else{
        //Adds the latest byte to the buffer if it's not a character we want to ignore (e.g. the extra terminator character)
        if (incomingByte != IGNORE_CHAR){
          txtSerialBuffer += incomingByte;
        }
      }
    }
}

//======================================================================
//Function: SetFanPWM
//Inputs: FanXPWM, a value between 0 to 100. 8 inputs total
//Outputs: none
//======================================================================
void SetFanPWM (int Fan1PWM, int Fan2PWM, int Fan3PWM, int Fan4PWM, int Fan5PWM, int Fan6PWM, int Fan7PWM, int Fan8PWM){
  //Performs saturation - 100%
  if (Fan1PWM > 100){Fan1PWM = 100;} 
  if (Fan2PWM > 100){Fan2PWM = 100;} 
  if (Fan3PWM > 100){Fan3PWM = 100;} 
  if (Fan4PWM > 100){Fan4PWM = 100;} 
  if (Fan5PWM > 100){Fan5PWM = 100;} 
  if (Fan6PWM > 100){Fan6PWM = 100;} 
  if (Fan7PWM > 100){Fan7PWM = 100;} 
  if (Fan8PWM > 100){Fan8PWM = 100;} 

  //Performs saturation - 0%
  if (Fan1PWM < 0){Fan1PWM = 0;} 
  if (Fan2PWM < 0){Fan2PWM = 0;} 
  if (Fan3PWM < 0){Fan3PWM = 0;} 
  if (Fan4PWM < 0){Fan4PWM = 0;} 
  if (Fan5PWM < 0){Fan5PWM = 0;} 
  if (Fan6PWM < 0){Fan6PWM = 0;} 
  if (Fan7PWM < 0){Fan7PWM = 0;} 
  if (Fan8PWM < 0){Fan8PWM = 0;} 

  
  //Sets the PWMs
  analogWrite(2, (int)((float)Fan1PWM/100*255));
  analogWrite(3, (int)((float)Fan2PWM/100*255));
  analogWrite(4, (int)((float)Fan3PWM/100*255));
  analogWrite(5, (int)((float)Fan4PWM/100*255));
  analogWrite(6, (int)((float)Fan5PWM/100*255));
  analogWrite(7, (int)((float)Fan6PWM/100*255));
  analogWrite(8, (int)((float)Fan7PWM/100*255));
  analogWrite(9, (int)((float)Fan8PWM/100*255));

  //Sets the target fan speed global variable
  TargetFanPWMSpeeds[0] = Fan1PWM;
  TargetFanPWMSpeeds[1] = Fan2PWM;
  TargetFanPWMSpeeds[2] = Fan3PWM;
  TargetFanPWMSpeeds[3] = Fan4PWM;
  TargetFanPWMSpeeds[4] = Fan5PWM;
  TargetFanPWMSpeeds[5] = Fan6PWM;
  TargetFanPWMSpeeds[6] = Fan7PWM;
  TargetFanPWMSpeeds[7] = Fan8PWM;
}


//======================================================================
//Function: CheckSmokeDetector
//Inputs: none
//Outputs: none
//Description: Checks the status of the smoke detector
//======================================================================

void CheckSmokeDetector(void){
  //Reads the input of the smoke detector status
  if (digitalRead(SMOKE_SENSOR_PIN) == true){
    //Smoke detected
    bolSmokeSensorLockout = true;
  }
}

//======================================================================
//Function: CalculatePWM
//Inputs: none
//Outputs: none
//Description: Calculates the PWM duty cycle of the fans
//======================================================================
void CalculatePWM(void){
  int Fan1PWM, Fan2PWM,Fan3PWM,Fan4PWM, Fan5PWM, Fan6PWM, Fan7PWM, Fan8PWM;
  
  //Checks to see if the bias current is on or not
  if (intBiasCurrentStatus == 0){
    //Regulates fans to no current temp
    Fan1PWM = ((new_max(TempData[0],TempData[1])) - intBiasCurrentOffTemp + 5)*10;
    Fan2PWM = ((new_max(TempData[2],TempData[3]))  - intBiasCurrentOffTemp + 5)*10;
    Fan3PWM = ((new_max(TempData[4],TempData[5]))  - intBiasCurrentOffTemp + 5)*10;
    Fan4PWM = ((new_max(TempData[6],TempData[7]))  - intBiasCurrentOffTemp + 5)*10;
    Fan5PWM = ((new_max(TempData[8],TempData[9]))  - intBiasCurrentOffTemp + 5)*10;
    Fan6PWM = ((new_max(TempData[10],TempData[11]))  - intBiasCurrentOffTemp + 5)*10;
    Fan7PWM = ((new_max(TempData[12],TempData[13]))  - intBiasCurrentOffTemp + 5)*10;
    Fan8PWM = ((new_max(TempData[14],TempData[15]))  - intBiasCurrentOffTemp + 5)*10;
  }
  else{
    Fan1PWM = ((new_max(TempData[0],TempData[1]))  - intBiasCurrentOnTemp + 5)*10;
    Fan2PWM = ((new_max(TempData[2],TempData[3]))  - intBiasCurrentOnTemp + 5)*10;
    Fan3PWM = ((new_max(TempData[4],TempData[5]))  - intBiasCurrentOnTemp + 5)*10;
    Fan4PWM = ((new_max(TempData[6],TempData[7]))  - intBiasCurrentOnTemp + 5)*10;
    Fan5PWM = ((new_max(TempData[8],TempData[9]))  - intBiasCurrentOnTemp + 5)*10;
    Fan6PWM = ((new_max(TempData[10],TempData[11]))  - intBiasCurrentOnTemp + 5)*10;
    Fan7PWM = ((new_max(TempData[12],TempData[13]))  - intBiasCurrentOnTemp + 5)*10;
    Fan8PWM = ((new_max(TempData[14],TempData[15]))  - intBiasCurrentOnTemp + 5)*10;
  }
  
  //Sets the PWM of all the fans
  if (bolPauseFans == false){
    SetFanPWM(Fan1PWM, Fan2PWM,Fan3PWM,Fan4PWM, Fan5PWM, Fan6PWM, Fan7PWM, Fan8PWM);
  }
  else{
    SetFanPWM(0,0,0,0,0,0,0,0);  
  }
  
}

//======================================================================
//Function: CheckOvertemp
//Inputs: none
//Outputs: none
//Description: Checks to see if we need to perform overtemp shutdown of power supply
//======================================================================
void CheckOvertemp(void){
  int i;
  bool bolOverTemp = false;

  //Loops through each temp sensor, looking to see if we hit over temp
  for (i = 0; i<16; i++){
    if (TempData[i] > intOverloadTemp){
      bolOverTemp = true;
    }
  }

  //Implements over temperature lockout
  if (bolOverTemp == true){
    //Sets the over temp trip to true
    bolOverTempTrip = true;
  }
  else{
    //Sets the over temp trip to false
    bolOverTempTrip = false;
  }
}

//======================================================================
//Function: CheckHeartBeat
//Inputs: none
//Outputs: returns false if failed, true if OK
//Description: implements heart beat
//======================================================================
bool CheckHeartBeat(void){
  //Increments the heart beat if we're not saturated
  if (intHeartBeatSeconds < 255) {
    intHeartBeatSeconds++;
  }

  //Checks to see if we have failed the heart beat
  if (intHeartBeatSeconds > MAX_SECONDS_BEFORE_FAIL_HEART_BEAT){
    //Sets fan to 100%
    SetFanPWM(100,100,100,100,100,100,100,100);

    //(Turns off power supply) Active low - write HIGH to turn off relay
    digitalWrite(RELAY_PIN, HIGH);

    //Returns a failed heart beat
    return false;
  }

  //Returns a good heart beat
  return true;
}

//======================================================================
//Function: PowerSupplyOutputEnableTrip
//Inputs: none
//Outputs: none
//Description: Checks to see if we need to trip the output enable on the TDK
//======================================================================
void PowerSupplyOutputEnableTrip(void){

  //Checks for smoke detector lockout - this If statement must happen first!!!!
  if (bolSmokeSensorLockout == true){
    //Active low - write HIGH to turn off relay
    digitalWrite(RELAY_PIN, HIGH);

    //Exits the function
    return;
  }

  //Checks for heartbeat
  if (bolHeartBeatGood == true){
    //Active low - writes LOW to turn on relay
    digitalWrite(RELAY_PIN, LOW);
  }
  else{
    //Active low - write HIGH to turn off relay
    digitalWrite(RELAY_PIN, HIGH);
  }

  //Checks for over temp trip
  if (bolOverTempTrip == true){
    //Active low - write HIGH to turn off relay
    digitalWrite(RELAY_PIN, HIGH);
  }
  else{
    //Active low - writes LOW to turn on relay
    digitalWrite(RELAY_PIN, LOW);
  }
}


//======================================================================
//Function: TimedLoop
//Description: This function runs at the specified timer interval
//======================================================================
void TimedLoop(){
  //Reads the temperature sensors
  ReadTemps();

  //Checks that we have a good heart beat
  bolHeartBeatGood = CheckHeartBeat();

  //Checks that we have a good heart beat
  if (bolHeartBeatGood == true){
    //Calculates the fan PWMs
    CalculatePWM();

    //Check to see if we need to shut down the power supply
    CheckOvertemp();

    //Checks the smoke sensor
    CheckSmokeDetector();
  }

  //Implements TDK Output Enable trips
  PowerSupplyOutputEnableTrip();

  //Pets watchdog
  wdt_reset(); 
}


//======================================================================
//Function: setup
//Description: this function runs once - use it to set up everything
//======================================================================
void setup() {
  //Timer that calls function TimedLoop every 1000000 uS (1 second).
  Timer1.initialize(1000000);
  Timer1.attachInterrupt(TimedLoop);
  
  //Sets up the serial at 57600
  Serial.begin(57600);
  
  //Sets up the relay pin
  pinMode(RELAY_PIN, OUTPUT);

  //Sets up the smoke detector pin as input and turns on the internal pull up
  pinMode (SMOKE_SENSOR_PIN, INPUT);
  digitalWrite(SMOKE_SENSOR_PIN, HIGH);
}

//======================================================================
//Function: loop
//Description: this function does an infinite loop at max speed
//======================================================================
void loop() {
  //Receives serial data
  ReceiveSerialData();
}
