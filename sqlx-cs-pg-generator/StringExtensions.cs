namespace Sqlx.Postgres.Generator;

internal static class StringExtensions
{
    extension(string str)
    {
        public string ToSnakeCase()
        {
            using CharEnumerator enumerator = str.GetEnumerator();
            return new string(Convert(enumerator).ToArray());
            
            static IEnumerable<char> Convert(CharEnumerator e)
            {
                if (!e.MoveNext()) yield break;
                yield return char.ToLowerInvariant(e.Current);
                while (e.MoveNext())
                {
                    if (char.IsUpper(e.Current))
                    {
                        yield return '_';
                        yield return char.ToLowerInvariant(e.Current);
                    }
                    else
                    {
                        yield return e.Current;
                    }
                }
            }
        }
        
        public string ToPascalCase()
        {
            using CharEnumerator enumerator = str.GetEnumerator();
            return new string(ConvertAlternatingCase(enumerator, isUpperStart: true).ToArray());
        }
        
        public string ToCamelCase()
        {
            using CharEnumerator enumerator = str.GetEnumerator();
            return new string(ConvertAlternatingCase(enumerator, isUpperStart: false).ToArray());
        }
    }
        
    private static IEnumerable<char> ConvertAlternatingCase(CharEnumerator e, bool isUpperStart)
    {
        if (!e.MoveNext()) yield break;

        if (isUpperStart)
        {
            yield return char.ToUpperInvariant(e.Current);
        }
        else
        {
            yield return char.ToLowerInvariant(e.Current);
        }

        var isUpper = false;
        while (e.MoveNext())
        {
            if (char.IsLetterOrDigit(e.Current))
            {
                if (isUpper)
                {
                    yield return char.ToUpperInvariant(e.Current);
                    isUpper = false;
                }
                else
                {
                    yield return e.Current;
                }
            }
            else
            {
                isUpper = true;
                yield return e.Current;
            }
        }
    }
}
