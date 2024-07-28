#include <Arduino.h>
#include "IECPrinter.h"
#include <WiFi.h>
#include "iecwifi.h"



#define IEC_ADDRESS_SW 36
#define ONLINE_SW 39

iecWifi espWifi;

#define DEVICE_NUMBER 4

// IEC Serial Connections
#define PIN_ATN   16
#define PIN_DATA  17
#define PIN_CLK   18
#define PIN_RESET 25
//
// DATA - X       X - SRQ
//    RESET - X
//  CLK - X       X - GND
//            X - ATN
//



IECPrinter iecPrinter;
int iecaddr;

void setup()
{


  pinMode(BUSY_LED, OUTPUT); // busy led
  pinMode(ONLINE_LED, OUTPUT); // online led
  digitalWrite(BUSY_LED, 0);
  printf("Module Initializing\n");
  espWifi.initialize();
  digitalWrite(ONLINE_LED, 1);
  pinMode(IEC_ADDRESS_SW, INPUT);   // iec address
  pinMode(ONLINE_SW ,INPUT);   // online button

  if(digitalRead(IEC_ADDRESS_SW)==HIGH)
   {
    iecaddr=5;
   }
  else
   {
    iecaddr=4;
   }


	// set all digital pins in a defined state.
  iecPrinter.begin(iecaddr);
  iecPrinter.setStatus(true);
  iecPrinter.espWifi=espWifi;
  digitalWrite(BUSY_LED, 1);

} // setup



void loop()
{
  // handle IEC bus communication (this will call the read and write functions above)
  iecPrinter.task();
}