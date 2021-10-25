# MFIT_Client

This is a C# client that interacts with mFIT.
mFIT (Teensy) is configured as a HID device for the operating system.
This client works on Windows platforms, including in WSL.

The HID interface implementation is taken from the [hid-sharp](https://github.com/jcoenraadts/hid-sharp) client.

# Usage

Running the client without any arguments will show a brief explaination of the commands implemented.

Below we list a sub-set of tasks that mFIT achieves, for the complete list,
please run the client with no arguments.
The "external trigger" in the help message of the client refers to the signal
used as a trigger (i.e., ``SDA`` on the SMBus).

## Enable ALERTn signal

By default, when mFIT starts, the ``ALERTn`` signal is disabled (floating).
This is indicated by one of the LEDs (red) on the mFIT's logic controller board.

```sh
$ MFIT_Client.exe enable-alertn
```

## Suppress the auto-refresh (REF) for 100 milliseconds

The suppression will start somewhere in the near-future (right away).

```sh
$ MFIT_Client.exe noref 100m
```


## Finding out the firmware version of mFIT

```sh
$ MFIT_Client.exe get-version
mFIT_version=ver23
```
The above command timeouts if mFIT does not answer to the ``get-version`` command.
mFIT might not answer to HID commands, when it is busy (e.g., waiting for a trigger).

## Booting to update mode

```sh
$ MFIT_Client.exe update
```

Now mFIT (Teensy) is in a mode that allows firmware update.
Further notes on how to push the firmware update are [here](../../firmware).

## Using multiple devices

The client is capable of selecting between multiple devices by specifying the
``-S`` parameter (the serial number of the device).

In order to list all the devices available and to find out their serial number (`SN=...`),
use the ``-v`` flag and no other command.
