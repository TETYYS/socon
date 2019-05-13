#include "UnicodeUtils.h"

BOOLEAN UnicodeEndsWith(PCWSTR In, USHORT Len, PUNICODE_STRING Cmp)
{
	if (Len > Cmp->Length)
		return FALSE;

	//DbgPrint("CMP %d vs %d\n", Cmp.Length, Len);

	PWCH cmp = Cmp->Buffer + (Cmp->Length / sizeof(*Cmp->Buffer)) - Len;

	for (USHORT x = 0; x < Len; x++) {
		if (cmp[x] != In[x])
			return FALSE;
	}

	return TRUE;
}

BOOLEAN UnicodeStartsWith(PUNICODE_STRING In, PUNICODE_STRING Cmp)
{
	if (Cmp->Length < In->Length)
		return FALSE;

	for (USHORT x = 0; x < (In->Length) / sizeof(WCHAR); x++) {
		if (In->Buffer[x] != Cmp->Buffer[x])
			return FALSE;
	}
	return TRUE;
}

LONG WStrIndexOfChar(WCHAR In, USHORT Start, PWSTR Cmp, USHORT Len)
{
	if (Start >= Len) {
		DbgBreakPoint();
		return -1;
	}

	for (USHORT x = Start; x < Len; x++) {
		if (Cmp[x] == In)
			return x;
	}

	return -1;
}

LONG UnicodeIndexOfChar(WCHAR In, PUNICODE_STRING Cmp)
{
	for (USHORT x = 0; x < (Cmp->Length / sizeof(*Cmp->Buffer)); x++) {
		if (Cmp->Buffer[x] == In)
			return x;
	}

	return -1;
}

BOOLEAN UnicodeContainsChar(WCHAR In, PUNICODE_STRING Cmp)
{
	return UnicodeIndexOfChar(In, Cmp) != -1;
}