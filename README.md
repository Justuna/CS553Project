# CS553Project

Instructions to run Punchies!

Open a terminal window and run the following commands:

`sudo apt install git`

`sudo apt-get update`

`sudo apt-get install -y dotnet-sdk-6.0`

-----

Then navigate to the Linux Godot Download page (https://godotengine.org/download/linux/) and download Godot Engine - .Net (the version with C# support)
Unzip the zip file and rename the file `Godot_v4.0.2-stable_mono_linux.x86_64` to `godot.`

Move the `godot` executable and the `GodotSharp` folder into `/usr/local/bin`

-----

Download the network tools:

`sudo apt-get -y install iproute2`

-----

Clone the repo, if you haven't already:

`git clone https://github.com/Justuna/CS553Project.git`

-----

Copy a dependency to the right directory:

`cd CS553Project/Punchies/Libraries/ValveSocketsC#`

`suco cp libc.musl-x86_64.so.1 /lib` **<--For some reason this shared library only worked for us if it was in that *specific* directory**

-----

Open 3 terminal windows. In the first two terminal windows, execute:

`unshare -f -n -r`

Then run the scripts provided in Punchies: 

`setup_master.sh` in the third window

`setup_alice.sh` and `setup_bob.sh` in the first two windows

-----

To run the evaluations:

`godot`

To open the godot editor (to swap out the library):

`godot -e`

-----

To play with network restrictions:

`tc qdisc add dev {name} root netem {property} {value}`

`tc qdisc show dev {name}`

`tc qdisc change dev {name} root netem {property} {newvalue}`

`tc qdisc del dev {name} root`
