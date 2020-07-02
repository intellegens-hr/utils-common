// API-to-Grid adapter service
// ----------------------------------------------------------------------------

// Import dependencies
import { Subject, interval } from 'rxjs';
import { debounce } from 'rxjs/operators';
import { EnTT } from '@ofzza/entt-rxjs';
import { ApiEndpoint } from '../../';
import { HttpRequestPromise } from '../../../Http'
import { EventEmitter, Output } from '@angular/core';

// Import data models
import { ApiSearchRequestModel } from '../../../../../data';

/**
 * Holds ApiEndpointToGridAdapter configuration
 */
export class ApiEndpointToGridAdapterConfiguration {

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
export class ApiEndpointBaseAdapter {

  /**
   * Injected ApiEndpoint service instance
   */
  protected _endpoint: ApiEndpoint;

  /**
   * Holds targeted EnTT class
   */
  protected _entt = undefined as new() => EnTT

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
  protected _searchReqPromise: HttpRequestPromise<ApiSearchRequestModel>;

  /**
   * Observable subject triggered by change event, used to debounce change event handling
   */
  protected _changeDebouncedSubject: Subject<any>

  /**
   * Holds promise of items found by the last search
   */
  protected _dataSource = Promise.resolve([]);

  /**
   * Holds total number of records found by the last search
   */
  protected _dataLength = 0;

  /**
   * Event fired before search request is fully formed allowing for changes to be made
   */
  public beforeSearch: EventEmitter<ApiSearchRequestModel> = new EventEmitter();

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
    // Store properties
    this._entt = entt;
    // Bind to endpoint
    this._endpoint.bind(endpoint, entt);
    // Reset request
    this._req = new ApiSearchRequestModel();
    this._req.limit = this._config.defaultPageLength;
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

          // Emit search request so other components can modify data before sending (if needed)
          this.beforeSearch.emit(this._req);

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
    // (Re)Run search
    this._search();
  }

}
