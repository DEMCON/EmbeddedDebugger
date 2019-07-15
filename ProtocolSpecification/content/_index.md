+++
title = "Embedded Debugger"
date = 2018-10-31T15:55:25+01:00
weight = 5
+++

# Introduction
There are 4 areas at which embedded software needs debugging:

*	Hardware testing  
For testing specific hardware, we need to write hardware-drivers, through which we can test the hardware. Often we need special test-software that use these drivers.  

*	Software debugging  
The development environment is often sufficient for debugging the software. It sometimes also has utilities for standard hardware testing (like default I/O-ports of a µC).

*	Application debugging  
At application-level we even need more sophisticated debugging-tools. In general we need plots of certain internal units (like pressure, temperature, motor-positions, servo-errors, etc) that can be graphically plotted simultaneously.

*	Production testing  
Production testing is close related to hardware testing. Yet it requires a higher degree of automation with growing production volumes.

This document describes a general debug-protocol that can be used in all areas, being least applicable for software debugging. It needs implementation at both embedded side as PC-side. The debug-protocol can run over several types of busses, like CAN-bus, serial, Ethernet etc. It is a binary protocol with a message structure.
