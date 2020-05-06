// API-to-Autocomplete adapter service
// ----------------------------------------------------------------------------

// Import dependencies
import { Injectable } from '@angular/core';
import { EnTT } from '@ofzza/entt-rxjs';
import { ApiEndpointFactory, ApiEndpoint } from '../../';

// Import ApiEndpointToGridAdapter
import { ApiEndpointToGridAdapterInternal } from '../ApiEndpointToGridAdapter';

/**
 * API Endpoint to Autocomplete component adapter (internal implementation)
 */
export class ApiEndpointToAutocompleteAdapterInternal extends ApiEndpointToGridAdapterInternal {

  /**
   * Holds name of property to search by
   */
  protected _key = null;

  /**
   * Holds items found by the last search
   */
  protected _dataItems = [];

  /**
   * Executes a search with current search request parameters and extracts returned items once resolved
   */
  protected _search () {
    // Run search
    super._search();
    // Subscribe to items once search resolved
    this._dataSource.then(
      (items) => {
        this._dataItems = items;
      },
      () => {
        this._dataItems = [];
      }
    );
  }

  /**
   * Binds service instance to a particular endpoint
   * @param endpoint Endpoint name (relative path)
   * @param entt (Optional) EnTT class to cast response as
   * @param key Name of property to be search by
   */
  protected _bind (endpoint: string, entt?: (new() => EnTT), { key = undefined as string } = {}) {
    this._key = key;
    super._bind(endpoint, entt);
  }

  /**
   * Autocomplete input adapter: handles autocomplete component's change event, updates and reruns the search
   * @param value Autocomplete change event value
   */
  protected _processChanged (value: any) {
    // Check if running locally
    if (!this._config.preload) {
      this._req.filters = [
        { key: this._key, value }
      ];
    }
    // (Re)Run search
    this._search();
  }

}

/**
 * API Endpoint to Autocomplete component adapter
 * Adapts standard API endpoint(s) for usage by a <mat-autocomplete /> component
 */
export class ApiEndpointToAutocompleteAdapter extends ApiEndpointToAutocompleteAdapterInternal {

  /**
   * Gets underlying endpoint service instance
   */
  public get endpoint () {
    return this._endpoint;
  }

  /**
   * Configures adapter behavior
   * @param preload If all data should be loaded initially and all further processing done locally
   * @param debounceInterval Debouncing interval to be used when handling <input /> component's change events
   * @param defaultPageLength Maximum number of displayed items
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


  /**
   * Autocomplete input adapter: Gets promise of items found by the last search
   */
  public get dataItems () {
    return this._dataItems;
  }

  constructor (private _endpointFactory: ApiEndpointFactory) {
    super();
  }

  /**
   * Binds service instance to a particular endpoint
   * @param endpoint Endpoint name (relative path)
   * @param key Name of property to search by
   * @param entt (Optional) EnTT class to cast response as
   */
  public bind (endpoint: string, key: string, entt?: (new() => EnTT)) {
    // (Re)Create endpoint instance
    this._endpoint = this._endpointFactory.create(endpoint, entt);
    // Bind to endpoint
    this._bind(endpoint, entt, { key });
  }

  /**
   * Repeats latest search request
   */
  public refresh () {
    this._search();
  }

  /**
   * Autocomplete input adapter: handles autocomplete component's change event, updates and reruns the search
   * @param value Updated search value
   */
  public changed (value: any) {
    this._changed(value);
  }


}

/**
 * API Endpoint to Autocomplete component adapter factory
 * Instantiates ApiEndpointToAutocompleteAdapter instances
 */
@Injectable()
export class ApiEndpointToAutocompleteAdapterFactory {
  constructor (private _endpointFactory: ApiEndpointFactory) {}

  /**
   * Creates a new adapter instance
   * @param endpoint Endpoint name (relative path)
   * @param keys Endpoint search key
   * @param entt (Optional) EnTT class to cast response as
   */
  public create (endpoint: string, key: string, entt?: (new() => EnTT)) {
    const adapter = new ApiEndpointToAutocompleteAdapter(this._endpointFactory);
    adapter.bind(endpoint, key, entt);
    return adapter;
  }
}
