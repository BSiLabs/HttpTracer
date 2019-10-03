using System;

namespace HttpTracer
{
    [Flags]
    public enum HttpMessageParts
    {
        Unspecified = 0,
        None = 1,

        RequestBody = 2,
        RequestHeaders = 4,
        RequestCookies = 8,
        RequestAll = RequestBody | RequestHeaders | RequestCookies,

        ResponseBody = 16,
        ResponseHeaders = 32,

        ResponseAll = ResponseBody | ResponseHeaders,

        All = ResponseAll | RequestAll
    }
}