using System;
using Orleans;

namespace Blauhaus.Orleans.TestHelpers.BaseTests
{
    public abstract class BaseStringGrainTest<TSut> : BaseGrainTest<TSut, string> where TSut : Grain, IGrainWithStringKey
    {
        
        public override void Setup()
        {
            base.Setup();

            GrainId = Guid.NewGuid().ToString();
        }

        protected override TSut ConstructSut()
        {
            return Silo.CreateGrainAsync<TSut>(GrainId).GetAwaiter().GetResult();
        }

        
    }
}