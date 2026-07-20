import { describe, it, expect } from 'vitest'
import {
  resultMap,
  resultFlatMap,
  resultTraverse,
  fromNullable,
  fromPromise,
  succeed,
  fail,
  resultAll,
  resultFilterMap,
} from '../../utils/result-helpers'
import {
  mapResponseToEntity,
  mapResponseToResult,
  mapResponseListToEntityList,
  mapResponseListToResult,
} from '../../mappers/response-mapper'

describe('Result Helpers', () => {
  describe('resultMap', () => {
    it('should transform success value', () => {
      const result = { isSuccess: true, isFailure: false, statusCode: 200, data: 5 }
      const mapped = resultMap(result, (v) => v * 2)
      expect(mapped.isSuccess).toBe(true)
      expect(mapped.data).toBe(10)
    })

    it('should propagate failure', () => {
      const result = { isSuccess: false, isFailure: true, statusCode: 404, message: 'Not found', errors: [] }
      const mapped = resultMap(result, (v: number) => v * 2)
      expect(mapped.isFailure).toBe(true)
      expect(mapped.statusCode).toBe(404)
    })
  })

  describe('resultFlatMap', () => {
    it('should chain successful results', () => {
      const result = { isSuccess: true, isFailure: false, statusCode: 200, data: 5 }
      const chained = resultFlatMap(result, (v) => ({ isSuccess: true, isFailure: false, statusCode: 200, data: v * 2 }))
      expect(chained.isSuccess).toBe(true)
      expect(chained.data).toBe(10)
    })

    it('should propagate failure', () => {
      const result = { isSuccess: false, isFailure: true, statusCode: 400, message: 'Error', errors: [] }
      const chained = resultFlatMap(result, (_v: number) => ({ isSuccess: true, isFailure: false, statusCode: 200, data: 10 }))
      expect(chained.isFailure).toBe(true)
    })
  })

  describe('resultTraverse', () => {
    it('should map array through Result', () => {
      const items = [1, 2, 3]
      const result = resultTraverse(items, (v) => ({ isSuccess: true, isFailure: false, statusCode: 200, data: v * 2 }))
      expect(result.isSuccess).toBe(true)
      expect(result.data).toEqual([2, 4, 6])
    })

    it('should return first failure', () => {
      const items = [1, 2, 3]
      const result = resultTraverse(items, (v) => v === 2
        ? { isSuccess: false, isFailure: true, statusCode: 400, message: 'Error', errors: [] }
        : { isSuccess: true, isFailure: false, statusCode: 200, data: v })
      expect(result.isFailure).toBe(true)
      expect(result.statusCode).toBe(400)
    })
  })

  describe('fromNullable', () => {
    it('should convert non-null value to success', () => {
      const result = fromNullable(42, 'Not found')
      expect(result.isSuccess).toBe(true)
      expect(result.data).toBe(42)
    })

    it('should convert null to failure', () => {
      const result = fromNullable(null, 'Not found', 404)
      expect(result.isFailure).toBe(true)
      expect(result.statusCode).toBe(404)
      expect(result.message).toBe('Not found')
    })

    it('should convert undefined to failure', () => {
      const result = fromNullable(undefined, 'Not found')
      expect(result.isFailure).toBe(true)
    })
  })

  describe('fromPromise', () => {
    it('should convert successful promise', async () => {
      const promise = Promise.resolve(42)
      const result = await fromPromise(promise, 'Failed')
      expect(result.isSuccess).toBe(true)
      expect(result.data).toBe(42)
    })

    it('should convert failed promise', async () => {
      const promise = Promise.reject(new Error('Error'))
      const result = await fromPromise(promise, 'Failed', 500)
      expect(result.isFailure).toBe(true)
      expect(result.statusCode).toBe(500)
    })
  })

  describe('succeed', () => {
    it('should create success result', () => {
      const result = succeed(42)
      expect(result.isSuccess).toBe(true)
      expect(result.data).toBe(42)
    })

    it('should allow custom status code', () => {
      const result = succeed(42, 201)
      expect(result.statusCode).toBe(201)
    })
  })

  describe('fail', () => {
    it('should create failure result', () => {
      const result = fail('Error', 400)
      expect(result.isFailure).toBe(true)
      expect(result.message).toBe('Error')
      expect(result.statusCode).toBe(400)
    })
  })

  describe('resultAll', () => {
    it('should combine multiple success results', () => {
      const result = resultAll(
        { isSuccess: true, isFailure: false, statusCode: 200, data: 1 },
        { isSuccess: true, isFailure: false, statusCode: 200, data: 2 }
      )
      expect(result.isSuccess).toBe(true)
      expect(result.data).toEqual([1, 2])
    })

    it('should return first failure', () => {
      const result = resultAll(
        { isSuccess: true, isFailure: false, statusCode: 200, data: 1 },
        { isSuccess: false, isFailure: true, statusCode: 400, message: 'Error', errors: [] }
      )
      expect(result.isFailure).toBe(true)
    })
  })

  describe('resultFilterMap', () => {
    it('should filter and map successful results', () => {
      const items = [1, 2, 3]
      const result = resultFilterMap(items, (v) => v > 1
        ? { isSuccess: true, isFailure: false, statusCode: 200, data: v * 2 }
        : { isSuccess: false, isFailure: true, statusCode: 400, message: 'Skip', errors: [] }
      )
      expect(result.isFailure).toBe(true)
    })

    it('should return all results when all succeed', () => {
      const items = [1, 2, 3]
      const result = resultFilterMap(items, (v) => 
        ({ isSuccess: true, isFailure: false, statusCode: 200, data: v * 2 })
      )
      expect(result.isSuccess).toBe(true)
      expect(result.data).toEqual([2, 4, 6])
    })
  })
})

describe('Response Mapper', () => {
  describe('mapResponseToEntity', () => {
    it('should map response to entity', () => {
      const response = { id: '1', name: 'Test' }
      const mapper = (r: typeof response) => ({ id: r.id, title: r.name })
      const entity = mapResponseToEntity(response, mapper)
      expect(entity).toEqual({ id: '1', title: 'Test' })
    })
  })

  describe('mapResponseToResult', () => {
    it('should map response to Result', () => {
      const response = { id: '1', name: 'Test' }
      const mapper = (r: typeof response) => ({ id: r.id, title: r.name })
      const result = mapResponseToResult(response, mapper)
      expect(result.isSuccess).toBe(true)
      expect(result.data).toEqual({ id: '1', title: 'Test' })
    })

    it('should handle mapping errors', () => {
      const response = { id: '1' }
      const mapper = (_r: typeof response) => { throw new Error('Map failed') }
      const result = mapResponseToResult(response, mapper)
      expect(result.isFailure).toBe(true)
    })
  })

  describe('mapResponseListToEntityList', () => {
    it('should map array of responses', () => {
      const responses = [{ id: '1' }, { id: '2' }]
      const mapper = (r: typeof responses[0]) => ({ id: r.id, title: 'Item' })
      const entities = mapResponseListToEntityList(responses, mapper)
      expect(entities).toEqual([{ id: '1', title: 'Item' }, { id: '2', title: 'Item' }])
    })
  })

  describe('mapResponseListToResult', () => {
    it('should map array to Result', () => {
      const responses = [{ id: '1' }, { id: '2' }]
      const mapper = (r: typeof responses[0]) => ({ id: r.id })
      const result = mapResponseListToResult(responses, mapper)
      expect(result.isSuccess).toBe(true)
      expect(result.data?.length).toBe(2)
    })
  })
})