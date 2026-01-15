using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Gateway.Api.Extensions;

public static class ResultExtensions
{
    public static IResult WithCookie(this IResult result, string name, string value, CookieOptions options)
    {
        return new CookieResult(result, name, value, options);
    }
}

public class CookieResult : IResult
{
    private readonly IResult _result;
    private readonly string _name;
    private readonly string _value;
    private readonly CookieOptions _options;

    public CookieResult(IResult result, string name, string value, CookieOptions options)
    {
        _result = result;
        _name = name;
        _value = value;
        _options = options;
    }

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.Cookies.Append(_name, _value, _options);
        await _result.ExecuteAsync(httpContext);
    }
}
