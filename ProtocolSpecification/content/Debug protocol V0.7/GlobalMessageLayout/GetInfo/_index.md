+++
title = "Get Info ('I')"
date = 2018-10-31T15:55:25+01:00
weight = 1
+++

<table style="text-align: center;" class="table table-bordered">
    <tr>
        <th></th>
        <th style="text-align: center; border-left: 1px solid black;">cmd-ID</th>
        <th style="text-align: center; border-left: 1px solid black;" colspan="8">cmd-data</th>
    </tr>
    <tr>
      <td> PC -> uC </td>
      <td> 'I' = 0x49 </td>
      <td colspan="8" >-</td>      
    </tr>
    <tr>
      <td> PC <- uC </td>
      <td> 'I' = 0x49 </td>
      <td> type_0 </td>
      <td> size_0 </td>
      <td> [RS] </td>
      <td> ... </td>
      <td> [type_n] </td>
      <td> [size_n] </td>
    </tr>
</table>​

* type_n:  
0x00 = memory alignment (given in size_n; typically 1 or 4; 
example: memory alignment = 4 -> addresses are a multiple of 4)  
0x01 = pointer  
0x02 = bool  
0x03 = char  
0x04 = short  
0x05 = int  
0x06 = long  
0x07 = float  
0x08 = double  
0x09 = long double  
0x0A = time-stamp units in µs (uses 4 bytes for size_n!)  
*	size_n = size of ‘type denoted by type_x’ in bytes  
note: for type_n = 0x0A -> 4 bytes, LSB first, MSB last  
*	RS = record separator (0x33)  
*	by using a record separator, types can be added in the future, or omitted by the slave without problems  