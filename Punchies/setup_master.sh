#!/bin/bash

sudo ip link add alice netns $(ps -C unshare -o pid= | head -n 1) type veth peer bob netns $(ps -C unshare -o pid= | tail +2 | head -n 1)

PS1="${PS1}\[\e]2;Master\a\]"
