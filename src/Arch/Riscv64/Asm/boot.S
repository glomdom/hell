// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

  .section .text.boot, "ax"
  .globl _start
_start:
  csrw sie, zero

  li t0, 0x6000
  csrs sstatus, t0

  la sp, __stack_top

  la t0, __bss_start
  la t1, __bss_end
1:
  bgeu t0, t1, 2f
  sd zero, 0(t0)
  addi t0, t0, 8
  j 1b
2:
  la t0, trap_entry
  csrw stvec, t0

  call kmain
3:
  wfi
  j 3b

  .section .bss.stack, "aw", @nobits
  .balign 16
  .space 64 * 1024
  .globl __stack_top
__stack_top: