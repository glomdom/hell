// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

  .section .text
  .globl sbi_call
sbi_call: # long sbi_call(eid, fid, a0, a1, a2)
  mv a7, a0
  mv a6, a1
  mv a0, a2
  mv a1, a3
  mv a2, a4
  ecall
  ret