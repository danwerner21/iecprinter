#include <Arduino.h>
#include "IECPrinter.h"

int8_t IECPrinter::canWrite()
{
  // Return -1 if we can't receive IEC bus data right now which will cause this
  // to be called again until we are ready and return 1.
  // Alternatively we could just wait within this function until we are ready.
  if(!online) return -1;
  return espWifi.canSendToConnection();
}


void IECPrinter::write(byte data)
{
  digitalWrite(BUSY_LED, 0);
  // write() will only be called if canWrite() returned >0.
  espWifi.outByteToConnection(data);
  digitalWrite(BUSY_LED, 1);
}

void IECPrinter::setStatus(bool status)
{
  online=status;
  if(online)
  {
    digitalWrite(ONLINE_LED, 0);
  }
  else
  {
      digitalWrite(ONLINE_LED, 1);
  }
}

int8_t IECPrinter::canRead()
{
  // Return 0 if we have nothing to send. This will indicate a "nothing to send"
  // (error) condition on the bus. If we returned -1 instead then canRead()
  // would be called repeatedly, blocking the bus, until we have something to send.
  // That would prevent us from receiving incoming data on the bus.
 // byte n = Serial.available();
 return 0;
  //return n>1 ? 2 : n;
}


byte IECPrinter::read()
{
  // read() will only be called if canRead() returned >0.
  return 0;
  //return Serial.read();
}
