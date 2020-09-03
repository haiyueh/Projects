//====================================================
//Includes
//====================================================

#include <SoftwareI2C.h>
#include <Adafruit_ADS1X15_Software_I2C.h>

//====================================================
//Definitions
//====================================================
#define NUMBER_OF_BOARDS 8
#define ADC_ADDRESS_CHANNELS_1_TO_4 0x4B
#define ADC_ADDRESS_CHANNELS_5_TO_8 0x4A

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


//====================================================
//Function: setup
//Input: none
//Output: none
//====================================================
void setup(void) {
  //DEBUG
  Serial.begin(115200);
}

//====================================================
//Function: loop
//Input: none
//Output: none
//====================================================
void loop(void){
  //Declarations
  int intBoardIndex = 0;
  
  //Loops through each I2C connection, retrieving ADC values for each connection
  for (intBoardIndex = 0; intBoardIndex < NUMBER_OF_BOARDS; intBoardIndex++){
    //Establishes connection to ADC for channels 1-4
    Adafruit_ADS1115 ads1(ADC_ADDRESS_CHANNELS_1_TO_4,intADCPin[intBoardIndex][0],intADCPin[intBoardIndex][1]); 
    ads1.begin();


    //Sets the gain and then reads the ADC values
    ads1.setGain(intPGADefinition[intADCPGA[intBoardIndex*8+0]]);
    intADCValues[intBoardIndex*8+0] = ads1.readADC_SingleEnded(0);
    
    ads1.setGain(intPGADefinition[intADCPGA[intBoardIndex*8+1]]);
    intADCValues[intBoardIndex*8+1] = ads1.readADC_SingleEnded(1);
    
    ads1.setGain(intPGADefinition[intADCPGA[intBoardIndex*8+2]]);
    intADCValues[intBoardIndex*8+2] = ads1.readADC_SingleEnded(2);
    
    ads1.setGain(intPGADefinition[intADCPGA[intBoardIndex*8+3]]);
    intADCValues[intBoardIndex*8+3] = ads1.readADC_SingleEnded(3);

    

    //Establishes connection to ADC for channels 5-8
    Adafruit_ADS1115 ads2(ADC_ADDRESS_CHANNELS_5_TO_8,intADCPin[intBoardIndex][0],intADCPin[intBoardIndex][1]); 
    ads2.begin();

    ads2.setGain(intPGADefinition[intADCPGA[intBoardIndex*8+4]]);
    intADCValues[intBoardIndex*8+4] = ads2.readADC_SingleEnded(0);
    
    ads2.setGain(intPGADefinition[intADCPGA[intBoardIndex*8+5]]);
    intADCValues[intBoardIndex*8+5] = ads2.readADC_SingleEnded(1);
    
    ads2.setGain(intPGADefinition[intADCPGA[intBoardIndex*8+6]]);
    intADCValues[intBoardIndex*8+6] = ads2.readADC_SingleEnded(2);
    
    ads2.setGain(intPGADefinition[intADCPGA[intBoardIndex*8+7]]);
    intADCValues[intBoardIndex*8+7] = ads2.readADC_SingleEnded(3);
  }

  //DEBUG
  for (int i = 0; i < 64; i++){
    Serial.print(intADCValues[i]);
    Serial.print(",");
  }
  Serial.println();
  //delay(100);
  
}
