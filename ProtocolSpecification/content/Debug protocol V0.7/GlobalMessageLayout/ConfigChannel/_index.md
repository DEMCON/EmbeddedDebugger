+++
title = "Config Channel ('C')"
date = 2018-10-31T15:55:25+01:00
weight = 5
+++
<table style="text-align: center;">
    <tr>
        <th></th>
        <th style="text-align: center; border-left: 1px solid black;">cmd-ID</th>
        <th style="text-align: center; border-left: 1px solid black;" colspan="10">cmd-data</th>
    </tr>
    <tr>
      <td> PC -> µC </td>
      <td> 'C' = 0x43 </td>
	  <td> chan </td>
      <td> [mode] </td>
	  <td> [off3] </td>
      <td> [off2] </td>
      <td> [off1] </td>
      <td> [off0] </td>
      <td> [ctrl] </td>
      <td> [size] </td>
    </tr>
    <tr>
      <td> PC <- µC </td>
      <td> 'C' = 0x43 </td>
	  <td> chan </td>
      <td> mode </td>
	  <td> [off3] </td>
      <td> [off2] </td>
      <td> [off1] </td>
      <td> [off0] </td>
      <td> [ctrl] </td>
      <td> [size] </td>
    </tr>
    <tr>
      <td> PC <- µC </td>
      <td> 'C' = 0x43 </td>
	  <td> chan </td>
      <td> mode </td>
    </tr>
</table>​

* chan: channel-number to config (range 0x00...0x0F)  
 if only channel is sent by PC, it means it wants to query the current config for this channel
* [mode]:  
 0x00 = turn channel off (don’t send any data anymore)  
 0x01 = send data when changed (max every sample)  
 0x02 = send data low-speed (typically once per sec)  
 0x03 = send the value-data once with next update or ‘once’ update  
 If only chan and mode are sent, we only change the mode, and only the mode is returned
* off3...off0: offset-address in bytes (4 Gbyte addressable)  
* ctrl: [see ctrl](../../#control-byte)
* size: number of bytes to read  
* If any of the settings is invalid (like offset or control), the µC replies with {mode off3…off0 ctrl size} all set to 0x00

overview of responses:

| PC -> µC                         | PC <- µC                         |
| -------------------------------- |--------------------------------- |
| chan mode off3…off0 ctrl size    | chan mode off3…off0 ctrl size    |
| chan                             | chan mode off3…off0 ctrl size    |
| chan mode                        | chan mode                        |


