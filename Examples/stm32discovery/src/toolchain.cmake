
# Cross compilation settings:
set(CMAKE_SYSTEM_NAME Generic)
set(CMAKE_SYSTEM_PROCESSOR arm)

# Define compiler:
set(CMAKE_ASM_COMPILER arm-none-eabi-gcc)
set(CMAKE_C_COMPILER arm-none-eabi-gcc)
set(CMAKE_CXX_COMPILER arm-none-eabi-g++)

# Compiler flags:
set(MCU "-mthumb -mcpu=cortex-m4 -mfpu=fpv4-sp-d16 -mfloat-abi=hard")
set(CMAKE_EXE_LINKER_FLAGS "${MCU} -specs=nosys.specs -specs=nano.specs -Wl,-cref,-u,Reset_Handler -Wl,-Map=memory_map.map -Wl,--gc-sections")
set(CMAKE_C_FLAGS "${MCU} -g3 -Wall -fdata-sections -ffunction-sections")

# Dark cmake voodoo:
set(CMAKE_ASM_COMPILER_ID GNU)
set(CMAKE_C_COMPILER_ID GNU)
set(CMAKE_CXX_COMPILER_ID GNU)

set(CMAKE_ASM_COMPILER_FORCED TRUE)
set(CMAKE_C_COMPILER_FORCED TRUE)
set(CMAKE_CXX_COMPILER_FORCED TRUE)
