#include "IECDevice.h"
#include "iecwifi.h"

#define BUSY_LED   2
#define ONLINE_LED 33

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



class IECPrinter : public IECDevice
{
 public:
  IECPrinter() : IECDevice(PIN_ATN, PIN_CLK, PIN_DATA) {}
  iecWifi espWifi;
  virtual int8_t canRead();
  virtual byte   read();

  virtual int8_t canWrite();
  virtual void   write(byte data);
  virtual void  setStatus(bool status);

 private:
  bool online;
};
