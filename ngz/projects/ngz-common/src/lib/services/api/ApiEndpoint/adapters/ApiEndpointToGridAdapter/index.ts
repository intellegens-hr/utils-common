// API-to-Grid adapter service
// ----------------------------------------------------------------------------

// Import dependencies
import { Subject, interval } from 'rxjs';
import { debounce } from 'rxjs/operators';
import { Injectable } from '@angular/core';
import { EnTT } from '@ofzza/entt-rxjs';
import { ApiEndpointFactory, ApiEndpoint } from '../../';
import { HttpRequestPromise } from '../../../Http'

// Import base
import { ApiEndpointBaseAdapter } from '../ApiEndpointBaseAdapter';

// Import data models
import { ApiSearchRequestOrderModel, ApiSearchRequestFilterModel } from '../../../../../data';

/**
 * Adapts standard API endpoint(s) for usage by a <ngz-grid /> component (internal implementation)
 */
export class ApiEndpointToGridAdapterInternal extends ApiEndpointBaseAdapter {

  /**
   * Binds service instance to a particular endpoint
   * @param endpoint Endpoint name (relative path)
   * @param entt (Optional) EnTT class to cast response as
   */
  protected _bind (
    endpoint: string,
    entt?: (new() => EnTT)
  ) {
    // Bind to endpoint
    super._bind(endpoint, entt);
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
      this._req.offset = (e.state.pageIndex * e.state.pageLength) || 0;
      this._req.limit  = e.state.pageLength;
      this._req.ordering = [];
      const order = new ApiSearchRequestOrderModel();
      order.key = e.state.orderingField;
      order.ascending = e.state.orderingAscDirection;
      this._req.ordering.push(order);
      this._req.filters = [];
      for (const filterKey in e.state.filters) {
        if (e.state.filters.hasOwnProperty(filterKey) && e.state.filters[filterKey]) {
          const filter = new ApiSearchRequestFilterModel();
          filter.keys = [filterKey];
          filter.values = [e.state.filters[filterKey]];
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
    if (defaultPageLength !== undefined) {
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

    // Bind toString to the adapter
    this.toString = this.toString.bind(this);
  }

  /**
   * Binds service instance to a particular endpoint
   * @param endpoint Endpoint name (relative path)
   * @param entt (Optional) EnTT class to cast response as
   */
  public bind (
    endpoint: string,
    entt?: (new() => EnTT)
  ) {
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
  public create (
    endpoint: string,
    entt?: (new() => EnTT)
  ) {
    const adapter = new ApiEndpointToGridAdapter(this._endpointFactory);
    adapter.bind(endpoint, entt);
    return adapter;
  }
}
