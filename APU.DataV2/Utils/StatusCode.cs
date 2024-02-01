namespace APU.DataV2.Utils;

public enum StatusCode
{
    /// <summary>
    /// Default OK. With data returning. On no data: use NoContent
    /// </summary>
    OK = 200,
    /// <summary>
    /// On creation of resouces. Like POST to add an entry.
    /// </summary>
    Created = 201,
    /// <summary>
    /// Async processing 'OK', but it might be that further processing reveals a failure.
    /// </summary>
    Accepted = 202,
    /// <summary>
    /// OK, but no data will return.
    /// </summary>
    NoContent = 204,

    //PartialContent = 206,  For paged content. But it needs more rework than just returning paged content https://stackoverflow.com/a/1031509



    /// <summary>
    /// Server refuses the request because something really appearent is off on the request. Malformed requests, wrong route, too large, etc.
    /// </summary>
    BadRequest = 400,
    ///// <summary>
    ///// No authorization has been provided yet or needs to be provided again. Must be accompanied by a header specifying what authorization is expected.
    ///// </summary>
    //UnAuthorized = 401,
    /// <summary>
    /// Missing access rights, or doing some invalid action like creating duplicate data.
    /// </summary>
    Forbidden = 403,
    /// <summary>
    /// Requested resource is not found in the database
    /// </summary>
    NotFound = 404,
    /// <summary>
    /// Using POST/GET/PUT/PATCH wrong. Or trying to PUT on a resource that is read only.
    /// </summary>
    MethodNotAllowed = 405,
    /// <summary>
    /// This data is conflicting with a previous update. Like an ETAG not matching up.
    /// </summary>
    Conflict = 409,
    /// <summary>
    /// Implementation has moved. Like using a depricated API call and finally it is removed, it returns 410.
    /// </summary>
    Gone = 410,

    /// <summary>
    /// Unexpected failure.
    /// </summary>
    InternalServerError = 500,

    /// <summary>
    /// Method not implemented yet.
    /// </summary>
    NotImplemented = 501,
}
