namespace Sqlx.Postgres.Generator;

internal static class RenameExtensions
{
    extension(Rename rename)
    {
        public string TransformName(string name)
        {
            return rename switch
            {
                Rename.SnakeCase => name.ToSnakeCase(),
                Rename.PascalCase => name.ToPascalCase(),
                Rename.CamelCase => name.ToCamelCase(),
                _ => name,
            };
        }
    }
}
