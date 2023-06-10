using System.Data;
using Dapper;
using Dodo.Primitives;

namespace CrowdParlay.Users.Infrastructure.Persistence.SqlTypeHandlers;

public class DodoUuidTypeHandler : SqlMapper.TypeHandler<Uuid>
{
    public override void SetValue(IDbDataParameter parameter, Uuid value) =>
        parameter.Value = value.ToGuidStringLayout();
    
    public override Uuid Parse(object value) => Uuid.Parse(value.ToString()!);
}