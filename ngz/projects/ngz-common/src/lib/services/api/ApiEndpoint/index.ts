// API Endpoint service
// ----------------------------------------------------------------------------

// Import dependencies
import { Injectable } from '@angular/core';
import { EnTT } from '@ofzza/entt-rxjs';
import { HttpService, HttpServiceError } from '../Http';

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
  public async list () {
    try {

      // Send request
      const data = await this._http.get(this._endpoint);
      // Cast and return data
      return (this._entt ? EnTT.cast(data, { into: [this._entt] }) : data);

    } catch (err) { throw err; }
  }

  /**
   * Searches API endpoint
   * @param req Search request
   * @returns Search results returned by the endpoint
   * @throws Errors returned by the API
   */
  public async search (req: ApiSearchRequestModel) {
    try {

      // Get API response
      const res = await this._http.request<ApiSearchResponseModel>('POST', `${this._endpoint}/search`, { body: req });

      // Process API response
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

    } catch (err) { throw err; }

  }

  /**
   * Gets single endpoint item
   * @param id Id of the endpoint item
   * @returns Single endpoint item
   * @throws Errors returned by the API
   */
  public async get (id: any) {
    try {

      // Send request
      const data = await this._http.get(`${this._endpoint}/${id}`);
      // Cast and return data
      return (this._entt ? EnTT.cast(data, { into: this._entt }) : data);

    } catch (err) { throw err; }
  }

  /**
   * Register a newly created item with the endpoint
   * @param item Newly created item
   * @returns Newly created item
   * @throws Errors returned by the API
   */
  public async create (item: any) {
    try {

      // Send request
      const data = await this._http.post(this._endpoint, item);
      // Cast and return data
      return (this._entt ? EnTT.cast(data, { into: this._entt }) : data);

    } catch (err) { throw err; }
  }

  /**
   * Updates an item with the endpoint
   * @param id Id of the endpoint item
   * @param item Item to update
   * @returns Updated item
   * @throws Errors returned by the API
   */
  public async update (id: any, item: any) {
    try {

      // Send request
      const data = await this._http.post(`${this._endpoint}/${id}`, item);
      // Cast and return data
      return (this._entt ? EnTT.cast(data, { into: this._entt }) : data);

    } catch (err) { throw err; }
  }

  /**
   * Deletes an item from the endpoint
   * @param id Id of the endpoint item
   * @throws Errors returned by the API
   */
  public async delete (id: any) {
    try {

      // Send request
      await this._http.delete(`${this._endpoint}/${id}`);

    } catch (err) { throw err; }
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
