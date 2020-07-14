// API search response data-model
// ----------------------------------------------------------------------------

// Import dependencies
import { ApiResponseModel } from '../ApiResponse';

/**
 * Base search response data-model
 */
export class ApiSearchResponseModel<T = any> extends ApiResponseModel<T> {
  /**
   * Additional search metadata
   */
  public metadata = {} as any;
}
