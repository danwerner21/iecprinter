#include <Arduino.h>
#include <Preferences.h>
#include "iecwifi.h"

void iecWifi::initialize()
{
        const char *host = IECHOST;
        const char *ssid = IECSSID;
        const char *password = IECPASSWORD;

        WiFi.mode(WIFI_STA);
        WiFi.setHostname(host);
        WiFi.begin(ssid, password);
        printf("Wifi Started\n");
        delay(10000);
        printf( "%sdB:%s\n", String(WiFi.RSSI()).substring(1).c_str(), WiFi.isConnected() ? WiFi.localIP().toString().c_str() : "None");
}

bool iecWifi::outByteToConnection(uint8_t b)
{
        if (!client.connected())
        {
                const char *server = IECSERVER;
                client.connect(server, IECPORT);
        }
        client.write(b);
        return true;
}

int8_t iecWifi::canSendToConnection()
{
        return 1;
}
