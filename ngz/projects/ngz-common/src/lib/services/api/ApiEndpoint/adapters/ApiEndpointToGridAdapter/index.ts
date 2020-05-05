// API-to-Grid adapter service
// ----------------------------------------------------------------------------

// Import dependencies
import { Injectable } from '@angular/core';
import { EnTT } from '@ofzza/entt-rxjs';
import { ApiEndpointFactory, ApiEndpoint } from '../../';

// Import data models
import { ApiSearchRequestModel, ApiSearchRequestOrderModel, ApiSearchRequestFilterModel } from '../../../../../data';

/**
 * Adapts standard API endpoint(s) for usage by a <ngz-grid /> component (internal implementation)
 */
class ApiEndpointToGridAdapterInternal {

  /**
   * Injected ApiEndpoint service instance
   */
  protected _endpoint: ApiEndpoint;

  /**
   * If all data should be loaded initially and all further processing done locally
   */
  protected _preload = true;

  /**
   * Default page length, used when not otherwise specified by the adapted <ngz-grid /> component
   */
  protected _pageLength = 20;

  /**
   * Holds search request parameters
   */
  protected _req = new ApiSearchRequestModel();

  /**
   * Holds promise of items found by the last search
   */
  protected _dataSource = Promise.resolve([]);

  /**
   * Holds total number of records found by the last search
   */
  protected _dataLength = 0;

  /**
   * Binds service instance to a particular endpoint
   * @param endpoint Endpoint name (relative path)
   * @param entt (Optional) EnTT class to cast response as
   */
  protected _bind (endpoint: string, entt?: (new() => EnTT)) {
    // Bind to endpoint
    this._endpoint.bind(endpoint, entt);
    // Reset request
    this._req = new ApiSearchRequestModel();
    this._req.limit = this._pageLength;
    // Search (after timeout to allow additional configuration)
    setTimeout(() => { this._search(); });
  }

  /**
   * Executes a search with current search request parameters
   */
  protected _search () {
    this._dataSource = new Promise(async (resolve, reject) => {
      try {

        // Check if running locally
        if (!this._preload) {

          // Run search
          const res = await this._endpoint.search(this._req);
          // Set metadata
          this._dataLength = res.metadata.totalRecordCount;
          // Resolve data
          resolve(res.data);

        } else {

          // Load all data from endpoint
          const data = await this._endpoint.list();
          // Set metadata
          this._dataLength = undefined;
          // Resolve data
          resolve(data);

        }

      } catch (err) { reject(err); }
    });
  }

  /**
   * Grid input adapter: handles grid component's change event, updates and reruns the search
   * @param e Grid change event descriptor
   */
  protected _changed (e: any) {
    // Check if running locally
    if (!this._preload) {
      // Update search request
      this._req.offset = e.state.pageIndex * e.state.pageLength;
      this._req.limit  = e.state.pageLength;
      this._req.ordering = [];
      const order = new ApiSearchRequestOrderModel();
      order.key = e.state.orderingField;
      order.ascending = e.state.orderingAscDirection;
      this._req.ordering.push(order);
      this._req.filters = [];
      for (const filterKey in e.state.filters) {
        if (e.state.filters.hasOwnProperty(filterKey)) {
          const filter = new ApiSearchRequestFilterModel();
          filter.key = filterKey;
          filter.value = e.state.filters[filterKey];
          this._req.filters.push(filter);
        }
      }
      // (Re)Run search
      this._search();
    }
  }

}

/**
 * API Endpoint to Grid component adapter
 * Adapts standard API endpoint(s) for usage by a <ngz-grid /> component
 */
export class ApiEndpointToGridAdapter extends ApiEndpointToGridAdapterInternal {

  /**
   * Gets underlying endpoint service instance
   */
  public get endpoint () {
    return this._endpoint;
  }

  /**
   * Gets/Sets value controlling if all data will be preloaded in advance and all later processing will be handled locally,
   * or if only visible data will be loaded at any time and all data processing wil lbe deferred to the api endpoint
   */
  public set preload (value) { this._preload = value; }
  public get preload () { return this._preload; }

  /**
   * Gets/Sets Default page length, used when not otherwise specified by the adapted <ngz-grid /> component
   */
  public set pageLength (value) { this._pageLength = value; }
  public get pageLength () { return this._pageLength; }


  /**
   * Grid input adapter: Gets promise of items found by the last search
   */
  public get dataSource () {
    return this._dataSource;
  }

  /**
   * Grid input adapter: Gets total number of records found by the last search
   */
  public get dataLength () {
    return this._dataLength;
  }

  constructor (private _endpointFactory: ApiEndpointFactory) {
    super();
  }

  /**
   * Binds service instance to a particular endpoint
   * @param endpoint Endpoint name (relative path)
   * @param entt (Optional) EnTT class to cast response as
   */
  public bind (endpoint: string, entt?: (new() => EnTT)) {
    // (Re)Create endpoint instance
    this._endpoint = this._endpointFactory.create(endpoint, entt);
    // Bind to endpoint
    this._bind(endpoint, entt);
  }

  /**
   * Repeats latest search request
   */
  public refresh () {
    this._search();
  }

  /**
   * Grid input adapter: handles grid component's change event, updates and reruns the search
   * @param e Grid change event descriptor
   */
  public changed (e: any) {
    this._changed(e);
  }


}

/**
 * API Endpoint to Grid component adapter factory
 * Instantiates ApiEndpointToGridAdapter instances
 */
@Injectable()
export class ApiEndpointToGridAdapterFactory {
  constructor (private _endpointFactory: ApiEndpointFactory) {}

  /**
   * Creates a new adapter instance
   * @param endpoint Endpoint name (relative path)
   * @param entt (Optional) EnTT class to cast response as
   */
  public create (endpoint: string, entt?: (new() => EnTT)) {
    const adapter = new ApiEndpointToGridAdapter(this._endpointFactory);
    adapter.bind(endpoint, entt);
    return adapter;
  }
}
