// API search response data-model
// ----------------------------------------------------------------------------

// Import dependencies
import { ApiResponseModel } from '../ApiResponse';

/**
 * Base search response data-model
 */
export class ApiSearchResponseModel extends ApiResponseModel {
  /**
   * Additional search metadata
   */
  public metadata = {} as any;
}
