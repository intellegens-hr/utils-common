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
  protected _filterBy = [] as string[];
  /**
   * Holds names of properties to order by (starting with a '!' character if ordering descending)
   */
  protected _orderBy = [] as string[];

  /**
   * Holds function converting EnTT instance to a representative string
   */
  protected _enttToString = undefined as (entt: EnTT) => string

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
   * @param filterBy Names of properties to be filter by
   * @param orderBy Names of properties to be order by (starting with a '!' character if ordering descending)
   * @param enttToString (Optional) Function converting EnTT instance to a representative string
   */
  protected _bind (
    endpoint: string,
    entt?: (new() => EnTT),
    {
      filterBy     = [] as string[],
      orderBy      = [] as string[],
      enttToString = undefined as (entt: EnTT) => string
    } = {}
  ) {
    // Bind to endpoint
    super._bind(endpoint, entt);
    // Store properties
    this._filterBy = filterBy;
    this._orderBy = orderBy;
    this._enttToString = enttToString;
  }

  /**
   * Autocomplete input adapter: handles autocomplete component's change event, updates and reruns the search
   * @param value Autocomplete change event value
   */
  protected _processChanged (value: any) {
    // Update request filters
    this._req.search = this._filterBy.map(filterBy => {
      const filter = new ApiSearchRequestFilterModel();
      filter.key = filterBy;
      filter.values = [value]
      return filter;
    });
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
   * @param filterBy Names of properties to search by
   * @param orderBy Names of properties to search by (starting with a '!' character if ordering descending)
   * @param entt (Optional) EnTT class to cast response as
   * @param enttToString (Optional) Function converting EnTT instance to a representative string
   */
  public bind (
    endpoint: string,
    filterBy: string[],
    orderBy: string[],
    entt?: (new() => EnTT),
    enttToString?: (entt: EnTT) => string
  ) {
    // (Re)Create endpoint instance
    this._endpoint = this._endpointFactory.create(endpoint, entt);
    // Bind to endpoint
    this._bind(endpoint, entt, { filterBy, orderBy, enttToString });
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

  /**
   * Converts EnTT instance to a representative string
   * @param instance EnTT instance to convert to string
   */
  public toString (instance: EnTT) {
    return (this._enttToString ? this._enttToString(instance) : instance);
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
   * @param filterBy Names of properties to search by
   * @param orderBy (Optional) Names of properties to search by (starting with a '!' character if ordering descending)
   * @param entt (Optional) EnTT class to cast response as
   * @param enttToString (Optional) Function converting EnTT instance to a representative string
   */
  public create (
    endpoint: string,
    filterBy: string[],
    orderBy?: string[],
    entt?: (new() => EnTT),
    enttToString?: (entt: EnTT) => string
  ) {
    const adapter = new ApiEndpointToAutocompleteAdapter(this._endpointFactory);
    adapter.bind(endpoint, filterBy, (orderBy || filterBy), entt, (enttToString || entt?.toString || undefined));
    return adapter;
  }
}
