// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

  .globl RhpAssignRefRiscV64
RhpAssignRefRiscV64:
  sd t4, 0(t3)
  addi t3, t3, 8
  ret