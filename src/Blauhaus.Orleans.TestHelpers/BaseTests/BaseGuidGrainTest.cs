using System;
using Blauhaus.Orleans.Grains;
using Orleans;

namespace Blauhaus.Orleans.TestHelpers.BaseTests
{
    public abstract class BaseGuidGrainTest<TSut> : BaseGrainTest<TSut, Guid> where TSut : BaseIdGrain
    {
        
        
        protected override void HandleSetup()
        {
            GrainId = Guid.NewGuid();
        }


        protected override TSut ConstructSut()
        {
            return Silo.CreateGrainAsync<TSut>(GrainId).GetAwaiter().GetResult();
        }


    }
}