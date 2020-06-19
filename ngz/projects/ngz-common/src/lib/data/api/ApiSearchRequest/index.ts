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
  //TODO: SA JUROM VIDJETI!!! Šaljemo li kao string ...
  public static Operators = {
    EQUALS: 0,
    STRING_CONTAINS: 1,
    STRING_WILDCARD: 2,
    LESS_THAN: 3,
    LESS_THAN_OR_EQUAL_TO: 4,
    GREATER_THAN: 5,
    GREATER_THAN_OR_EQUAL_TO: 6,
    FULL_TEXT_SEARCH: 7
  };

  constructor () { super(); super.entt(); }

  /**
   * Name of the filtering property
   */
  public keys   = undefined as string[];
  /**
   * Direct or negated filtering comparison
   */
  public negateExpression   = false;
  /**
   * Filtering operator being used
   */
  public operator  = ApiSearchRequestFilterModel.Operators.STRING_CONTAINS;
  /**
   * Value to filter by
   */
  public values = [] as string[];
}

/**
 * Base search request data-model
 */
export class ApiSearchRequestModel extends ApiRequestModel {

  /**
   * Enumerates allowed ApiSearchRequestFilterModel.type values
   */
  // tslint:disable-next-line: variable-name
  public static FilterType = ApiSearchRequestFilterModel.Operators;

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
   * Array of filters to filter by
   */
  @Serializable({ cast: [ApiSearchRequestFilterModel] })
  public filters = [];
  /**
   * Array of search criteria to search by
   */
  @Serializable({ cast: [ApiSearchRequestFilterModel] })
  public search = [];
  /**
   * Array of ordering rules to order the searched for results by
   */
  @Serializable({ cast: [ApiSearchRequestOrderModel] })
  public ordering = [];
}
