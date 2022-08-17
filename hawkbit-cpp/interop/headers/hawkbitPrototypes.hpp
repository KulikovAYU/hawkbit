#pragma once

namespace hawkbit_interop
{
	typedef struct _HAWKBIT_END_POINT_ENV
	{
		char name[50] = "";
		char value[50] = "";
	} HAWKBIT_END_POINT_ENV, * LP_HAWKBIT_END_POINT_ENV;

	typedef struct _HAWKBIT_GATEWAY_TOKEN_ENV
	{
		char name[50] = "";
		char value[50] = "";
	} HAWKBIT_GATEWAY_TOKEN_ENV, * LP_HAWKBIT_GATEWAY_TOKEN_ENV;

	typedef struct _HAWKBIT_CONTROLLER_ID_ENV
	{
		char name[50] = "";
		char value[50] = "";
	} HAWKBIT_CONTROLLER_ID_ENV, * LP_HAWKBIT_CONTROLLER_ID_ENV;

	typedef struct _HAWKBIT_CONNECTION_CFG
	{
		char downloadFilesPath[1024] = "";
		HAWKBIT_END_POINT_ENV endPoint;
		HAWKBIT_GATEWAY_TOKEN_ENV gatewayToken;
		HAWKBIT_CONTROLLER_ID_ENV controllerId;
	} HAWKBIT_CONNECTION_CFG, * LP_HAWKBIT_CONNECTION_CFG;

	typedef struct _HAWKBIT_HASHES
	{
		char md5[100] = "";
		char sha1[100] = "";
		char sha256[100] = "";
	} HAWKBIT_HASHES, * LP_HAWKBIT_HASHES;

	typedef struct _HAWKBIT_ARTIFACT
	{
		int size = 0;
		char fileName[100] = "";

		HAWKBIT_HASHES hashes;

	} HAWKBIT_ARTIFACT, * LP_HAWKBIT_ARTIFACT;


	typedef struct _HAWKBIT_DEPLOYMENT_DATA
	{
		char part[50] = "";
		char name[50] = "";
		int version = -1;
		int type = 0;

		HAWKBIT_ARTIFACT payload;

	} HAWKBIT_DEPLOYMENT_DATA, * LP_HAWKBIT_DEPLOYMENT_DATA;


	typedef struct _RESPONSE
	{
		int statusCode = 0;
		int type = 0;
		char detail[250] = "";
	} RESPONSE, * LP_RESPONSE;
}