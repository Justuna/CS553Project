#!/bin/bash

ip link set alice up

ip addr add 7.7.7.7 dev alice

ip route add 8.8.8.8 via 7.7.7.7

PS1="${PS1}\[\e]2;Alice @ 7.7.7.7\a\]"
