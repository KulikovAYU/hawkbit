#pragma once
#include "hawkbitPrototypes.hpp"

#if defined(_MSC_VER)
//  Microsoft
#define HAWKBIT_EXPORT_API __declspec(dllexport)
#define HAWKBIT_CALLING_CONV __cdecl
#elif defined(__GNUC__)
//  GCC
#define HAWKBIT_EXPORT_API __attribute__((visibility("default")))
#define HAWKBIT_CALLING_CONV __attribute__((__cdecl__))
#else
#define HAWKBIT_EXPORT_API
#pragma warning Unknown dynamic link import/export semantics.
#endif

#define hawkbit_in
#define hawkbit_out

namespace hawkbit_interop
{
	extern "C"
	{
        HAWKBIT_EXPORT_API void HAWKBIT_CALLING_CONV StartClient();

		HAWKBIT_EXPORT_API bool HAWKBIT_CALLING_CONV SetConfig(hawkbit_in LP_HAWKBIT_CONNECTION_CFG pInitCfg);

		HAWKBIT_EXPORT_API void HAWKBIT_CALLING_CONV Put(hawkbit_in LP_RESPONSE pResponse);

	    HAWKBIT_EXPORT_API void HAWKBIT_CALLING_CONV Get(hawkbit_out LP_HAWKBIT_DEPLOYMENT_DATA pPayloadData);
	}
}