// API-to-CRUD-Component adapter service
// ----------------------------------------------------------------------------

// Import dependencies
import { Subject, interval } from 'rxjs';
import { debounce } from 'rxjs/operators';
import { Injectable } from '@angular/core';
import { EnTT } from '@ofzza/entt-rxjs';
import { ApiEndpointFactory, ApiEndpoint } from '../../';
import { HttpRequestPromise } from '../../../Http'

/**
 * Marks an EnTT data model property as a primary key
 */
export const ApiEndpointPrimaryKeyTag = Symbol('ApiEndpointToCrudComponentAdapter PrimaryKey EnTT Tag');

/**
 * Adapts standard API endpoint(s) for usage by a typical CRUD component
 */
export class ApiEndpointToCrudComponentAdapter<T extends EnTT> {

  /**
   * Holds name of the primary key property
   */
  private _primaryKey: string;
  /**
   * Gets name of the primary key property
   */
  public get primaryKey () { return this._primaryKey; }

  /**
   * Holds API endpoint instance
   */
  private _endpoint: ApiEndpoint;
  /**
   * Gets API endpoint instance
   */
  public get endpoint () { return this._endpoint; }

  /**
   * Holds adapter processing status
   */
  private _isProcessing = false;
  /**
   * Gets adapter processing status
   */
  public get isProcessing () { return this._isProcessing; }

  /**
   * Holds model instance fetched from the API
   */
  private _fetchedModel: T;
  /**
   * Gets model instance fetched from the API
   */
  public get fetchedModel () { return this._fetchedModel; }

  /**
   * Holds new model instance to be added via the API
   */
  private _createModel: T;
  /**
   * Gets new model instance to be added via the API
   */
  public get createModel () { return this._createModel; }
  /**
   * Holds new model errors from last API create attempt
   */
  private _createErrors: any = {};
  /**
   * Gets new model errors from last API create attempt
   */
  public get createErrors () { return this._createErrors; }

  /**
   * Holds editing model instance to be updated via the API
   */
  private _editingModel: T;
  /**
   * Holds editing model original instance to be updated after cloned instance is returned via the API
   */
  private _editingModelOriginal: T;
  /**
   * Gets editing model instance to be updated via the API
   */
  public get editingModel () { return this._editingModel; }
  /**
   * Holds editing model errors from last API update attempt
   */
  private _editingErrors: any = {};
  /**
   * Gets editing model errors from last API update attempt
   */
  public get editingErrors () { return this._editingErrors; }

  /**
   * Holds deleting model instance to be deleted via the API
   */
  private _deletingModel: T;
  /**
   * Gets deleting model instance to be deleted via the API
   */
  public get deletingModel () { return this._deletingModel; }

  constructor (
    private _endpointFactory: ApiEndpointFactory,
    private _entt: (new() => T)
  ) {
    // Check for primary key
    const pks = EnTT.findTaggedProperties(ApiEndpointPrimaryKeyTag, { from: this._entt });
    if (pks.length === 1) {
      this._primaryKey = pks[0];
    } else {
      throw new Error('Only models with a single property tagged with ApiEndpointPrimaryKeyTag can be used with ApiEndpointToCrudComponentAdapter!');
    }
    // Initialize create model
    this._createModel = new this._entt();
    // Initialize adapter
    this._endpoint = this._endpointFactory.create();
    // Bind toString to the adapter
    this.toString = this.toString.bind(this);
  }

  /**
   * Creates a new instance via the API
   * @param callback Method to run once API create has been executed
   */
  public async create () {
    return new Promise<T>(async (resolve, reject) => {
      // Check validation errors
      if (this._createModel.valid) {

        // Set loading state
        this._isProcessing = true;

        // Register changes with API endpoint
        try {
          // Store changes
          const updated = await this.endpoint.create(this._createModel);
          // Resolve promise
          resolve(updated);
          // Reset new
          this._createModel = new this._entt();
        } catch (err) {
          // Reject promise
          reject(err);
        }

        // Reset loading state
        this._isProcessing = false;

        // Reset errors
        this._createErrors = {};

      } else {

        // Set errors
        this._createErrors = this._createModel.errors;

        // Reject promise
        reject(new Error('Validation failed!'));

      }
    })
  }

  /**
   * Gets instance from endpoint and sets it for editing
   * @param ID of the instance to be edited
   */
  public async fetch (id: any) {
    // Set loading state
    this._isProcessing = true;

    // Fetch instance
    this._fetchedModel = await this._endpoint.get(id);

    // Reset loading state
    this._isProcessing = false;
  }
  /**
   * Sets fetched model
   */
  public fetchedSet (instance: T) {
    this._fetchedModel = instance;
  }
  /**
   * Clears fetched model
   */
  public fetchedClear () {
    this._fetchedModel = undefined;
  }

  /**
   * Begins editing a model
   * @param instance Model instance to edit
   */
  public editBegin (instance: T) {
    // Clone editing instance as editing model
    this._editingModel = EnTT.clone(instance);
    this._editingModelOriginal = instance;
  }
  /**
   * Checks if model instance is currently being edited
   * @param instance INstance to check
   */
  public isEditing (instance: T) {
    return (this._editingModelOriginal === instance);
  }
  /**
   * Cancels editing a model
   */
  public editCancel () {
    // Clear editing model
    this._editingModel = undefined;
    this._editingModelOriginal = undefined;
    // Reset errors
    this._editingErrors = {};
  }
  /**
   * Applies changes to the editing model via the API
   */
  public async editConfirm () {
    return new Promise<T>(async (resolve, reject) => {
      // Check validation errors
      if (this._editingModel.valid) {

        // Set loading state
        this._isProcessing = true;

        // Register changes with API endpoint
        try {
          // Store changes
          const updated = await this.endpoint.update(this._editingModel[this._primaryKey], this._editingModel);
          // Update table view
          EnTT.clone(updated, { target: this._editingModelOriginal });
          // Resolve promise
          resolve(this._editingModelOriginal);
          // Stop editing
          this.editCancel();
        } catch (err) {
          // Reject promise
          reject(err);
        }

        // Reset loading state
        this._isProcessing = false;

      } else {

        // Set errors
        this._editingErrors = this._editingModel.errors;

        // Reject promise
        reject(new Error('Validation failed!'));

      }
    });
  }

  /**
   * Begins deleting a model
   * @param instance Model instance to delete
   */
  public deleteBegin (instance: T) {
    // Store editing instance as editing model
    this._deletingModel = instance;
  }
  /**
   * Checks if model instance is currently being deleted
   * @param instance INstance to check
   */
  public isDeleting (instance?: T) {
    if (instance) {
      return (this._deletingModel === instance);
    } else {
      return !!this._deletingModel;
    }
  }
  /**
   * Cancels deleting a model
   */
  public deleteCancel () {
    // Clear deleting model
    this._deletingModel = undefined;
  }
  /**
   * Applies delete of the selected model via the API
   */
  public deleteConfirm () {
    return new Promise<T>(async (resolve, reject) => {

      // Set loading state
      this._isProcessing = true;

      // Register changes with API endpoint
      try {
        // Store changes
        const updated = await this.endpoint.delete(this._deletingModel[this._primaryKey], this._deletingModel);
        // Resolve promise
        resolve(updated);
        // Stop editing
        this.deleteCancel();
      } catch (err) {
        reject(err);
      }

      // Reset loading state
      this._isProcessing = false;

    });
  }

}

/**
 * API Endpoint to CRUD component component adapter factory
 * Instantiates ApiEndpointToCrudComponentAdapter instances
 */
@Injectable()
export class ApiEndpointToCrudComponentAdapterFactory {

  constructor (private _endpointFactory: ApiEndpointFactory) {}

  /**
   * Creates a new adapter instance
   * @param entt  EnTT class to cast response as
   */
  public create<T extends EnTT> (entt: (new() => T)) {
    const adapter = new ApiEndpointToCrudComponentAdapter<T>(
      this._endpointFactory,
      entt
    );
    return adapter;
  }

}
