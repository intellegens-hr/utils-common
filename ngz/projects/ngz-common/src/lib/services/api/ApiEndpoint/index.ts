// API Endpoint service
// ----------------------------------------------------------------------------

// Import dependencies
import { EnTT, EnttValidationError } from '@ofzza/entt-rxjs';
import { Injectable, EventEmitter } from '@angular/core';
import { HttpService, HttpServiceError, HttpRequestPromise } from '../Http';

// Import data-models
import { ApiSearchRequestModel, ApiSearchResponseModel } from '../../../data';

/**
 * Enumerated endpoint actions
 */
export enum ApiEndpointAction {
  CREATE = 'create',
  UPDATE = 'update',
  DELETE = 'delete'
}

/**
 * API Endpoint service
 * Provides communication with a standardized API endpoint
 */
export class ApiEndpoint {

  /**
   * Global API error event
   */
  public static error = new EventEmitter<EnttValidationError[]>();

  /**
   * Global API action event
   */
  public static action = new EventEmitter<ApiEndpointActionEvent>();

  /**
   * Initializes the API service
   */
  public static initialize () {
    // Store initialized properties
    HttpService.error.subscribe((err) => {
      if (err?.error?.errors) {
        ApiEndpoint.error.emit(
          err.error.errors.map(error => new EnttValidationError({ type: error.code }))
        );
      } else {
        ApiEndpoint.error.emit([new EnttValidationError({ type: 'unknown' })]);
      }
    });
  }

  /**
   * API action event
   */
  public action = new EventEmitter<ApiEndpointActionEvent>();

  /**
   * Holds endpoint name (relative path)
   */
  private _endpoint: string;

  /**
   * Holds (optional) EnTT class to cast response as
   */
  private _entt: (new() => EnTT);
  /**
   * Holds function converting EnTT instance to a representative string
   */
  protected _enttToString = undefined as (entt: EnTT) => string

  constructor (private _http: HttpService) {}

  /**
   * Binds service instance to a particular endpoint
   * @param endpoint Endpoint name (relative path)
   * @param entt (Optional) EnTT class to cast response as
   * @param enttToString (Optional) Function converting EnTT instance to a representative string
   */
  public bind (endpoint: string, entt?: (new() => EnTT), { enttToString = undefined as (entt: EnTT) => string } = {}) {
    this._endpoint = endpoint;
    this._entt = entt;
    this._enttToString = enttToString;
  }

  /**
   * Lists all items contained by the endpoint
   * @returns All items the endpoint contains
   * @throws Errors returned by the API
   */
  public list () {
    return this._action(
      this._http.get(this._endpoint),
      (data: any[]) => (this._entt ? EnTT.cast(data, { into: [this._entt] }) : data)
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
      (data: any[]) => {
        if (data && data.length) {
          return (this._entt ? EnTT.cast(data[0], { into: this._entt }) : data[0]);
        } else {
          return null;
        }
      }
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
      (data: any[]) => {
        const result = (data && data.length ? (this._entt ? EnTT.cast(data[0], { into: this._entt }) : data[0]) : undefined);
        this._triggerActionExecutedEvent(ApiEndpointAction.CREATE, result);
        return result;
      }
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
      this._http.put(`${this._endpoint}/${id}`, item),
      (data: any[]) => {
        const result = (data && data.length ? (this._entt ? EnTT.cast(data[0], { into: this._entt }) : data[0]) : undefined);
        this._triggerActionExecutedEvent(ApiEndpointAction.UPDATE, result);
        return result;
      }
    );
  }

  /**
   * Deletes an item from the endpoint
   * @param id Id of the endpoint item
   * @param item (Optional) Item to update
   * @throws Errors returned by the API
   */
  public delete (id: any, item?: any) {
    return this._action(
      this._http.delete(`${this._endpoint}/${id}`),
      () => {
        this._triggerActionExecutedEvent(ApiEndpointAction.DELETE, item);
      }
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
      // Handle HTTP request promise
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
      // Implement .cancel() method
      () => { req.cancel(); },
      // Copy request info
      httpReqPromise.info
    );
  }

  /**
   * Triggers global and local action events
   * @param action Executed action
   * @param entt EnTT instance resulting from the action
   */
  private _triggerActionExecutedEvent (action: ApiEndpointAction, entt?: any) {
    // Compose action descriptor
    const e = new ApiEndpointActionEvent(action, entt, this._enttToString);
    // Trigger local event
    if (!e._preventedDefault) { this.action.emit(e); }
    // Trigger global event
    if (!e._preventedDefault) { ApiEndpoint.action.emit(e); }
  }

}

/**
 * Holds description of an executed endpoint action
 */
export class ApiEndpointActionEvent {

  /**
   * Holds .preventDefault() method having been called status
   */
  public _preventedDefault = false;

  constructor (action: ApiEndpointAction, entt?: any, enttToString?: ((entt: any) => string)) {
    // Store properties
    this.action = action;
    this.entt = entt;
    this.enttToString = enttToString;
  }

  /**
   * Executed action
   */
  public action: ApiEndpointAction;
  /**
   * EnTT instance resulting from the action
   */
  public entt: any;
  /**
   * Function converting the EnTT instance into a string representation
   */
  public enttToString: (entt: any) => string

  /**
   * Prevents the event from continuing execution (usually used in local event handler to prevent global from triggering)
   */
  public preventDefault () {
    this._preventedDefault = true;
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
   * @param enttToString (Optional) Function converting EnTT instance to a representative string
   */
  public create (endpoint: string, entt?: (new() => EnTT), { enttToString = undefined as (entt: EnTT) => string } = {}) {
    const service = new ApiEndpoint(this._http);
    service.bind(endpoint, entt, { enttToString });
    return service;
  }
}
