# BluePill Example
This example uses the [BluePill](https://stm32-base.org/boards/STM32F103C8T6-Blue-Pill.html) development board, which contains the STM32F103C8.

Hardware requirements:
* BluePill containing the STM32F103C8
* ST-LINK V2
* (optional) USB -> serial adapter

Software requirements:
* Visual Studio code
* Configuration of Visual Studio code, to be able to build, flash and debug. An excellent tutorial can be found [here](https://github.com/damogranlabs/VS-Code-STM32-IDE)
* (optional) STM32CubeMX, to be able to quickly reconfigure the chip configuration

To get started:
1. Place the Embedded Debugger configuration (Configuration/*.xml) in the correct configurations folder on your machine, which should be "C:\ProgramData\EmbeddedDebugger\Configurations\Serial\BluePill\". This configuration can be adapted once you require other memory addresses to write to or read from.
2. Build the example project in Visual Studio code (Terminal -> Run Build Task)
3. Download the build onto the BluePill (Terminal -> Run Task -> CPU: Download and run)
4. Open the Embedded Debugger
5. Select Serial and press "Settings"
6. Select the correct COM-port, set baud rate to 115200, leave the rest as they are
7. Press "OK" and "Connect"
8. Under "PORTC(Write)" enter -1 (0xFFFF) and select another register to write the value
9. The Green LED on the BluePill should turn off

Success!!!