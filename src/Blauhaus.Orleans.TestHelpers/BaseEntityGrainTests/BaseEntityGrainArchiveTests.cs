using System;
using System.Linq;
using System.Threading.Tasks;
using Blauhaus.Analytics.TestHelpers.Extensions;
using Blauhaus.Auth.Abstractions.Errors;
using Blauhaus.Auth.Abstractions.User;
using Blauhaus.Domain.Abstractions.CommandHandlers;
using Blauhaus.Domain.Abstractions.Commands;
using Blauhaus.Domain.Abstractions.DtoHandlers;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Abstractions.Errors;
using Blauhaus.Domain.Server.Entities;
using Blauhaus.Domain.TestHelpers.EntityBuilders;
using Blauhaus.Orleans.Abstractions.Resolver;
using Blauhaus.Orleans.EfCore.Grains;
using Blauhaus.Orleans.TestHelpers.BaseTests;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using EntityState = Blauhaus.Domain.Abstractions.Entities.EntityState;

namespace Blauhaus.Orleans.TestHelpers.BaseEntityGrainTests
{
    public abstract class BaseEntityGrainArchiveTests<TDbContext, TGrain, TEntity, TEntityBuilder, TGrainResolver, TDto> 
        : BaseEntityGrainTest<TDbContext, TGrain, TEntity, TEntityBuilder, TGrainResolver>
        where TGrain: BaseEntityGrain<TGrain, TDbContext, TEntity, TGrainResolver>, IVoidAuthenticatedCommandHandler<ArchiveCommand, IAuthenticatedUser> 
        where TEntity : BaseServerEntity, IDtoOwner<TDto>
        where TEntityBuilder : BaseServerEntityBuilder<TEntityBuilder, TEntity>
        where TDbContext : DbContext
        where TDto : IClientEntity<Guid>
        where TGrainResolver : class, IGrainResolver
    {
        protected ArchiveCommand Command = null!;

        public override void Setup()
        {
            base.Setup();
            Command = new ArchiveCommand();
        }

        [Test]
        public async Task IF_not_admin_user_SHOULD_fail()
        {
            //Act
            var result = await Sut.HandleAsync(Command, User);

            //Assert
            result.VerifyResponseError(AuthError.NotAuthorized, MockAnalyticsService);
            Assert.That(DbContextAfter.Set<TEntity>().First(x => x.Id == GrainId).EntityState, Is.Not.EqualTo(EntityState.Archived));
        }

        [TestCase(EntityState.Draft)]
        [TestCase(EntityState.Archived)]
        [TestCase(EntityState.Deleted)]
        public async Task IF_not_Active_SHOULD_fail(EntityState invalidState)
        {
            //Arrange
            ExistingEntityBuilder.With(x => x.EntityState, invalidState);

            //Act
            var result = await Sut.HandleAsync(Command, AdminUser);

            //Assert
            result.VerifyResponseError(DomainErrors.InvalidEntityState(invalidState), MockAnalyticsService);
        }

        [TestCase(EntityState.Active)]
        public async Task IF_Active_SHOULD_Activate(EntityState validState)
        {
            //Arrange
            ExistingEntityBuilder.With(x => x.EntityState, validState);

            //Act
            var result = await Sut.HandleAsync(Command, AdminUser);

            //Assert
            Assert.That(result.IsSuccess);
            var modifiedEntity = DbContextAfter.Set<TEntity>().First(x => x.Id == GrainId);
            Assert.That(modifiedEntity.ModifiedAt, Is.EqualTo(RunTime));
            Assert.That(modifiedEntity.EntityState, Is.EqualTo(EntityState.Archived)); 
        }
    }
}