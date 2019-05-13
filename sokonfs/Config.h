#pragma once

#include "fltKernel.h"

typedef enum _FS_BLOCK_MODE {
	BlockModify = 0x01,
	BlockAccess = 0x02,
	BlockFilenameStartsWith = 0x04
} FS_BLOCK_MODE;

typedef struct _FS_BLOCK_CONFIG_ENTRY {
	UNICODE_STRING Filename;
	FS_BLOCK_MODE BlockMode;
	struct _FS_BLOCK_CONFIG_ENTRY *Next;
} FS_BLOCK_CONFIG_ENTRY, *PFS_BLOCK_CONFIG_ENTRY;

PFS_BLOCK_CONFIG_ENTRY FsBlockConfigHead;

VOID FreeFsBlockConfig();
NTSTATUS LoadFsBlockConfig();

#define BUFFER_SIZE 512