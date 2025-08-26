namespace Sqlx.Postgres.Message.Backend.Information;

public enum SqlState
{
    SuccessfulCompletion,
    Warning,
    DynamicResultSetsReturned,
    ImplicitZeroBitPadding,
    NullValueEliminatedInSetFunction,
    PrivilegeNotGranted,
    PrivilegeNotRevoked,
    StringDataRightTruncation,
    DeprecatedFeature,
    NoData,
    NoAdditionalDynamicResultSetsReturned,
    SqlStatementNotYetComplete,
    ConnectionException,
    ConnectionDoesNotExist,
    ConnectionFailure,
    SqlClientUnableToEstablishSqlConnection,
    SqlServerRejectedEstablishmentOfSqlConnection,
    TransactionResolutionUnknown,
    ProtocolViolation,
    TriggeredActionException,
    FeatureNotSupported,
    InvalidTransactionInitiation,
    LocatorException,
    InvalidLocatorSpecification,
    InvalidGrantor,
    InvalidGrantOperation,
    InvalidRoleSpecification,
    DiagnosticsException,
    StackedDiagnosticsAccessedWithoutActiveHandler,
    CaseNotFound,
    CardinalityViolation,
    DataException,
    ArraySubscriptError,
    CharacterNotInRepertoire,
    DatetimeFieldOverflow,
    DivisionByZero,
    ErrorInAssignment,
    EscapeCharacterConflict,
    IndicatorOverflow,
    IntervalFieldOverflow,
    InvalidArgumentForLogarithm,
    InvalidArgumentForNtileFunction,
    InvalidArgumentForNthValueFunction,
    InvalidArgumentForPowerFunction,
    InvalidArgumentForWidthBucketFunction,
    InvalidCharacterValueForCast,
    InvalidDatetimeFormat,
    InvalidEscapeCharacter,
    InvalidEscapeOctet,
    InvalidEscapeSequence,
    NonstandardUseOfEscapeCharacter,
    InvalidIndicatorParameterValue,
    InvalidParameterValue,
    InvalidPrecedingOrFollowingSize,
    InvalidRegularExpression,
    InvalidRowCountInLimitClause,
    InvalidRowCountInResultOffsetClause,
    InvalidTableSampleArgument,
    InvalidTableSampleRepeat,
    InvalidTimeZoneDisplacementValue,
    InvalidUseOfEscapeCharacter,
    MostSpecificTypeMismatch,
    NullValueNotAllowed,
    NullValueNoIndicatorParameter,
    NumericValueOutOfRange,
    SequenceGeneratorLimitExceeded,
    StringDataLengthMismatch,
    StringDataRightTruncation2,
    SubstringError,
    TrimError,
    UnterminatedCString,
    ZeroLengthCharacterString,
    FloatingPointException,
    InvalidTextRepresentation,
    InvalidBinaryRepresentation,
    BadCopyFileFormat,
    UntranslatableCharacter,
    NotAnXmlDocument,
    InvalidXmlDocument,
    InvalidXmlContent,
    InvalidXmlComment,
    InvalidXmlProcessingInstruction,
    DuplicateJsonObjectKeyValue,
    InvalidArgumentForSqlJsonDatetimeFunction,
    InvalidJsonText,
    InvalidSqlJsonSubscript,
    MoreThanOneSqlJsonItem,
    NoSqlJsonItem,
    NonNumericSqlJsonItem,
    NonUniqueKeysInAJsonObject,
    SingletonSqlJsonItemRequired,
    SqlJsonArrayNotFound,
    SqlJsonMemberNotFound,
    SqlJsonNumberNotFound,
    SqlJsonObjectNotFound,
    TooManyJsonArrayElements,
    TooManyJsonObjectMembers,
    SqlJsonScalarRequired,
    SqlJsonItemCannotBeCastToTargetType,
    IntegrityConstraintViolation,
    RestrictViolation,
    NotNullViolation,
    ForeignKeyViolation,
    UniqueViolation,
    CheckViolation,
    ExclusionViolation,
    InvalidCursorState,
    InvalidTransactionState,
    ActiveSqlTransaction,
    BranchTransactionAlreadyActive,
    HeldCursorRequiresSameIsolationLevel,
    InappropriateAccessModeForBranchTransaction,
    InappropriateIsolationLevelForBranchTransaction,
    NoActiveSqlTransactionForBranchTransaction,
    ReadOnlySqlTransaction,
    SchemaAndDataStatementMixingNotSupported,
    NoActiveSqlTransaction,
    InFailedSqlTransaction,
    IdleInTransactionSessionTimeout,
    InvalidSqlStatementName,
    TriggeredDataChangeViolation,
    InvalidAuthorizationSpecification,
    InvalidPassword,
    DependentPrivilegeDescriptorsStillExist,
    DependentObjectsStillExist,
    InvalidTransactionTermination,
    SqlRoutineException,
    FunctionExecutedNoReturnStatement,
    ModifyingSqlDataNotPermitted,
    ProhibitedSqlStatementAttempted,
    ReadingSqlDataNotPermitted,
    InvalidCursorName,
    ExternalRoutineException,
    ContainingSqlNotPermitted,
    ModifyingSqlDataNotPermitted2,
    ProhibitedSqlStatementAttempted2,
    ReadingSqlDataNotPermitted2,
    ExternalRoutineInvocationException,
    InvalidSqlstateReturned,
    NullValueNotAllowed2,
    TriggerProtocolViolated,
    SrfProtocolViolated,
    EventTriggerProtocolViolated,
    SavepointException,
    InvalidSavepointSpecification,
    InvalidCatalogName,
    InvalidSchemaName,
    TransactionRollback,
    TransactionIntegrityConstraintViolation,
    SerializationFailure,
    StatementCompletionUnknown,
    DeadlockDetected,
    SyntaxErrorOrAccessRuleViolation,
    SyntaxError,
    InsufficientPrivilege,
    CannotCoerce,
    GroupingError,
    WindowingError,
    InvalidRecursion,
    InvalidForeignKey,
    InvalidName,
    NameTooLong,
    ReservedName,
    DatatypeMismatch,
    IndeterminateDatatype,
    CollationMismatch,
    IndeterminateCollation,
    WrongObjectType,
    GeneratedAlways,
    UndefinedColumn,
    UndefinedFunction,
    UndefinedTable,
    UndefinedParameter,
    UndefinedObject,
    DuplicateColumn,
    DuplicateCursor,
    DuplicateDatabase,
    DuplicateFunction,
    DuplicatePreparedStatement,
    DuplicateSchema,
    DuplicateTable,
    DuplicateAlias,
    DuplicateObject,
    AmbiguousColumn,
    AmbiguousFunction,
    AmbiguousParameter,
    AmbiguousAlias,
    InvalidColumnReference,
    InvalidColumnDefinition,
    InvalidCursorDefinition,
    InvalidDatabaseDefinition,
    InvalidFunctionDefinition,
    InvalidPreparedStatementDefinition,
    InvalidSchemaDefinition,
    InvalidTableDefinition,
    InvalidObjectDefinition,
    WithCheckOptionViolation,
    InsufficientResources,
    DiskFull,
    OutOfMemory,
    TooManyConnections,
    ConfigurationLimitExceeded,
    ProgramLimitExceeded,
    StatementTooComplex,
    TooManyColumns,
    TooManyArguments,
    ObjectNotInPrerequisiteState,
    ObjectInUse,
    CantChangeRuntimeParam,
    LockNotAvailable,
    UnsafeNewEnumValueUsage,
    OperatorIntervention,
    QueryCanceled,
    AdminShutdown,
    CrashShutdown,
    CannotConnectNow,
    DatabaseDropped,
    IdleSessionTimeout,
    SystemError,
    IoError,
    UndefinedFile,
    DuplicateFile,
    SnapshotTooOld,
}

public static class SqlStateExtensions
{
    private const string SuccessfulCompletion = "00000";
    private const string Warning = "01000";
    private const string DynamicResultSetsReturned = "0100C";
    private const string ImplicitZeroBitPadding = "01008";
    private const string NullValueEliminatedInSetFunction = "01003";
    private const string PrivilegeNoteGranted = "01007";
    private const string PrivilegeNotRevoked = "01006";
    private const string StringDataRightTruncation = "01004";
    private const string DeprecatedFeature = "01P01";
    private const string NoData = "02000";
    private const string NoAdditionalDynamicResultSetsReturned = "02001";
    private const string SqlStatementNotYetComplete = "03000";
    private const string ConnectionException = "08000";
    private const string ConnectionDoesNotExist = "08003";
    private const string ConnectionFailure = "08006";
    private const string SqlClientUnableToEstablishSqlConnection = "08001";
    private const string SqlServerRejectedEstablishmentOfSqlConnection = "08004";
    private const string TransactionResolutionUnknown = "08007";
    private const string ProtocolViolation = "08P01";
    private const string TriggeredActionException = "09000";
    private const string FeatureNotSupported = "0A000";
    private const string InvalidTransactionInitiation = "0B000";
    private const string LocatorException = "0F000";
    private const string InvalidLocatorSpecification = "0F001";
    private const string InvalidGrantor = "0L000";
    private const string InvalidGrantOperation = "0LP01";
    private const string InvalidRoleSpecification = "0P000";
    private const string DiagnosticsException = "0Z000";
    private const string StackedDiagnosticsAccessedWithoutActiveHandler = "0Z002";
    private const string CaseNotFound = "20000";
    private const string CardinalityViolation = "21000";
    private const string DataException = "22000";
    private const string ArraySubscriptError = "2202E";
    private const string CharacterNotInRepertoire = "22021";
    private const string DatetimeFieldOverflow = "22008";
    private const string DivisionByZero = "22012";
    private const string ErrorInAssignment = "22005";
    private const string EscapeCharacterConflict = "2200B";
    private const string IndicatorOverflow = "22022";
    private const string IntervalFieldOverflow = "22015";
    private const string InvalidArgumentForLogarithm = "2201E";
    private const string InvalidArgumentForNtileFunction = "22014";
    private const string InvalidArgumentForNthValueFunction = "22016";
    private const string InvalidArgumentForPowerFunction = "2201F";
    private const string InvalidArgumentForWidthBucketFunction = "2201G";
    private const string InvalidCharacterValueForCast = "22018";
    private const string InvalidDatetimeFormat = "22007";
    private const string InvalidEscapeCharacter = "22019";
    private const string InvalidEscapeOctet = "2200D";
    private const string InvalidEscapeSequence = "22025";
    private const string NonstandardUseOfEscapeCharacter = "22P06";
    private const string InvalidIndicatorParameterValue = "22010";
    private const string InvalidParameterValue = "22023";
    private const string InvalidPrecedingOrFollowingSize = "22013";
    private const string InvalidRegularExpression = "2201B";
    private const string InvalidRowCountInLimitClause = "2201W";
    private const string InvalidRowCountInResultOffsetClause = "2201X";
    private const string InvalidTableSampleArgument = "2202H";
    private const string InvalidTableSampleRepeat = "2202G";
    private const string InvalidTimeZoneDisplacementValue = "22009";
    private const string InvalidUseOfEscapeCharacter = "2200C";
    private const string MostSpecificTypeMismatch = "2200G";
    private const string NullValueNotAllowed = "22004";
    private const string NullValueNoIndicatorParameter = "22002";
    private const string NumericValueOutOfRange = "22003";
    private const string SequenceGeneratorLimitExceeded = "2200H";
    private const string StringDataLengthMismatch = "22026";
    private const string StringDataRightTruncation2 = "22001";
    private const string SubstringError = "22011";
    private const string TrimError = "22027";
    private const string UnterminatedCString = "22024";
    private const string ZeroLengthCharacterString = "2200F";
    private const string FloatingPointException = "22P01";
    private const string InvalidTextRepresentation = "22P02";
    private const string InvalidBinaryRepresentation = "22P03";
    private const string BadCopyFileFormat = "22P04";
    private const string UntranslatableCharacter = "22P05";
    private const string NotAnXmlDocument = "2200L";
    private const string InvalidXmlDocument = "2200M";
    private const string InvalidXmlContent = "2200N";
    private const string InvalidXmlComment = "2200S";
    private const string InvalidXmlProcessingInstruction = "2200T";
    private const string DuplicateJsonObjectKeyValue = "22030";
    private const string InvalidArgumentForSqlJsonDatetimeFunction = "22031";
    private const string InvalidJsonText = "22032";
    private const string InvalidSqlJsonSubscript = "22033";
    private const string MoreThanOneSqlJsonItem = "22034";
    private const string NoSqlJsonItem = "22035";
    private const string NonNumericSqlJsonItem = "22036";
    private const string NonUniqueKeysInAJsonObject = "22037";
    private const string SingletonSqlJsonItemRequired = "22038";
    private const string SqlJsonArrayNotFound = "22039";
    private const string SqlJsonMemberNotFound = "2203A";
    private const string SqlJsonNumberNotFound = "2203B";
    private const string SqlJsonObjectNotFound = "2203C";
    private const string TooManyJsonArrayElements = "2203D";
    private const string TooManyJsonObjectMembers = "2203E";
    private const string SqlJsonScalarRequired = "2203F";
    private const string SqlJsonItemCannotBeCastToTargetType = "2203G";
    private const string IntegrityConstraintViolation = "23000";
    private const string RestrictViolation = "23001";
    private const string NotNullViolation = "23502";
    private const string ForeignKeyViolation = "23503";
    private const string UniqueViolation = "23505";
    private const string CheckViolation = "23514";
    private const string ExclusionViolation = "23P01";
    private const string InvalidCursorState = "24000";
    private const string InvalidTransactionState = "25000";
    private const string ActiveSqlTransaction = "25001";
    private const string BranchTransactionAlreadyActive = "25002";
    private const string HeldCursorRequiresSameIsolationLevel = "25008";
    private const string InappropriateAccessModeForBranchTransaction = "25003";
    private const string InappropriateIsolationLevelForBranchTransaction = "25004";
    private const string NoActiveSqlTransactionForBranchTransaction = "25005";
    private const string ReadOnlySqlTransaction = "25006";
    private const string SchemaAndDataStatementMixingNotSupported = "25007";
    private const string NoActiveSqlTransaction = "25P01";
    private const string InFailedSqlTransaction = "25P02";
    private const string IdleInTransactionSessionTimeout = "25P03";
    private const string InvalidSqlStatementName = "26000";
    private const string TriggeredDataChangeViolation = "27000";
    private const string InvalidAuthorizationSpecification = "28000";
    private const string InvalidPassword = "28P01";
    private const string DependentPrivilegeDescriptorsStillExist = "2B000";
    private const string DependentObjectsStillExist = "2BP01";
    private const string InvalidTransactionTermination = "2D000";
    private const string SqlRoutineException = "2F000";
    private const string FunctionExecutedNoReturnStatement = "2F005";
    private const string ModifyingSqlDataNotPermitted = "2F002";
    private const string ProhibitedSqlStatementAttempted = "2F003";
    private const string ReadingSqlDataNotPermitted = "2F004";
    private const string InvalidCursorName = "34000";
    private const string ExternalRoutineException = "38000";
    private const string ContainingSqlNotPermitted = "38001";
    private const string ModifyingSqlDataNotPermitted2 = "38002";
    private const string ProhibitedSqlStatementAttempted2 = "38003";
    private const string ReadingSqlDataNotPermitted2 = "38004";
    private const string ExternalRoutineInvocationException = "39000";
    private const string InvalidSqlstateReturned = "39001";
    private const string NullValueNotAllowed2 = "39004";
    private const string TriggerProtocolViolated = "39P01";
    private const string SrfProtocolViolated = "39P02";
    private const string EventTriggerProtocolViolated = "39P03";
    private const string SavepointException = "3B000";
    private const string InvalidSavepointSpecification = "3B001";
    private const string InvalidCatalogName = "3D000";
    private const string InvalidSchemaName = "3F000";
    private const string TransactionRollback = "40000";
    private const string TransactionIntegrityConstraintViolation = "40002";
    private const string SerializationFailure = "40001";
    private const string StatementCompletionUnknown = "40003";
    private const string DeadlockDetected = "40P01";
    private const string SyntaxErrorOrAccessRuleViolation = "42000";
    private const string SyntaxError = "42601";
    private const string InsufficientPrivilege = "42501";
    private const string CannotCoerce = "42846";
    private const string GroupingError = "42803";
    private const string WindowingError = "42P20";
    private const string InvalidRecursion = "42P19";
    private const string InvalidForeignKey = "42830";
    private const string InvalidName = "42602";
    private const string NameTooLong = "42622";
    private const string ReservedName = "42939";
    private const string DatatypeMismatch = "42804";
    private const string IndeterminateDatatype = "42P18";
    private const string CollationMismatch = "42P21";
    private const string IndeterminateCollation = "42P22";
    private const string WrongObjectType = "42809";
    private const string GeneratedAlways = "428C9";
    private const string UndefinedColumn = "42703";
    private const string UndefinedFunction = "42883";
    private const string UndefinedTable = "42P01";
    private const string UndefinedParameter = "42P02";
    private const string UndefinedObject = "42704";
    private const string DuplicateColumn = "42701";
    private const string DuplicateCursor = "42P03";
    private const string DuplicateDatabase = "42P04";
    private const string DuplicateFunction = "42723";
    private const string DuplicatePreparedStatement = "42P05";
    private const string DuplicateSchema = "42P06";
    private const string DuplicateTable = "42P07";
    private const string DuplicateAlias = "42712";
    private const string DuplicateObject = "42710";
    private const string AmbiguousColumn = "42702";
    private const string AmbiguousFunction = "42725";
    private const string AmbiguousParameter = "42P08";
    private const string AmbiguousAlias = "42P09";
    private const string InvalidColumnReference = "42P10";
    private const string InvalidColumnDefinition = "42611";
    private const string InvalidCursorDefinition = "42P11";
    private const string InvalidDatabaseDefinition = "42P12";
    private const string InvalidFunctionDefinition = "42P13";
    private const string InvalidPreparedStatementDefinition = "42P14";
    private const string InvalidSchemaDefinition = "42P15";
    private const string InvalidTableDefinition = "42P16";
    private const string InvalidObjectDefinition = "42P17";
    private const string WithCheckOptionViolation = "44000";
    private const string InsufficientResources = "53000";
    private const string DiskFull = "53100";
    private const string OutOfMemory = "53200";
    private const string TooManyConnections = "53300";
    private const string ConfigurationLimitExceeded = "53400";
    private const string ProgramLimitExceeded = "54000";
    private const string StatementTooComplex = "54001";
    private const string TooManyColumns = "54011";
    private const string TooManyArguments = "54023";
    private const string ObjectNotInPrerequisiteState = "55000";
    private const string ObjectInUse = "55006";
    private const string CantChangeRuntimeParam = "55P02";
    private const string LockNotAvailable = "55P03";
    private const string UnsafeNewEnumValueUsage = "55P04";
    private const string OperatorIntervention = "57000";
    private const string QueryCanceled = "57014";
    private const string AdminShutdown = "57P01";
    private const string CrashShutdown = "57P02";
    private const string CannotConnectNow = "57P03";
    private const string DatabaseDropped = "57P04";
    private const string IdleSessionTimeout = "57P05";
    private const string SystemError = "58000";
    private const string IoError = "58030";
    private const string UndefinedFile = "58P01";
    private const string DuplicateFile = "58P02";
    private const string SnapshotTooOld = "72000";

    public static SqlState FromChars(ReadOnlySpan<char> chars)
    {
        return chars switch
        {
            SuccessfulCompletion => SqlState.SuccessfulCompletion,
            Warning => SqlState.Warning,
            DynamicResultSetsReturned => SqlState.DynamicResultSetsReturned,
            ImplicitZeroBitPadding => SqlState.ImplicitZeroBitPadding,
            NullValueEliminatedInSetFunction => SqlState.NullValueEliminatedInSetFunction,
            PrivilegeNoteGranted => SqlState.PrivilegeNotGranted,
            PrivilegeNotRevoked => SqlState.PrivilegeNotRevoked,
            StringDataRightTruncation => SqlState.StringDataRightTruncation,
            DeprecatedFeature => SqlState.DeprecatedFeature,
            NoData => SqlState.NoData,
            NoAdditionalDynamicResultSetsReturned => SqlState.NoAdditionalDynamicResultSetsReturned,
            SqlStatementNotYetComplete => SqlState.SqlStatementNotYetComplete,
            ConnectionException => SqlState.ConnectionException,
            ConnectionDoesNotExist => SqlState.ConnectionDoesNotExist,
            ConnectionFailure => SqlState.ConnectionFailure,
            SqlClientUnableToEstablishSqlConnection => SqlState.SqlClientUnableToEstablishSqlConnection,
            SqlServerRejectedEstablishmentOfSqlConnection => SqlState.SqlServerRejectedEstablishmentOfSqlConnection,
            TransactionResolutionUnknown => SqlState.TransactionResolutionUnknown,
            ProtocolViolation => SqlState.ProtocolViolation,
            TriggeredActionException => SqlState.TriggeredActionException,
            FeatureNotSupported => SqlState.FeatureNotSupported,
            InvalidTransactionInitiation => SqlState.InvalidTransactionInitiation,
            LocatorException => SqlState.LocatorException,
            InvalidLocatorSpecification => SqlState.InvalidLocatorSpecification,
            InvalidGrantor => SqlState.InvalidGrantor,
            InvalidGrantOperation => SqlState.InvalidGrantOperation,
            InvalidRoleSpecification => SqlState.InvalidRoleSpecification,
            DiagnosticsException => SqlState.DiagnosticsException,
            StackedDiagnosticsAccessedWithoutActiveHandler => SqlState.StackedDiagnosticsAccessedWithoutActiveHandler,
            CaseNotFound => SqlState.CaseNotFound,
            CardinalityViolation => SqlState.CardinalityViolation,
            DataException => SqlState.DataException,
            ArraySubscriptError => SqlState.ArraySubscriptError,
            CharacterNotInRepertoire => SqlState.CharacterNotInRepertoire,
            DatetimeFieldOverflow => SqlState.DatetimeFieldOverflow,
            DivisionByZero => SqlState.DivisionByZero,
            ErrorInAssignment => SqlState.ErrorInAssignment,
            EscapeCharacterConflict => SqlState.EscapeCharacterConflict,
            IndicatorOverflow => SqlState.IndicatorOverflow,
            IntervalFieldOverflow => SqlState.IntervalFieldOverflow,
            InvalidArgumentForLogarithm => SqlState.InvalidArgumentForLogarithm,
            InvalidArgumentForNtileFunction => SqlState.InvalidArgumentForNtileFunction,
            InvalidArgumentForNthValueFunction => SqlState.InvalidArgumentForNthValueFunction,
            InvalidArgumentForPowerFunction => SqlState.InvalidArgumentForPowerFunction,
            InvalidArgumentForWidthBucketFunction => SqlState.InvalidArgumentForWidthBucketFunction,
            InvalidCharacterValueForCast => SqlState.InvalidCharacterValueForCast,
            InvalidDatetimeFormat => SqlState.InvalidDatetimeFormat,
            InvalidEscapeCharacter => SqlState.InvalidEscapeCharacter,
            InvalidEscapeOctet => SqlState.InvalidEscapeOctet,
            InvalidEscapeSequence => SqlState.InvalidEscapeSequence,
            NonstandardUseOfEscapeCharacter => SqlState.NonstandardUseOfEscapeCharacter,
            InvalidIndicatorParameterValue => SqlState.InvalidIndicatorParameterValue,
            InvalidParameterValue => SqlState.InvalidParameterValue,
            InvalidPrecedingOrFollowingSize => SqlState.InvalidPrecedingOrFollowingSize,
            InvalidRegularExpression => SqlState.InvalidRegularExpression,
            InvalidRowCountInLimitClause => SqlState.InvalidRowCountInLimitClause,
            InvalidRowCountInResultOffsetClause => SqlState.InvalidRowCountInResultOffsetClause,
            InvalidTableSampleArgument => SqlState.InvalidTableSampleArgument,
            InvalidTableSampleRepeat => SqlState.InvalidTableSampleRepeat,
            InvalidTimeZoneDisplacementValue => SqlState.InvalidTimeZoneDisplacementValue,
            InvalidUseOfEscapeCharacter => SqlState.InvalidUseOfEscapeCharacter,
            MostSpecificTypeMismatch => SqlState.MostSpecificTypeMismatch,
            NullValueNotAllowed => SqlState.NullValueNotAllowed,
            NullValueNoIndicatorParameter => SqlState.NullValueNoIndicatorParameter,
            NumericValueOutOfRange => SqlState.NumericValueOutOfRange,
            SequenceGeneratorLimitExceeded => SqlState.SequenceGeneratorLimitExceeded,
            StringDataLengthMismatch => SqlState.StringDataLengthMismatch,
            StringDataRightTruncation2 => SqlState.StringDataRightTruncation2,
            SubstringError => SqlState.SubstringError,
            TrimError => SqlState.TrimError,
            UnterminatedCString => SqlState.UnterminatedCString,
            ZeroLengthCharacterString => SqlState.ZeroLengthCharacterString,
            FloatingPointException => SqlState.FloatingPointException,
            InvalidTextRepresentation => SqlState.InvalidTextRepresentation,
            InvalidBinaryRepresentation => SqlState.InvalidBinaryRepresentation,
            BadCopyFileFormat => SqlState.BadCopyFileFormat,
            UntranslatableCharacter => SqlState.UntranslatableCharacter,
            NotAnXmlDocument => SqlState.NotAnXmlDocument,
            InvalidXmlDocument => SqlState.InvalidXmlDocument,
            InvalidXmlContent => SqlState.InvalidXmlContent,
            InvalidXmlComment => SqlState.InvalidXmlComment,
            InvalidXmlProcessingInstruction => SqlState.InvalidXmlProcessingInstruction,
            DuplicateJsonObjectKeyValue => SqlState.DuplicateJsonObjectKeyValue,
            InvalidArgumentForSqlJsonDatetimeFunction => SqlState.InvalidArgumentForSqlJsonDatetimeFunction,
            InvalidJsonText => SqlState.InvalidJsonText,
            InvalidSqlJsonSubscript => SqlState.InvalidSqlJsonSubscript,
            MoreThanOneSqlJsonItem => SqlState.MoreThanOneSqlJsonItem,
            NoSqlJsonItem => SqlState.NoSqlJsonItem,
            NonNumericSqlJsonItem => SqlState.NonNumericSqlJsonItem,
            NonUniqueKeysInAJsonObject => SqlState.NonUniqueKeysInAJsonObject,
            SingletonSqlJsonItemRequired => SqlState.SingletonSqlJsonItemRequired,
            SqlJsonArrayNotFound => SqlState.SqlJsonArrayNotFound,
            SqlJsonMemberNotFound => SqlState.SqlJsonMemberNotFound,
            SqlJsonNumberNotFound => SqlState.SqlJsonNumberNotFound,
            SqlJsonObjectNotFound => SqlState.SqlJsonObjectNotFound,
            TooManyJsonArrayElements => SqlState.TooManyJsonArrayElements,
            TooManyJsonObjectMembers => SqlState.TooManyJsonObjectMembers,
            SqlJsonScalarRequired => SqlState.SqlJsonScalarRequired,
            SqlJsonItemCannotBeCastToTargetType => SqlState.SqlJsonItemCannotBeCastToTargetType,
            IntegrityConstraintViolation => SqlState.IntegrityConstraintViolation,
            RestrictViolation => SqlState.RestrictViolation,
            NotNullViolation => SqlState.NotNullViolation,
            ForeignKeyViolation => SqlState.ForeignKeyViolation,
            UniqueViolation => SqlState.UniqueViolation,
            CheckViolation => SqlState.CheckViolation,
            ExclusionViolation => SqlState.ExclusionViolation,
            InvalidCursorState => SqlState.InvalidCursorState,
            InvalidTransactionState => SqlState.InvalidTransactionState,
            ActiveSqlTransaction => SqlState.ActiveSqlTransaction,
            BranchTransactionAlreadyActive => SqlState.BranchTransactionAlreadyActive,
            HeldCursorRequiresSameIsolationLevel => SqlState.HeldCursorRequiresSameIsolationLevel,
            InappropriateAccessModeForBranchTransaction => SqlState.InappropriateAccessModeForBranchTransaction,
            InappropriateIsolationLevelForBranchTransaction => SqlState.InappropriateIsolationLevelForBranchTransaction,
            NoActiveSqlTransactionForBranchTransaction => SqlState.NoActiveSqlTransactionForBranchTransaction,
            ReadOnlySqlTransaction => SqlState.ReadOnlySqlTransaction,
            SchemaAndDataStatementMixingNotSupported => SqlState.SchemaAndDataStatementMixingNotSupported,
            NoActiveSqlTransaction => SqlState.NoActiveSqlTransaction,
            InFailedSqlTransaction => SqlState.InFailedSqlTransaction,
            IdleInTransactionSessionTimeout => SqlState.IdleInTransactionSessionTimeout,
            InvalidSqlStatementName => SqlState.InvalidSqlStatementName,
            TriggeredDataChangeViolation => SqlState.TriggeredDataChangeViolation,
            InvalidAuthorizationSpecification => SqlState.InvalidAuthorizationSpecification,
            InvalidPassword => SqlState.InvalidPassword,
            DependentPrivilegeDescriptorsStillExist => SqlState.DependentPrivilegeDescriptorsStillExist,
            DependentObjectsStillExist => SqlState.DependentObjectsStillExist,
            InvalidTransactionTermination => SqlState.InvalidTransactionTermination,
            SqlRoutineException => SqlState.SqlRoutineException,
            FunctionExecutedNoReturnStatement => SqlState.FunctionExecutedNoReturnStatement,
            ModifyingSqlDataNotPermitted => SqlState.ModifyingSqlDataNotPermitted,
            ProhibitedSqlStatementAttempted => SqlState.ProhibitedSqlStatementAttempted,
            ReadingSqlDataNotPermitted => SqlState.ReadingSqlDataNotPermitted,
            InvalidCursorName => SqlState.InvalidCursorName,
            ExternalRoutineException => SqlState.ExternalRoutineException,
            ContainingSqlNotPermitted => SqlState.ContainingSqlNotPermitted,
            ModifyingSqlDataNotPermitted2 => SqlState.ModifyingSqlDataNotPermitted2,
            ProhibitedSqlStatementAttempted2 => SqlState.ProhibitedSqlStatementAttempted2,
            ReadingSqlDataNotPermitted2 => SqlState.ReadingSqlDataNotPermitted2,
            ExternalRoutineInvocationException => SqlState.ExternalRoutineInvocationException,
            InvalidSqlstateReturned => SqlState.InvalidSqlstateReturned,
            NullValueNotAllowed2 => SqlState.NullValueNotAllowed2,
            TriggerProtocolViolated => SqlState.TriggerProtocolViolated,
            SrfProtocolViolated => SqlState.SrfProtocolViolated,
            EventTriggerProtocolViolated => SqlState.EventTriggerProtocolViolated,
            SavepointException => SqlState.SavepointException,
            InvalidSavepointSpecification => SqlState.InvalidSavepointSpecification,
            InvalidCatalogName => SqlState.InvalidCatalogName,
            InvalidSchemaName => SqlState.InvalidSchemaName,
            TransactionRollback => SqlState.TransactionRollback,
            TransactionIntegrityConstraintViolation => SqlState.TransactionIntegrityConstraintViolation,
            SerializationFailure => SqlState.SerializationFailure,
            StatementCompletionUnknown => SqlState.StatementCompletionUnknown,
            DeadlockDetected => SqlState.DeadlockDetected,
            SyntaxErrorOrAccessRuleViolation => SqlState.SyntaxErrorOrAccessRuleViolation,
            SyntaxError => SqlState.SyntaxError,
            InsufficientPrivilege => SqlState.InsufficientPrivilege,
            CannotCoerce => SqlState.CannotCoerce,
            GroupingError => SqlState.GroupingError,
            WindowingError => SqlState.WindowingError,
            InvalidRecursion => SqlState.InvalidRecursion,
            InvalidForeignKey => SqlState.InvalidForeignKey,
            InvalidName => SqlState.InvalidName,
            NameTooLong => SqlState.NameTooLong,
            ReservedName => SqlState.ReservedName,
            DatatypeMismatch => SqlState.DatatypeMismatch,
            IndeterminateDatatype => SqlState.IndeterminateDatatype,
            CollationMismatch => SqlState.CollationMismatch,
            IndeterminateCollation => SqlState.IndeterminateCollation,
            WrongObjectType => SqlState.WrongObjectType,
            GeneratedAlways => SqlState.GeneratedAlways,
            UndefinedColumn => SqlState.UndefinedColumn,
            UndefinedFunction => SqlState.UndefinedFunction,
            UndefinedTable => SqlState.UndefinedTable,
            UndefinedParameter => SqlState.UndefinedParameter,
            UndefinedObject => SqlState.UndefinedObject,
            DuplicateColumn => SqlState.DuplicateColumn,
            DuplicateCursor => SqlState.DuplicateCursor,
            DuplicateDatabase => SqlState.DuplicateDatabase,
            DuplicateFunction => SqlState.DuplicateFunction,
            DuplicatePreparedStatement => SqlState.DuplicatePreparedStatement,
            DuplicateSchema => SqlState.DuplicateSchema,
            DuplicateTable => SqlState.DuplicateTable,
            DuplicateAlias => SqlState.DuplicateAlias,
            DuplicateObject => SqlState.DuplicateObject,
            AmbiguousColumn => SqlState.AmbiguousColumn,
            AmbiguousFunction => SqlState.AmbiguousFunction,
            AmbiguousParameter => SqlState.AmbiguousParameter,
            AmbiguousAlias => SqlState.AmbiguousAlias,
            InvalidColumnReference => SqlState.InvalidColumnReference,
            InvalidColumnDefinition => SqlState.InvalidColumnDefinition,
            InvalidCursorDefinition => SqlState.InvalidCursorDefinition,
            InvalidDatabaseDefinition => SqlState.InvalidDatabaseDefinition,
            InvalidFunctionDefinition => SqlState.InvalidFunctionDefinition,
            InvalidPreparedStatementDefinition => SqlState.InvalidPreparedStatementDefinition,
            InvalidSchemaDefinition => SqlState.InvalidSchemaDefinition,
            InvalidTableDefinition => SqlState.InvalidTableDefinition,
            InvalidObjectDefinition => SqlState.InvalidObjectDefinition,
            WithCheckOptionViolation => SqlState.WithCheckOptionViolation,
            InsufficientResources => SqlState.InsufficientResources,
            DiskFull => SqlState.DiskFull,
            OutOfMemory => SqlState.OutOfMemory,
            TooManyConnections => SqlState.TooManyConnections,
            ConfigurationLimitExceeded => SqlState.ConfigurationLimitExceeded,
            ProgramLimitExceeded => SqlState.ProgramLimitExceeded,
            StatementTooComplex => SqlState.StatementTooComplex,
            TooManyColumns => SqlState.TooManyColumns,
            TooManyArguments => SqlState.TooManyArguments,
            ObjectNotInPrerequisiteState => SqlState.ObjectNotInPrerequisiteState,
            ObjectInUse => SqlState.ObjectInUse,
            CantChangeRuntimeParam => SqlState.CantChangeRuntimeParam,
            LockNotAvailable => SqlState.LockNotAvailable,
            UnsafeNewEnumValueUsage => SqlState.UnsafeNewEnumValueUsage,
            OperatorIntervention => SqlState.OperatorIntervention,
            QueryCanceled => SqlState.QueryCanceled,
            AdminShutdown => SqlState.AdminShutdown,
            CrashShutdown => SqlState.CrashShutdown,
            CannotConnectNow => SqlState.CannotConnectNow,
            DatabaseDropped => SqlState.DatabaseDropped,
            IdleSessionTimeout => SqlState.IdleSessionTimeout,
            SystemError => SqlState.SystemError,
            IoError => SqlState.IoError,
            UndefinedFile => SqlState.UndefinedFile,
            DuplicateFile => SqlState.DuplicateFile,
            SnapshotTooOld => SqlState.SnapshotTooOld,
            _ => throw new ArgumentOutOfRangeException(nameof(chars), new string(chars), ""),
        };
    }

    public static (string ErrorCode, string ConditionName) GetDetails(this SqlState sqlState)
    {
        return sqlState switch
        {
            SqlState.SuccessfulCompletion => (SuccessfulCompletion, "successful_completion"),
            SqlState.Warning => (Warning, "warning"),
            SqlState.DynamicResultSetsReturned => (DynamicResultSetsReturned, "dynamic_result_sets_returned"),
            SqlState.ImplicitZeroBitPadding => (ImplicitZeroBitPadding, "implicit_zero_bit_padding"),
            SqlState.NullValueEliminatedInSetFunction => (NullValueEliminatedInSetFunction, "null_value_eliminated_in_set_function"),
            SqlState.PrivilegeNotGranted => (PrivilegeNoteGranted, "privilege_not_granted"),
            SqlState.PrivilegeNotRevoked => (PrivilegeNotRevoked, "privilege_not_revoked"),
            SqlState.StringDataRightTruncation => (StringDataRightTruncation, "string_data_right_truncation"),
            SqlState.DeprecatedFeature => (DeprecatedFeature, "deprecated_feature"),
            SqlState.NoData => (NoData, "no_data"),
            SqlState.NoAdditionalDynamicResultSetsReturned => (NoAdditionalDynamicResultSetsReturned, "no_additional_dynamic_result_sets_returned"),
            SqlState.SqlStatementNotYetComplete => (SqlStatementNotYetComplete, "sql_statement_not_yet_complete"),
            SqlState.ConnectionException => (ConnectionException, "connection_exception"),
            SqlState.ConnectionDoesNotExist => (ConnectionDoesNotExist, "connection_does_not_exist"),
            SqlState.ConnectionFailure => (ConnectionFailure, "connection_failure"),
            SqlState.SqlClientUnableToEstablishSqlConnection => (SqlClientUnableToEstablishSqlConnection, "sqlclient_unable_to_establish_sqlconnection"),
            SqlState.SqlServerRejectedEstablishmentOfSqlConnection => (SqlServerRejectedEstablishmentOfSqlConnection, "sqlserver_rejected_establishment_of_sqlconnection"),
            SqlState.TransactionResolutionUnknown => (TransactionResolutionUnknown, "transaction_resolution_unknown"),
            SqlState.ProtocolViolation => (ProtocolViolation, "protocol_violation"),
            SqlState.TriggeredActionException => (TriggeredActionException, "triggered_action_exception"),
            SqlState.FeatureNotSupported => (FeatureNotSupported, "feature_not_supported"),
            SqlState.InvalidTransactionInitiation => (InvalidTransactionInitiation, "invalid_transaction_initiation"),
            SqlState.LocatorException => (LocatorException, "locator_exception"),
            SqlState.InvalidLocatorSpecification => (InvalidLocatorSpecification, "invalid_locator_specification"),
            SqlState.InvalidGrantor => (InvalidGrantor, "invalid_grantor"),
            SqlState.InvalidGrantOperation => (InvalidGrantOperation, "invalid_grant_operation"),
            SqlState.InvalidRoleSpecification => (InvalidRoleSpecification, "invalid_role_specification"),
            SqlState.DiagnosticsException => (DiagnosticsException, "diagnostics_exception"),
            SqlState.StackedDiagnosticsAccessedWithoutActiveHandler => (StackedDiagnosticsAccessedWithoutActiveHandler, "stacked_diagnostics_accessed_without_active_handler"),
            SqlState.CaseNotFound => (CaseNotFound, "case_not_found"),
            SqlState.CardinalityViolation => (CardinalityViolation, "cardinality_violation"),
            SqlState.DataException => (DataException, "data_exception"),
            SqlState.ArraySubscriptError => (ArraySubscriptError, "array_subscript_error"),
            SqlState.CharacterNotInRepertoire => (CharacterNotInRepertoire, "character_not_in_repertoire"),
            SqlState.DatetimeFieldOverflow => (DatetimeFieldOverflow, "datetime_field_overflow"),
            SqlState.DivisionByZero => (DivisionByZero, "division_by_zero"),
            SqlState.ErrorInAssignment => (ErrorInAssignment, "error_in_assignment"),
            SqlState.EscapeCharacterConflict => (EscapeCharacterConflict, "escape_character_conflict"),
            SqlState.IndicatorOverflow => (IndicatorOverflow, "indicator_overflow"),
            SqlState.IntervalFieldOverflow => (IntervalFieldOverflow, "interval_field_overflow"),
            SqlState.InvalidArgumentForLogarithm => (InvalidArgumentForLogarithm, "invalid_argument_for_logarithm"),
            SqlState.InvalidArgumentForNtileFunction => (InvalidArgumentForNtileFunction, "invalid_argument_for_ntile_function"),
            SqlState.InvalidArgumentForNthValueFunction => (InvalidArgumentForNthValueFunction, "invalid_argument_for_nth_value_function"),
            SqlState.InvalidArgumentForPowerFunction => (InvalidArgumentForPowerFunction, "invalid_argument_for_power_function"),
            SqlState.InvalidArgumentForWidthBucketFunction => (InvalidArgumentForWidthBucketFunction, "invalid_argument_for_width_bucket_function"),
            SqlState.InvalidCharacterValueForCast => (InvalidCharacterValueForCast, "invalid_character_value_for_cast"),
            SqlState.InvalidDatetimeFormat => (InvalidDatetimeFormat, "invalid_datetime_format"),
            SqlState.InvalidEscapeCharacter => (InvalidEscapeCharacter, "invalid_escape_character"),
            SqlState.InvalidEscapeOctet => (InvalidEscapeOctet, "invalid_escape_octet"),
            SqlState.InvalidEscapeSequence => (InvalidEscapeSequence, "invalid_escape_sequence"),
            SqlState.NonstandardUseOfEscapeCharacter => (NonstandardUseOfEscapeCharacter, "nonstandard_use_of_escape_character"),
            SqlState.InvalidIndicatorParameterValue => (InvalidIndicatorParameterValue, "invalid_indicator_parameter_value"),
            SqlState.InvalidParameterValue => (InvalidParameterValue, "invalid_parameter_value"),
            SqlState.InvalidPrecedingOrFollowingSize => (InvalidPrecedingOrFollowingSize, "invalid_preceding_or_following_size"),
            SqlState.InvalidRegularExpression => (InvalidRegularExpression, "invalid_regular_expression"),
            SqlState.InvalidRowCountInLimitClause => (InvalidRowCountInLimitClause, "invalid_row_count_in_limit_clause"),
            SqlState.InvalidRowCountInResultOffsetClause => (InvalidRowCountInResultOffsetClause, "invalid_row_count_in_result_offset_clause"),
            SqlState.InvalidTableSampleArgument => (InvalidTableSampleArgument, "invalid_tablesample_argument"),
            SqlState.InvalidTableSampleRepeat => (InvalidTableSampleRepeat, "invalid_tablesample_repeat"),
            SqlState.InvalidTimeZoneDisplacementValue => (InvalidTimeZoneDisplacementValue, "invalid_time_zone_displacement_value"),
            SqlState.InvalidUseOfEscapeCharacter => (InvalidUseOfEscapeCharacter, "invalid_use_of_escape_character"),
            SqlState.MostSpecificTypeMismatch => (MostSpecificTypeMismatch, "most_specific_type_mismatch"),
            SqlState.NullValueNotAllowed => (NullValueNotAllowed, "null_value_not_allowed"),
            SqlState.NullValueNoIndicatorParameter => (NullValueNoIndicatorParameter, "null_value_no_indicator_parameter"),
            SqlState.NumericValueOutOfRange => (NumericValueOutOfRange, "numeric_value_out_of_range"),
            SqlState.SequenceGeneratorLimitExceeded => (SequenceGeneratorLimitExceeded, "sequence_generator_limit_exceeded"),
            SqlState.StringDataLengthMismatch => (StringDataLengthMismatch, "string_data_length_mismatch"),
            SqlState.StringDataRightTruncation2 => (StringDataRightTruncation2, "string_data_right_truncation"),
            SqlState.SubstringError => (SubstringError, "substring_error"),
            SqlState.TrimError => (TrimError, "trim_error"),
            SqlState.UnterminatedCString => (UnterminatedCString, "unterminated_c_string"),
            SqlState.ZeroLengthCharacterString => (ZeroLengthCharacterString, "zero_length_character_string"),
            SqlState.FloatingPointException => (FloatingPointException, "floating_point_exception"),
            SqlState.InvalidTextRepresentation => (InvalidTextRepresentation, "invalid_text_representation"),
            SqlState.InvalidBinaryRepresentation => (InvalidBinaryRepresentation, "invalid_binary_representation"),
            SqlState.BadCopyFileFormat => (BadCopyFileFormat, "bad_copy_file_format"),
            SqlState.UntranslatableCharacter => (UntranslatableCharacter, "untranslatable_character"),
            SqlState.NotAnXmlDocument => (NotAnXmlDocument, "not_an_xml_document"),
            SqlState.InvalidXmlDocument => (InvalidXmlDocument, "invalid_xml_document"),
            SqlState.InvalidXmlContent => (InvalidXmlContent, "invalid_xml_content"),
            SqlState.InvalidXmlComment => (InvalidXmlComment, "invalid_xml_comment"),
            SqlState.InvalidXmlProcessingInstruction => (InvalidXmlProcessingInstruction, "invalid_xml_processing_instruction"),
            SqlState.DuplicateJsonObjectKeyValue => (DuplicateJsonObjectKeyValue, "duplicate_json_object_key_value"),
            SqlState.InvalidArgumentForSqlJsonDatetimeFunction => (InvalidArgumentForSqlJsonDatetimeFunction, "invalid_argument_for_sql_json_datetime_function"),
            SqlState.InvalidJsonText => (InvalidJsonText, "invalid_json_text"),
            SqlState.InvalidSqlJsonSubscript => (InvalidSqlJsonSubscript, "invalid_sql_json_subscript"),
            SqlState.MoreThanOneSqlJsonItem => (MoreThanOneSqlJsonItem, "more_than_one_sql_json_item"),
            SqlState.NoSqlJsonItem => (NoSqlJsonItem, "no_sql_json_item"),
            SqlState.NonNumericSqlJsonItem => (NonNumericSqlJsonItem, "non_numeric_sql_json_item"),
            SqlState.NonUniqueKeysInAJsonObject => (NonUniqueKeysInAJsonObject, "non_unique_keys_in_a_json_object"),
            SqlState.SingletonSqlJsonItemRequired => (SingletonSqlJsonItemRequired, "singleton_sql_json_item_required"),
            SqlState.SqlJsonArrayNotFound => (SqlJsonArrayNotFound, "sql_json_array_not_found"),
            SqlState.SqlJsonMemberNotFound => (SqlJsonMemberNotFound, "sql_json_member_not_found"),
            SqlState.SqlJsonNumberNotFound => (SqlJsonNumberNotFound, "sql_json_number_not_found"),
            SqlState.SqlJsonObjectNotFound => (SqlJsonObjectNotFound, "sql_json_object_not_found"),
            SqlState.TooManyJsonArrayElements => (TooManyJsonArrayElements, "too_many_json_array_elements"),
            SqlState.TooManyJsonObjectMembers => (TooManyJsonObjectMembers, "too_many_json_object_members"),
            SqlState.SqlJsonScalarRequired => (SqlJsonScalarRequired, "sql_json_scalar_required"),
            SqlState.SqlJsonItemCannotBeCastToTargetType => (SqlJsonItemCannotBeCastToTargetType, "sql_json_item_cannot_be_cast_to_target_type"),
            SqlState.IntegrityConstraintViolation => (IntegrityConstraintViolation, "integrity_constraint_violation"),
            SqlState.RestrictViolation => (RestrictViolation, "restrict_violation"),
            SqlState.NotNullViolation => (NotNullViolation, "not_null_violation"),
            SqlState.ForeignKeyViolation => (ForeignKeyViolation, "foreign_key_violation"),
            SqlState.UniqueViolation => (UniqueViolation, "unique_violation"),
            SqlState.CheckViolation => (CheckViolation, "check_violation"),
            SqlState.ExclusionViolation => (ExclusionViolation, "exclusion_violation"),
            SqlState.InvalidCursorState => (InvalidCursorState, "invalid_cursor_state"),
            SqlState.InvalidTransactionState => (InvalidTransactionState, "invalid_transaction_state"),
            SqlState.ActiveSqlTransaction => (ActiveSqlTransaction, "active_sql_transaction"),
            SqlState.BranchTransactionAlreadyActive => (BranchTransactionAlreadyActive, "branch_transaction_already_active"),
            SqlState.HeldCursorRequiresSameIsolationLevel => (HeldCursorRequiresSameIsolationLevel, "held_cursor_requires_same_isolation_level"),
            SqlState.InappropriateAccessModeForBranchTransaction => (InappropriateAccessModeForBranchTransaction, "inappropriate_access_mode_for_branch_transaction"),
            SqlState.InappropriateIsolationLevelForBranchTransaction => (InappropriateIsolationLevelForBranchTransaction, "inappropriate_isolation_level_for_branch_transaction"),
            SqlState.NoActiveSqlTransactionForBranchTransaction => (NoActiveSqlTransactionForBranchTransaction, "no_active_sql_transaction_for_branch_transaction"),
            SqlState.ReadOnlySqlTransaction => (ReadOnlySqlTransaction, "read_only_sql_transaction"),
            SqlState.SchemaAndDataStatementMixingNotSupported => (SchemaAndDataStatementMixingNotSupported, "schema_and_data_statement_mixing_not_supported"),
            SqlState.NoActiveSqlTransaction => (NoActiveSqlTransaction, "no_active_sql_transaction"),
            SqlState.InFailedSqlTransaction => (InFailedSqlTransaction, "in_failed_sql_transaction"),
            SqlState.IdleInTransactionSessionTimeout => (IdleInTransactionSessionTimeout, "idle_in_transaction_session_timeout"),
            SqlState.InvalidSqlStatementName => (InvalidSqlStatementName, "invalid_sql_statement_name"),
            SqlState.TriggeredDataChangeViolation => (TriggeredDataChangeViolation, "triggered_data_change_violation"),
            SqlState.InvalidAuthorizationSpecification => (InvalidAuthorizationSpecification, "invalid_authorization_specification"),
            SqlState.InvalidPassword => (InvalidPassword, "invalid_password"),
            SqlState.DependentPrivilegeDescriptorsStillExist => (DependentPrivilegeDescriptorsStillExist, "dependent_privilege_descriptors_still_exist"),
            SqlState.DependentObjectsStillExist => (DependentObjectsStillExist, "dependent_objects_still_exist"),
            SqlState.InvalidTransactionTermination => (InvalidTransactionTermination, "invalid_transaction_termination"),
            SqlState.SqlRoutineException => (SqlRoutineException, "sql_routine_exception"),
            SqlState.FunctionExecutedNoReturnStatement => (FunctionExecutedNoReturnStatement, "function_executed_no_return_statement"),
            SqlState.ModifyingSqlDataNotPermitted => (ModifyingSqlDataNotPermitted, "modifying_sql_data_not_permitted"),
            SqlState.ProhibitedSqlStatementAttempted => (ProhibitedSqlStatementAttempted, "prohibited_sql_statement_attempted"),
            SqlState.ReadingSqlDataNotPermitted => (ReadingSqlDataNotPermitted, "reading_sql_data_not_permitted"),
            SqlState.InvalidCursorName => (InvalidCursorName, "invalid_cursor_name"),
            SqlState.ExternalRoutineException => (ExternalRoutineException, "external_routine_exception"),
            SqlState.ContainingSqlNotPermitted => (ContainingSqlNotPermitted, "containing_sql_not_permitted"),
            SqlState.ModifyingSqlDataNotPermitted2 => (ModifyingSqlDataNotPermitted2, "modifying_sql_data_not_permitted"),
            SqlState.ProhibitedSqlStatementAttempted2 => (ProhibitedSqlStatementAttempted2, "prohibited_sql_statement_attempted"),
            SqlState.ReadingSqlDataNotPermitted2 => (ReadingSqlDataNotPermitted2, "reading_sql_data_not_permitted"),
            SqlState.ExternalRoutineInvocationException => (ExternalRoutineInvocationException, "external_routine_invocation_exception"),
            SqlState.InvalidSqlstateReturned => (InvalidSqlstateReturned, "invalid_sqlstate_returned"),
            SqlState.NullValueNotAllowed2 => (NullValueNotAllowed2, "null_value_not_allowed"),
            SqlState.TriggerProtocolViolated => (TriggerProtocolViolated, "trigger_protocol_violated"),
            SqlState.SrfProtocolViolated => (SrfProtocolViolated, "srf_protocol_violated"),
            SqlState.EventTriggerProtocolViolated => (EventTriggerProtocolViolated, "event_trigger_protocol_violated"),
            SqlState.SavepointException => (SavepointException, "savepoint_exception"),
            SqlState.InvalidSavepointSpecification => (InvalidSavepointSpecification, "invalid_savepoint_specification"),
            SqlState.InvalidCatalogName => (InvalidCatalogName, "invalid_catalog_name"),
            SqlState.InvalidSchemaName => (InvalidSchemaName, "invalid_schema_name"),
            SqlState.TransactionRollback => (TransactionRollback, "transaction_rollback"),
            SqlState.TransactionIntegrityConstraintViolation => (TransactionIntegrityConstraintViolation, "transaction_integrity_constraint_violation"),
            SqlState.SerializationFailure => (SerializationFailure, "serialization_failure"),
            SqlState.StatementCompletionUnknown => (StatementCompletionUnknown, "statement_completion_unknown"),
            SqlState.DeadlockDetected => (DeadlockDetected, "deadlock_detected"),
            SqlState.SyntaxErrorOrAccessRuleViolation => (SyntaxErrorOrAccessRuleViolation, "syntax_error_or_access_rule_violation"),
            SqlState.SyntaxError => (SyntaxError, "syntax_error"),
            SqlState.InsufficientPrivilege => (InsufficientPrivilege, "insufficient_privilege"),
            SqlState.CannotCoerce => (CannotCoerce, "cannot_coerce"),
            SqlState.GroupingError => (GroupingError, "grouping_error"),
            SqlState.WindowingError => (WindowingError, "windowing_error"),
            SqlState.InvalidRecursion => (InvalidRecursion, "invalid_recursion"),
            SqlState.InvalidForeignKey => (InvalidForeignKey, "invalid_foreign_key"),
            SqlState.InvalidName => (InvalidName, "invalid_name"),
            SqlState.NameTooLong => (NameTooLong, "name_too_long"),
            SqlState.ReservedName => (ReservedName, "reserved_name"),
            SqlState.DatatypeMismatch => (DatatypeMismatch, "datatype_mismatch"),
            SqlState.IndeterminateDatatype => (IndeterminateDatatype, "indeterminate_datatype"),
            SqlState.CollationMismatch => (CollationMismatch, "collation_mismatch"),
            SqlState.IndeterminateCollation => (IndeterminateCollation, "indeterminate_collation"),
            SqlState.WrongObjectType => (WrongObjectType, "wrong_object_type"),
            SqlState.GeneratedAlways => (GeneratedAlways, "generated_always"),
            SqlState.UndefinedColumn => (UndefinedColumn, "undefined_column"),
            SqlState.UndefinedFunction => (UndefinedFunction, "undefined_function"),
            SqlState.UndefinedTable => (UndefinedTable, "undefined_table"),
            SqlState.UndefinedParameter => (UndefinedParameter, "undefined_parameter"),
            SqlState.UndefinedObject => (UndefinedObject, "undefined_object"),
            SqlState.DuplicateColumn => (DuplicateColumn, "duplicate_column"),
            SqlState.DuplicateCursor => (DuplicateCursor, "duplicate_cursor"),
            SqlState.DuplicateDatabase => (DuplicateDatabase, "duplicate_database"),
            SqlState.DuplicateFunction => (DuplicateFunction, "duplicate_function"),
            SqlState.DuplicatePreparedStatement => (DuplicatePreparedStatement, "duplicate_prepared_statement"),
            SqlState.DuplicateSchema => (DuplicateSchema, "duplicate_schema"),
            SqlState.DuplicateTable => (DuplicateTable, "duplicate_table"),
            SqlState.DuplicateAlias => (DuplicateAlias, "duplicate_alias"),
            SqlState.DuplicateObject => (DuplicateObject, "duplicate_object"),
            SqlState.AmbiguousColumn => (AmbiguousColumn, "ambiguous_column"),
            SqlState.AmbiguousFunction => (AmbiguousFunction, "ambiguous_function"),
            SqlState.AmbiguousParameter => (AmbiguousParameter, "ambiguous_parameter"),
            SqlState.AmbiguousAlias => (AmbiguousAlias, "ambiguous_alias"),
            SqlState.InvalidColumnReference => (InvalidColumnReference, "invalid_column_reference"),
            SqlState.InvalidColumnDefinition => (InvalidColumnDefinition, "invalid_column_definition"),
            SqlState.InvalidCursorDefinition => (InvalidCursorDefinition, "invalid_cursor_definition"),
            SqlState.InvalidDatabaseDefinition => (InvalidDatabaseDefinition, "invalid_database_definition"),
            SqlState.InvalidFunctionDefinition => (InvalidFunctionDefinition, "invalid_function_definition"),
            SqlState.InvalidPreparedStatementDefinition => (InvalidPreparedStatementDefinition, "invalid_prepared_statement_definition"),
            SqlState.InvalidSchemaDefinition => (InvalidSchemaDefinition, "invalid_schema_definition"),
            SqlState.InvalidTableDefinition => (InvalidTableDefinition, "invalid_table_definition"),
            SqlState.InvalidObjectDefinition => (InvalidObjectDefinition, "invalid_object_definition"),
            SqlState.WithCheckOptionViolation => (WithCheckOptionViolation, "with_check_option_violation"),
            SqlState.InsufficientResources => (InsufficientResources, "insufficient_resources"),
            SqlState.DiskFull => (DiskFull, "disk_full"),
            SqlState.OutOfMemory => (OutOfMemory, "out_of_memory"),
            SqlState.TooManyConnections => (TooManyConnections, "too_many_connections"),
            SqlState.ConfigurationLimitExceeded => (ConfigurationLimitExceeded, "configuration_limit_exceeded"),
            SqlState.ProgramLimitExceeded => (ProgramLimitExceeded, "program_limit_exceeded"),
            SqlState.StatementTooComplex => (StatementTooComplex, "statement_too_complex"),
            SqlState.TooManyColumns => (TooManyColumns, "too_many_columns"),
            SqlState.TooManyArguments => (TooManyArguments, "too_many_arguments"),
            SqlState.ObjectNotInPrerequisiteState => (ObjectNotInPrerequisiteState, "object_not_in_prerequisite_state"),
            SqlState.ObjectInUse => (ObjectInUse, "object_in_use"),
            SqlState.CantChangeRuntimeParam => (CantChangeRuntimeParam, "cant_change_runtime_param"),
            SqlState.LockNotAvailable => (LockNotAvailable, "lock_not_available"),
            SqlState.UnsafeNewEnumValueUsage => (UnsafeNewEnumValueUsage, "unsafe_new_enum_value_usage"),
            SqlState.OperatorIntervention => (OperatorIntervention, "operator_intervention"),
            SqlState.QueryCanceled => (QueryCanceled, "query_canceled"),
            SqlState.AdminShutdown => (AdminShutdown, "admin_shutdown"),
            SqlState.CrashShutdown => (CrashShutdown, "crash_shutdown"),
            SqlState.CannotConnectNow => (CannotConnectNow, "cannot_connect_now"),
            SqlState.DatabaseDropped => (DatabaseDropped, "database_dropped"),
            SqlState.IdleSessionTimeout => (IdleSessionTimeout, "idle_session_timeout"),
            SqlState.SystemError => (SystemError, "system_error"),
            SqlState.IoError => (IoError, "io_error"),
            SqlState.UndefinedFile => (UndefinedFile, "undefined_file"),
            SqlState.DuplicateFile => (DuplicateFile, "duplicate_file"),
            SqlState.SnapshotTooOld => (SnapshotTooOld, "snapshot_too_old"),
            _ => throw new ArgumentOutOfRangeException(nameof(sqlState), sqlState, "Invalid SqlState"),
        };

    }
}
