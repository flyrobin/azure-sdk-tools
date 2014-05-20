﻿// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------
namespace Microsoft.WindowsAzure.Commands.Storage.File
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Management.Automation;
    using System.Net;
    using Microsoft.WindowsAzure.Storage;
    using System.IO;

    public abstract class StorageCmdletBase : PSCmdlet
    {
        private const string TraceWithTimestampFormat = "{0:T} - {1}";

        protected abstract void ExecuteCmdletInternal();

        protected void WriteVerboseWithTimestamp(string message, params object[] args)
        {
            string messageToWrite = args.Length == 0 ? message : string.Format(CultureInfo.CurrentCulture, message, args);
            this.WriteVerbose(string.Format(CultureInfo.CurrentCulture, TraceWithTimestampFormat, DateTime.Now, string.Format(message, args)));
        }

        protected void WriteDebugWithTimestamp(string message, params object[] args)
        {
            string messageToWrite = args.Length == 0 ? message : string.Format(CultureInfo.CurrentCulture, message, args);
            this.WriteDebug(string.Format(CultureInfo.CurrentCulture, TraceWithTimestampFormat, DateTime.Now, string.Format(message, args)));
        }

        /// <summary>
        /// Write error with category and identifier
        /// </summary>
        /// <param name="e">an exception object</param>
        protected void WriteExceptionError(Exception e)
        {
            Debug.Assert(e != null, Resources.ExceptionCannotEmpty);
            this.WriteError(this.BuildErrorRecordForException(e));
        }

        /// <summary>
        /// write terminating error
        /// </summary>
        /// <param name="e">exception object</param>
        protected void WriteTerminatingError(Exception e)
        {
            Debug.Assert(e != null, Resources.ExceptionCannotEmpty);
            ThrowTerminatingError(this.BuildErrorRecordForException(e));
        }

        protected override void ProcessRecord()
        {
            try
            {
                this.ExecuteCmdletInternal();
            }
            catch (Exception e)
            {
                this.WriteExceptionError(e);
            }
        }

        private static ErrorCategory SelectErrorCategoryFromHttpStatusCode(int httpStatusCode)
        {
            switch ((HttpStatusCode)httpStatusCode)
            {
                case HttpStatusCode.NotModified:
                case HttpStatusCode.Conflict:
                    return ErrorCategory.MetadataError;

                case HttpStatusCode.BadRequest:
                    return ErrorCategory.InvalidArgument;

                case HttpStatusCode.Unauthorized:
                case HttpStatusCode.ProxyAuthenticationRequired:
                    return ErrorCategory.AuthenticationError;

                case HttpStatusCode.Forbidden:
                    return ErrorCategory.PermissionDenied;

                case HttpStatusCode.NotImplemented:
                    return ErrorCategory.NotImplemented;

                case HttpStatusCode.RequestTimeout:
                case HttpStatusCode.GatewayTimeout:
                    return ErrorCategory.OperationTimeout;

                default:
                    return ErrorCategory.InvalidOperation;
            }
        }

        private static string TranslateStorageErrorCodeToErrorId(string errorCode, out string details)
        {
            switch (errorCode)
            {
                case "InvalidUri":
                case "OutOfRangeInput":
                case "UnsupportedHttpVerb":
                    details = Resources.InvalidResource;
                    return ErrorIdConstants.InvalidResource;

                default:
                    details = null;
                    return errorCode;
            }
        }

        private ErrorRecord BuildErrorRecordForException(Exception e)
        {
            if (e is AzureStorageFileException)
            {
                return ((AzureStorageFileException)e).GetErrorRecord();
            }
            else if (e is ArgumentException)
            {
                return new ErrorRecord(e, ErrorIdConstants.InvalidArgument, ErrorCategory.InvalidArgument, this);
            }
            else if (e is ItemNotFoundException || e is FileNotFoundException || e is DirectoryNotFoundException)
            {
                return new ErrorRecord(e, ErrorIdConstants.PathNotFound, ErrorCategory.ObjectNotFound, this);
            }
            else if (e is TimeoutException)
            {
                return new ErrorRecord(e, ErrorIdConstants.Timeout, ErrorCategory.OperationTimeout, this);
            }
            else if (e is StorageException)
            {
                var storageException = e as StorageException;
                Debug.Assert(storageException != null, "Strong cast should not return null because we just checked the type.");
                if (storageException.RequestInformation != null)
                {
                    // Select an error category using the given HTTP status code.
                    var category = SelectErrorCategoryFromHttpStatusCode(storageException.RequestInformation.HttpStatusCode);
                    string errorDetails = storageException.RequestInformation.HttpStatusMessage;
                    string errorId = null;

                    if (storageException.RequestInformation.ExtendedErrorInformation != null)
                    {
                        string details;
                        errorId = TranslateStorageErrorCodeToErrorId(storageException.RequestInformation.ExtendedErrorInformation.ErrorCode, out details);

                        // Overrides the error details with error message inside
                        // extended error information if avaliable.
                        errorDetails = details ?? storageException.RequestInformation.ExtendedErrorInformation.ErrorMessage;
                    }
                    else
                    {
                        // If available, try fetch the WebException from the inner exception
                        // to provide error id about network failure.
                        if (storageException.InnerException != null)
                        {
                            var webException = storageException.InnerException as WebException;
                            if (webException != null)
                            {
                                errorId = webException.Status.ToString();
                            }
                            else
                            {
                                var timeoutException = storageException.InnerException as TimeoutException;
                                if (timeoutException != null)
                                {
                                    errorDetails = timeoutException.Message;
                                    errorId = ErrorIdConstants.Timeout;
                                    category = ErrorCategory.OperationTimeout;
                                }
                            }
                        }
                        else
                        {
                            errorId = ErrorIdConstants.UnknownError;
                        }
                    }

                    var errorRecord = new ErrorRecord(e, errorId, category, this);
                    if (!string.IsNullOrWhiteSpace(errorDetails))
                    {
                        errorRecord.ErrorDetails = new ErrorDetails(errorDetails);
                    }

                    return errorRecord;
                }
            }

            return new ErrorRecord(e, e.GetType().Name, ErrorCategory.NotSpecified, this);
        }
    }
}
