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
        RequestCookies = 32,
        RequestAll = RequestBody | RequestHeaders | RequestCookies,

        ResponseBody = 8,
        ResponseHeaders = 16,

        ResponseAll = ResponseBody | ResponseHeaders,

        All = ResponseAll | RequestAll
    }
}