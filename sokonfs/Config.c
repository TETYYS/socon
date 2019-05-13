#include "Config.h"
#include "UnicodeUtils.h"

VOID FreeFsBlockConfig()
{
	PFS_BLOCK_CONFIG_ENTRY entry = FsBlockConfigHead;
	while (entry != NULL) {
		PFS_BLOCK_CONFIG_ENTRY saved = entry;
		ExFreePoolWithTag(entry->Filename.Buffer, 'FBCF');
		entry = saved->Next;
		ExFreePoolWithTag(saved, 'ECBP');
	}
}

NTSTATUS LoadFsBlockConfig()
{
	UNICODE_STRING name = RTL_CONSTANT_STRING(L"\\DosDevices\\C:\\WINDOWS\\System32\\config\\SOKONFS");//RTL_CONSTANT_STRING(L"\\SystemRoot\\System32\\config\\SOKONFS");
	OBJECT_ATTRIBUTES objAttr;
	HANDLE hFile;
	NTSTATUS ntstatus;
	IO_STATUS_BLOCK ioStatusBlock;
	LARGE_INTEGER byteOffset;
	CHAR fileBuffer[BUFFER_SIZE];

	FreeFsBlockConfig();

	InitializeObjectAttributes(&objAttr, &name, OBJ_CASE_INSENSITIVE | OBJ_KERNEL_HANDLE, NULL, NULL);

	// Do not try to perform any file operations at higher IRQL levels.
	// Instead, you may use a work item or a system worker thread to perform file operations.

	if (KeGetCurrentIrql() != PASSIVE_LEVEL)
		return STATUS_INVALID_DEVICE_STATE;

	ntstatus = ZwCreateFile(&hFile,
		GENERIC_READ,
		&objAttr,
		&ioStatusBlock,
		NULL,
		FILE_ATTRIBUTE_NORMAL,
		0,
		FILE_OPEN_IF,
		FILE_SYNCHRONOUS_IO_NONALERT,
		NULL,
		0);

	if (!NT_SUCCESS(ntstatus)) {
		DbgPrint("SOKONFS config open failed: %08x, %08x", ntstatus, ioStatusBlock.Status);
		return ntstatus;
	}
	if (ioStatusBlock.Information != FILE_OPENED) {
		ZwClose(hFile);
		return STATUS_SUCCESS;
	}

	byteOffset.QuadPart = 0;

	while (1) {
		ntstatus = ZwReadFile(
			hFile,
			NULL,
			NULL,
			NULL,
			&ioStatusBlock,
			fileBuffer,
			BUFFER_SIZE,
			&byteOffset,
			NULL);

		if (!NT_SUCCESS(ntstatus)) {
			ZwClose(hFile);
			return ntstatus;
		}

		PWSTR fileUnicode;
		ULONG unicodeBytes;

		/* UTF-8 to unicode */ {
			USHORT bytesRead = (USHORT)(ioStatusBlock.Information);
			ASSERT(bytesRead <= BUFFER_SIZE);

			if ((ntstatus = RtlUTF8ToUnicodeN(NULL, 0, &unicodeBytes, fileBuffer, bytesRead)) != STATUS_SUCCESS) {
				ZwClose(hFile);
				return ntstatus;
			}
			fileUnicode = ExAllocatePool(PagedPool, unicodeBytes);

			if (fileUnicode == NULL) {
				ZwClose(hFile);
				return STATUS_INSUFFICIENT_RESOURCES;
			}

			if ((ntstatus = RtlUTF8ToUnicodeN(fileUnicode, unicodeBytes, &unicodeBytes, fileBuffer, bytesRead)) != STATUS_SUCCESS) {
				ExFreePool(fileUnicode);
				ZwClose(hFile);
				return ntstatus;
			}
		}

		USHORT index = 0;
		LONG newIndex = 0;

		if (WStrIndexOfChar(L'\n', 0, fileUnicode, (USHORT)(unicodeBytes / sizeof(WCHAR))) == -1) {
			ExFreePool(fileUnicode);
			ZwClose(hFile);
			return STATUS_SUCCESS;
		}

		while ((newIndex = WStrIndexOfChar(L'\n', index, fileUnicode, (USHORT)(unicodeBytes / sizeof(WCHAR)))) != -1) {
			PFS_BLOCK_CONFIG_ENTRY newEntry = ExAllocatePoolWithTag(PagedPool, sizeof(FS_BLOCK_CONFIG_ENTRY), 'ECBP');

			if (newEntry == NULL) {
				ExFreePool(fileUnicode);
				ZwClose(hFile);
				return STATUS_INSUFFICIENT_RESOURCES;
			}

			if (newIndex - index < 2) {
				ExFreePool(newEntry);
				ExFreePool(fileUnicode);
				ZwClose(hFile);
				return STATUS_FILE_CORRUPT_ERROR;
			}

			newEntry->BlockMode = BlockModify;
			switch (fileUnicode[newIndex - 1]) {
				case '1':
					break;
				case '2':
					newEntry->BlockMode = BlockAccess;
					break;
				case '4':
					newEntry->BlockMode = BlockFilenameStartsWith;
					break;
				case '5':
					newEntry->BlockMode = BlockFilenameStartsWith | BlockModify;
					break;
				case '6':
					newEntry->BlockMode = BlockFilenameStartsWith | BlockAccess;
					break;
			}

			ULONG filenameLen = newIndex - 1 - index;
			PWSTR filename = ExAllocatePoolWithTag(PagedPool, (filenameLen + 1) * sizeof(WCHAR), 'FBCF');

			if (filename == NULL) {
				ExFreePool(newEntry);
				ExFreePool(fileUnicode);
				ZwClose(hFile);
				return STATUS_INSUFFICIENT_RESOURCES;
			}

			filename[filenameLen] = L'\0';
			RtlCopyMemory(filename, &(fileUnicode[index]), filenameLen * sizeof(WCHAR));
			RtlInitUnicodeString(&(newEntry->Filename), filename);

			newEntry->Next = NULL;

			PFS_BLOCK_CONFIG_ENTRY last = FsBlockConfigHead;
			if (last != NULL) {
				while (last->Next != NULL)
					last = last->Next;

				last->Next = newEntry;
			} else
				FsBlockConfigHead = newEntry;

			DbgPrint("Filter %wZ added (blockmode %d)\n", &(newEntry->Filename), newEntry->BlockMode);

			index = (USHORT)(newIndex + 1);

			if (index >= (USHORT)(unicodeBytes / sizeof(WCHAR)))
				break;
		}

		ExFreePool(fileUnicode);
		byteOffset.QuadPart += index;
	}

	ZwClose(hFile);

	return STATUS_SUCCESS;
}