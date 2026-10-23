// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

  .section .text
  .globl write_satp
write_satp: // void WriteSatp(ulong value)
  csrw satp, a0
  sfence.vma zero, zero
  ret
  
  .section .text
  .globl write_satp
read_satp: // ulong ReadSatp()
  csrr a0, satp
  ret
 