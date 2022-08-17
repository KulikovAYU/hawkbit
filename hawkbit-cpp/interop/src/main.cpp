#include <filesystem>
#include <string>
#include <deque>
#include <mutex>
#include <memory>
#include <condition_variable>
#include "hawkbitApi.hpp"
#include "hawkbit.hpp"

namespace hawkbit_interop
{

#define ARRAYSIZE(a) \
  ((sizeof(a) / sizeof(*(a))) / \
  static_cast<size_t>(!(sizeof(a) % sizeof(*(a)))))

	enum class StatCode
	{
		eOk = 200,
		eError = 300,
		eFailed = 400
	};

	enum class DeploymentDataType
	{
		eUndefined = -1,
		eBeginPayload = 0,
		ePayloadContent = 1,
		eEndPayload = 2,
		eStartTransaction = 3,
		eCommitTransaction = 4,
		eAbortTransaction = 5
	};


#pragma region UnboundedMpMcBuffer

	template<typename T>
	class ThreadSafeBuffer
	{
	public:
		// produce  data to the worker thread
		void put(T data)
		{
			std::unique_lock<std::mutex> lock(m_isBusy);
			m_buffer.push_back(std::move(data));
			m_notEmpty.notify_all();
		}

		// consume data to the worker thread
		T get()
		{
			std::unique_lock<std::mutex> lock(m_isBusy);
			m_notEmpty.wait(lock, [this]() {return !m_buffer.empty(); });//wait for pushing task
			return take_locked();
		}

	private:
		T take_locked()
		{
			T front = std::move(m_buffer.front());
			m_buffer.pop_front();

			return front;
		}

		std::deque<T> m_buffer;
		std::condition_variable m_notEmpty;
		std::mutex m_isBusy;
	};
#pragma endregion


#pragma region Client
	class HawkbitClient
	{

		struct ConnectionCfg
		{
			std::string m_controllerId;
			std::string m_hawkbitEndpoint;
			std::string m_gatewayToken;
			std::string m_downloadPath;
		};

		class DeploymentBaseFeedbackDeliveryListener : public hawkbit::ResponseDeliveryListener {
		public:
			void onSuccessfulDelivery() override {

			}

			void onError() override {

			}
		};

		class Handler : public hawkbit::EventHandler
		{
		public:
			Handler(HawkbitClient* pClient) : m_pClient(pClient)
			{

			}

			std::unique_ptr<hawkbit::ConfigResponse> onConfigRequest() override {

				return hawkbit::ConfigResponseBuilder::newInstance()
					->addData("some", "config1")
					->addData("some1", "new config")
					->addData("some2", "RITMS123")
					->addData("some3", "TES_TEST_TEST")
					->setIgnoreSleep()
					->build();
			}

			std::unique_ptr<hawkbit::Response> onDeploymentAction(std::unique_ptr<hawkbit::DeploymentBase> dp) override
			{
				auto builder = hawkbit::ResponseBuilder::newInstance();
				builder->addDetail("Printed deployment base info");

				//send to consumer packet like: [start transaction]->[eBeginPayload]->[ePayloadContent][ePayloadContent][...]->[eEndPayload]

				HAWKBIT_DEPLOYMENT_DATA deploymentData;
				deploymentData.type = static_cast<int>(DeploymentDataType::eStartTransaction);
				m_pClient->m_messageBuffer.put(deploymentData);

				deploymentData.type = static_cast<int>(DeploymentDataType::eBeginPayload);
				m_pClient->m_messageBuffer.put(deploymentData);

				for (const auto& chunk : dp->getChunks()) {
                    HAWKBIT_DEPLOYMENT_DATA chunkDeploymentData;
                    chunkDeploymentData.type = static_cast<int>(DeploymentDataType::ePayloadContent);

					//ATTENTION: Must to copy chunks
					std::string sData = chunk->getPart();
					std::copy(sData.begin(), sData.end(), chunkDeploymentData.part);

					sData = chunk->getName();
					std::copy(sData.begin(), sData.end(), chunkDeploymentData.name);

					sData = chunk->getVersion();
                    chunkDeploymentData.version = std::stoi(sData);

					for (const auto& artifact : chunk->getArtifacts()) {

						HAWKBIT_ARTIFACT& payload = chunkDeploymentData.payload;
						payload.size = artifact->size();

						const std::string& sFileName = artifact->getFilename();
						std::copy(sFileName.begin(), sFileName.end(), payload.fileName);

						HAWKBIT_HASHES& hashes = payload.hashes;

						const std::string& sMd5Hash = artifact->getFileHashes().md5;
						std::copy(sMd5Hash.begin(), sMd5Hash.end(), hashes.md5);

						const std::string& sSha1Hash = artifact->getFileHashes().sha1;
						std::copy(sSha1Hash.begin(), sSha1Hash.end(), hashes.sha1);

						const std::string& sSha1256 = artifact->getFileHashes().sha256;
						std::copy(sSha1256.begin(), sSha1256.end(), hashes.sha256);

						builder->addDetail(artifact->getFilename() + " described. Starting download ...");

                        std::filesystem::path fsPath(m_pClient->m_connectionCfg.m_downloadPath);
                        fsPath.append(artifact->getFilename());
                        const std::string& sDownloadPath = fsPath.u8string();
						artifact->downloadTo(sDownloadPath);
						builder->addDetail("Downloaded to" + sDownloadPath);

                        m_pClient->m_messageBuffer.put(chunkDeploymentData);

                        //reset char values to default
                        std::fill( payload.fileName, payload.fileName + ARRAYSIZE(payload.fileName) - 1, 0);
                        std::fill( payload.hashes.md5, payload.hashes.md5 + ARRAYSIZE(payload.hashes.md5) - 1, 0);
                        std::fill( payload.hashes.sha1, payload.hashes.sha1 + ARRAYSIZE(payload.hashes.sha1) - 1, 0);
                        std::fill( payload.hashes.sha256, payload.hashes.sha256 + ARRAYSIZE(payload.hashes.sha256) - 1, 0);
					}
				}

				//finish set end of data
				deploymentData.type = static_cast<int>(DeploymentDataType::eEndPayload);
				m_pClient->m_messageBuffer.put(deploymentData);

				const RESPONSE& updResponse = m_pClient->m_responseBuffer.get();

				hawkbit::Response::Execution response =
					(updResponse.type ==
						static_cast<int>(DeploymentDataType::eCommitTransaction)) ?
					hawkbit::Response::Execution::CLOSED :
					hawkbit::Response::Execution::REJECTED;

				return builder->addDetail(updResponse.detail)
				->setIgnoreSleep()
				->setExecution(response)
				->setFinished(hawkbit::Response::SUCCESS)
				->setResponseDeliveryListener(std::make_shared<DeploymentBaseFeedbackDeliveryListener>())
				->build();
			}


			std::unique_ptr<hawkbit::Response> onCancelAction(std::unique_ptr<hawkbit::CancelAction> action) override {

				return hawkbit::ResponseBuilder::newInstance()
					->setExecution(hawkbit::Response::CLOSED)
					->setFinished(hawkbit::Response::SUCCESS)
					->addDetail("Some feedback")
					->addDetail("One more feedback")
					->addDetail("Really important feedback")
					->setIgnoreSleep()
					->build();
			}

			void onNoActions() override {

			}

			~Handler() = default;

			Handler() = default;

			HawkbitClient* m_pClient;
		};

	public:

		static std::unique_ptr<HawkbitClient> newInstance() {
			return std::make_unique<HawkbitClient>();
		}

		void startClient()
		{
			using namespace hawkbit;

			auto builder = DefaultClientBuilder::newInstance();
			builder->setControllerId(m_connectionCfg.m_controllerId)->setHawkbitEndpoint(m_connectionCfg.m_hawkbitEndpoint)
				->setGatewayToken(m_connectionCfg.m_gatewayToken)->notVerifyServerCertificate()
				->setEventHandler(std::make_shared<Handler>(this))
				->build()->run();
		}

		bool setConfig(hawkbit_in LP_HAWKBIT_CONNECTION_CFG pInitCfg)
		{
			std::pair<std::string, std::string> sGatewayTokenNameAndValue(pInitCfg->gatewayToken.name, pInitCfg->gatewayToken.value);
			std::pair<std::string, std::string> sHawkbitEndpointNameAndValue(pInitCfg->endPoint.name, pInitCfg->endPoint.value);
			std::pair<std::string, std::string> sHawkbitControllerIdNameAndValue(pInitCfg->controllerId.name, pInitCfg->controllerId.value);

			if (sGatewayTokenNameAndValue.second.empty())
                return false;

			if (sHawkbitEndpointNameAndValue.second.empty())
                return false;

			if (sHawkbitControllerIdNameAndValue.second.empty())
                return false;

			m_connectionCfg.m_downloadPath = pInitCfg->downloadFilesPath;
            m_connectionCfg.m_gatewayToken = sGatewayTokenNameAndValue.second;
            m_connectionCfg.m_hawkbitEndpoint = sHawkbitEndpointNameAndValue.second;
			m_connectionCfg.m_controllerId = sHawkbitControllerIdNameAndValue.second;

            return true;
		}

        void put(hawkbit_out LP_HAWKBIT_DEPLOYMENT_DATA pData)
		{
			//we wait incoming data,
			//and copy it by pointer "pData" when data is coming
			//therefore we copy data to managed struct on which points pData
			*pData = m_messageBuffer.get();
		}

		void put(hawkbit_in LP_RESPONSE pResponse)
		{
			//we put input incoming data,
			//and copy it by to response buffer

			m_responseBuffer.put(*pResponse);
		}

		ThreadSafeBuffer<HAWKBIT_DEPLOYMENT_DATA> m_messageBuffer; //data to forte loader
		ThreadSafeBuffer<RESPONSE> m_responseBuffer; //data from forte loader
		ConnectionCfg m_connectionCfg;

	};
#pragma endregion


#pragma region P/Invoke implementations

	std::unique_ptr<HawkbitClient> pInstance = HawkbitClient::newInstance();

    void StartClient() { pInstance->startClient(); }

    bool SetConfig(hawkbit_in LP_HAWKBIT_CONNECTION_CFG pInitCfg) { return pInstance->setConfig(pInitCfg); }

    void Put(hawkbit_in LP_RESPONSE pResponse) { pInstance->put(pResponse); }

	void Get(hawkbit_out LP_HAWKBIT_DEPLOYMENT_DATA pPayloadData) { pInstance->put(pPayloadData); }

#pragma endregion

}