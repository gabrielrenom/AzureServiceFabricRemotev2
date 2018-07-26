# AzureServiceFabricRemotev2
ECommerce modification from Pluralsight. The project in the Pluralsight course doesn't work in the latest version of the Azure Service Fabric with .NET Core. I have created a similar solution which shows the approach of how to fix the issue.

The issue it is solved implementing a IServiceRemotingMessageSerializationProvider for FabricTransportServiceRemotingListener

We are talking about this concepts here:
* FabricTransportServiceRemotingClientFactory
* ServiceProxyFactory
* CreateServiceReplicaListeners
* Stateful Service
* FabricTransportServiceRemotingListener
* IServiceRemotingMessageSerializationProvider
* ServiceRemotingDataContractSerializationProvider

The entire solution has used Microsoft.ServiceFabric.Services.Remoting v3.2.162 and Microsoft.NETCore.App v2.0.0
