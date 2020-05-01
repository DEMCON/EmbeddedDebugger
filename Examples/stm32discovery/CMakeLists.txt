cmake_minimum_required(VERSION 3.14)

project(demo C ASM)

add_subdirectory(../../TargetSide/src TargetSideLib)

add_executable(
    demo
    src/Src/main.c
    src/Src/gpio.c
    src/Src/i2s.c
    src/Src/usart.c
    src/startup_stm32f407xx.s
    src/Src/stm32f4xx_it.c
    src/Src/stm32f4xx_hal_msp.c
    src/Drivers/STM32F4xx_HAL_Driver/Src/stm32f4xx_hal_i2s.c
    src/Drivers/STM32F4xx_HAL_Driver/Src/stm32f4xx_hal_i2s_ex.c
    src/Drivers/STM32F4xx_HAL_Driver/Src/stm32f4xx_hal_rcc.c
    src/Drivers/STM32F4xx_HAL_Driver/Src/stm32f4xx_hal_rcc_ex.c
    src/Drivers/STM32F4xx_HAL_Driver/Src/stm32f4xx_hal_flash.c
    src/Drivers/STM32F4xx_HAL_Driver/Src/stm32f4xx_hal_flash_ex.c
    src/Drivers/STM32F4xx_HAL_Driver/Src/stm32f4xx_hal_flash_ramfunc.c
    src/Drivers/STM32F4xx_HAL_Driver/Src/stm32f4xx_hal_gpio.c
    src/Drivers/STM32F4xx_HAL_Driver/Src/stm32f4xx_hal_dma_ex.c
    src/Drivers/STM32F4xx_HAL_Driver/Src/stm32f4xx_hal_dma.c
    src/Drivers/STM32F4xx_HAL_Driver/Src/stm32f4xx_hal_pwr.c
    src/Drivers/STM32F4xx_HAL_Driver/Src/stm32f4xx_hal_pwr_ex.c
    src/Drivers/STM32F4xx_HAL_Driver/Src/stm32f4xx_hal_cortex.c
    src/Drivers/STM32F4xx_HAL_Driver/Src/stm32f4xx_hal.c
    src/Drivers/STM32F4xx_HAL_Driver/Src/stm32f4xx_hal_exti.c
    src/Drivers/STM32F4xx_HAL_Driver/Src/stm32f4xx_hal_tim.c
    src/Drivers/STM32F4xx_HAL_Driver/Src/stm32f4xx_hal_tim_ex.c
    src/Drivers/STM32F4xx_HAL_Driver/Src/stm32f4xx_hal_uart.c
    src/Src/system_stm32f4xx.c
    src/Src/stm32f4xx_hal_timebase_tim.c
)

target_compile_definitions(
    demo
    PRIVATE
    -DUSE_HAL_DRIVER
    -DSTM32F407xx
)

target_link_libraries(
    demo
    c
    m
    nosys
    embeddeddebugger
)

target_include_directories(
    demo
    PRIVATE
    src/Inc
    src/Drivers/STM32F4xx_HAL_Driver/Inc
    src/Drivers/STM32F4xx_HAL_Driver/Inc/Legacy
    src/Drivers/CMSIS/Device/ST/STM32F4xx/Include
    src/Drivers/CMSIS/Include
)

target_link_options(
  demo
  PRIVATE
  -T${CMAKE_SOURCE_DIR}/src/STM32F407VGTx_FLASH.ld
)
