namespace Flipard.Domain.Common;

public interface IFullAuditedEntity : IEntityBase<Guid>, ICreatedByEntity, IModifiedByEntity, IDeletedByEntity
{
    
}