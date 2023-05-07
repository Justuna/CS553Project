#!/bin/bash

ip link set bob up

ip addr add 8.8.8.8 dev bob

ip route add 7.7.7.7 via 8.8.8.8

PS1="${PS1}\[\e]2;Bob @ 8.8.8.8\a\]"
