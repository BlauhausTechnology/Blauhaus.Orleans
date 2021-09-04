using System;
using Blauhaus.Orleans.Grains;
using Orleans;

namespace Blauhaus.Orleans.TestHelpers.BaseTests
{
    public class BaseGuidGrainTest<TSut> : BaseGrainTest<TSut, Guid> where TSut : Grain, IGrainWithGuidKey
    {
        public override void Setup()
        {
            base.Setup();

            GrainId = Guid.NewGuid();
        }

        protected override TSut ConstructGrain()
        {
            return Silo.CreateGrainAsync<TSut>(GrainId).GetAwaiter().GetResult();
        }

    }
}