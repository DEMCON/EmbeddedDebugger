+++
title = "Debug String ('S')"
date = 2018-10-31T15:55:25+01:00
weight = 9
+++
<table style="text-align: center;">
    <tr>
        <th></th>
        <th style="text-align: center; border-left: 1px solid black;">cmd-ID</th>
        <th style="text-align: center; border-left: 1px solid black;" colspan="3">cmd-data</th>
    </tr>
    <tr>
      <td> PC -> uC </td>
      <td> 'S' = 0x53 </td>
      <td> char_n </td>     
	  <td> ... </td>     
	  <td> char_0 </td>     
    </tr>
    <tr>
      <td> PC <- uC </td>
      <td> 'S' = 0x53 </td>
      <td> char_n </td>     
	  <td> ... </td>     
	  <td> char_0 </td>    
    </tr>
</table>​

* char_n…char_0: debug-string send by the µC to the PC, and vice-versa
* it acts like a stdout that the µC can print to (like a printf),
 and a stdin that the µC can react on (like a getchar)
 together, it acts like a ‘terminal’
