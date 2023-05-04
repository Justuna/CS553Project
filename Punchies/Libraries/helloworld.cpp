#include <iostream>
#include <string>
#include <thread>
#include "include/steam/steamnetworkingsockets.h"
#include "include/steam/isteamnetworkingutils.h"

int main() {
    std::cout << "Network Identity size: " << 
            sizeof(SteamNetworkingIdentity) << std::endl;
    std::cout << "Address size: " << 
            sizeof(SteamIPAddress_t) << std::endl;
    std::cout << "Status Info size: " << 
            sizeof(SteamNetConnectionStatusChangedCallback_t) << std::endl;
    std::cout << "Connection size: " << 
            sizeof(HSteamNetConnection) << std::endl;
    std::cout << "Connection Info size: " << 
            sizeof(SteamNetConnectionInfo_t) << std::endl;
    std::cout << "Connection State size:" << 
            sizeof(ESteamNetworkingConnectionState) << std::endl;
}