// API response data-model
// ----------------------------------------------------------------------------

/**
 * Base response data-model
 */
export class ApiResponseModel {
  /**
   * If request was processed successfully
   */
  public success = undefined as boolean;
  /**
   * Response data
   */
  public data    = undefined as any[];
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
