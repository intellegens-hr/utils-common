// API-to-Autocomplete adapter service
// ----------------------------------------------------------------------------

// Import dependencies
import { Injectable } from '@angular/core';
import { EnTT } from '@ofzza/entt-rxjs';
import { ApiEndpointFactory } from '../../';

// Import base
import { ApiEndpointBaseAdapter } from '../ApiEndpointBaseAdapter';

// Import data models
import { ApiSearchRequestOrderModel, ApiSearchRequestFilterModel } from '../../../../../data';

/**
 * API Endpoint to Autocomplete component adapter (internal implementation)
 */
export class ApiEndpointToAutocompleteAdapterInternal extends ApiEndpointBaseAdapter {

  /**
   * Holds names of properties to filter by
   */
  protected _searchBy = [] as string[];
  /**
   * Gets names of properties to filter by
   */
  public get searchBy () { return this._searchBy; }
  /**
   * Holds names of properties to order by (starting with a '!' character if ordering descending)
   */
  protected _orderBy = [] as string[];
  /**
   * Gets names of properties to order by (starting with a '!' character if ordering descending)
   */
  public get orderBy () { return this._orderBy; }
  /**
   * Holds array of IDs, or a function returning an array of IDs to exclude from results
   */
  protected _excludeIds = undefined as { [key: string]: string[] } | (() => { [key: string]: string[] });
  /**
   * Gets array of IDs, or a function returning an array of IDs to exclude from results
   */
  public get excludeIds () { return this._excludeIds; }

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
   * @param searchBy Names of properties to be filter by
   * @param orderBy Names of properties to be order by (starting with a '!' character if ordering descending)
   * @param excludeIds (Optional) Array of IDs, or a function returning an array of IDs to exclude from results
   */
  protected _bind (
    endpoint: string,
    entt?: (new() => EnTT),
    {
      searchBy     = [] as string[],
      orderBy      = [] as string[],
      excludeIds   = undefined as { [key: string]: string[] } | (() => { [key: string]: string[] })
    } = {}
  ) {
    // Bind to endpoint
    super._bind(endpoint, entt);
    // Store properties
    this._searchBy = searchBy;
    this._orderBy = orderBy;
    this._excludeIds = excludeIds;
  }

  /**
   * Autocomplete input adapter: handles autocomplete component's change event, updates and reruns the search
   * @param value Autocomplete change event value
   */
  protected _processChanged (value: any) {
    // Update require filters
    const excludedIds = (this._excludeIds ? (this._excludeIds instanceof Function ? this._excludeIds() : this._excludeIds) : {});
    for (const key of Object.keys(excludedIds)) {
      this._req.filters = excludedIds[key].map(excludedId => {
        const filter = new ApiSearchRequestFilterModel();
        filter.key = key;
        filter.type = ApiSearchRequestFilterModel.Type.ExactMatch;
        filter.comparisonType = ApiSearchRequestFilterModel.ComparisonType.Negated;
        filter.values = [excludedId]
        return filter;
      });
    }
    // Update request search
    if (value) {
      this._req.search = this._searchBy.map(searchBy => {
        const search = new ApiSearchRequestFilterModel();
        search.key = searchBy;
        search.values = [value, ...(value.indexOf(' ') !== -1 ? value.split(' ') : [])]
        return search;
      });
    } else {
      this._req.search = [];
    }
    // Update request ordering
    this._req.ordering = this._orderBy.map(orderBy => {
      const ordering = new ApiSearchRequestOrderModel();
      ordering.key = (orderBy.startsWith('!') ? orderBy.substr(1) : orderBy);
      ordering.ascending = !orderBy.startsWith('!');
      return ordering;
    });
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
   * @param debounceInterval Debouncing interval to be used when handling <input /> component's change events
   * @param defaultPageLength Maximum number of displayed items
   */
  public configure ({
    debounceInterval  = undefined as number,
    defaultPageLength = undefined as number
  } = {}) {
    this._config.preload = false;
    if (debounceInterval !== undefined) {
      this._config.debounceInterval = debounceInterval;
    }
    if (defaultPageLength !== undefined) {
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

    // Bind toString to the adapter
    this.toString = this.toString.bind(this);
  }

  /**
   * Binds service instance to a particular endpoint
   * @param endpoint Endpoint name (relative path)
   * @param entt (Optional) EnTT class to cast response as
   * @param searchBy Names of properties to search by
   * @param orderBy Names of properties to search by (starting with a '!' character if ordering descending)
   * @param excludeIds (Optional) Array of IDs, or a function returning an array of IDs to exclude from results
   */
  public bind (
    endpoint: string,
    entt?: (new() => EnTT),
    {
      searchBy     = [] as string[],
      orderBy      = [] as string[],
      excludeIds   = undefined as { [key: string]: string[] } | (() => { [key: string]: string[] })
    } = {}
  ) {
    // (Re)Create endpoint instance
    this._endpoint = this._endpointFactory.create(endpoint, entt);
    // Bind to endpoint
    this._bind(endpoint, entt, { searchBy, orderBy, excludeIds });
  }

  /**
   * Repeats latest search request
   */
  public refresh () {
    this._search();
  }

  /**
   * Autocomplete input adapter: handles autocomplete component's opened event, updates and reruns the search
   * @param e Event?
   */
  public opened (e) {
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
   * @param entt (Optional) EnTT class to cast response as
   * @param searchBy Names of properties to search by
   * @param orderBy (Optional) Names of properties to search by (starting with a '!' character if ordering descending)
   * @param excludeIds (Optional) Array of IDs, or a function returning an array of IDs to exclude from results
   */
  public create (
    endpoint: string,
    entt?: (new() => EnTT),
    {
      searchBy     = [] as string[],
      orderBy      = [] as string[],
      excludeIds   = undefined as { [key: string]: string[] } | (() => { [key: string]: string[] })
    } = {}
  ) {
    const adapter = new ApiEndpointToAutocompleteAdapter(this._endpointFactory);
    adapter.bind(endpoint, entt, {
      searchBy,
      orderBy: (orderBy || searchBy),
      excludeIds
    });
    return adapter;
  }
}
