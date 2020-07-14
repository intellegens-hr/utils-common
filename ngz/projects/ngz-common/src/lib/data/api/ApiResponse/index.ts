// API response data-model
// ----------------------------------------------------------------------------

/**
 * Base response data-model
 */
export class ApiResponseModel<T = any> {
  /**
   * If request was processed successfully
   */
  public success = undefined as boolean;
  /**
   * Response data
   */
  public data    = undefined as T[];
  /**
   * Response errors
   */
  public errors  = [] as ApiRequestModelError[];
}

/**
 * Response error
 */
class ApiRequestModelError {
  /**
   * Unique error code
   */
  public code: string;
}
