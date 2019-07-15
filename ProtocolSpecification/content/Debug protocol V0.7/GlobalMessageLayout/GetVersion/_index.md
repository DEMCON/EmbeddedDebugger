+++
title = "Get version ('V')"
date = 2018-10-31T15:55:25+01:00
weight = 2
+++
<table style="text-align: center;" class="table table-bordered">
    <tr>
        <th></th>
        <th style="text-align: center; border-left: 1px solid black;">cmd-ID</th>
        <th style="text-align: center; border-left: 1px solid black;" colspan="14">cmd-data</th>
    </tr>
    <tr>
      <td> PC -> uC </td>
      <td> 'V' = 0x56 </td>
      <td colspan="14">-</td>      
    </tr>
    <tr>
      <td> PC <- uC </td>
      <td> 'V' = 0x56 </td>
      <td> dv3 </td>
      <td> ... </td>
      <td> dv0 </td>
      <td> av3 </td>
      <td> ... </td>
      <td> av0 </td>
      <td> nmSize </td>
      <td> nmN </td>
      <td> ... </td>
      <td> nm0 </td>
      <td> snSize </td>
      <td> [snN] </td>
      <td> [...] </td>
      <td> [sn0] </td>
    </tr>
</table>​

*	dv3…dv0 = debug library/protocol version (dv1, dv0 = LSB, MSB)  
example: V2.3.1113 -> dv3 = 0x02, dv2 = 0x03, dv1 = 0x59, dv0 = 0x04
*	av3…av0 = application version (same format as dv3…dv0)
*	nmSize: number of bytes that follow for the name (nmN…nm0)  
nmN…nm0 = node-name (string which can have a variable length)
*	snSize: number of bytes that follow for the serial-nr (snN…sn0)  
snN…sn0 = serial number (string which can have a variable length)  
the serial number may be omitted