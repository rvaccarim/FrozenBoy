# FrozenBoy
A GameBoy emulator written in C# (.Net Core + Monogame).

This project started as a personal learning journey to understand emulation in general. If you are interested, you can read about it on my [blog](https://robertovaccari.com/blog/2020_09_26_gameboy/).

The emulator supports the original GameBoy only (the one with the green LCD screen). I've tested several games with good results, but there might still be lots of bugs and glitches.

### Requisites
- .Net 8.0 (C# 12)
- Monogame 3.8.1.303

### Missing Features
- Sound (I will probably never implement it as I find 8-bit music annoying).
- GameBoy Color support.
- Save/Restore state.
- MBCs other then MBC1.
- Full Serial Link emulation.

### Input
The JoyPad is mapped to the following keyboard keys:

```
Left Arrow
Right Arrow
Up Arrow
Down Arrow
A = A
S = B
Enter = Start
Space = Select
```

### Tests
The emulator passes the following tests. Just in case, some tests expect game ROMs to be present, but I can't distribute them.

- *Blargg*
    - CPU Instructions, 01-special.gb
    - CPU Instructions, 02-interrupts.gb
    - CPU Instructions, 03-op sp,hl.gb
    - CPU Instructions, 04-op r,imm.gb
    - CPU Instructions, 05-op rp.gb
    - CPU Instructions, 06-ld r,r.gb
    - CPU Instructions, 07-jr,jp,call,ret,rst.gb
    - CPU Instructions, 08-misc instrs.gb
    - CPU Instructions, 09-op r,r.gb
    - CPU Instructions, 10-bit ops.gb
    - CPU Instructions, 11-op a,(hl).gb
    - Instruction Timing, instr_timing.gb
    - Memory Timing, 01-read_timing.gb
    - Memory Timing, 02-write_timing.gb
    - Memory Timing, 03-modify_timing.gb
    - Memory Timing 2, 01-read_timing2.gb
    - Memory Timing 2, 02-write_timing2.gb
    - Memory Timing 2, 03-modify_timing2.gb
    - halt_bug.gb
    - OAM_Bug, 1-lcd_sync.gb
    - OAM_Bug, 2-causes.gb
    - OAM_Bug, 3-non_causes.gb

- *Mooneye*
    - bits,	mem_oam.gb
    - bits,	reg_f.gb
    - instr, daa.gb
    - oam_dma, basic.gb
    - oam_dma, reg_read.gb
    - timer, div_write.gb
    - timer, rapid_toggle.gb
    - timer, tim00.gb
    - timer, tim00_div_trigger.gb
    - timer, tim01.gb
    - timer, tim01_div_trigger.gb
    - timer, tim10.gb
    - timer, tim10_div_trigger.gb
    - timer, tim11.gb
    - timer, tim11_div_trigger.gb
    - timer, tima_reload.gb
    - timer, tima_write_reloading.gb
    - timer, tma_write_reloading.gb
    - root,	call_cc_timing.gb
    - root,	call_timing.gb
    - root,	div_timing.gb
    - root,	ei_sequence.gb
    - root,	ei_timing.gb
    - root,	halt_ime1_timing.gb
    - root,	if_ie_registers.gb
    - root,	intr_timing.gb
    - root,	jp_cc_timing.gb
    - root,	jp_timing.gb
    - root,	ld_hl_sp_e_timing.gb
    - root,	oam_dma_restart.gb
    - root,	oam_dma_start.gb
    - root,	oam_dma_timing.gb
    - root,	pop_timing.gb
    - root,	push_timing.gb
    - root,	rapid_di_ei.gb
    - root,	ret_cc_timing.gb
    - root,	ret_timing.gb
    - root,	reti_intr_timing.gb
    - root,	reti_timing.gb
    - MBC1, bits_bank1.gb	
    - MBC1, bits_bank2.gb	
    - MBC1, bits_mode.gb	
    - MBC1, bits_ramg.gb	
    - MBC1, multicart_rom_8Mb.gb
    - MBC1, ram_256kb.gb	
    - MBC1, ram_64kb.gb	
    - MBC1, rom_16Mb.gb	
    - MBC1, rom_1Mb.gb	
    - MBC1, rom_2Mb.gb	
    - MBC1, rom_4Mb.gb	
    - MBC1, rom_512kb.gb	
    - MBC1, rom_8Mb.gb	
