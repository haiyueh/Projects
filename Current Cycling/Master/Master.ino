//============================================================================
//Notes
//============================================================================
// 1. Please set UART buffer to 256 before uploading. Refer to
//    http://www.hobbytronics.co.uk/arduino-serial-buffer-size
//    for reference. The files might be in a different location depending on
//    what version of Arduino you use. 


//============================================================================
//Includes
//============================================================================
#include "TimerOne.h"


//============================================================================
//Definitions
//============================================================================

//Fans
#define FAN1_CTRL 3
#define FAN2_CTRL 4
#define FAN3_CTRL 5
#define FAN4_CTRL 6
#define FAN5_CTRL 7
#define FAN6_CTRL 8
#define FAN7_CTRL 8
#define FAN8_CTRL 10

//LEDs
#define LED_POWER 30
#define LED_OVERTEMP 32
#define LED_CURRENT_ON 34
#define LED_CYCLE_ON 36
#define LED_COOLING 38
#define LED_SMOKE_ALARM 40


//EMO
#define EMO_SW 48

//Interlocks
#define POWER_SUPPLY_INTERLOCK 14
#define FAN_POWER_INTERLOCK 15

//Misc
#define PC_HEARTBEAT_LENGTH 5000
#define THERMAL_CONTROL_HEARTBEAT_LENGTH 5000
#define BAUD_RATE_THERMAL_CONTROLLER 115200
#define BAUD_RATE_PC 115200
#define TIMED_LOOP_DURATION_US 1000000
#define new_max(x,y) ((x) >= (y)) ? (x) : (y)
#define NUMBER_OF_SMOKE_SENSORS 8
#define NUMBER_OF_TEMP_SENSORS 16


//============================================================================
//Declarations
//============================================================================

//Target fan speed
int TargetFanPWMSpeeds[8];

//Master data arrays
float floTemp[NUMBER_OF_TEMP_SENSORS];
float floSmokeLevel[NUMBER_OF_SMOKE_SENSORS];
bool bolSmokeAlarmOn = false;
bool bolOverTempAlarmOn = false;
bool bolCurrentOn = false;
bool bolCycleOn = false;
bool bolCooling = false;
String strSmokeAlarm;
String strOverTempAlarm;
bool bolThermalControllerHeartBeatGood = false;

//Trip levels
float floSmokeSensorTripLevel = 3.0; 
float floOverTempDegC = 100; 

//Calculate PWM
int intBiasCurrentOnTemp = 85;
int intBiasCurrentOffTemp = 25;
int intBiasCurrentStatus = 0;
int intPauseFans = 0;

//Misc
int intStart = 0;

//============================================================================
//Function: void execEmergencyAction (bool isEmergency)
//Notes: executes actions based on whether there is an emergency detected
//============================================================================
void execEmergencyAction (bool bolIsEmergency){
  //Checks to see if there is an emergency
  if (bolIsEmergency == true){
    //Executes emergency action
    setPowerSupplyInterlock(false);
    setFanPowerInterlock(false);
    bolCurrentOn = false;
    bolCycleOn = false;
    bolCooling = false;
  }
}


//============================================================================
//Function: bool isEMOPressed()
//Notes: returns if the emergency power off is pressed
//============================================================================
bool isEMOPressed(){
  //Reads the EMO switch
  if (digitalRead(EMO_SW) == HIGH){
    //This means stop; EMO switch has been pressed
    return true; 
  }
  else{
    //This means run, nothing has been pressed
    return false;
    
  }
}

//============================================================================
//Function: void setPowerSupplyInterlock(bool bolEnablePowerSupply)
//Notes: sets the interlock for the power supplies
//============================================================================
void setPowerSupplyInterlock(bool bolEnablePowerSupply){
  //Turns the power supply interlock depending on the input
  digitalWrite(POWER_SUPPLY_INTERLOCK, bolEnablePowerSupply);
}

//============================================================================
//Function: void setFanPowerInterlock(bool bolFanPowerEnable)
//Notes: enable/disable the fan power
//============================================================================
void setFanPowerInterlock(bool bolFanPowerEnable){
  //Enables/disables the fan
  digitalWrite(FAN_POWER_INTERLOCK, bolFanPowerEnable);
}

//============================================================================
//Function: void SetFanPWM (int Fan1PWM, int Fan2PWM, int Fan3PWM, int Fan4PWM, int Fan5PWM, int Fan6PWM, int Fan7PWM, int Fan8PWM)
//Notes: FanXPWM, a value between 0 to 100. 8 inputs total
//============================================================================
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
  analogWrite(FAN1_CTRL, (int)((float)Fan1PWM/100*255));
  analogWrite(FAN2_CTRL, (int)((float)Fan2PWM/100*255));
  analogWrite(FAN3_CTRL, (int)((float)Fan3PWM/100*255));
  analogWrite(FAN4_CTRL, (int)((float)Fan4PWM/100*255));
  analogWrite(FAN5_CTRL, (int)((float)Fan5PWM/100*255));
  analogWrite(FAN6_CTRL, (int)((float)Fan6PWM/100*255));
  analogWrite(FAN7_CTRL, (int)((float)Fan7PWM/100*255));
  analogWrite(FAN8_CTRL, (int)((float)Fan8PWM/100*255));

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
    Fan1PWM = ((new_max(floTemp[0],floTemp[1])) - intBiasCurrentOffTemp + 5)*10;
    Fan2PWM = ((new_max(floTemp[2],floTemp[3]))  - intBiasCurrentOffTemp + 5)*10;
    Fan3PWM = ((new_max(floTemp[4],floTemp[5]))  - intBiasCurrentOffTemp + 5)*10;
    Fan4PWM = ((new_max(floTemp[6],floTemp[7]))  - intBiasCurrentOffTemp + 5)*10;
    Fan5PWM = ((new_max(floTemp[8],floTemp[9]))  - intBiasCurrentOffTemp + 5)*10;
    Fan6PWM = ((new_max(floTemp[10],floTemp[11]))  - intBiasCurrentOffTemp + 5)*10;
    Fan7PWM = ((new_max(floTemp[12],floTemp[13]))  - intBiasCurrentOffTemp + 5)*10;
    Fan8PWM = ((new_max(floTemp[14],floTemp[15]))  - intBiasCurrentOffTemp + 5)*10;
  }
  else{
    Fan1PWM = ((new_max(floTemp[0],floTemp[1]))  - intBiasCurrentOnTemp + 5)*10;
    Fan2PWM = ((new_max(floTemp[2],floTemp[3]))  - intBiasCurrentOnTemp + 5)*10;
    Fan3PWM = ((new_max(floTemp[4],floTemp[5]))  - intBiasCurrentOnTemp + 5)*10;
    Fan4PWM = ((new_max(floTemp[6],floTemp[7]))  - intBiasCurrentOnTemp + 5)*10;
    Fan5PWM = ((new_max(floTemp[8],floTemp[9]))  - intBiasCurrentOnTemp + 5)*10;
    Fan6PWM = ((new_max(floTemp[10],floTemp[11]))  - intBiasCurrentOnTemp + 5)*10;
    Fan7PWM = ((new_max(floTemp[12],floTemp[13]))  - intBiasCurrentOnTemp + 5)*10;
    Fan8PWM = ((new_max(floTemp[14],floTemp[15]))  - intBiasCurrentOnTemp + 5)*10;
  }
  
  //Sets the PWM of all the fans
  if (intPauseFans == 1){
    SetFanPWM(Fan1PWM, Fan2PWM,Fan3PWM,Fan4PWM, Fan5PWM, Fan6PWM, Fan7PWM, Fan8PWM);
  }
  else{
    SetFanPWM(0,0,0,0,0,0,0,0);  
  }
  
}


//============================================================================
//Function: void readWriteDataFromThermalController()
//Notes: reads and writes the data from the thermal controller
//============================================================================
void readWriteDataFromThermalController(){
  //Declarations
  String strSerialData;
  
  //Sends data to the thermal controller
  Serial2.print(floOverTempDegC);
  Serial2.print(",");
  Serial2.println(floSmokeSensorTripLevel);
  //Serial2.println("&");

   //Reads data back from the thermal controller
  strSerialData = Serial2.readStringUntil('\n');

  //Checks to see if we got data back
  if (strSerialData.length() > 0){
    //Heartbeat success
    bolThermalControllerHeartBeatGood = true;

   
    
    //Parses the data into the appropriate variables
    sscanf(strSerialData.c_str(),"%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%s,%s",
      &floTemp[0],&floTemp[1],&floTemp[2],&floTemp[3],&floTemp[4],&floTemp[5],&floTemp[6],&floTemp[7],&floTemp[8],&floTemp[9],&floTemp[10],&floTemp[11],&floTemp[12],&floTemp[13],&floTemp[14],&floTemp[15],
      &floSmokeLevel[0],&floSmokeLevel[1],&floSmokeLevel[2],&floSmokeLevel[3],&floSmokeLevel[4],&floSmokeLevel[5],&floSmokeLevel[6],&floSmokeLevel[7],&strSmokeAlarm,&strOverTempAlarm);

    }
  else {
    //Heartbeat fails - disables power supplies and fans
    bolThermalControllerHeartBeatGood = false;
    execEmergencyAction(true);
  }

 //Serial.println(floTemp[0]);
  
      
  
  //Converts alarms to booleans [TODO]
}

//============================================================================
//Function: void readDataFromPC(String strSerialPC)
//Notes: reads the data from the PC
//============================================================================
void readDataFromPC(String strSerialPC){
  //Parses the data into the appropriate variables [FIX THIS]
  sscanf(strSerialPC.c_str(),"%f,%f,%f,%f,%d,%d",floOverTempDegC,floSmokeSensorTripLevel,intBiasCurrentOnTemp,intBiasCurrentOffTemp,intBiasCurrentStatus,intPauseFans,intStart);

}


//============================================================================
//Function: void sendDataToPC ()
//Notes: sends data to the PC
//============================================================================
void sendDataToPC(){
  //Sends all the temperature data
  for (int i = 0; i < NUMBER_OF_TEMP_SENSORS; i++){
    Serial.print(floTemp[i]);
    Serial.print(",");
  }

  //Sends all the smoke data
  for (int i = 0; i < NUMBER_OF_SMOKE_SENSORS; i++){
    Serial.print(floSmokeLevel[i]);
    Serial.print(",");
  }

  //Sends all the booleans
  Serial.print(bolSmokeAlarmOn);
  Serial.print(",");
  Serial.print(bolOverTempAlarmOn);
  Serial.print(",");
  Serial.print(isEMOPressed());
  Serial.print(",");
  Serial.println(bolThermalControllerHeartBeatGood);
  
}

//============================================================================
//Function: void CheckAlarmTripped()
//Notes:  Checks to see if alarm is tripped (smoke or overtemp)
//============================================================================
bool CheckAlarmTripped(){
  //Checks for smoke alarm
  if (bolSmokeAlarmOn == true){
    //Disables the power supplies and fans
    setPowerSupplyInterlock(false);
    setFanPowerInterlock(false);
  }

  //Checks for over temperature alarm
  if (bolOverTempAlarmOn == true){
    //Disables the power supplies and fans
    setPowerSupplyInterlock(false);
    setFanPowerInterlock(true);
  }
}

//============================================================================
//Function: void UpdateLEDDisplay()
//Notes:  updates the LED display
//============================================================================
void UpdateLEDDisplay(){
  //Turns on the LEDs based on their state
  digitalWrite(LED_POWER,HIGH);
  digitalWrite(LED_OVERTEMP,bolOverTempAlarmOn);
  digitalWrite(LED_CURRENT_ON,bolCurrentOn);
  digitalWrite(LED_CYCLE_ON,bolCycleOn);
  digitalWrite(LED_COOLING,bolCooling);
  digitalWrite(LED_SMOKE_ALARM,bolSmokeAlarmOn);
}

//============================================================================
//Function: void InitLED()
//Notes:  tests the LEDs upon power up to make sure they all work good
//============================================================================
void InitLED(){
  //Cute little scrolly sequence
  digitalWrite(LED_POWER,HIGH);
  digitalWrite(LED_OVERTEMP,LOW);
  digitalWrite(LED_CURRENT_ON,LOW);
  digitalWrite(LED_CYCLE_ON,LOW);
  digitalWrite(LED_COOLING,LOW);
  digitalWrite(LED_SMOKE_ALARM,LOW);
  delay(200);
  digitalWrite(LED_OVERTEMP,HIGH);
  delay(200);
  digitalWrite(LED_CURRENT_ON,HIGH);
  delay(200);
  digitalWrite(LED_CYCLE_ON,HIGH);
  delay(200);
  digitalWrite(LED_COOLING,HIGH);
  delay(200);
  digitalWrite(LED_SMOKE_ALARM,HIGH);
  delay(1000);
  digitalWrite(LED_POWER,HIGH);
  digitalWrite(LED_OVERTEMP,LOW);
  digitalWrite(LED_CURRENT_ON,LOW);
  digitalWrite(LED_CYCLE_ON,LOW);
  digitalWrite(LED_COOLING,LOW);
  digitalWrite(LED_SMOKE_ALARM,LOW);
}

//======================================================================
//Function: TimedLoop
//Description: This function runs at the specified timer interval
//======================================================================
void TimedLoop(){
    //Reads and writes the data from the thermal controller
    readWriteDataFromThermalController();

    //Checks to see if E-Stop is pressed
    execEmergencyAction(isEMOPressed);

    //Turns on the LEDs based on their state
    UpdateLEDDisplay();    

    //Sends the data to the PC
    sendDataToPC();
}

//============================================================================
//Function: void setup()
//Notes: code runs once
//============================================================================
void setup() {
  //Serial ports
  Serial.begin(BAUD_RATE_PC);
  Serial2.begin(BAUD_RATE_THERMAL_CONTROLLER);
  Serial.setTimeout(PC_HEARTBEAT_LENGTH);
  Serial2.setTimeout(THERMAL_CONTROL_HEARTBEAT_LENGTH); 

  //Data direction
  pinMode(EMO_SW,INPUT);
  pinMode(POWER_SUPPLY_INTERLOCK,OUTPUT);
  pinMode(FAN_POWER_INTERLOCK,OUTPUT);
  pinMode(LED_POWER,OUTPUT);
  pinMode(LED_OVERTEMP,OUTPUT);
  pinMode(LED_CURRENT_ON,OUTPUT);
  pinMode(LED_CYCLE_ON,OUTPUT);
  pinMode(LED_COOLING,OUTPUT);
  pinMode(LED_SMOKE_ALARM,OUTPUT);

  //Initializes the LEDs
  InitLED();

  //Timer that calls function TimedLoop every TIMED_LOOP_DURATION_US uS 
  //Timer1.initialize(TIMED_LOOP_DURATION_US);
  //Timer1.attachInterrupt(TimedLoop);

}

//============================================================================
//Function: void loop()
//Notes: main loop, continually loops
//============================================================================
void loop() {
  //Declarations
  String strSerialPC;
  
  readWriteDataFromThermalController();
  sendDataToPC();
  delay(100);
  /*//Reads data from the PC
  strSerialPC = Serial.readStringUntil('\n');

  //Checks to see if we got data back
  if (strSerialPC.length() > 0){
    //Heartbeat success

    //Parses data from the PC
    readDataFromPC(strSerialPC);
    
    }
  else {
    //Heartbeat fails - disables power supplies and fans
    execEmergencyAction(true);
  }*/
}
