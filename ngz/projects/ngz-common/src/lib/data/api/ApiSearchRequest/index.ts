// API search request base data-model
// ----------------------------------------------------------------------------

// Import dependencies
import { EnTT, Serializable } from '@ofzza/entt-rxjs';
import { ApiRequestModel } from '../ApiRequest';

/**
 * Base search request order data-model
 */
export class ApiSearchRequestOrderModel extends EnTT {
  constructor () { super(); super.entt(); }

  /**
   * Name of the ordering property
   */
  public key = undefined as string;
  /**
   * Ordering direction
   */
  public ascending = true as boolean;
}

/**
 * Base search request filter data-model
 */
export class ApiSearchRequestFilterModel extends EnTT {

  /**
   * Enumerates allowed ApiSearchRequestFilterModel.type values
   */
  // tslint:disable-next-line: variable-name
  public static Type = {
    Default: 1
  };

  constructor () { super(); super.entt(); }

  /**
   * Name of the filtering property
   */
  public key   = undefined as string;
  /**
   * Type of filtering comparison being used
   */
  public type  = ApiSearchRequestFilterModel.Type.Default;
  /**
   * Value to filter by
   */
  public value = undefined as string;
}

/**
 * Base search request data-model
 */
export class ApiSearchRequestModel extends ApiRequestModel {

  /**
   * Enumerates allowed ApiSearchRequestFilterModel.type values
   */
  // tslint:disable-next-line: variable-name
  public static FilterType = ApiSearchRequestFilterModel.Type;

  constructor () { super(); super.entt(); }

  /**
   * Offset of searched for records
   */
  public offset = 0 as number;
  /**
   * Limited number of searched for records to return
   */
  public limit  = undefined as number;
  /**
   * Array of filters to search by
   */
  @Serializable({ cast: [ApiSearchRequestFilterModel] })
  public filters = [];
  /**
   * Array of ordering rules to order the searched for results by
   */
  @Serializable({ cast: [ApiSearchRequestOrderModel] })
  public ordering = [];
}
