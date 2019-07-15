+++
title = "Write Register ('W')"
date = 2018-10-31T15:55:25+01:00
weight = 3
+++
<table style="text-align: center;">
    <tr>
        <th></th>
        <th style="text-align: center; border-left: 1px solid black;">cmd-ID</th>
        <th style="text-align: center; border-left: 1px solid black;" colspan="10">cmd-data</th>
    </tr>
    <tr>
      <td> PC -> µC </td>
      <td> 'W' = 0x57 </td>
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
    <tr>
      <td> PC <- µC </td>
      <td> 'W' = 0x57 </td>
      <td> result </td>
      <td colspan="9"> - </td>
    </tr>
</table>​

* off3...off0: offset-address in bytes (4 Gbyte addressable)  
* ctrl: [see ctrl](../../#control-byte)
* size: number of bytes to write (also determines number of data-bytes that follow)
* d7 ... d0: data-bytes, only number of bytes used are sent (indicated by size)  
* result:  
 0x00 = ok, value is written   
 0x01 = invalid (offset) address  
 0x02 = error dereferencing (null-pointer appeared at some dereference)  