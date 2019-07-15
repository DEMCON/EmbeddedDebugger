+++
title = "Reset Time ('T')"
date = 2018-10-31T15:55:25+01:00
weight = 7
+++
<table style="text-align: center;">
    <tr>
        <th></th>
        <th style="text-align: center; border-left: 1px solid black;">cmd-ID</th>
        <th style="text-align: center; border-left: 1px solid black;" colspan="10">cmd-data</th>
    </tr>
    <tr>
      <td> PC -> µC </td>
      <td> 'T' = 0x54 </td>
	  <td> - </td>
    </tr>
    <tr>
      <td> PC <- µC </td>
      <td> 'T' = 0x54 </td>
	  <td> - </td>
    </tr>
</table>​

* reset the time that is used for generating time-stamps  
* by sending this command with a µC-ID of 0xFF, all slaves are reset simultaneously (approximately), and the time-stamps of all µCs will relate to each other