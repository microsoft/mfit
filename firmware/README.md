# Intro

This is the firmware for mFIT that receives commands from a host PC, via the USB-HID interface.
It runs on the Teensy 3.6 board and manipulates the signals of the interposer.

# How to compile and deploy the firmware

The below instructions work in WSL.

## Clone ``--recursive`` and apply the core patch

`cd firmware/cores && patch -p1 --dry-run < ../patches/0001-teensy3-lib-comment-out-the-main-file.patch`

The purpose of the core patch is to remove the main function from the cores.
This allows us to provide our own main function.

## Download and install Arudino 1.8.9 Linux 64 Bit

You can download ArduinoStudio from
[here](https://www.arduino.cc/en/Main/OldSoftwareReleases) or from
[here](https://www.arduino.cc/download_handler.php?f=/arduino-1.8.9-linux64.tar.xz).


Create a folder `${PATH_TO_TEENSY_TOOLS}`.

`mdkir ${PATH_TO_TEENSY_TOOLS}`
`cd ${PATH_TO_TEENSY_TOOLS} && ls`
arduino-1.8.9-linux64.tar.xz
`tar xf arduino-1.8.9-linux64.tar.xz`


## Download and install Teensyduino (Version 1.53)

Unfortuntally, the TeensyDuino installer requires a GUI so it will not work by
default in WSL.
However, you can install [xming](https://sourceforge.net/projects/xming/) on
Windows to continue.

`wget https://www.pjrc.com/teensy/td_153/TeensyduinoInstall.linux64`
`export DISPLAY=:0`
`./TeensyduinoInstall.linux64`
Select the Arduino folder where you unpacked ArduinoStudio (i.e.,
${PATH_TO_TEENSY_TOOLS}/arduino-1.8.9/).


## Compile the firmware
In the root of the repository we can finally compile the firmware.

`TOOLSPATH=${PATH_TO_TEENSY_TOOLS}/arduino-1.8.9/hardware/tools/ make`

Your generated file should be `mfit-fw.hex`.
```
$ ls *.hex
mfit-fw.hex
```

## Deploying the new firmware

### Switch Teensy to programming mode

To reprogram Teensy, you will need to put mFIT in "programming mode".
You can use this command:
`${MFIT_CLIENT_PATH}/MFIT_Client.exe update`.
You can check a detailed usage of MFIT_Client [here](../client/csharp).

Alternatively, if you bricked mFIT (e.g., the `MFIT_client.exe` does not work at all)
or if this is the first time flashing the firmware, then you will need to
physically press the button on the Teensy board to switch to "programming mode".

### Flash the firmware (Windows)

Use the teensy client on Windows from
[here](https://www.pjrc.com/teensy/loader_win10.html) or from
[here](https://www.pjrc.com/teensy/teensy.exe).


## Flash the firmware (Linux)

You can compile the `teensy_loader_cli` from [here](https://github.com/PaulStoffregen/teensy_loader_cli).

`./teensy_loader_cli --mcu=mk66fx1m0 -w mfit-fw.hex`
