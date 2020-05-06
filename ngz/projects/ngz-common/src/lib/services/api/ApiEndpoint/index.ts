// API Endpoint service
// ----------------------------------------------------------------------------

// Import dependencies
import { Injectable } from '@angular/core';
import { EnTT } from '@ofzza/entt-rxjs';
import { HttpService, HttpServiceError, HttpRequestPromise } from '../Http';

// Import data-models
import { ApiSearchRequestModel, ApiSearchResponseModel } from '../../../data';

/**
 * API Endpoint service
 * Provides communication with a standardized API endpoint
 */
export class ApiEndpoint {

  /**
   * Holds endpoint name (relative path)
   */
  private _endpoint: string;

  /**
   * Holds (optional) EnTT class to cast response as
   */
  private _entt: (new() => EnTT);

  constructor (private _http: HttpService) {}

  /**
   * Binds service instance to a particular endpoint
   * @param endpoint Endpoint name (relative path)
   * @param entt (Optional) EnTT class to cast response as
   */
  public bind (endpoint: string, entt?: (new() => EnTT)) {
    this._endpoint = endpoint;
    this._entt = entt;
  }

  /**
   * Lists all items contained by the endpoint
   * @returns All items the endpoint contains
   * @throws Errors returned by the API
   */
  public list () {
    return this._action(
      this._http.get(this._endpoint),
      (data) => (this._entt ? EnTT.cast(data, { into: this._entt }) : data)
    );
  }

  /**
   * Searches API endpoint
   * @param req Search request
   * @returns Search results returned by the endpoint
   * @throws Errors returned by the API
   */
  public search (req: ApiSearchRequestModel) {
    return this._action(
      this._http._request<ApiSearchResponseModel>('POST', `${this._endpoint}/search`, { body: req }),
      (res) => {
        if (res.success) {
          // Process and cast successful response
          const data     = (this._entt ? EnTT.cast(res.data, { into: [this._entt] }) : res.data),
                metadata = res.metadata;
          // Return data
          return { data, metadata };
        } else {
          // Throw errors
          throw new HttpServiceError(res.errors);
        }
      }
    );
  }

  /**
   * Gets single endpoint item
   * @param id Id of the endpoint item
   * @returns Single endpoint item
   * @throws Errors returned by the API
   */
  public get (id: any) {
    return this._action(
      this._http.get(`${this._endpoint}/${id}`),
      (data) => (this._entt ? EnTT.cast(data, { into: this._entt }) : data)
    );
  }

  /**
   * Register a newly created item with the endpoint
   * @param item Newly created item
   * @returns Newly created item
   * @throws Errors returned by the API
   */
  public create (item: any) {
    return this._action(
      this._http.post(this._endpoint, item),
      (data) => (this._entt ? EnTT.cast(data, { into: this._entt }) : data)
    );
  }

  /**
   * Updates an item with the endpoint
   * @param id Id of the endpoint item
   * @param item Item to update
   * @returns Updated item
   * @throws Errors returned by the API
   */
  public update (id: any, item: any) {
    return this._action(
      this._http.post(`${this._endpoint}/${id}`, item),
      (data) => (this._entt ? EnTT.cast(data, { into: this._entt }) : data)
    );
  }

  /**
   * Deletes an item from the endpoint
   * @param id Id of the endpoint item
   * @throws Errors returned by the API
   */
  public delete (id: any) {
    return this._action(
      this._http.delete(`${this._endpoint}/${id}`),
      (data) => (this._entt ? EnTT.cast(data, { into: this._entt }) : data)
    );
  }

  /**
   * Executes an HTTP request action, wrapping it in an HTTP request promise and casts results once received as EnTT
   * @param httpReqPromise Function that should execute an HTTP action and return a HTTP request promise
   * @param processDataCallback Function called on received data, allowed to pre-process it before being resolved
   * @returns HTTP request promise of response casts as EnTT
   */
  private _action <ApiResponseModelType> (
    httpReqPromise: HttpRequestPromise<ApiResponseModelType>,
    processDataCallback?: (data: ApiResponseModelType) => any
  ) {
    // Place for internal HTTP request promise to be used by .cancel() call
    let req: HttpRequestPromise<ApiResponseModelType>;
    // Create and return HTTP request instance
    return new HttpRequestPromise<ApiResponseModelType>(
      async (resolve, reject) => {
        try {
          // Execute requestor function to get a HTTP request promise
          const data = await (req = httpReqPromise);
          // Process result if required and resolve
          resolve(processDataCallback ? processDataCallback(data) : data);
        } catch (err) {
          reject(err);
        }
      },
      () => { req.cancel(); }
    );
  }

}

/**
 * API Endpoint service factory
 * Instantiates ApiEndpoint service instances, providing communication with a standardized API endpoint
 */
@Injectable()
export class ApiEndpointFactory {
  constructor (private _http: HttpService) {}

  /**
   * Creates a new api endpoint instance
   * @param endpoint Endpoint name (relative path)
   * @param entt (Optional) EnTT class to cast response as
   */
  public create (endpoint: string, entt?: (new() => EnTT)) {
    const service = new ApiEndpoint(this._http);
    service.bind(endpoint, entt);
    return service;
  }
}
