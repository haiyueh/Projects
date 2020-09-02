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
int intADCValues[16][4];

//ADC programmable gain amplifier
int intADCPGA[16] = {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1};

//ADC programmable gain amplifier constants
uint16_t intPGADefinition[6] = {GAIN_TWOTHIRDS, GAIN_ONE, GAIN_TWO, GAIN_FOUR, GAIN_EIGHT, GAIN_SIXTEEN};




//====================================================
//Function: setADCGain
//Input: ads - instance of the Adafruit ADC
//       intADCPGAIndex - index of the ADC (1 to 16)
//====================================================
void setADCGain(Adafruit_ADS1115 ads, int intADCPGAIndex){
  ads.setGain(intPGADefinition[intADCPGAIndex]
}

void setup(void) {
  
}

void loop(void){
  //Declarations
  int intBoardIndex = 0;
  
  //Loops through each I2C connection, retrieving ADC values for each connection
  for (intBoardIndex = 0; intBoardIndex < NUMBER_OF_BOARDS; intBoardIndex++){
    //Establishes connection to ADC for channels 1-4
    Adafruit_ADS1115 ads1(ADC_ADDRESS_CHANNELS_1_TO_4,intADCPin[intBoardIndex][0],intADCPin[intBoardIndex][1]); 

    //Sets the gain for the ADC
    ads1.setGain(intPGADefinition[intADCPGA[intBoardIndex*2]]);

    //Stores the ADC values
    intADCValues[intBoardIndex*2][0] = ads1.readADC_SingleEnded(0);
    intADCValues[intBoardIndex*2][1] = ads1.readADC_SingleEnded(1);
    intADCValues[intBoardIndex*2][2] = ads1.readADC_SingleEnded(2);
    intADCValues[intBoardIndex*2][3] = ads1.readADC_SingleEnded(3);
    
    
    
  }
  
}



//CLK,DA
//Adafruit_ADS1115 ads(0x4a,intADCPin[5][0],intADCPin[5][1]);  /* Use this for the 16-bit version */


//void setup(void) 
//{
//  Serial.begin(9600);
//  Serial.println("Hello!");
//  
//  Serial.println("Getting single-ended readings from AIN0..3");
//  Serial.println("ADC Range: +/- 6.144V (1 bit = 3mV/ADS1015, 0.1875mV/ADS1115)");
//  
//  // The ADC input range (or gain) can be changed via the following
//  // functions, but be careful never to exceed VDD +0.3V max, or to
//  // exceed the upper and lower limits if you adjust the input range!
//  // Setting these values incorrectly may destroy your ADC!
//  //                                                                ADS1015  ADS1115
//  //                                                                -------  -------
//  // ads.setGain(GAIN_TWOTHIRDS);  // 2/3x gain +/- 6.144V  1 bit = 3mV      0.1875mV (default)
//  ads.setGain(GAIN_ONE);        // 1x gain   +/- 4.096V  1 bit = 2mV      0.125mV
//  ads.begin();
//}
//
//void loop(void) 
//{
//  //Est
//
//  
//  int16_t adc0, adc1, adc2, adc3;
//  adc0 = ads.readADC_SingleEnded(0);
//  adc1 = ads.readADC_SingleEnded(1);
//  adc2 = ads.readADC_SingleEnded(2);
//  adc3 = ads.readADC_SingleEnded(3);
//  Serial.print("AIN0: "); Serial.println(adc0);
//  Serial.print("AIN1: "); Serial.println(adc1);
//  Serial.print("AIN2: "); Serial.println(adc2);
//  Serial.print("AIN3: "); Serial.println(adc3);
//  Serial.println(" ");
//  delay(1000);
//  
//}
