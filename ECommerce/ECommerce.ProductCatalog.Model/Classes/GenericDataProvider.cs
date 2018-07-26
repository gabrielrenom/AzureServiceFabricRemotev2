using Microsoft.ServiceFabric.Services.Remoting.V2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECommerce.ProductCatalog.Model.Classes
{
    public class GenericDataProvider : IServiceRemotingMessageSerializationProvider
    {
        private readonly ServiceRemotingDataContractSerializationProvider _provider;
        private readonly IEnumerable<Type> _types;

        public GenericDataProvider(IList<Type> types)
        {
            this._provider = new ServiceRemotingDataContractSerializationProvider();
            this._types = types;
        }

        public IServiceRemotingMessageBodyFactory CreateMessageBodyFactory()
        {
            return this._provider.CreateMessageBodyFactory();
        }

        public IServiceRemotingRequestMessageBodySerializer CreateRequestMessageSerializer(Type serviceInterfaceType, IEnumerable<Type> requestWrappedTypes, IEnumerable<Type> requestBodyTypes = null)
        {
            var result = requestBodyTypes.Concat(this._types);
            return this._provider.CreateRequestMessageSerializer(serviceInterfaceType, result);
        }

        public IServiceRemotingResponseMessageBodySerializer CreateResponseMessageSerializer(Type serviceInterfaceType, IEnumerable<Type> responseWrappedTypes, IEnumerable<Type> responseBodyTypes = null)
        {
            var result = responseBodyTypes.Concat(this._types);
            return this._provider.CreateResponseMessageSerializer(serviceInterfaceType, result);
        }
    }
}
