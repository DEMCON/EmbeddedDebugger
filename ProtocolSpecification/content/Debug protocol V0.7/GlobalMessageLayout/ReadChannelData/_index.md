+++
title = "Read Channel Data ('R')"
date = 2018-10-31T15:55:25+01:00
weight = 8
+++
<table style="text-align: center;">
    <tr>
        <th></th>
        <th style="text-align: center; border-left: 1px solid black;">cmd-ID</th>
        <th style="text-align: center; border-left: 1px solid black;" colspan="10">cmd-data</th>
    </tr>
    <tr>
      <td> PC -> uC </td>
      <td> 'R' = 0x52 </td>
      <td> trace </td>     
    </tr>
    <tr>
      <td> PC <- uC </td>
      <td> 'R' = 0x52 </td>
	  <td> time2 </td>
      <td> time1 </td>
      <td> time0 </td>
      <td> mask1 </td>
      <td> mask0 </td>
      <td> d_n </td>
	  <td> ... </td>
      <td> d_0 </td>
    </tr>
</table>​

* trace:  
 0 = turn off (µC stops sending channel data)  
 1 = turn on (µC starts send channel data continuously)  
 2 = once (µC sends debug-data once of channels that are configured as ‘once’)  
* time2…time0: 24-bit relative time-stamp; the unit of the time-stamp is given with the GetInfo command  
 with a usual time-stamp unit of 1ms, the relative time-stamp wraps each 4.6 hours which should be sufficient  
* mask1…mask0: 16-bit channel mask  
 if data contains values for a channel, its corresponding bit is set
* d_n…d_0: data of all channels that are sent  
 highest channels first  
 nbr bytes per channel are determined by the ConfigChannel command  
 MSB first for each value  
* not having separation characters between the values makes dispatching less trivial, but overhead is dramatically reduced
