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
        RequestAll = RequestBody | RequestHeaders,

        ResponseBody = 8,
        ResponseHeaders = 16,

        ResponseAll = ResponseBody | ResponseHeaders,

        All = ResponseAll | RequestAll
    }
}