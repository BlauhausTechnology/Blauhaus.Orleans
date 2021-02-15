using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Streams;

namespace Blauhaus.Orleans.Grains
{
    public abstract class BaseIdGrain : BaseGrain, IGrainWithGuidKey
    {
        protected Guid Id;

        public override Task OnActivateAsync()
        {
            Id = this.GetPrimaryKey();

            if (Id == Guid.Empty)
            {
                throw new ArgumentException($"Grain requires a GUID id. \"{this.GetPrimaryKey()}\" is not valid");
            }

            return base.OnActivateAsync();
        }

       
    }
}