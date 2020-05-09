// HTTP service
// ----------------------------------------------------------------------------

// Import dependencies
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams, HttpInterceptor, HttpRequest, HttpHandler } from '@angular/common/http';

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
   * Holds API base url service was initialized with
   */
  private _url: string = null;

  /**
   * Creates an instance of ApiService.
   */
  constructor (private _http: HttpClient) {}

  /**
   * Initializes the API service
   * @param url API base url
   */
  public initialize (url: string) {
    // Store initialized properties
    this._url = url;
  }

  /**
   * Executes an HTTP GET request
   * @param path URI path to GET from
   * @param query URI query parameters hashmap
   * @param headers HTTP headers to set for the request
   * @param options Additional NG HTTP service options
   * @returns Promise of HTTP response content
   * @throws Errors returned by the API
   */
  public get (
    path: string,
    {
      query   = {} as EnTT | object,
      headers = {} as object,
      options = {} as object
    } = {}
  ): HttpRequestPromise<any> {
    return this.request('GET', path, { query, headers, options });
  }

  /**
   * Executes an HTTP POST request
   * @param path URI path to POST to
   * @param body Request body data model
   * @param query URI query parameters hashmap
   * @param headers HTTP headers to set for the request
   * @param options Additional NG HTTP service options
   * @returns Promise of HTTP response data
   * @throws Errors returned by the API
   */
  public post (
    path: string,
    body: (ApiRequestModel | object),
    {
      query   = {} as EnTT | object,
      headers = {} as object,
      options = {} as object
    } = {}
  ): HttpRequestPromise<any> {
    return this.request('POST', path, { body, query, headers, options });
  }

  /**
   * Executes an HTTP PUT request
   * @param path URI path to PUT to
   * @param body Request body data model
   * @param query URI query parameters hashmap
   * @param headers HTTP headers to set for the request
   * @param options Additional NG HTTP service options
   * @returns Promise of HTTP response data
   * @throws Errors returned by the API
   */
  public put (
    path: string,
    body: (ApiRequestModel | object),
    {
      query   = {} as EnTT | object,
      headers = {} as object,
      options = {} as object
    } = {}
  ): HttpRequestPromise<any> {
    return this.request('PUT', path, { body, query, headers, options });
  }

  /**
   * Executes an HTTP DELETE request
   * @param path URI path to DELETE from
   * @param query URI query parameters hashmap
   * @param headers HTTP headers to set for the request
   * @param options Additional NG HTTP service options
   * @returns Promise of HTTP response content
   * @throws Errors returned by the API
   */
  public delete (
    path: string,
    {
      query   = {} as EnTT | object,
      headers = {} as object,
      options = {} as object
    } = {}
  ): HttpRequestPromise<any> {
    return this.request('DELETE', path, { query, headers, options });
  }

  /**
   * Executes an HTTP request
   * @param method HTTP verb to use
   * @param path URI path to GET from
   * @param body Request body data model
   * @param query URI query parameters hashmap
   * @param headers HTTP headers to set for the request
   * @param options Additional NG HTTP service options
   * @returns Promise of Extracted HTTP response data
   * @throws Errors returned by the API
   */
  public request <ApiResponseModelType extends ApiResponseModel> (
    method: string,
    path: string,
    {
      body    = undefined as (ApiRequestModel | object),
      query   = {} as EnTT | object,
      headers = {} as object,
      options = {} as object
    } = {}
  ): HttpRequestPromise<any> {
    // Place for internal HTTP request instance to be used by .cancel() call
    let req;
    // Create and return HTTP request instance
    return new HttpRequestPromise(
      // Handle HTTP request promise
      async (resolve, reject) => {
        try {
          // Send HTTP request
          const res = await (req = this._request<ApiResponseModelType>(method, path, { body, query, headers, options }));
          // Process HTTP response
          resolve(this._processApiResponse(res));
        } catch (err) {
          reject(err);
        }
      },
      // Implement .cancel() method
      () => { req.cancel(); }
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
   * @returns Promise of HTTP response
   * @throws Errors returned by the API
   */
  public _request <ApiResponseModelType extends ApiResponseModel> (
    method: string,
    path: string,
    {
      body    = undefined as (ApiRequestModel | object),
      query   = {} as EnTT | object,
      headers = {} as object,
      options = {} as object
    } = {}
  ): HttpRequestPromise<any> {
    try {

      // Execute HTTP request
      const req = this._http.request(
        method,
        `${this._url}/${path.length && path[0] === '/' ? path.substr(1) : path}`,
        {
          body: (body ? (body instanceof EnTT ? body.serialize() : body) : undefined),
          headers: new HttpHeaders({
            'Content-Type': 'application/json;charset=UTF-8',
            ...headers
          }),
          params: new HttpParams({
            fromObject: (query instanceof EnTT ? query.serialize() : query) as {[param: string]: string | readonly string[]}
          }),
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
            (err: Error) => reject(err)
          );
        },
        // Implement .cancel() method
        () => {
          subscription.unsubscribe();
        }
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
 * HTTP Request Promise
 * Extends promise with additional .cancel() method for canceling an in-flight request
 */
export class HttpRequestPromise<T> extends Promise<T> {

  /**
   * Holds cancel method implementation
   */
  protected _cancel: () => void;

  /**
   * Constructor
   * @param executor Promise executor function
   * @param cancel Implementation of the export .cancel() method
   */
  constructor (
    executor: (resolve: (value?: T | PromiseLike<T>) => void, reject: (reason?: any) => void) => void,
    cancel?: () => void
  ) {
    super(executor);

    // Store cancel function if provided
    this._cancel = cancel;

  }

  /**
   * Executes promise cancellation
   */
  public cancel () {
    if (this._cancel) { this._cancel(); }
  }

}

/**
 * Intercepts HTTP requests and injects authentication token
 */
@Injectable()
export class AuthTokenInjector implements HttpInterceptor {

  public intercept (req: HttpRequest<any>, next: HttpHandler) {

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
