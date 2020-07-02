// API search request base data-model
// ----------------------------------------------------------------------------

// Import dependencies
import { EnTT, Serializable } from '@ofzza/entt-rxjs';
import { ApiRequestModel } from '../ApiRequest';

  /**
   * Enumerates allowed ApiSearchRequestFilterModel.type values
   */
export enum Operators {
  EQUALS = 'EQUALS',
  STRING_CONTAINS = 'STRING_CONTAINS',
  STRING_WILDCARD = 'STRING_WILDCARD',
  LESS_THAN = 'LESS_THAN',
  LESS_THAN_OR_EQUAL_TO = 'LESS_THAN_OR_EQUAL_TO',
  GREATER_THAN = 'GREATER_THAN',
  GREATER_THAN_OR_EQUAL_TO = 'GREATER_THAN_OR_EQUAL_TO'
};

export enum LogicOperators {
  ANY = 'ANY',
  ALL = 'ALL'
}

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
export class ApiSearchRequestCriteriaModel extends EnTT {

  constructor () { super(); super.entt(); }

  /**
   * Name of the filtering property
   */
  public keys = [] as string[];

  /**
   * Relation when matching multiple keys
   */
  public keysLogic = LogicOperators.ALL;

  /**
   * Value to filter by
   */
  public values = [] as string[];

  /**
   * Relation when matching multiple values
   */
  public valuesLogic = LogicOperators.ANY;

  /**
   * Direct or negated filtering comparison
   */
  public negate = false;
  /**
   * Filtering operator being used
   */
  public operator  = Operators.STRING_CONTAINS;

  /**
   * Nested filters
   */
  @Serializable({ cast: [ApiSearchRequestCriteriaModel] })
  public criteria = [] as ApiSearchRequestCriteriaModel[];

  /**
   * Relation between multiple nested filters
   */
  public criteriaLogic = LogicOperators.ALL;
}

/**
 * Base search request data-model
 */
export class ApiSearchRequestModel extends ApiSearchRequestCriteriaModel implements ApiRequestModel {

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
   * Array of ordering rules to order the searched for results by
   */
  @Serializable({ cast: [ApiSearchRequestOrderModel] })
  public order = [];

  /**
   * If set to true, first rule for ordering result dataset will be
   * number of matching fields found
   */
  public orderByMatchCount = false;
}
