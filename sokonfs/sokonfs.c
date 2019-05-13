#include "sokonfs.h"
#include "UnicodeUtils.h"
#include "Config.h"



NTSTATUS
sokonfsInstanceSetup (
    _In_ PCFLT_RELATED_OBJECTS FltObjects,
    _In_ FLT_INSTANCE_SETUP_FLAGS Flags,
    _In_ DEVICE_TYPE VolumeDeviceType,
    _In_ FLT_FILESYSTEM_TYPE VolumeFilesystemType
    )
/*++

Routine Description:

    This routine is called whenever a new instance is created on a volume. This
    gives us a chance to decide if we need to attach to this volume or not.

    If this routine is not defined in the registration structure, automatic
    instances are always created.

Arguments:

    FltObjects - Pointer to the FLT_RELATED_OBJECTS data structure containing
        opaque handles to this filter, instance and its associated volume.

    Flags - Flags describing the reason for this attach request.

Return Value:

    STATUS_SUCCESS - attach
    STATUS_FLT_DO_NOT_ATTACH - do not attach

--*/
{
    UNREFERENCED_PARAMETER( FltObjects );
    UNREFERENCED_PARAMETER( Flags );
    UNREFERENCED_PARAMETER( VolumeDeviceType );
    UNREFERENCED_PARAMETER( VolumeFilesystemType );

    PAGED_CODE();

    PT_DBG_PRINT( PTDBG_TRACE_ROUTINES,
                  "sokonfs!sokonfsInstanceSetup: Entered\n" );

    return STATUS_SUCCESS;
}


NTSTATUS
sokonfsInstanceQueryTeardown (
    _In_ PCFLT_RELATED_OBJECTS FltObjects,
    _In_ FLT_INSTANCE_QUERY_TEARDOWN_FLAGS Flags
    )
/*++

Routine Description:

    This is called when an instance is being manually deleted by a
    call to FltDetachVolume or FilterDetach thereby giving us a
    chance to fail that detach request.

    If this routine is not defined in the registration structure, explicit
    detach requests via FltDetachVolume or FilterDetach will always be
    failed.

Arguments:

    FltObjects - Pointer to the FLT_RELATED_OBJECTS data structure containing
        opaque handles to this filter, instance and its associated volume.

    Flags - Indicating where this detach request came from.

Return Value:

    Returns the status of this operation.

--*/
{
    UNREFERENCED_PARAMETER( FltObjects );
    UNREFERENCED_PARAMETER( Flags );

    PAGED_CODE();

    PT_DBG_PRINT( PTDBG_TRACE_ROUTINES,
                  "sokonfs!sokonfsInstanceQueryTeardown: Entered\n" );

    return STATUS_SUCCESS;
}


VOID
sokonfsInstanceTeardownStart (
    _In_ PCFLT_RELATED_OBJECTS FltObjects,
    _In_ FLT_INSTANCE_TEARDOWN_FLAGS Flags
    )
/*++

Routine Description:

    This routine is called at the start of instance teardown.

Arguments:

    FltObjects - Pointer to the FLT_RELATED_OBJECTS data structure containing
        opaque handles to this filter, instance and its associated volume.

    Flags - Reason why this instance is being deleted.

Return Value:

    None.

--*/
{
    UNREFERENCED_PARAMETER( FltObjects );
    UNREFERENCED_PARAMETER( Flags );

    PAGED_CODE();

    PT_DBG_PRINT( PTDBG_TRACE_ROUTINES,
                  "sokonfs!sokonfsInstanceTeardownStart: Entered\n" );
}


VOID
sokonfsInstanceTeardownComplete (
    _In_ PCFLT_RELATED_OBJECTS FltObjects,
    _In_ FLT_INSTANCE_TEARDOWN_FLAGS Flags
    )
/*++

Routine Description:

    This routine is called at the end of instance teardown.

Arguments:

    FltObjects - Pointer to the FLT_RELATED_OBJECTS data structure containing
        opaque handles to this filter, instance and its associated volume.

    Flags - Reason why this instance is being deleted.

Return Value:

    None.

--*/
{
    UNREFERENCED_PARAMETER( FltObjects );
    UNREFERENCED_PARAMETER( Flags );

    PAGED_CODE();

    PT_DBG_PRINT( PTDBG_TRACE_ROUTINES,
                  "sokonfs!sokonfsInstanceTeardownComplete: Entered\n" );
}


/*************************************************************************
    MiniFilter initialization and unload routines.
*************************************************************************/

NTSTATUS
DriverEntry (
    _In_ PDRIVER_OBJECT DriverObject,
    _In_ PUNICODE_STRING RegistryPath
    )
/*++

Routine Description:

    This is the initialization routine for this miniFilter driver.  This
    registers with FltMgr and initializes all global data structures.

Arguments:

    DriverObject - Pointer to driver object created by the system to
        represent this driver.

    RegistryPath - Unicode string identifying where the parameters for this
        driver are located in the registry.

Return Value:

    Routine can return non success error codes.

--*/
{
	FsBlockConfigHead = NULL;
    NTSTATUS status;
	
    UNREFERENCED_PARAMETER( RegistryPath );

    PT_DBG_PRINT( PTDBG_TRACE_ROUTINES,
                  "sokonfs!DriverEntry: Entered\n" );

    //
    //  Register with FltMgr to tell it our callback routines
    //

    status = FltRegisterFilter( DriverObject,
                                &FilterRegistration,
                                &gFilterHandle );

    FLT_ASSERT( NT_SUCCESS( status ) );

    if (NT_SUCCESS( status )) {

        //
        //  Start filtering i/o
        //

        status = FltStartFiltering( gFilterHandle );

        if (!NT_SUCCESS( status )) {

            FltUnregisterFilter( gFilterHandle );
        }
    }

	DbgBreakPoint();
	LoadFsBlockConfig();

    return status;
}

NTSTATUS
sokonfsUnload (
    _In_ FLT_FILTER_UNLOAD_FLAGS Flags
    )
/*++

Routine Description:

    This is the unload routine for this miniFilter driver. This is called
    when the minifilter is about to be unloaded. We can fail this unload
    request if this is not a mandatory unload indicated by the Flags
    parameter.

Arguments:

    Flags - Indicating if this is a mandatory unload.

Return Value:

    Returns STATUS_SUCCESS.

--*/
{
    UNREFERENCED_PARAMETER( Flags );

    PAGED_CODE();

    PT_DBG_PRINT( PTDBG_TRACE_ROUTINES,
                  "sokonfs!sokonfsUnload: Entered\n" );

    FltUnregisterFilter( gFilterHandle );

	FreeFsBlockConfig();

    return STATUS_SUCCESS;
}


/*************************************************************************
    MiniFilter callback routines.
*************************************************************************/
FLT_PREOP_CALLBACK_STATUS
sokonfsPreOperation (
    _Inout_ PFLT_CALLBACK_DATA Data,
    _In_ PCFLT_RELATED_OBJECTS FltObjects,
    _Flt_CompletionContext_Outptr_ PVOID *CompletionContext
    )
/*++

Routine Description:

    This routine is a pre-operation dispatch routine for this miniFilter.

    This is non-pageable because it could be called on the paging path

Arguments:

    Data - Pointer to the filter callbackData that is passed to us.

    FltObjects - Pointer to the FLT_RELATED_OBJECTS data structure containing
        opaque handles to this filter, instance, its associated volume and
        file object.

    CompletionContext - The context for the completion routine for this
        operation.

Return Value:

    The return value is the status of the operation.

--*/
{
    UNREFERENCED_PARAMETER( CompletionContext );
	NTSTATUS status;

    //PT_DBG_PRINT( PTDBG_TRACE_ROUTINES,
    //              "sokonfs!sokonfsPreOperation: Entered\n" );

    //
    //  See if this is an operation we would like the operation status
    //  for.  If so request it.
    //
    //  NOTE: most filters do NOT need to do this.  You only need to make
    //        this call if, for example, you need to know if the oplock was
    //        actually granted.
    //


	if (FsBlockConfigHead == NULL || FltObjects->FileObject == NULL)
		return FLT_PREOP_SUCCESS_WITH_CALLBACK;

	UCHAR fx = Data->Iopb->MajorFunction;
	if (!((fx >= IRP_MJ_CREATE && fx <= IRP_MJ_SET_EA) || fx == IRP_MJ_QUERY_SECURITY || fx == IRP_MJ_SET_SECURITY))
		return FLT_PREOP_SUCCESS_WITH_CALLBACK;

	PFLT_FILE_NAME_INFORMATION info = NULL;
	if (!NT_SUCCESS(status = FltGetFileNameInformation(Data, FLT_FILE_NAME_NORMALIZED | FLT_FILE_NAME_QUERY_FILESYSTEM_ONLY, &info))) {
		//DbgPrint("FltGetFileNameInformation failed with status %08x\n", status);
		//ExFreePoolWithTag(infoAlloc, 'INFF');
		return FLT_PREOP_SUCCESS_WITH_CALLBACK;
	}

	//DbgPrint("IN: FX %d -> %wZ !!\n", fx, &(info->Name));

	// = FltObjects->FileObject != NULL && UnicodeEndsWith(L"ProcessHacker.exe", 17, &FltObjects->FileObject->FileName);
	

	PFS_BLOCK_CONFIG_ENTRY entry = FsBlockConfigHead;
	while (entry->Next != NULL) {
		BOOLEAN block = FALSE;
		if (entry->BlockMode & BlockFilenameStartsWith)
			block = UnicodeStartsWith(&(entry->Filename), &(info->Name));
		else
			block = RtlCompareUnicodeString(&(entry->Filename), &(info->Name), TRUE) == 0;

		if (block) {
			if (entry->BlockMode & BlockModify) {
				if (fx == IRP_MJ_WRITE || fx == IRP_MJ_SET_INFORMATION || fx == IRP_MJ_SET_EA || fx == IRP_MJ_SET_SECURITY || (fx == IRP_MJ_CREATE && (FltObjects->FileObject->DeleteAccess || FltObjects->FileObject->DeletePending || FltObjects->FileObject->SharedDelete)))
					goto block;
			}
			if (entry->BlockMode & BlockAccess) {
				if (((fx >= IRP_MJ_CREATE && fx <= IRP_MJ_SET_EA) || fx == IRP_MJ_QUERY_SECURITY))
					goto block;
			}

			FltReleaseFileNameInformation(info);
			//ExFreePoolWithTag(infoAlloc, 'INFF');
			DbgPrint("filter %wZ found, but not blocked IRP_MJ %d\n", &(entry->Filename), fx);
			return FLT_PREOP_SUCCESS_WITH_CALLBACK;
		block:
			DbgPrint("%wZ blocked on filter %wZ blockmode %d\n", &(info->Name), &(entry->Filename), entry->BlockMode);
			Data->IoStatus.Status = STATUS_ACCESS_DENIED;
			FltReleaseFileNameInformation(info);
			//ExFreePoolWithTag(infoAlloc, 'INFF');
			return FLT_PREOP_COMPLETE;
		}
		entry = entry->Next;
	}
	FltReleaseFileNameInformation(info);
	//ExFreePoolWithTag(infoAlloc, 'INFF');

	/*if (ends) {
		DbgPrint("PROC %d\n", Data->Iopb->MajorFunction);
	}

	if (contains &&
		(FltObjects->FileObject->LockOperation ||
		FltObjects->FileObject->DeletePending ||
		FltObjects->FileObject->WriteAccess ||
		FltObjects->FileObject->DeleteAccess ||
		FltObjects->FileObject->SharedWrite ||
		FltObjects->FileObject->SharedDelete)) {
		DbgPrint("sokonfs!sokonfsPreOperation: Disallowed %d on %wZ\n", Data->Iopb->MajorFunction, FltObjects->FileObject->FileName);
		// LOOOOOOOOOOOOOOOOOOOOL
		Data->IoStatus.Status = STATUS_SUCCESS;
		return FLT_PREOP_COMPLETE;
	}*/

    /*if (sokonfsDoRequestOperationStatus(Data)) {
		status = FltRequestOperationStatusCallback(Data, sokonfsOperationStatusCallback, /*(PVOID)(++OperationStatusCtx)*NULL );
        if (!NT_SUCCESS(status)) {
            PT_DBG_PRINT(PTDBG_TRACE_OPERATION_STATUS, "sokonfs!sokonfsPreOperation: FltRequestOperationStatusCallback Failed, status=%08x\n", status);
        }
    }*/

    // This template code does not do anything with the callbackData, but
    // rather returns FLT_PREOP_SUCCESS_WITH_CALLBACK.
    // This passes the request down to the next miniFilter in the chain.

    return FLT_PREOP_SUCCESS_WITH_CALLBACK;
}



VOID
sokonfsOperationStatusCallback (
    _In_ PCFLT_RELATED_OBJECTS FltObjects,
    _In_ PFLT_IO_PARAMETER_BLOCK ParameterSnapshot,
    _In_ NTSTATUS OperationStatus,
    _In_ PVOID RequesterContext
    )
/*++

Routine Description:

    This routine is called when the given operation returns from the call
    to IoCallDriver.  This is useful for operations where STATUS_PENDING
    means the operation was successfully queued.  This is useful for OpLocks
    and directory change notification operations.

    This callback is called in the context of the originating thread and will
    never be called at DPC level.  The file object has been correctly
    referenced so that you can access it.  It will be automatically
    dereferenced upon return.

    This is non-pageable because it could be called on the paging path

Arguments:

    FltObjects - Pointer to the FLT_RELATED_OBJECTS data structure containing
        opaque handles to this filter, instance, its associated volume and
        file object.

    RequesterContext - The context for the completion routine for this
        operation.

    OperationStatus -

Return Value:

    The return value is the status of the operation.

--*/
{
    UNREFERENCED_PARAMETER( FltObjects );

    PT_DBG_PRINT( PTDBG_TRACE_ROUTINES,
                  "sokonfs!sokonfsOperationStatusCallback: Entered\n" );

    PT_DBG_PRINT( PTDBG_TRACE_OPERATION_STATUS,
                  "sokonfs!sokonfsOperationStatusCallback: Status=%08x ctx=%p IrpMj=%02x.%02x \"%s\"\n",
                   OperationStatus,
                   RequesterContext,
                   ParameterSnapshot->MajorFunction,
                   ParameterSnapshot->MinorFunction,
                   FltGetIrpName(ParameterSnapshot->MajorFunction) );
}


FLT_POSTOP_CALLBACK_STATUS
sokonfsPostOperation (
    _Inout_ PFLT_CALLBACK_DATA Data,
    _In_ PCFLT_RELATED_OBJECTS FltObjects,
    _In_opt_ PVOID CompletionContext,
    _In_ FLT_POST_OPERATION_FLAGS Flags
    )
/*++

Routine Description:

    This routine is the post-operation completion routine for this
    miniFilter.

    This is non-pageable because it may be called at DPC level.

Arguments:

    Data - Pointer to the filter callbackData that is passed to us.

    FltObjects - Pointer to the FLT_RELATED_OBJECTS data structure containing
        opaque handles to this filter, instance, its associated volume and
        file object.

    CompletionContext - The completion context set in the pre-operation routine.

    Flags - Denotes whether the completion is successful or is being drained.

Return Value:

    The return value is the status of the operation.

--*/
{
    UNREFERENCED_PARAMETER( Data );
    UNREFERENCED_PARAMETER( FltObjects );
    UNREFERENCED_PARAMETER( CompletionContext );
    UNREFERENCED_PARAMETER( Flags );

    //PT_DBG_PRINT( PTDBG_TRACE_ROUTINES,
    //              "sokonfs!sokonfsPostOperation: Entered\n" );

    return FLT_POSTOP_FINISHED_PROCESSING;
}


FLT_PREOP_CALLBACK_STATUS
sokonfsPreOperationNoPostOperation (
    _Inout_ PFLT_CALLBACK_DATA Data,
    _In_ PCFLT_RELATED_OBJECTS FltObjects,
    _Flt_CompletionContext_Outptr_ PVOID *CompletionContext
    )
/*++

Routine Description:

    This routine is a pre-operation dispatch routine for this miniFilter.

    This is non-pageable because it could be called on the paging path

Arguments:

    Data - Pointer to the filter callbackData that is passed to us.

    FltObjects - Pointer to the FLT_RELATED_OBJECTS data structure containing
        opaque handles to this filter, instance, its associated volume and
        file object.

    CompletionContext - The context for the completion routine for this
        operation.

Return Value:

    The return value is the status of the operation.

--*/
{
    UNREFERENCED_PARAMETER( Data );
    UNREFERENCED_PARAMETER( FltObjects );
    UNREFERENCED_PARAMETER( CompletionContext );

    PT_DBG_PRINT( PTDBG_TRACE_ROUTINES,
                  "sokonfs!sokonfsPreOperationNoPostOperation: Entered\n" );

    // This template code does not do anything with the callbackData, but
    // rather returns FLT_PREOP_SUCCESS_NO_CALLBACK.
    // This passes the request down to the next miniFilter in the chain.

    return FLT_PREOP_SUCCESS_NO_CALLBACK;
}


BOOLEAN
sokonfsDoRequestOperationStatus(
    _In_ PFLT_CALLBACK_DATA Data
    )
/*++

Routine Description:

    This identifies those operations we want the operation status for.  These
    are typically operations that return STATUS_PENDING as a normal completion
    status.

Arguments:

Return Value:

    TRUE - If we want the operation status
    FALSE - If we don't

--*/
{
    PFLT_IO_PARAMETER_BLOCK iopb = Data->Iopb;

    //
    //  return boolean state based on which operations we are interested in
    //

    return (BOOLEAN)

            //
            //  Check for oplock operations
            //

             (((iopb->MajorFunction == IRP_MJ_FILE_SYSTEM_CONTROL) &&
               ((iopb->Parameters.FileSystemControl.Common.FsControlCode == FSCTL_REQUEST_FILTER_OPLOCK)  ||
                (iopb->Parameters.FileSystemControl.Common.FsControlCode == FSCTL_REQUEST_BATCH_OPLOCK)   ||
                (iopb->Parameters.FileSystemControl.Common.FsControlCode == FSCTL_REQUEST_OPLOCK_LEVEL_1) ||
                (iopb->Parameters.FileSystemControl.Common.FsControlCode == FSCTL_REQUEST_OPLOCK_LEVEL_2)))

              ||

              //
              //    Check for directy change notification
              //

              ((iopb->MajorFunction == IRP_MJ_DIRECTORY_CONTROL) &&
               (iopb->MinorFunction == IRP_MN_NOTIFY_CHANGE_DIRECTORY))
             );
}
