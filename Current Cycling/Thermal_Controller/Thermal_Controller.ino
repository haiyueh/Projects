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
#include <max6675.h>
#include <EEPROM.h>


//============================================================================
//Definitions
//============================================================================

//Smoke sensor ADC channel
#define ADC_SMOKE_0 A0
#define ADC_SMOKE_1 A1
#define ADC_SMOKE_2 A2
#define ADC_SMOKE_3 A3
#define ADC_SMOKE_4 A4
#define ADC_SMOKE_5 A5
#define ADC_SMOKE_6 A6
#define ADC_SMOKE_7 A7
#define ADC_MAX_VOLTAGE 5
#define ADD_MAX_VALUE 1023
#define SMOKE_MAX 8

//Smoke sensor digital channel
#define DIGITAL_SMOKE_0 38
#define DIGITAL_SMOKE_1 40
#define DIGITAL_SMOKE_2 42
#define DIGITAL_SMOKE_3 44
#define DIGITAL_SMOKE_4 46
#define DIGITAL_SMOKE_5 48
#define DIGITAL_SMOKE_6 47
#define DIGITAL_SMOKE_7 49

//Smoke sensor trip threshold
#define SMOKE_SENSOR_TRIP_LEVEL_ADDRESS 0
#define OVER_TEMP_LEVEL_ADDRESS 4

//Thermocouple conditioners
#define TC_MISO 50
#define TC_SCK 52
#define TC_CS0 30
#define TC_CS1 31
#define TC_CS2 32
#define TC_CS3 33
#define TC_CS4 34
#define TC_CS5 35
#define TC_CS6 36
#define TC_CS7 37
#define TC_CS8 22
#define TC_CS9 23
#define TC_CS10 24
#define TC_CS11 25
#define TC_CS12 26 
#define TC_CS13 27
#define TC_CS14 28 
#define TC_CS15 29
#define TC_MAX 16

//LEDs
#define POWER_LED 39
#define OVERTEMP_LED 43
#define SMOKE_ALARM_LED 41

//Misc
#define EEPROM_USED_ADDRESS 4093
#define EEPROM_USED_CHECKNUMBER 42
#define THERMAL_CONTROL_HEARTBEAT_LENGTH 5000
#define BAUD_RATE_MASTER 115200


//============================================================================
//Declarations
//============================================================================

//Master data arrays
float floTemp[16];
float floSmokeLevel[8];
bool bolSmokeTripped[8];

//Trip levels
float floSmokeSensorTripLevel = 3.0; 
float floOverTempDegC = 100; 

//Thermocouple conditioners
MAX6675 TC0(TC_SCK, TC_CS0, TC_MISO);
MAX6675 TC1(TC_SCK, TC_CS1, TC_MISO);
MAX6675 TC2(TC_SCK, TC_CS2, TC_MISO);
MAX6675 TC3(TC_SCK, TC_CS3, TC_MISO);
MAX6675 TC4(TC_SCK, TC_CS4, TC_MISO);
MAX6675 TC5(TC_SCK, TC_CS5, TC_MISO);
MAX6675 TC6(TC_SCK, TC_CS6, TC_MISO);
MAX6675 TC7(TC_SCK, TC_CS7, TC_MISO);
MAX6675 TC8(TC_SCK, TC_CS8, TC_MISO);
MAX6675 TC9(TC_SCK, TC_CS9, TC_MISO);
MAX6675 TC10(TC_SCK, TC_CS10, TC_MISO);
MAX6675 TC11(TC_SCK, TC_CS11, TC_MISO);
MAX6675 TC12(TC_SCK, TC_CS12, TC_MISO);
MAX6675 TC13(TC_SCK, TC_CS13, TC_MISO);
MAX6675 TC14(TC_SCK, TC_CS14, TC_MISO);
MAX6675 TC15(TC_SCK, TC_CS15, TC_MISO);


//============================================================================
//Function: void readEEPROM()
//Notes: reads the data from the EEPROM
//============================================================================
void readEEPROM(){
  //Checks to see if the EEPROM has been previously written to; if so, retrieve values
  if (EEPROM.read(EEPROM_USED_ADDRESS) == EEPROM_USED_CHECKNUMBER){
    //Retrieves the over temp value
    floOverTempDegC = EEPROM.read(OVER_TEMP_LEVEL_ADDRESS);
  
    //Retrieves the smoke sensor trip level
    floSmokeSensorTripLevel = EEPROM.read(SMOKE_SENSOR_TRIP_LEVEL_ADDRESS);
  }
}

//============================================================================
//Function: void writeEEPROM()
//Notes: writes the data to the EEPROM
//============================================================================
void writeEEPROM(){
  //Writes the data to the EEPROM
  EEPROM.write(OVER_TEMP_LEVEL_ADDRESS,floOverTempDegC);
  EEPROM.write(SMOKE_SENSOR_TRIP_LEVEL_ADDRESS,floSmokeSensorTripLevel);
  
  //Checks to see if the EEPROM has been previously written to; if so, write the value once
  if (EEPROM.read(EEPROM_USED_ADDRESS) != EEPROM_USED_CHECKNUMBER){
    EEPROM.write(EEPROM_USED_ADDRESS,EEPROM_USED_CHECKNUMBER);
  }
}

//============================================================================
//Function: bool isSmokeAlarmTripped()
//Notes: Polls the smoke sensor and returns a boolean for smoke sensor being tripped
//============================================================================
bool isSmokeAlarmTripped(){
  //Populates the analog voltage readout for the smoke sensor
  floSmokeLevel[0] = ((float)analogRead(ADC_SMOKE_0)/(float)ADD_MAX_VALUE)*(float)ADC_MAX_VOLTAGE;
  floSmokeLevel[1] = ((float)analogRead(ADC_SMOKE_1)/(float)ADD_MAX_VALUE)*(float)ADC_MAX_VOLTAGE;
  floSmokeLevel[2] = ((float)analogRead(ADC_SMOKE_2)/(float)ADD_MAX_VALUE)*(float)ADC_MAX_VOLTAGE;
  floSmokeLevel[3] = ((float)analogRead(ADC_SMOKE_3)/(float)ADD_MAX_VALUE)*(float)ADC_MAX_VOLTAGE;
  floSmokeLevel[4] = ((float)analogRead(ADC_SMOKE_4)/(float)ADD_MAX_VALUE)*(float)ADC_MAX_VOLTAGE;
  floSmokeLevel[5] = ((float)analogRead(ADC_SMOKE_5)/(float)ADD_MAX_VALUE)*(float)ADC_MAX_VOLTAGE;
  floSmokeLevel[6] = ((float)analogRead(ADC_SMOKE_6)/(float)ADD_MAX_VALUE)*(float)ADC_MAX_VOLTAGE;
  floSmokeLevel[7] = ((float)analogRead(ADC_SMOKE_7)/(float)ADD_MAX_VALUE)*(float)ADC_MAX_VOLTAGE;

  //Loops through the entire loop to see if any of the smoke sensors are tripped
  for (int i = 0; i < SMOKE_MAX; i++){
    if (floSmokeLevel[i] > floSmokeSensorTripLevel){
      //Returns alarm tripped
      return true;
    }
  }

  //Returns alarm not tripped
  return false;
}

//============================================================================
//Function: bool isOverTempAlarmTripped()
//Notes: Polls the temp sensors and see if the over temp alarm is tripped
//============================================================================
bool isOverTempAlarmTripped(){
  //Reads all the temperatures
  floTemp[0] = (float)TC0.readCelsius();
  floTemp[1] = (float)TC1.readCelsius();
  floTemp[2] = (float)TC2.readCelsius();
  floTemp[3] = (float)TC3.readCelsius();
  floTemp[4] = (float)TC4.readCelsius();
  floTemp[5] = (float)TC5.readCelsius();
  floTemp[6] = (float)TC6.readCelsius();
  floTemp[7] = (float)TC7.readCelsius();
  floTemp[8] = (float)TC8.readCelsius();
  floTemp[9] = (float)TC9.readCelsius();
  floTemp[10] = (float)TC10.readCelsius();
  floTemp[11] = (float)TC11.readCelsius();
  floTemp[12] = (float)TC12.readCelsius();
  floTemp[13] = (float)TC13.readCelsius();
  floTemp[14] = (float)TC14.readCelsius();
  floTemp[15] = (float)TC15.readCelsius();
  
  //Loops through the entire loop to see if any of the thermocouples are over temperatured
  for (int i = 0; i < TC_MAX; i++){
    if (floTemp[i] > floOverTempDegC){
      //Returns alarm tripped
      return true;
    }
  }

  //Returns alarm not tripped
  return false;
}


//============================================================================
//Function: void setup()
//Notes: code runs once
//============================================================================
void setup() {
  //Sets up the Serial port
  Serial2.begin(BAUD_RATE_MASTER);
  Serial2.setTimeout(THERMAL_CONTROL_HEARTBEAT_LENGTH); 

  //Sets the data direction
  pinMode(TC_SCK, OUTPUT);
  pinMode(TC_CS0, OUTPUT);
  pinMode(TC_CS1, OUTPUT);
  pinMode(TC_CS2, OUTPUT);
  pinMode(TC_CS3, OUTPUT);
  pinMode(TC_CS4, OUTPUT);
  pinMode(TC_CS5, OUTPUT);
  pinMode(TC_CS6, OUTPUT);
  pinMode(TC_CS7, OUTPUT);
  pinMode(TC_CS8, OUTPUT);
  pinMode(TC_CS9, OUTPUT);
  pinMode(TC_CS10, OUTPUT);
  pinMode(TC_CS11, OUTPUT);
  pinMode(TC_CS12, OUTPUT);
  pinMode(TC_CS13, OUTPUT);
  pinMode(TC_CS14, OUTPUT);
  pinMode(TC_CS15, OUTPUT);
  pinMode(DIGITAL_SMOKE_0, INPUT);
  pinMode(DIGITAL_SMOKE_1, INPUT);
  pinMode(DIGITAL_SMOKE_2, INPUT);
  pinMode(DIGITAL_SMOKE_3, INPUT);
  pinMode(DIGITAL_SMOKE_4, INPUT);
  pinMode(DIGITAL_SMOKE_5, INPUT);
  pinMode(DIGITAL_SMOKE_6, INPUT);
  pinMode(DIGITAL_SMOKE_7, INPUT);
  pinMode(POWER_LED, OUTPUT);
  pinMode(OVERTEMP_LED, OUTPUT);
  pinMode(SMOKE_ALARM_LED, OUTPUT);

  //Sets the power LED to on
  digitalWrite(POWER_LED, HIGH);

  //Reads the EEPROM
  readEEPROM();
}



//============================================================================
//Function: void loop()
//Notes: main loop, continually loops
//============================================================================
void loop() {
  //Declarations
  String strSerialData; 
  float floOverTempDegCNew = 100;
  float floSmokeSensorTripLevelNew = 3;
  boolean bolSmokeAlarm = false;
  boolean bolOverTempAlarm = false;

  //Reads the serial port
  strSerialData = Serial2.readStringUntil('\n');  

  //Checks to see if we got data back
  if (strSerialData.length() > 0){
    //Parses the data into the appropriate variables
    sscanf(strSerialData.c_str(),"%f,%f", &floOverTempDegCNew,&floSmokeSensorTripLevelNew);

    //Write the EEPROM if the data has changed
    if ((floOverTempDegCNew != floOverTempDegC) || (floSmokeSensorTripLevelNew != floSmokeSensorTripLevel)){
      writeEEPROM();
    }

    //Checks for smoke alarm (also populates smoke levels)
    bolSmokeAlarm =  isSmokeAlarmTripped();
    digitalWrite(SMOKE_ALARM_LED,bolSmokeAlarm);
  
    //Checks for over temp alarm (also populates temperature data)
    bolOverTempAlarm = isOverTempAlarmTripped();
    digitalWrite(OVERTEMP_LED,bolOverTempAlarm);

  
    //Sends data back to the master - temperatures
    for (int i = 0; i < TC_MAX; i++){
     Serial2.print(floTemp[i]);
     Serial2.print(",");
    }
    
    //Sends data back to the master - smoke levels
    for (int i = 0; i < SMOKE_MAX; i++){
     Serial2.print(floSmokeLevel[i]);
     Serial2.print(",");
    }

    //Sends data back to the master - trip states
    Serial2.print(bolSmokeAlarm);
    Serial2.print(",");
    Serial2.println(bolOverTempAlarm);
    //Serial2.println("&");
  }
  else{
    //Checks for smoke and over temp alarm anyhow
    digitalWrite(SMOKE_ALARM_LED, isSmokeAlarmTripped());
    digitalWrite(OVERTEMP_LED, isOverTempAlarmTripped());
  }
  
  
}
