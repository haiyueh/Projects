//====================================================
//Includes
//====================================================

#include <SoftwareI2C.h>
#include <Adafruit_ADS1X15_Software_I2C.h>
#include "TimerOne.h"

//====================================================
//Definitions
//====================================================
#define NUMBER_OF_BOARDS 8
#define ADC_ADDRESS_CHANNELS_1_TO_4 0x4B
#define ADC_ADDRESS_CHANNELS_5_TO_8 0x4A
#define UART_BUFFER 256
#define TIMED_LOOP_DURATION_US 1000000

//====================================================
//Declarations
//====================================================

//ADC channels
int intADCPin[8][2] = {
  {37,36},
  {41,40},
  {45,44},
  {49,48},
  {21,20},
  {11,10},
  {7,6},
  {14,15}
};

//ADC values
int intADCValues[70];

//ADC programmable gain amplifier
int intADCPGA[64] = {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1};

//ADC programmable gain amplifier constants
uint16_t intPGADefinition[6] = {GAIN_TWOTHIRDS, GAIN_ONE, GAIN_TWO, GAIN_FOUR, GAIN_EIGHT, GAIN_SIXTEEN};

//UART
char chrPCData[UART_BUFFER];
boolean newData = false;
char startMarker = '<';
char endMarker = '>';
int intFlag = -255;

//============================================================================
//Function: void ParseDataFromPC(char chrSerialData[UART_BUFFER])
//Notes:  parses data from the PC
//============================================================================
void ParseDataFromPC(char chrSerialData[UART_BUFFER]){
  //Uses string tokenizer to parse the data from the PC (sample: <75.0,3.5,70,50,1,0,0>)
  intADCPGA[0] = atof(strtok(chrSerialData,","));  

  //Reads in all the gains for the PGA
  for (int i = 1; i < 64; i++){
    intADCPGA[i] = atof(strtok(NULL, ",")); 
  }

  //Reads the flag
  intFlag = atof(strtok(NULL, ","));

}


//============================================================================
//Function: void ReadDatafromPC()
//Notes:  reads data from the PC
//============================================================================
void ReadDatafromPC() {

    static boolean recvInProgress = false;
    static byte ndx = 0;
    char rc;

    //Reads the data from the serial port if there is data
    while (Serial.available() > 0 && newData == false) {
        //Gets a byte back from the serial port
        rc = Serial.read();

        //Checks to see if we are at a new line or in progress of constructing a line
        if (recvInProgress == true) {
            //Checks for end marker
            if (rc != endMarker) {
                chrPCData[ndx] = rc;
                ndx++;
                if (ndx >= UART_BUFFER) {
                    ndx = UART_BUFFER - 1;
                }
            }
            else {
                chrPCData[ndx] = '\0'; 
                recvInProgress = false;
                ndx = 0;

                //Parses the data
                ParseDataFromPC(chrPCData);
            }
        }
        else if (rc == startMarker) {
            recvInProgress = true;
        }
    }
}

//======================================================================
//Function: TimedLoop
//Description: This function runs at the specified timer interval
//======================================================================
void TimedLoop(){
  //Declarations
  int intBoardIndex = 0;
  
  //Loops through each I2C connection, retrieving ADC values for each connection
  for (intBoardIndex = 0; intBoardIndex < NUMBER_OF_BOARDS; intBoardIndex++){
    //Establishes connection to ADC for channels 1-4
    Adafruit_ADS1115 ads1(ADC_ADDRESS_CHANNELS_1_TO_4,intADCPin[intBoardIndex][0],intADCPin[intBoardIndex][1]); 
    ads1.begin();


    //Sets the gain and then reads the ADC values
    ads1.setGain(GAIN_TWOTHIRDS);
    ads1.readADC_SingleEnded(0);
    ads1.setGain(intPGADefinition[intADCPGA[intBoardIndex*8+0]]);
    intADCValues[intBoardIndex*8+0] = ads1.readADC_SingleEnded(0);
    
    ads1.setGain(GAIN_TWOTHIRDS);
    ads1.readADC_SingleEnded(1);
    ads1.setGain(intPGADefinition[intADCPGA[intBoardIndex*8+1]]);
    intADCValues[intBoardIndex*8+1] = ads1.readADC_SingleEnded(1);

    ads1.setGain(GAIN_TWOTHIRDS);
    ads1.readADC_SingleEnded(2);
    ads1.setGain(intPGADefinition[intADCPGA[intBoardIndex*8+2]]);
    intADCValues[intBoardIndex*8+2] = ads1.readADC_SingleEnded(2);

    ads1.setGain(GAIN_TWOTHIRDS);
    ads1.readADC_SingleEnded(3);
    ads1.setGain(intPGADefinition[intADCPGA[intBoardIndex*8+3]]);
    intADCValues[intBoardIndex*8+3] = ads1.readADC_SingleEnded(3);

    

    //Establishes connection to ADC for channels 5-8
    Adafruit_ADS1115 ads2(ADC_ADDRESS_CHANNELS_5_TO_8,intADCPin[intBoardIndex][0],intADCPin[intBoardIndex][1]); 
    ads2.begin();

    ads2.setGain(GAIN_TWOTHIRDS);
    ads2.readADC_SingleEnded(0);
    ads2.setGain(intPGADefinition[intADCPGA[intBoardIndex*8+4]]);
    intADCValues[intBoardIndex*8+4] = ads2.readADC_SingleEnded(0);

    ads2.setGain(GAIN_TWOTHIRDS);
    ads2.readADC_SingleEnded(1);
    ads2.setGain(intPGADefinition[intADCPGA[intBoardIndex*8+5]]);
    intADCValues[intBoardIndex*8+5] = ads2.readADC_SingleEnded(1);

    ads2.setGain(GAIN_TWOTHIRDS);
    ads2.readADC_SingleEnded(2);
    ads2.setGain(intPGADefinition[intADCPGA[intBoardIndex*8+6]]);
    intADCValues[intBoardIndex*8+6] = ads2.readADC_SingleEnded(2);

    ads2.setGain(GAIN_TWOTHIRDS);
    ads2.readADC_SingleEnded(3);
    ads2.setGain(intPGADefinition[intADCPGA[intBoardIndex*8+7]]);
    intADCValues[intBoardIndex*8+7] = ads2.readADC_SingleEnded(3);
  }

  //Prints out the data to the computer
  Serial.print("<");
  for (int i = 0; i < 64; i++){
    Serial.print(intADCValues[i]);
    Serial.print(",");
  }
  Serial.print(intFlag);
  Serial.println(">");
}


//====================================================
//Function: setup
//Input: none
//Output: none
//====================================================
void setup(void) {
  //Serial init
  Serial.begin(115200);

  //Timer that calls function TimedLoop every TIMED_LOOP_DURATION_US uS 
  Timer1.initialize(TIMED_LOOP_DURATION_US);
  Timer1.attachInterrupt(TimedLoop);

}

//====================================================
//Function: loop
//Input: none
//Output: none
//====================================================
void loop(void){

  //Recieve data from PC  
  ReadDatafromPC();
  
} 
