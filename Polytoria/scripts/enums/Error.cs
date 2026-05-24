// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Godot;
using Polytoria.Attributes;

namespace Polytoria.Enums;

/// <summary>
/// Error codes returned by various methods.
/// </summary>
[ScriptEnum]
public enum ErrorEnum
{
    /// <summary>
    /// Methods that return Error return OK when no error occurred.
    /// Since OK has value 0, and all other error constants are positive integers, 
    /// it can also be used in boolean checks.
    /// </summary>
    OK = 0,

    /// <summary>
    /// Generic error.
    /// </summary>
    FAILED = 1,

    /// <summary>
    /// Unavailable error.
    /// </summary>
    ERR_UNAVAILABLE = 2,

    /// <summary>
    /// Unconfigured error.
    /// </summary>
    ERR_UNCONFIGURED = 3,

    /// <summary>
    /// Unauthorized error.
    /// </summary>
    ERR_UNAUTHORIZED = 4,

    /// <summary>
    /// Parameter range error.
    /// </summary>
    ERR_PARAMETER_RANGE_ERROR = 5,

    /// <summary>
    /// Out of memory (OOM) error.
    /// </summary>
    ERR_OUT_OF_MEMORY = 6,

    /// <summary>
    /// File: Not found error.
    /// </summary>
    ERR_FILE_NOT_FOUND = 7,

    /// <summary>
    /// File: Bad drive error.
    /// </summary>
    ERR_FILE_BAD_DRIVE = 8,

    /// <summary>
    /// File: Bad path error.
    /// </summary>
    ERR_FILE_BAD_PATH = 9,

    /// <summary>
    /// File: No permission error.
    /// </summary>
    ERR_FILE_NO_PERMISSION = 10,

    /// <summary>
    /// File: Already in use error.
    /// </summary>
    ERR_FILE_ALREADY_IN_USE = 11,

    /// <summary>
    /// File: Can't open error.
    /// </summary>
    ERR_FILE_CANT_OPEN = 12,

    /// <summary>
    /// File: Can't write error.
    /// </summary>
    ERR_FILE_CANT_WRITE = 13,

    /// <summary>
    /// File: Can't read error.
    /// </summary>
    ERR_FILE_CANT_READ = 14,

    /// <summary>
    /// File: Unrecognized error.
    /// </summary>
    ERR_FILE_UNRECOGNIZED = 15,

    /// <summary>
    /// File: Corrupt error.
    /// </summary>
    ERR_FILE_CORRUPT = 16,

    /// <summary>
    /// File: Missing dependencies error.
    /// </summary>
    ERR_FILE_MISSING_DEPENDENCIES = 17,

    /// <summary>
    /// File: End of file (EOF) error.
    /// </summary>
    ERR_FILE_EOF = 18,

    /// <summary>
    /// Can't open error.
    /// </summary>
    ERR_CANT_OPEN = 19,

    /// <summary>
    /// Can't create error.
    /// </summary>
    ERR_CANT_CREATE = 20,

    /// <summary>
    /// Query failed error.
    /// </summary>
    ERR_QUERY_FAILED = 21,

    /// <summary>
    /// Already in use error.
    /// </summary>
    ERR_ALREADY_IN_USE = 22,

    /// <summary>
    /// Locked error.
    /// </summary>
    ERR_LOCKED = 23,

    /// <summary>
    /// Timeout error.
    /// </summary>
    ERR_TIMEOUT = 24,

    /// <summary>
    /// Can't connect error.
    /// </summary>
    ERR_CANT_CONNECT = 25,

    /// <summary>
    /// Can't resolve error.
    /// </summary>
    ERR_CANT_RESOLVE = 26,

    /// <summary>
    /// Connection error.
    /// </summary>
    ERR_CONNECTION_ERROR = 27,

    /// <summary>
    /// Can't acquire resource error.
    /// </summary>
    ERR_CANT_ACQUIRE_RESOURCE = 28,

    /// <summary>
    /// Can't fork process error.
    /// </summary>
    ERR_CANT_FORK = 29,

    /// <summary>
    /// Invalid data error.
    /// </summary>
    ERR_INVALID_DATA = 30,

    /// <summary>
    /// Invalid parameter error.
    /// </summary>
    ERR_INVALID_PARAMETER = 31,

    /// <summary>
    /// Already exists error.
    /// </summary>
    ERR_ALREADY_EXISTS = 32,

    /// <summary>
    /// Does not exist error.
    /// </summary>
    ERR_DOES_NOT_EXIST = 33,

    /// <summary>
    /// Database: Read error.
    /// </summary>
    ERR_DATABASE_CANT_READ = 34,

    /// <summary>
    /// Database: Write error.
    /// </summary>
    ERR_DATABASE_CANT_WRITE = 35,

    /// <summary>
    /// Compilation failed error.
    /// </summary>
    ERR_COMPILATION_FAILED = 36,

    /// <summary>
    /// Method not found error.
    /// </summary>
    ERR_METHOD_NOT_FOUND = 37,

    /// <summary>
    /// Linking failed error.
    /// </summary>
    ERR_LINK_FAILED = 38,

    /// <summary>
    /// Script failed error.
    /// </summary>
    ERR_SCRIPT_FAILED = 39,

    /// <summary>
    /// Cycling link (import cycle) error.
    /// </summary>
    ERR_CYCLIC_LINK = 40,

    /// <summary>
    /// Invalid declaration error.
    /// </summary>
    ERR_INVALID_DECLARATION = 41,

    /// <summary>
    /// Duplicate symbol error.
    /// </summary>
    ERR_DUPLICATE_SYMBOL = 42,

    /// <summary>
    /// Parse error.
    /// </summary>
    ERR_PARSE_ERROR = 43,

    /// <summary>
    /// Busy error.
    /// </summary>
    ERR_BUSY = 44,

    /// <summary>
    /// Skip error.
    /// </summary>
    ERR_SKIP = 45,

    /// <summary>
    /// Help error. Used internally when passing --version or --help as executable options.
    /// </summary>
    ERR_HELP = 46,

    /// <summary>
    /// Bug error, caused by an implementation issue in the method.
    /// If a built-in method returns this code, please open an issue on the GitHub Issue Tracker.
    /// </summary>
    ERR_BUG = 47,

    /// <summary>
    /// Printer on fire error (This is an easter egg, no built-in methods return this error code).
    /// </summary>
    ERR_PRINTER_ON_FIRE = 48
}