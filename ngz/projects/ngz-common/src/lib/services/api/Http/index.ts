// HTTP service
// ----------------------------------------------------------------------------

// Import dependencies
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpInterceptor, HttpRequest, HttpHandler } from '@angular/common/http';

// Import data-models
import { EnTT } from '@ofzza/entt-rxjs';
import { ApiRequestModel, ApiResponseModel } from '../../../data';

/**
 * HTTP service
 * Provides communication with a HTTP API
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
  private url: string = null;

  /**
   * Creates an instance of HttpService.
   */
  constructor (private http: HttpClient) {}

  /**
   * Initializes the API service
   * @param url API base url
   */
  public initialize (url: string) {
    // Store initialized properties
    this.url = url;
  }

  /**
   * Executes an HTTP GET request
   * @param path URI path to GET from
   * @param query URI query parameters hashmap
   * @returns Promise of HTTP response content
   * @throws Errors returned by the API
   */
  public async get (path: string, { query = {} as object } = {}): Promise<any> {
    try {
      const res = await this.request<ApiResponseModel>('GET', path, { query });
      return this._processApiResponse(res);
    } catch (err) { throw err; }
  }

  /**
   * Executes an HTTP POST request
   * @param path URI path to POST to
   * @param body Request body data model
   * @param query URI query parameters hashmap
   * @returns Promise of HTTP response data
   * @throws Errors returned by the API
   */
  public async post (path: string, body: (ApiRequestModel | object), { query = {} as object } = {}): Promise<any> {
    try {
      const res = await this.request<ApiResponseModel>('POST', path, { body, query });
      return this._processApiResponse(res);
    } catch (err) { throw err; }
  }

  /**
   * Executes an HTTP PUT request
   * @param path URI path to PUT to
   * @param body Request body data model
   * @param query URI query parameters hashmap
   * @returns Promise of HTTP response data
   * @throws Errors returned by the API
   */
  public async put (path: string, body: (ApiRequestModel | object), { query = {} as object } = {}): Promise<any> {
    try {
      const res = await this.request<ApiResponseModel>('PUT', path, { body, query });
      return this._processApiResponse(res);
    } catch (err) { throw err; }
  }

  /**
   * Executes an HTTP DELETE request
   * @param path URI path to DELETE from
   * @param query URI query parameters hashmap
   * @returns Promise of HTTP response content
   * @throws Errors returned by the API
   */
  public async delete (path: string, { query = {} as object } = {}): Promise<any> {
    try {
      const res = await this.request<ApiResponseModel>('DELETE', path, { query });
      return this._processApiResponse(res);
    } catch (err) { throw err; }
  }

  /**
   * Executes an HTTP request
   * @param method HTTP verb to use
   * @param path URI path to GET from
   * @param body Request body data model
   * @param query URI query parameters hashmap
   * @returns Promise of HTTP response
   */
  public async request<ApiResponseModelType> (method: string, path: string, { body = undefined as (ApiRequestModel | object), query = {} as object } = {}): Promise<ApiResponseModelType> {
    try {

      // Execute HTTP request
      const httpRequest = await this.http.request(
        method,
        `${this.url}/${path.length && path[0] === '/' ? path.substr(1) : path}`,
        {
          body: (body ? (body instanceof EnTT ? body.serialize() : body) : undefined),
          headers: new HttpHeaders({
            'Content-Type': 'application/json;charset=UTF-8'
          })
        }
      );

      // Await HTTP request to resolve
      return (await httpRequest.toPromise()) as ApiResponseModelType;

    } catch (err) { throw err; }
  }

  /**
   * Processes standard API response
   * @param res Response returned from API
   * @returns HTTP response data
   * @throws Errors returned by the API
   */
  private _processApiResponse (res: ApiResponseModel): any {
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
 * API error
 */
export class HttpServiceError {
  constructor (errors) { this.codes = errors.map(err => err.code); }
  public codes: string[];
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

