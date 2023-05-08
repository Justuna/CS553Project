# CS553Project

Instructions to run Punchies!
Open a terminal window and run the following commands:
sudo apt install git
sudo apt-get update
sudo apt-get install -y dotnet-sdk-6.0

Then navigate to the Linux Godot Download page (https://godotengine.org/download/linux/) and download Godot Engine - .Net (the version with C# support)
Unzip the zip file and rename the file Godot_v4.0.2-stable_mono_linux.x86_64 to godot
Move the godot file and the GodotSharp folder into /usr/local/bin


git clone https://github.com/Justuna/CS553Project.git
sudo apt-get -y install iproute2

cd CS553Project/Punchies/Libraries/ValveSocketsC#
suco cp libc.musl-x86_64.so.1 /lib


Open 3 terminal windows
In the first two terminal windows execute:
unshare -f -n -r

Then run the scripts provided in Punchies: 
setup_master.sh in the third window
setup_alice.sh and setup_bob.sh in the first two windows

To run the evaluations:
godot


tc qdisc add dev {name} root netem {property} {value}