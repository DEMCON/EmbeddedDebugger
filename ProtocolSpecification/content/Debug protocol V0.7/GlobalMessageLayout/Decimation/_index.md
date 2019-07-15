+++
title = "Decimation ('D')"
date = 2018-10-31T15:55:25+01:00
weight = 6
+++
<table style="text-align: center;">
    <tr>
        <th></th>
        <th style="text-align: center; border-left: 1px solid black;">cmd-ID</th>
        <th style="text-align: center; border-left: 1px solid black;" colspan="10">cmd-data</th>
    </tr>
    <tr>
      <td> PC -> µC </td>
      <td> 'D' = 0x44 </td>
	  <td> [dec] </td>
    </tr>
    <tr>
      <td> PC <- µC </td>
      <td> 'D' = 0x44 </td>
	  <td> dec </td>
    </tr>
</table>​

* set the decimation for generating channel-data  
* when [dec] is omitted, the current decimation is returned  