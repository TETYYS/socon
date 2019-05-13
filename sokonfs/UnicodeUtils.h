#pragma once

#include "fltKernel.h"

BOOLEAN UnicodeEndsWith(PCWSTR In, USHORT Len, PUNICODE_STRING Cmp);
BOOLEAN UnicodeStartsWith(PUNICODE_STRING a, PUNICODE_STRING b);
LONG WStrIndexOfChar(WCHAR In, USHORT Start, PWSTR Cmp, USHORT Len);
LONG UnicodeIndexOfChar(WCHAR In, PUNICODE_STRING Cmp);
BOOLEAN UnicodeContainsChar(WCHAR In, PUNICODE_STRING Cmp);