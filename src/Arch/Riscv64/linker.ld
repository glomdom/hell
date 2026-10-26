/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

OUTPUT_ARCH(riscv)
ENTRY(_start)

PAGE_SIZE = 4096;

SECTIONS {
  . = 0x80200000;
  _kernel_start = .;

  .text : ALIGN(PAGE_SIZE) {
    _text_start = .;

    KEEP(*(.text.boot))
    *(.text .text.*)
    *(__managedcode)

    _text_end = .;
  }

  .rodata : ALIGN(PAGE_SIZE) {
    _rodata_start = .;

    *(.rodata .rodata.*)
    *(.srodata .srodata.*)
    *(.dotnet_eh_table)
    *(.eh_frame)
    KEEP(*(__modules))

    _rodata_end = .;
  }

  .data : ALIGN(PAGE_SIZE) {
    _data_start = .;

    *(.data .data.*)
    *(.data.rel.ro .data.rel.ro.*)
    *(.sdata .sdata.*)

    _data_end = .;
  }

  .bss (NOLOAD) : ALIGN(PAGE_SIZE) {
    _bss_start = .;
    __bss_start = .;

    *(.sbss .sbss.*)
    *(.bss .bss.*)
    *(COMMON)
    . = ALIGN(16);

    __bss_end = .;
    _bss_end = .;
  }

  . = ALIGN(PAGE_SIZE);
  _kernel_end = .;
}