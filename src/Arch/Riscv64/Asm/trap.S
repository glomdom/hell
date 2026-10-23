// hell TrapFrame is defined like so:
//   [0..31] x0..x31 (0 - unused, 2 - sp at trap)
//   [32] sepc
//   [33] sstatus
//   [34] scause
//   [35] stval

#define FRAME_SIZE (36 * 8)

  .section .text
  .globl trap_entry
  .balign 4
trap_entry:
  addi sp, sp, -FRAME_SIZE
  
  sd x1, 1*8(sp)
  .irp n, 3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31
  sd x\n, \n*8(sp)
  .endr
  
  // original sp
  addi t0, sp, FRAME_SIZE
  sd t0, 2*8(sp)
  
  csrr t0, sepc
  sd t0, 32*8(sp)
  csrr t0, sstatus
  sd t0, 33*8(sp)
  csrr t0, scause
  sd t0, 34*8(sp)
  csrr t0, stval
  sd t0, 35*8(sp)
  
  mv a0, sp
  call trap_handler
  
  // in case sepc/sstatus is modified
  ld t0, 32*8(sp)
  csrw sepc, t0
  ld t0, 33*8(sp)
  csrw sstatus, t0
  
  ld x1, 1*8(sp)
  .irp n, 3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31
  ld x\n, \n*8(sp)
  .endr
  
  addi sp, sp, FRAME_SIZE
  sret