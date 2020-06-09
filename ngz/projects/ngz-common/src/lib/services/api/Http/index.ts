// HTTP service
// ----------------------------------------------------------------------------

// Import dependencies
import { throwError } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { Injectable, EventEmitter } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams, HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';

// Import data-models
import { EnTT } from '@ofzza/entt-rxjs';
import { ApiRequestModel, ApiResponseModel } from '../../../data'

/**
 * HTTP service
 * Provides communication with the HTTP API
 */
@Injectable()
export class HttpService {

  /**
   * Holds API base url service was initialized with
   */
  private static _url: string = null;

  /**
   * Global HTTP error event
   */
  public static error = new EventEmitter<HttpErrorResponse>();

  /**
   * Initializes the HTTP service
   * @param url HTTP base url
   */
  public static initialize (url: string) {
    // Store initialized properties
    HttpService._url = url;
  }

  /**
   * Sets authentication token to be used with all requests
   * @param token Authentication token to be used with all requests
   */
  public static setAuthToken (token: string) {
    sessionStorage.setItem('token', token);
  }
  /**
   * Gets authentication token to be used with all requests
   * @returns Authentication token to be used with all requests
   */
  public static getAuthToken (): string {
    return sessionStorage.getItem('token');
  }
  /**
   * Revoke authentication token to be used with all requests
   */
  public static revokeAuthToken () {
    sessionStorage.setItem('token', '');
  }

  /**
   * Creates an instance of ApiService.
   */
  constructor (private _http: HttpClient) {}

  /**
   * Executes an HTTP GET request
   * @param path URI path to GET from
   * @param query URI query parameters hashmap
   * @param headers HTTP headers to set for the request
   * @param options Additional NG HTTP service options
   * @param circumvent Circumvention rules of HTTP interceptors for a particular request
   * @returns Promise of HTTP response content
   * @throws Errors returned by the API
   */
  public get (
    path: string,
    {
      query      = {} as EnTT | object,
      headers    = {} as object,
      options    = {} as object,
      circumvent = undefined as HttpInterceptorsCircumventionDefinition
    } = {}
  ): HttpRequestPromise<any> {
    return this.request('GET', path, { query, headers, options, circumvent });
  }

  /**
   * Executes an HTTP POST request
   * @param path URI path to POST to
   * @param body Request body data model
   * @param query URI query parameters hashmap
   * @param headers HTTP headers to set for the request
   * @param options Additional NG HTTP service options
   * @param circumvent Circumvention rules of HTTP interceptors for a particular request
   * @returns Promise of HTTP response data
   * @throws Errors returned by the API
   */
  public post (
    path: string,
    body: (ApiRequestModel | object),
    {
      query      = {} as EnTT | object,
      headers    = {} as object,
      options    = {} as object,
      circumvent = undefined as HttpInterceptorsCircumventionDefinition
    } = {}
  ): HttpRequestPromise<any> {
    return this.request('POST', path, { body, query, headers, options, circumvent });
  }

  /**
   * Executes an HTTP PUT request
   * @param path URI path to PUT to
   * @param body Request body data model
   * @param query URI query parameters hashmap
   * @param headers HTTP headers to set for the request
   * @param options Additional NG HTTP service options
   * @param circumvent Circumvention rules of HTTP interceptors for a particular request
   * @returns Promise of HTTP response data
   * @throws Errors returned by the API
   */
  public put (
    path: string,
    body: (ApiRequestModel | object),
    {
      query      = {} as EnTT | object,
      headers    = {} as object,
      options    = {} as object,
      circumvent = undefined as HttpInterceptorsCircumventionDefinition
    } = {}
  ): HttpRequestPromise<any> {
    return this.request('PUT', path, { body, query, headers, options, circumvent });
  }

  /**
   * Executes an HTTP DELETE request
   * @param path URI path to DELETE from
   * @param query URI query parameters hashmap
   * @param headers HTTP headers to set for the request
   * @param options Additional NG HTTP service options
   * @param circumvent Circumvention rules of HTTP interceptors for a particular request
   * @returns Promise of HTTP response content
   * @throws Errors returned by the API
   */
  public delete (
    path: string,
    {
      query      = {} as EnTT | object,
      headers    = {} as object,
      options    = {} as object,
      circumvent = undefined as HttpInterceptorsCircumventionDefinition
    } = {}
  ): HttpRequestPromise<any> {
    return this.request('DELETE', path, { query, headers, options, circumvent });
  }

  /**
   * Executes an HTTP request
   * @param method HTTP verb to use
   * @param path URI path to GET from
   * @param body Request body data model
   * @param query URI query parameters hashmap
   * @param headers HTTP headers to set for the request
   * @param options Additional NG HTTP service options
   * @param circumvent Circumvention rules of HTTP interceptors for a particular request
   * @returns Promise of Extracted HTTP response data
   * @throws Errors returned by the API
   */
  public request <ApiResponseModelType extends ApiResponseModel> (
    method: string,
    path: string,
    {
      body       = undefined as (ApiRequestModel | object),
      query      = {} as EnTT | object,
      headers    = {} as object,
      options    = {} as object,
      circumvent = undefined as HttpInterceptorsCircumventionDefinition
    } = {}
  ): HttpRequestPromise<any> {
    // Send HTTP request
    const req = this._request<ApiResponseModelType>(method, path, { body, query, headers, options, circumvent });
    // Create and return HTTP request promise instance
    return new HttpRequestPromise(
      // Handle HTTP request promise
      async (resolve, reject) => {
        try {
          // Process HTTP response
          resolve(this._processApiResponse(await req));
        } catch (err) {
          reject(err);
        }
      },
      // Implement .cancel() method
      () => { req.cancel(); },
      // Copy request info
      req.info
    );
  }

  /**
   * Executes an HTTP request
   * @param method HTTP verb to use
   * @param path URI path to GET from
   * @param body Request body data model
   * @param query URI query parameters hashmap
   * @param headers HTTP headers to set for the request
   * @param options Additional NG HTTP service options
   * @param circumvent Circumvention rules of HTTP interceptors for a particular request
   * @returns Promise of HTTP response
   * @throws Errors returned by the API
   */
  public _request <ApiResponseModelType extends ApiResponseModel> (
    method: string,
    path: string,
    {
      body       = undefined as (ApiRequestModel | object),
      query      = {} as EnTT | object,
      headers    = {} as object,
      options    = {} as object,
      circumvent = undefined as HttpInterceptorsCircumventionDefinition
    } = {}
  ): HttpRequestPromise<any> {
    try {

      // Ready HTTP request
      const reqParams = {
        method,
        url:     `${HttpService._url}/${path.length && path[0] === '/' ? path.substr(1) : path}`,
        body:    (body ? (body instanceof EnTT ? (body as EnTT).serialize() : body) : undefined),
        headers: {
          'Content-Type': 'application/json;charset=UTF-8',
          ...headers
        },
        query:   (query instanceof EnTT ? query.serialize() : query)
      };

      // Execute HTTP request
      const req = this._http.request(
        reqParams.method,
        reqParams.url,
        {
          body:    reqParams.body,
          headers: new HttpHeaders(reqParams.headers),
          params:  (Object.entries(reqParams.query) as unknown as string[])
            .reduce((params, [key, value]) => params.set(key, value), new HttpRequestParams(circumvent)),
          ...options
        }
      );

      // Place for subscription instance to be used by .cancel() call
      let subscription;
      // Return request observable converted into a HTTP request promise
      return new HttpRequestPromise<ApiResponseModelType>(
        (resolve, reject) => {
          // Subscribe and handle HTTP request promise
          subscription = req.subscribe(
            (data: ApiResponseModelType) => resolve(data),
            (err) => reject(err)
          );
        },
        // Implement .cancel() method
        () => {
          subscription.unsubscribe();
        },
        // Request info
        new HttpRequestInfo(reqParams)
      );

    } catch (err) { throw err; }
  }

  /**
   * Processes standard API response
   * @param res Response returned from API
   * @returns HTTP response data
   * @throws Errors returned by the API
   */
  private _processApiResponse <ApiResponseModelType extends ApiResponseModel> (res: ApiResponseModelType): any {
    // Process API response
    if (res.success) {
      // Process successful response
      return res.data;
    } else {
      // Throw errors
      throw new HttpServiceError(res.errors);
    }
  }

}

/**
 * HTTP error
 */
export class HttpServiceError {
  constructor (errors) { this.codes = errors.map(err => err.code); }
  public codes: string[];
}

/**
 * Extended HTTP request parameters
 */
export class HttpRequestParams extends HttpParams {
  constructor (
    public circumvent: HttpInterceptorsCircumventionDefinition
  ) {
    super();
  }
}

/**
 * HTTP Request Promise
 * Extends promise with additional .cancel() method for canceling an in-flight request
 */
export class HttpRequestPromise<T> extends Promise<T> {

  /**
   * Holds cancel method implementation
   */
  protected _cancel: () => void;

  /**
   * Holds description of request being handled by the promise
   */
  protected _info: HttpRequestInfo;
  /**
   * Description of request being handled by the promise
   */
  public get info () { return this._info; }

  /**
   * Constructor
   * @param executor Promise executor function
   * @param cancel Implementation of the export .cancel() method
   */
  constructor (
    executor: (resolve: (value?: T | PromiseLike<T>) => void, reject: (reason?: any) => void) => void,
    cancel?: () => void,
    info?: HttpRequestInfo
  ) {
    super(executor);

    // Store cancel function if provided
    this._cancel = cancel;

    // Store request info if provided
    this._info = info;

  }

  /**
   * Executes promise cancellation
   */
  public cancel () {
    if (this._cancel) { this._cancel(); }
  }

}

/**
 * HTTP Request information
 */
export class HttpRequestInfo {

  /**
   * Holds request method
   */
  public method: string;
  /**
   * Holds request URL
   */
  public url: string;
  /**
   * Holds request query
   */
  public query: object;
  /**
   * Holds request headers
   */
  public headers: object;
  /**
   * Holds request body
   */
  public body: object;

  constructor ({
    method  = undefined as string,
    url     = undefined as string,
    query   = {} as object,
    headers = {} as object,
    body    = {} as object
  } = {}) {
    // Set properties
    this.method     = method;
    this.url     = url;
    this.query   = query;
    this.headers = headers;
    this.body    = body;
  }

}

/**
 * Define circumvention rules of HTTP interceptors for a particular request
 */
export class HttpInterceptorsCircumventionDefinition {
  constructor (
    public all = false,
    public allOutgoing = false,
    public allIncomming = false,
    public httpAuthTokenInjector = false,
    public httpErrorInterceptor = false,
  ) {}
}

/**
 * Intercepts HTTP requests and injects authentication token
 */
@Injectable()
export class HttpAuthTokenInjector implements HttpInterceptor {

  public intercept (req: HttpRequest<any>, next: HttpHandler) {

    // Check if being circumvented
    if (req.params instanceof HttpRequestParams) {
      const params = (req.params as HttpRequestParams)
      if (params?.circumvent?.all || params?.circumvent?.allOutgoing || params?.circumvent?.httpAuthTokenInjector) {
        // Continue processing unmodified request
        return next.handle(req);
      }
    }

    // Get authentication token
    const token = HttpService.getAuthToken();
    if (token) {

      // Inject token into request
      const authenticatedReq = req.clone({ headers: req.headers.set('Authorization', `Bearer ${ token }`) });
      return next.handle(authenticatedReq);

    } else {

      // Continue processing unmodified request
      return next.handle(req);

    }
  }

}

/**
 * Intercepts HTTP requests and surfaces API errors
 */
@Injectable()
export class HttpErrorInterceptor implements HttpInterceptor {

  public intercept (req: HttpRequest<any>, next: HttpHandler) {

    // Check if being circumvented
    if (req.params instanceof HttpRequestParams) {
      const params = (req.params as HttpRequestParams)
      if (params?.circumvent?.all || params?.circumvent?.allIncomming || params?.circumvent?.httpErrorInterceptor) {
        // Continue processing unmodified request
        return next.handle(req);
      }
    }

    // Intercept errors
    return next.handle(req)
      .pipe(
        map((event: HttpEvent<any>) => event),
        catchError((err: HttpErrorResponse) => {
          HttpService.error.emit(err);
          return throwError(err);
        })
      );

  }
}
