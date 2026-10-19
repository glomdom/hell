/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

OUTPUT_ARCH(riscv)
ENTRY(_start)

SECTIONS {
  . = 0x80200000;

  .text : {
    KEEP(*(.text.boot))
    *(.text .text.*)
  }

  .rodata : ALIGN(16) {
    *(.rodata .rodata.*)
  }

  .data : ALIGN(16) {
    *(.data .data.*)
    *(.sdata .sdata.*)
  }

  .bss : ALIGN(16) {
    __bss_start = .;

    *(.sbss .sbss.*)
    *(.bss .bss.*)
    *(COMMON)
    . = ALIGN(16);

    __bss_end = .;
  }
}