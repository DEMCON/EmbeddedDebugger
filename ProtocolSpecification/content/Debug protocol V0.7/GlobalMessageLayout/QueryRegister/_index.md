+++
title = "Query Register ('Q')"
date = 2018-10-31T15:55:25+01:00
weight = 4
+++
<table style="text-align: center;">
    <tr>
        <th></th>
        <th style="text-align: center; border-left: 1px solid black;">cmd-ID</th>
        <th style="text-align: center; border-left: 1px solid black;" colspan="10">cmd-data</th>
    </tr>
    <tr>
      <td> PC -> uC </td>
      <td> 'Q' = 0x51 </td>
      <td> off3 </td>
      <td> off2 </td>
      <td> off1 </td>
      <td> off0 </td>
      <td> ctrl </td>
      <td> size </td>
	  <td colspan="4"> - </td>
    </tr>
    <tr>
      <td> PC <- uC </td>
      <td> 'Q' = 0x51 </td>
	  <td> off3 </td>
      <td> off2 </td>
      <td> off1 </td>
      <td> off0 </td>
      <td> ctrl </td>
      <td> size </td>
	  <td> d7 </td>
      <td> [d6] </td>
      <td> ... </td>
      <td> [d0] </td>
    </tr>
</table>​

* off3...off0: offset-address in bytes (4 Gbyte addressable)  
* ctrl: [see ctrl](../../#control-byte)
* size: number of bytes to read (also determines number of data-bytes that follow in the reply from µC -> PC)
if size = 0, there is an error in reading the specified register
* d7 ... d0: data-bytes, only number of bytes used are sent (indicated by size)  