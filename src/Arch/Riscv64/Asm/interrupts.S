// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

  .section .text
  .globl enable_interrupts
enable_interrupts:
  csrsi sstatus, (1 << 1)
  ret

  .section .text
  .globl enable_timer_interrupt
enable_timer_interrupt:
  li t0, (1 << 5)
  csrs sie, t0
  ret
 