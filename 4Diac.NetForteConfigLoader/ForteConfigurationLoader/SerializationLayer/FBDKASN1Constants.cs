namespace ForteConfigurationLoader.SerializationLayer
{
    public enum EASN1TagClass
    {
        e_UNIVERSAL = 0,
        e_APPLICATION = 64,
        e_CONTEXT = 128,
        e_PRIVATE = 192
    }

    public enum EASN1EncodingType
    {
        e_PRIMITIVE = 0,
        e_CONSTRUCTED = 32
    }

    public enum EDataTypeTags
    {
        e_ANY_TAG = 0,
        e_BOOL_TAG = 1,
        e_SINT_TAG = 2,
        e_INT_TAG = 3,
        e_DINT_TAG = 4,
        e_LINT_TAG = 5,
        e_USINT_TAG = 6,
        e_UINT_TAG = 7,
        e_UDINT_TAG = 8,
        e_ULINT_TAG = 9,
        e_REAL_TAG = 10,
        e_LREAL_TAG = 11,
        e_TIME_TAG = 12,
        e_DATE_TAG = 13,
        e_TIME_OF_DAY_TAG = 14,
        e_DATE_AND_TIME_TAG = 15,
        e_STRING_TAG = 16,
        e_BYTE_TAG = 17,
        e_WORD_TAG = 18,
        e_DWORD_TAG = 19,
        e_LWORD_TAG = 20,
        e_WSTRING_TAG = 21,
        e_DerivedData_TAG = 26,
        e_DirectlyDerivedData_TAG = 27,
        e_EnumeratedData_TAG = 28,
        e_SubrangeData_TAG = 29,
        e_ARRAY_TAG = 22,//according to the compliance profile
        e_STRUCT_TAG = 31
    }

    public enum TagIdentify
    {
        e_ANY,
        e_BOOL,
        e_SINT,
        e_INT,
        e_DINT,
        e_LINT,
        e_USINT,
        e_UINT,
        e_UDINT,
        e_ULINT,
        e_BYTE,
        e_WORD,
        e_DWORD,
        e_LWORD,
        e_DATE,
        e_TIME_OF_DAY,
        e_DATE_AND_TIME,
        e_TIME, //until here simple Datatypes
        e_REAL,
        e_LREAL,
        e_STRING,
        e_WSTRING,
        e_DerivedData,
        e_DirectlyDerivedData,
        e_EnumeratedData,
        e_SubrangeData,
        e_ARRAY, //according to the compliance profile
        e_STRUCT,
        e_External = 256, // Base for CIEC_ANY based types outside of the forte base
        e_Max = 65535 // Guarantees at least 16 bits - otherwise gcc will optimizes on some platforms
    }
}