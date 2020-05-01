
This folder contains an example usage of the embedded debugger
target code on the STM32F407 discovery board.

# Building

    cmd> mkdir build
    cmd> cd build
    cmd> cmake -DCMAKE_TOOLCHAIN_FILE=src/toolchain.cmake -G Ninja ..
    cmd> ninja