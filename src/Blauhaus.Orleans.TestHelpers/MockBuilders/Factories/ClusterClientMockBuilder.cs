using Moq;
using Orleans;
using Orleans.Streams;
using System;

namespace Blauhaus.Orleans.TestHelpers.MockBuilders.Factories
{
    public class ClusterClientMockBuilder : BaseGrainFactoryMockBuilder<ClusterClientMockBuilder, IClusterClient>
    {
        
        public ClusterClientMockBuilder Where_GetStream_returns<T>(IAsyncStream<T> stream)
        {
            var streamProvider = new Mock<IStreamProvider>();
            streamProvider.Setup(x => x.GetStream<T>(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(stream);
            Mock.Setup(x => x.GetStreamProvider(It.IsAny<string>())).Returns(streamProvider.Object);
            return this;
        }

        public ClusterClientMockBuilder Where_GetStream_returns<T>(IAsyncStream<T> stream, string providerName, string streamName, Guid id)
        {
            var streamProvider = new Mock<IStreamProvider>();
            streamProvider.Setup(x => x.GetStream<T>(id, streamName)).Returns(stream);
            Mock.Setup(x => x.GetStreamProvider(providerName)).Returns(streamProvider.Object);
            return this;
        }
    }
}