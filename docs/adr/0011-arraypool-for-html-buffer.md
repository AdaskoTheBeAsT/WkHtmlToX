# 0011 - Adopt `ArrayPool<byte>.Shared` for HTML buffer allocation

- Status: Accepted
- Date: 2020-03-27
- Traceability: `35b8f92` "ArrayPool<byte>.Shared.Rent introduced"

## Context

Every conversion copied HTML through a fresh `byte[]`. Under load this
allocated large buffers on the Large Object Heap and caused noticeable
Gen 2 pressure.

## Decision

Rent HTML buffers from `ArrayPool<byte>.Shared` and return them in
`finally` blocks. Callers keep passing `string` / `Stream` /
`byte[]`; pooling is an implementation detail.

## Consequences

- Significantly lower allocation counts for batch workloads.
- Buffers must be returned on every path - enforced with `try/finally`
  and covered by memory tests (ADR 0010, ADR 0034).
- Combined with ADR 0013 this lets us drop `RecyclableMemoryStream`.
