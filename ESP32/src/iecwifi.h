#ifndef IECWIFI_H
#define IECWIFI_H

#include <WiFi.h>
#include"secrets.h"


class iecWifi
{

public:
    void initialize();
    bool outByteToConnection(uint8_t b);
    int8_t canSendToConnection();

private:
    WiFiClient client;


};

#endif