using System;
using System.Threading;
using System.Threading.Tasks;
using ForteConfigurationLoader.CmdExecutionLayer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ForteConfigurationLoader.InteropLayer.HostedService
{
    public class DataConsumer : IHostedService
    {
        private readonly CommunicationService _hawkbitUnmanagedApi;
        private readonly  ILogger<DataConsumer> _logger;
        private readonly ICmdSet _commands;
        
        public DataConsumer(CommunicationService hawkbitUnmanagedApi, ICmdSet commands, ILogger<DataConsumer> logger)
        {
            _hawkbitUnmanagedApi = hawkbitUnmanagedApi;
            _commands = commands;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
             Task.Run(() =>
             {
                 while (!cancellationToken.IsCancellationRequested)
                 {
                    
                     NativeResponse response = default;
                     try
                     {
                         NativeHawkbitDeploymentData chunk;
                         chunk.type = (int) NativeDeploymentDataType.eUndefined;
                         
                         _logger.LogInformation("waiting for data from hawkbit client...");
                         
                         //packet representation: [start transaction]->[eBeginPayload]->[ePayloadContent][ePayloadContent][...]->[eEndPayload]

                         _hawkbitUnmanagedApi.Get(out chunk);
                         if (chunk.type != (int) NativeDeploymentDataType.eStartTransaction)
                             throw new Exception("incorrect type of chunk.");

                         _hawkbitUnmanagedApi.Get(out chunk);
                         if (chunk.type != (int) NativeDeploymentDataType.eBeginPayload)
                             throw new Exception("incorrect type of chunk.");

                        // _executor.StartTransaction();
                        _commands.StartTransaction(); 
                        
                         //take DeploymentData from unmanaged code
                         //for a while we will not get end of flag (eEndOfData)
                         while (chunk.type != (int) NativeDeploymentDataType.eEndPayload)
                         {
                             //read data
                             if (chunk.type == (int) NativeDeploymentDataType.ePayloadContent)
                             {
                                 _logger.LogInformation($"received deployment base info part:{chunk.part}; name:{chunk.name}; version = {chunk.version}");
                                 //_executor.FromDeploymentData(chunk);
                                 _commands.FromDeploymentData(chunk);
                             }

                             //take next chunk
                             _hawkbitUnmanagedApi.Get(out chunk);
                         }
                         
                       
                         if (_commands.Execute())
                         {
                             response.detail = "success publishing config";
                             //_executor.CommitTransaction();
                             _logger.LogInformation(response.detail);
                             _commands.CommitTransaction();
                         }
                         else
                         {
                             response.detail = "rollback publishing config";
                             _logger.LogWarning(response.detail);
                             _commands.RollBackTransaction();
                         }

                         response.type = (int)NativeDeploymentDataType.eCommitTransaction;

                     }
                     catch (Exception ex)
                     {
                         _logger.LogError(ex.Message);
                         response.detail = ex.Message;
                         response.type = (int) NativeDeploymentDataType.eAbortTransaction;
                     }
                     finally
                     {
                         _hawkbitUnmanagedApi.Put(ref response);
                     }
                 }
             }, cancellationToken);
            
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
