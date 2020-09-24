//====================================================
//Includes
//====================================================
#include "TimerOne.h"

//====================================================
//Definitions
//====================================================
#define TIMED_LOOP_DURATION_US 10000
#define NUM_OF_RELAYS 16
#define ADDR1_INPUT 19
#define ADR0_INPUT 18
#define TIMER1_CLOCK_DIVIDER 100

//====================================================
//Declarations
//====================================================

//Relay to close
int intRelayToClose = 0;

//Current relay state
int intCurrentRelayState = 0

//Relay pin array
int intRelayIO[NUM_OF_RELAYS] = {17,16,15,14,13,2,3,4,5,6,7,8,9,10,11,12};

//Timer clock divider
int intClockDivider = 0;

//Address
int intAddress = 0;


//======================================================================
//Function: TimedLoop
//Description: This function runs at the specified timer interval
//======================================================================
void TimedLoop(){
  //Reads the address
  if ((digitalRead(ADR1_INPUT) == LOW) && (digitalRead(ADR1_INPUT) == LOW)){
    //Address 0
    intAddress = 0;
  }
  else if ((digitalRead(ADR1_INPUT) == LOW) && (digitalRead(ADR1_INPUT) == HIGH)){
    //Address 1
    intAddress = 1;
  }
  else if ((digitalRead(ADR1_INPUT) == HIGH) && (digitalRead(ADR1_INPUT) == LOW)){
    //Address 2
    intAddress = 2;
  }
  else if ((digitalRead(ADR1_INPUT) == HIGH) && (digitalRead(ADR1_INPUT) == HIGH)){
    //Address 3
    intAddress = 3;
  }
  
  
  //Checks if user commanded a different relay state
  if (intRelayToClose != intCurrentRelayState){
    //Opens all the relays
    for (int i = 0; i <NUM_OF_RELAYS; i++){
      //Turns off all the relays
      digitalWrite(intRelayIO[i],LOW);
    }

    //Closes the relay the user commanded
    if (intRelayToClose > 0){
      //Closes the appropriate realy
      digitalWrite(intRelayIO[intRelayToClose - 1],HIGH);

      //Sets the current relay state to the new relay that's closed
      intCurrentRelayState = intRelayToClose;
    }
  }

  //Checks to see if we have reached our clock divider value so that we send data to the PC
  if (intClockDivider >= TIMER1_CLOCK_DIVIDER){
    //Sends the data to the PC
    Serial.print("<");
    Serial.print("4WIRERELAY,");
    Serial.print(intAddress);
    Serial.print(",");
    Serial.print(intCurrentRelayState);
    Serial.println(">");
  }
  else {
    //Increments the clock divider
    intClockDivider++;
  }
  
}

//============================================================================
//Function: void ParseDataFromPC(char chrSerialData[UART_BUFFER])
//Notes:  parses data from the PC
//============================================================================
void ParseDataFromPC(char chrSerialData[UART_BUFFER]){
  //Uses string tokenizer to parse the data from the PC (sample: <75.0,3.5,70,50,1,0,0>)
  intRelayToClose= atof(strtok(chrSerialData,","));  
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


//====================================================
//Function: setup
//Input: none
//Output: none
//====================================================
void setup() {
  //Serial init
  Serial.begin(115200);

  //Sets the relay control pins to output
  for (int i = 0; i <NUM_OF_RELAYS; i++){
    //Sets all the pins to output
    pinMode(intRelayIO[i],OUTPUT);
  }

  //Sets the address pins as input
  pinMode(ADR0_INPUT,INPUT);
  pinMode(ADR1_INPUT,INPUT);
  
  //Timer that calls function TimedLoop every TIMED_LOOP_DURATION_US uS 
  Timer1.initialize(TIMED_LOOP_DURATION_US);
  Timer1.attachInterrupt(TimedLoop);
}

void loop() {
  //Recieve data from PC  
  ReadDatafromPC();
}
