// API-to-Grid adapter service
// ----------------------------------------------------------------------------

// Import dependencies
import { Subject, interval } from 'rxjs';
import { debounce } from 'rxjs/operators';
import { Injectable } from '@angular/core';
import { EnTT } from '@ofzza/entt-rxjs';
import { ApiEndpointFactory, ApiEndpoint } from '../../';
import { HttpRequestPromise } from '../../../Http'

// Import data models
import { ApiSearchRequestModel, ApiSearchRequestOrderModel, ApiSearchRequestFilterModel } from '../../../../../data';

/**
 * Holds ApiEndpointToGridAdapter configuration
 */
class ApiEndpointToGridAdapterConfiguration {

  /**
   * If all data should be loaded initially and all further processing done locally
   */
  public preload = true;

  /**
   * Debouncing interval to be used when handling <ngz-grid /> component's change events
   */
  public debounceInterval = 0;

  /**
   * Default page length, used when not otherwise specified by the adapted <ngz-grid /> component
   */
  public defaultPageLength = 20;

}

/**
 * Adapts standard API endpoint(s) for usage by a <ngz-grid /> component (internal implementation)
 */
class ApiEndpointToGridAdapterInternal {

  /**
   * Injected ApiEndpoint service instance
   */
  protected _endpoint: ApiEndpoint;

  /**
   * Adapter configuration
   */
  protected _config = new ApiEndpointToGridAdapterConfiguration();

  /**
   * Holds search request parameters
   */
  protected _req = new ApiSearchRequestModel();

  /**
   * Holds lats HTTP search request promise to be sent
   */
  protected _searchReqPromise: HttpRequestPromise<any>;

  /** */
  protected _changeDebouncedSubject: Subject<any>

  /**
   * Holds promise of items found by the last search
   */
  protected _dataSource = Promise.resolve([]);

  /**
   * Holds total number of records found by the last search
   */
  protected _dataLength = 0;

  constructor () {
    // Set up debounced change event handling
    this._changeDebouncedSubject = new Subject<any>();
    this._changeDebouncedSubject
      .pipe(debounce(() => interval(this._config.debounceInterval)))
      .subscribe((observer) => {
        this._processChanged(observer);
      });
  }

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
    this._req.limit = this._config.defaultPageLength;
    // Search (after timeout to allow additional configuration)
    setTimeout(() => { this._search(); });
  }

  /**
   * Executes a search with current search request parameters
   */
  protected _search () {
    this._dataSource = new Promise(async (resolve, reject) => {
      try {

        // Cancel last request, if in flight
        if (this._searchReqPromise) {
          this._searchReqPromise.cancel();
        }

        // Check if running locally
        if (!this._config.preload) {

          // Run search
          const res = await (this._searchReqPromise = this._endpoint.search(this._req));
          // Set metadata
          this._dataLength = res.metadata.totalRecordCount;
          // Resolve data
          resolve(res.data);

        } else {

          // Load all data from endpoint
          const data = await (this._searchReqPromise = this._endpoint.list());
          // Set metadata
          this._dataLength = undefined;
          // Resolve data
          resolve(data);

        }

      } catch (err) { reject(err); }
    });
  }

  /**
   * Grid input adapter: queues up change handling of change event
   * @param e Grid change event descriptor
   */
  protected _changed (e: any) {
    this._changeDebouncedSubject.next(e);
  }

  /**
   * Grid input adapter: handles grid component's change event, updates and reruns the search
   * @param e Grid change event descriptor
   */
  protected _processChanged (e: any) {
    // Check if running locally
    if (!this._config.preload) {
      // Cancel local ordering, pagination and filtering
      e.preventDefault();
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
   * Configures adapter behavior
   * @param preload If all data should be loaded initially and all further processing done locally
   * @param debounceInterval Debouncing interval to be used when handling <ngz-grid /> component's change events
   * @param defaultPageLength Default page length, used when not otherwise specified by the adapted <ngz-grid /> component
   */
  public configure ({
    preload           = undefined as boolean,
    debounceInterval  = undefined as number,
    defaultPageLength = undefined as number
  } = {}) {
    if (preload !== undefined) {
      this._config.preload = preload;
    }
    if (debounceInterval !== undefined) {
      this._config.debounceInterval = debounceInterval;
    }
    if (preload !== undefined) {
      this._config.defaultPageLength = defaultPageLength;
    }
  }

  // TODO: Deprecated, drop with next minor version
  /**
   * [DEPRECATED, use .configure() instead]
   * Gets/Sets value controlling if all data will be preloaded in advance and all later processing will be handled locally,
   * or if only visible data will be loaded at any time and all data processing wil lbe deferred to the api endpoint
   */
  public set preload (value) { this._config.preload = value; }
  public get preload () { return this._config.preload; }

  // TODO: Deprecated, drop with next minor version
  /**
   * [DEPRECATED, use .configure() instead]
   * Gets/Sets Default page length, used when not otherwise specified by the adapted <ngz-grid /> component
   */
  public set pageLength (value) { this._config.defaultPageLength = value; }
  public get pageLength () { return this._config.defaultPageLength; }


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
