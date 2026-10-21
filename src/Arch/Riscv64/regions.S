// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

  .section .text
  .globl kernel_start
kernel_start:
  la a0, _kernel_start
  ret

  .globl kernel_end
kernel_end:
  la a0, _kernel_end
  ret